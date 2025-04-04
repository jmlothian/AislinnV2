using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Aislinn.ChunkStorage.Interfaces;
using Aislinn.ChunkStorage.Storage;
using Aislinn.Core.Models;
using Aislinn.Core.Activation;
using Aislinn.Models.Activation;

namespace Aislinn.Core.Services
{
    public class ChunkActivationService
    {
        private readonly IChunkStore _chunkStore;
        private readonly IAssociationStore _associationStore;
        private readonly string _chunkCollectionId;
        private readonly string _associationCollectionId;
        private readonly IActivationModel _activationModel;

        // Default spreading configuration 
        private readonly ActivationParametersRegistry _parametersRegistry;


        public ChunkActivationService(
            IChunkStore chunkStore,
            IAssociationStore associationStore,
            IActivationModel activationModel,
            ActivationParametersRegistry parametersRegistry = null,
            string chunkCollectionId = "default",
            string associationCollectionId = "default")
        {
            _chunkStore = chunkStore ?? throw new ArgumentNullException(nameof(chunkStore));
            _associationStore = associationStore ?? throw new ArgumentNullException(nameof(associationStore));
            _activationModel = activationModel ?? throw new ArgumentNullException(nameof(activationModel));
            _parametersRegistry = parametersRegistry ?? new ActivationParametersRegistry();
            _chunkCollectionId = chunkCollectionId ?? throw new ArgumentNullException(nameof(chunkCollectionId));
            _associationCollectionId = associationCollectionId ?? throw new ArgumentNullException(nameof(associationCollectionId));
        }

        /// <summary>
        /// Activates a chunk with the specified ID and applies spreading activation
        /// </summary>
        public async Task<Chunk> ActivateChunkAsync(Guid chunkId, string emotionName = null, double activationBoost = 1.0)
        {
            // Get the collections
            var chunkCollection = await _chunkStore.GetCollectionAsync(_chunkCollectionId);
            if (chunkCollection == null)
                throw new InvalidOperationException($"Chunk collection '{_chunkCollectionId}' not found");

            var associationCollection = await _associationStore.GetCollectionAsync(_associationCollectionId);
            if (associationCollection == null)
                throw new InvalidOperationException($"Association collection '{_associationCollectionId}' not found");

            // Get the chunk to activate
            var chunk = await chunkCollection.GetChunkAsync(chunkId);
            if (chunk == null) return null;

            var parameters = _parametersRegistry.GetParameters(chunk);

            // Record the previous activation level for history
            double previousActivation = chunk.ActivationLevel;

            // Calculate new activation using the activation model
            chunk.ActivationLevel = _activationModel.CalculateActivation(chunk);

            // Apply activation boost adjusted by type-specific factor
            double adjustedBoost = activationBoost * parameters.BaseActivationBoost;
            chunk.ActivationLevel += adjustedBoost;

            // Create activation history item
            var activationItem = new ActivationHistoryItem
            {
                PreviousValue = previousActivation,
                NewValue = chunk.ActivationLevel,
                Change = chunk.ActivationLevel - previousActivation,
                SequenceNumber = chunk.ActivationHistory.Count > 0
                    ? chunk.ActivationHistory[0].SequenceNumber + 1
                    : 1,
                EmotionName = emotionName,
                ActivationDate = DateTime.Now
            };

            // Add to history (most recent first)
            chunk.ActivationHistory.Insert(0, activationItem);

            // Update the chunk
            await chunkCollection.UpdateChunkAsync(chunk);

            // Use type-specific parameters for max spreading depth
            int maxSpreadingDepth = Convert.ToInt32(parameters.SpreadingFactor * 3); // Scale depth by spreading factor
            maxSpreadingDepth = Math.Max(1, Math.Min(4, maxSpreadingDepth)); // Between 1-4

            // Start spreading activation
            await SpreadActivationAsync(
                chunk,
                null,
                maxSpreadingDepth,
                activationBoost,
                new HashSet<Guid> { chunkId },  // Mark the source as already visited
                activationItem, parameters);

            return chunk;
        }

