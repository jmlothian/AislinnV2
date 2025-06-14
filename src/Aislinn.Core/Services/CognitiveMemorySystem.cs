using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Aislinn.ChunkStorage.Interfaces;
using Aislinn.Core.Activation;
using Aislinn.Core.Memory;
using Aislinn.Core.Models;
using Aislinn.Core.Services;
using Aislinn.Configuration;

namespace Aislinn.Core.Cognitive
{
    /// <summary>
    /// Coordinates different memory systems including working memory, long-term memory,
    /// and spreading activation. Serves as the central memory manager for a cognitive agent.
    /// </summary>
    public class CognitiveMemorySystem : IDisposable
    {
        // Core memory systems
        private readonly WorkingMemoryManager _workingMemory;
        private readonly ChunkActivationService _activationService;
        public readonly CognitiveTimeManager _timeManager;

        // Storage references
        private readonly IChunkStore _chunkStore;
        private readonly IAssociationStore _associationStore;

        // Configuration
        private readonly string _chunkCollectionId;
        private readonly string _associationCollectionId;

        private readonly ActivationParametersRegistry _parametersRegistry;

        // System state
        private bool _isDisposed = false;

        /// <summary>
        /// Creates a new cognitive memory system with working memory and activation components
        /// </summary>
        // public CognitiveMemorySystem(
        //     IChunkStore chunkStore,
        //     IAssociationStore associationStore,
        //     IActivationModel activationModel,
        //     CognitiveTimeManager timeManager,
        //     
        //     ChunkActivationService chunkActivationService,
        //     WorkingMemoryManager workingMemoryManager,
        //     string chunkCollectionId = "default",
        //     string associationCollectionId = "default")
        public CognitiveMemorySystem(
            IChunkStore chunkStore,
            IAssociationStore associationStore,
            CognitiveTimeManager timeManager,
            ActivationParametersRegistry parametersRegistry,
            ChunkActivationService chunkActivationService,
            WorkingMemoryManager workingMemoryManager,
            AislinnConfiguration config)
        {
            _chunkStore = chunkStore;
            _associationStore = associationStore;
            _activationService = chunkActivationService;

            _chunkCollectionId = config.ChunkCollectionId;
            _associationCollectionId = config.AssociationCollectionId;
            // Store references
            _timeManager = timeManager;
            _parametersRegistry = parametersRegistry ?? throw new ArgumentNullException(nameof(parametersRegistry));
            _chunkCollectionId = config.ChunkCollectionId;
            _associationCollectionId = config.AssociationCollectionId;


            // Initialize working memory
            _workingMemory = workingMemoryManager;
        }


        #region Working Memory Management

        /// <summary>
        /// Start automatic working memory refresh on a timer
        /// </summary>
        public void StartWorkingMemoryRefresh(long intervalMs = 200)
        {
            _workingMemory.StartAutoRefresh(intervalMs);
        }

        /// <summary>
        /// Stop automatic working memory refresh
        /// </summary>
        public void StopWorkingMemoryRefresh()
        {
            _workingMemory.StopAutoRefresh();
        }

        /// <summary>
        /// Manually trigger a working memory refresh cycle
        /// </summary>
        public async Task ManualRefreshCycleAsync(List<Guid> focusedChunks = null)
        {
            await _workingMemory.RefreshCycleAsync(focusedChunks);
        }

        /// <summary>
        /// Get the current contents of working memory
        /// </summary>
        public async Task<List<Chunk>> GetWorkingMemoryContentsAsync()
        {
            return await _workingMemory.GetWorkingMemoryContentsAsync();
        }

        /// <summary>
        /// Get chunks that are primed but not in working memory
        /// </summary>
        public async Task<List<Chunk>> GetPrimedChunksAsync()
        {
            return await _workingMemory.GetPrimedChunksAsync();
        }

