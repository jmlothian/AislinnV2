using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Aislinn.ChunkStorage.Interfaces;
using Aislinn.ChunkStorage.Storage;
using Aislinn.Core.Models;

namespace Aislinn.Core.Services
{
    public class ChunkActivationService
    {
        private readonly IChunkStore _chunkStore;
        private readonly IAssociationStore _associationStore;
        private readonly string _chunkCollectionId;
        private readonly string _associationCollectionId;

        // Configuration parameters
        private readonly double _baseActivation = 1.0;
        private readonly double _decayRate = 0.5;
        private readonly double _spreadingFactor = 0.5;
        private readonly int _maxSpreadingDepth = 2;
        private readonly double _associationStrengthIncrement = 0.1;

        public ChunkActivationService(
            IChunkStore chunkStore,
            IAssociationStore associationStore,
            string chunkCollectionId = "default",
            string associationCollectionId = "default")
        {
            _chunkStore = chunkStore ?? throw new ArgumentNullException(nameof(chunkStore));
            _associationStore = associationStore ?? throw new ArgumentNullException(nameof(associationStore));
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

            // Record the previous activation level for history
            double previousActivation = chunk.ActivationLevel;

            // Boost the activation level
            chunk.ActivationLevel += _baseActivation * activationBoost;

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

            // Start spreading activation
            await SpreadActivationAsync(
                chunk,
                null,
                _maxSpreadingDepth,
                1.0,
                new HashSet<Guid> { chunkId },  // Mark the source as already visited
                activationItem);

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
                    chunk.ActivationLevel *= (1 - _decayRate);

                    /*
                    // Create decay history item
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
            ActivationHistoryItem originalActivation)
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

                // Calculate spread amount based on weight, current factor, and source activation
                double previousActivation = targetChunk.ActivationLevel;
                double spreadAmount = weight * currentSpreadingFactor * _spreadingFactor * sourceChunk.ActivationLevel;
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
                    association.WeightAtoB = Math.Min(1.0, association.WeightAtoB + _associationStrengthIncrement);
                else
                    association.WeightBtoA = Math.Min(1.0, association.WeightBtoA + _associationStrengthIncrement);

                // Update association timestamp
                association.LastActivated = DateTime.Now;

                // Add to association history
                var associationHistoryItem = new ActivationHistoryItem
                {
                    PreviousValue = previousWeight,
                    NewValue = isSourceA ? association.WeightAtoB : association.WeightBtoA,
                    Change = _associationStrengthIncrement,
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
                    currentSpreadingFactor * _spreadingFactor,
                    visitedChunks,
                    originalActivation);
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