        /// <summary>
        /// Apply activation decay to all chunks in the system
        /// </summary>
        public async Task ApplyDecayAsync()
        {
            var chunkCollection = await _chunkStore.GetCollectionAsync(_chunkCollectionId);
            if (chunkCollection == null) return;

            var chunks = await chunkCollection.GetAllChunksAsync();

            foreach (var chunk in chunks)
            {
                if (chunk.ActivationLevel > 0)
                {
                    double previousActivation = chunk.ActivationLevel;

                    // Apply decay using the activation model
                    // Using 1.0 as default time since last update (in seconds)
                    chunk.ActivationLevel = _activationModel.ApplyDecay(chunk, 1.0);

                    // We could optionally add a decay history item here
                    // Not sure how this might affect activation frequency though
                    /*
                    var decayItem = new ActivationHistoryItem
                    {
                        PreviousValue = previousActivation,
                        NewValue = chunk.ActivationLevel,
                        Change = chunk.ActivationLevel - previousActivation,
                        SequenceNumber = chunk.ActivationHistory.Count > 0 
                            ? chunk.ActivationHistory[0].SequenceNumber + 1
                            : 1,
                        EmotionName = "decay",
                        ActivationDate = DateTime.Now
                    };
                    
                    chunk.ActivationHistory.Insert(0, decayItem);
                    */

                    await chunkCollection.UpdateChunkAsync(chunk);
                }
            }
        }

        /// <summary>
        /// Create an association between two chunks and update their slots
        /// </summary>
        public async Task<ChunkAssociation> CreateAssociationAsync(
            Guid chunkAId,
            Guid chunkBId,
            string relationAtoB,
            string relationBtoA,
            double initialWeightAtoB = 0.5,
            double initialWeightBtoA = 0.5)
        {
            var associationCollection = await _associationStore.GetCollectionAsync(_associationCollectionId);
            if (associationCollection == null)
                throw new InvalidOperationException($"Association collection '{_associationCollectionId}' not found");

            var chunkCollection = await _chunkStore.GetCollectionAsync(_chunkCollectionId);
            if (chunkCollection == null)
                throw new InvalidOperationException($"Chunk collection '{_chunkCollectionId}' not found");

            // Check if the association already exists
            var existingAssociation = await associationCollection.GetAssociationAsync(chunkAId, chunkBId);
            if (existingAssociation != null)
                return existingAssociation;

            // Create new association
            var association = new ChunkAssociation
            {
                ChunkAId = chunkAId,
                ChunkBId = chunkBId,
                RelationAtoB = relationAtoB,
                RelationBtoA = relationBtoA,
                WeightAtoB = initialWeightAtoB,
                WeightBtoA = initialWeightBtoA,
                LastActivated = DateTime.Now
            };

            // Get chunks to update their slots
            var chunkA = await chunkCollection.GetChunkAsync(chunkAId);
            var chunkB = await chunkCollection.GetChunkAsync(chunkBId);

            if (chunkA != null && chunkB != null)
            {
                // Add ChunkB to ChunkA's slots using the relation name
                string slotNameInA = relationAtoB.Replace(" ", "");
                chunkA.Slots[slotNameInA] = new ModelSlot
                {
                    Name = slotNameInA,
                    Value = chunkBId
                };

                // Add ChunkA to ChunkB's slots
                string slotNameInB = relationBtoA.Replace(" ", "");
                chunkB.Slots[slotNameInB] = new ModelSlot
                {
                    Name = slotNameInB,
                    Value = chunkAId
                };

                // Update both chunks
                await chunkCollection.UpdateChunkAsync(chunkA);
                await chunkCollection.UpdateChunkAsync(chunkB);
            }

            return await associationCollection.AddAssociationAsync(association);
        }