        /// <summary>
        /// Manually move a chunk into working memory
        /// </summary>
        public async Task<bool> ForceChunkIntoWorkingMemoryAsync(Guid chunkId)
        {
            var chunkCollection = await _chunkStore.GetCollectionAsync(_chunkCollectionId);
            if (chunkCollection == null) return false;

            var chunk = await chunkCollection.GetChunkAsync(chunkId);
            if (chunk == null) return false;

            return await _workingMemory.UpdateWorkingMemoryAsync(chunk, forceEntry: true);
        }

        /// <summary>
        /// Remove a chunk from working memory
        /// </summary>
        public bool RemoveFromWorkingMemory(Guid chunkId)
        {
            return _workingMemory.RemoveFromWorkingMemory(chunkId);
        }

        /// <summary>
        /// Clear all contents from working memory
        /// </summary>
        public void ClearWorkingMemory()
        {
            _workingMemory.ClearWorkingMemory();
        }

        #endregion

        #region Activation Management

        /// <summary>
        /// Activate a chunk and optionally bring it into working memory
        /// </summary>
        public async Task<Chunk> ActivateChunkAsync(Guid chunkId, string emotionName = null, double activationBoost = 1.0)
        {
            // Activate the chunk using the activation service
            var chunk = await _activationService.ActivateChunkAsync(chunkId, emotionName, activationBoost);

            // Update working memory with the activated chunk
            if (chunk != null)
            {
                await _workingMemory.UpdateWorkingMemoryAsync(chunk);
            }

            return chunk;
        }
        /// <summary>
        /// Activate a chunk with context-filtered spreading activation
        /// </summary>
        public async Task<Chunk> ActivateChunkAsync(Guid chunkId, SpreadingContext context, string emotionName = null, double activationBoost = 1.0)
        {
            // Activate the chunk using the activation service with context
            var chunk = await _activationService.ActivateChunkAsync(chunkId, context, emotionName, activationBoost);

            // Update working memory with the activated chunk
            if (chunk != null)
            {
                await _workingMemory.UpdateWorkingMemoryAsync(chunk);
            }

            return chunk;
        }
        /// <summary>
        /// Apply activation decay to all chunks in the system
        /// </summary>
        public async Task ApplyDecayAsync()
        {
            await _activationService.ApplyDecayAsync();
        }

        /// <summary>
        /// Get all chunks above an activation threshold
        /// </summary>
        public async Task<List<Chunk>> GetActiveChunksAsync(double threshold = 0.1)
        {
            return await _activationService.GetActiveChunksAsync(threshold);
        }

        /// <summary>
        /// Create an association between two chunks
        /// </summary>
        public async Task<ChunkAssociation> CreateAssociationAsync(
            Guid chunkAId,
            Guid chunkBId,
            string relationAtoB,
            string relationBtoA,
            double initialWeightAtoB = 0.5,
            double initialWeightBtoA = 0.5)
        {
            return await _activationService.CreateAssociationAsync(
                chunkAId,
                chunkBId,
                relationAtoB,
                relationBtoA,
                initialWeightAtoB,
                initialWeightBtoA);
        }
        public async Task<IChunkAssociationCollection> GetAssociationCollectionAsync()
        {
            return await _associationStore.GetOrCreateCollectionAsync(_associationCollectionId);
        }

        #endregion

        #region Memory Operations

        /// <summary>
        /// Find chunks that are semantically similar to a text query
        /// </summary>
        public async Task<List<Chunk>> FindSimilarChunksAsync(string query, int topN = 5, double minSimilarity = 0.6)
        {
            // TODO: Vector system integration
            // For now, return an empty list as placeholder
            return new List<Chunk>();
        }