        /// <summary>
        /// Recursive method to spread activation through the network
        /// </summary>
        private async Task SpreadActivationAsync(
            Chunk sourceChunk,
            ChunkAssociation incomingAssociation,
            int remainingDepth,
            double currentSpreadingFactor,
            HashSet<Guid> visitedChunks,
            ActivationHistoryItem originalActivation, TypeActivationParameters parameters)
        {
            if (remainingDepth <= 0) return;

            // Get collections
            var associationCollection = await _associationStore.GetCollectionAsync(_associationCollectionId);
            var chunkCollection = await _chunkStore.GetCollectionAsync(_chunkCollectionId);

            // Get associations for this chunk
            var associations = await associationCollection.GetAssociationsForChunkAsync(sourceChunk.ID);

            foreach (var association in associations)
            {
                // Determine target chunk and direction
                bool isSourceA = association.ChunkAId == sourceChunk.ID;
                Guid targetChunkId = isSourceA ? association.ChunkBId : association.ChunkAId;

                // Skip if already visited
                if (visitedChunks.Contains(targetChunkId)) continue;

                // Add to visited set
                visitedChunks.Add(targetChunkId);

                // Get weight in the correct direction
                double weight = isSourceA ? association.WeightAtoB : association.WeightBtoA;

                // Get the target chunk
                var targetChunk = await chunkCollection.GetChunkAsync(targetChunkId);
                if (targetChunk == null) continue;

                // Calculate spread amount using the activation model
                double previousActivation = targetChunk.ActivationLevel;
                double spreadAmount = _activationModel.CalculateSpreadingActivation(
                    sourceChunk,
                    targetChunk,
                    weight,
                    currentSpreadingFactor
                );

                targetChunk.ActivationLevel += spreadAmount;

                // Create activation history item for target chunk
                var activationItem = new ActivationHistoryItem
                {
                    PreviousValue = previousActivation,
                    NewValue = targetChunk.ActivationLevel,
                    Change = spreadAmount,
                    SequenceNumber = targetChunk.ActivationHistory.Count > 0
                        ? targetChunk.ActivationHistory[0].SequenceNumber + 1
                        : 1,
                    EmotionName = originalActivation.EmotionName,
                    ActivationDate = DateTime.Now,
                    ActivatedByChunk = sourceChunk.ID
                };

                // Add the activation chain
                activationItem.ActivatedBy = new List<Guid>();
                if (originalActivation.ActivatedBy != null && originalActivation.ActivatedBy.Count > 0)
                {
                    activationItem.ActivatedBy.AddRange(originalActivation.ActivatedBy);
                }
                activationItem.ActivatedBy.Insert(0, sourceChunk.ID);

                // Add to history
                targetChunk.ActivationHistory.Insert(0, activationItem);

                // Update the target chunk
                await chunkCollection.UpdateChunkAsync(targetChunk);

                // Strengthen the association
                double previousWeight = isSourceA ? association.WeightAtoB : association.WeightBtoA;
                if (isSourceA)
                    association.WeightAtoB = Math.Min(1.0, association.WeightAtoB + parameters.AssociationStrengthIncrement);
                else
                    association.WeightBtoA = Math.Min(1.0, association.WeightBtoA + parameters.AssociationStrengthIncrement);

                // Update association timestamp
                association.LastActivated = DateTime.Now;

                // Add to association history
                var associationHistoryItem = new ActivationHistoryItem
                {
                    PreviousValue = previousWeight,
                    NewValue = isSourceA ? association.WeightAtoB : association.WeightBtoA,
                    Change = parameters.AssociationStrengthIncrement,
                    ActivationDate = DateTime.Now,
                    ActivatedByChunk = sourceChunk.ID
                };
                association.ActivationHistory.Add(associationHistoryItem);

                // Update the association
                await associationCollection.UpdateAssociationAsync(association);

                // Continue spreading (recursive call with reduced factor)
                await SpreadActivationAsync(
                    targetChunk,
                    association,
                    remainingDepth - 1,
                    currentSpreadingFactor * parameters.SpreadingFactor,
                    visitedChunks,
                    originalActivation, parameters);
            }
        }

        /// <summary>
        /// Get chunks above a certain activation threshold
        /// </summary>
        public async Task<List<Chunk>> GetActiveChunksAsync(double threshold = 0.1)
        {
            var chunkCollection = await _chunkStore.GetCollectionAsync(_chunkCollectionId);
            if (chunkCollection == null)
                return new List<Chunk>();

            var allChunks = await chunkCollection.GetAllChunksAsync();

            return allChunks
                .Where(c => c.ActivationLevel >= threshold)
                .OrderByDescending(c => c.ActivationLevel)
                .ToList();
        }
    }
}