        /// <summary>
        /// Add a new chunk to memory
        /// </summary>
        /// <param name="chunk">The chunk to add</param>
        /// <param name="includeTimestamp">Whether to include default timestamp values</param>
        public async Task<Chunk> AddChunkAsync(Chunk chunk, bool includeTimestamp = true)
        {
            if (chunk == null)
                throw new ArgumentNullException(nameof(chunk));

            var chunkCollection = await _chunkStore.GetCollectionAsync(_chunkCollectionId);
            if (chunkCollection == null)
                throw new InvalidOperationException($"Chunk collection '{_chunkCollectionId}' not found");

            //including default timestamp values
            if (includeTimestamp)
            {
                chunk.Slots["$CreatedOn"] = new ModelSlot { Name = "$CreatedOn", Value = DateTime.Now };
                chunk.Slots["$CreatedOnCognitiveTime"] = new ModelSlot { Name = "$CreatedOn", Value = _timeManager.GetCognitiveSteps() };
                chunk.Slots["$CreatedOnAgentTime"] = new ModelSlot { Name = "$CreatedOn", Value = _timeManager.GetAgentTime() };
            }

            return await chunkCollection.AddChunkAsync(chunk);
        }

        /// <summary>
        /// Retrieve a chunk from memory
        /// </summary>
        public async Task<Chunk> GetChunkAsync(Guid chunkId)
        {
            var chunkCollection = await _chunkStore.GetCollectionAsync(_chunkCollectionId);
            if (chunkCollection == null)
                return null;

            return await chunkCollection.GetChunkAsync(chunkId);
        }

        /// <summary>
        /// Update a chunk in memory
        /// </summary>
        public async Task<bool> UpdateChunkAsync(Chunk chunk)
        {
            if (chunk == null)
                throw new ArgumentNullException(nameof(chunk));

            var chunkCollection = await _chunkStore.GetCollectionAsync(_chunkCollectionId);
            if (chunkCollection == null)
                return false;

            return await chunkCollection.UpdateChunkAsync(chunk);
        }

        #endregion
        public async Task<Chunk> FindChunkBySemanticTypeAndName(string semanticType, string chunkName)
        {
            var chunkCollection = await _chunkStore.GetCollectionAsync(_chunkCollectionId);
            var allChunks = await chunkCollection.GetAllChunksAsync();
            return allChunks.FirstOrDefault(c =>
                string.Equals(c.SemanticType, semanticType, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(c.Name, chunkName, StringComparison.OrdinalIgnoreCase));
        }
        /// <summary>
        /// Disposes of resources
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
                return;

            _workingMemory.Dispose();
            _isDisposed = true;
        }

        /// <summary>
        /// Activate multiple chunks with different boost levels
        /// </summary>
        public async Task ActivateChunksAsync(Dictionary<Guid, double> chunkBoosts, string emotionName = null)
        {
            foreach (var kvp in chunkBoosts)
            {
                await ActivateChunkAsync(kvp.Key, emotionName, kvp.Value);
            }
        }

        /// <summary>
        /// Get the top N chunks by activation level from the entire memory system
        /// </summary>
        public async Task<List<Chunk>> GetTopActivatedChunksAsync(int topN, bool excludeWorkingMemory = true)
        {
            var chunkCollection = await _chunkStore.GetCollectionAsync(_chunkCollectionId);
            if (chunkCollection == null)
                return new List<Chunk>();

            var allChunks = await chunkCollection.GetAllChunksAsync();

            // Filter out working memory chunks if requested
            List<Chunk> candidateChunks = allChunks;
            if (excludeWorkingMemory)
            {
                var workingMemoryChunks = await GetWorkingMemoryContentsAsync();
                var workingMemoryIds = workingMemoryChunks.Select(c => c.ID).ToHashSet();
                candidateChunks = allChunks.Where(c => !workingMemoryIds.Contains(c.ID)).ToList();
            }

            return candidateChunks
                .OrderByDescending(c => c.ActivationLevel)
                .Take(topN)
                .ToList();
        }

        /// <summary>
        /// Push highly activated chunks into working memory without focus
        /// </summary>
        public async Task PushChunksToWorkingMemoryAsync(List<Guid> chunkIds)
        {
            var chunkCollection = await _chunkStore.GetCollectionAsync(_chunkCollectionId);
            if (chunkCollection == null) return;

            foreach (var chunkId in chunkIds)
            {
                var chunk = await chunkCollection.GetChunkAsync(chunkId);
                if (chunk != null)
                {
                    await _workingMemory.UpdateWorkingMemoryAsync(chunk, forceEntry: true);
                }
            }
        }
    }
}