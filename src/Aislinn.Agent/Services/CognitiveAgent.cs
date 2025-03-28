using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Aislinn.ChunkStorage;
using Aislinn.ChunkStorage.Interfaces;
using Aislinn.ChunkStorage.Storage;
using Aislinn.Core.Activation;
using Aislinn.Core.Cognitive;
using Aislinn.Core.Models;
using Aislinn.Core.Services;
using Aislinn.VectorStorage.Interfaces;
using Aislinn.VectorStorage.Storage;

namespace Aislinn.Core.Agent
{
    /// <summary>
    /// A cognitive agent that integrates memory, goals, and other cognitive faculties
    /// </summary>
    public class CognitiveAgent : IDisposable
    {
        // Core systems
        private readonly CognitiveMemorySystem _memorySystem;
        private readonly CognitiveTimeManager _timeManager;

        // Storage
        private readonly IChunkStore _chunkStore;
        private readonly IAssociationStore _associationStore;
        private readonly VectorStore _vectorStore;

        // Configuration
        private readonly string _chunkCollectionId;
        private readonly string _associationCollectionId;
        private readonly string _vectorCollectionId;

        // Agent state
        private bool _isInitialized = false;
        private DateTime _lastActivityTime;

        /// <summary>
        /// Creates a new cognitive agent with specified subsystems
        /// </summary>
        public CognitiveAgent(
            IChunkStore chunkStore,
            IAssociationStore associationStore,
            VectorStore vectorStore,
            IVectorizer vectorizer,
            CognitiveTimeManager timeManager = null,
            string chunkCollectionId = "default",
            string associationCollectionId = "default",
            string vectorCollectionId = "default",
            IActivationModel activationModel = null)
        {
            // Initialize storage and components
            _chunkStore = chunkStore ?? throw new ArgumentNullException(nameof(chunkStore));
            _associationStore = associationStore ?? throw new ArgumentNullException(nameof(associationStore));
            _vectorStore = vectorStore ?? throw new ArgumentNullException(nameof(vectorStore));

            _chunkCollectionId = chunkCollectionId;
            _associationCollectionId = associationCollectionId;
            _vectorCollectionId = vectorCollectionId;

            // Create time manager if not provided
            _timeManager = timeManager ?? new CognitiveTimeManager();

            // Create activation model if not provided
            activationModel ??= new ActRActivationModel(_timeManager);

            // Initialize cognitive memory system
            _memorySystem = new CognitiveMemorySystem(
                _chunkStore,
                _associationStore,
                activationModel,
                _timeManager,
                _chunkCollectionId,
                _associationCollectionId);

            _lastActivityTime = DateTime.Now;
        }

        /// <summary>
        /// Initialize the agent and its subsystems
        /// </summary>
        public async Task InitializeAsync()
        {
            if (_isInitialized)
                return;

            // Ensure collections exist
            await _chunkStore.GetOrCreateCollectionAsync(_chunkCollectionId);

            // Initialize ChunkContext for easy chunk reference
            var chunkCollection = await _chunkStore.GetCollectionAsync(_chunkCollectionId);
            ChunkContext.Initialize(_chunkStore, _chunkCollectionId);

            // Ensure vector collection exists
            await _vectorStore.GetOrCreateCollectionAsync(_vectorCollectionId, null); // Vectorizer will need to be passed

            // Start cognitive processes
            _memorySystem.StartWorkingMemoryRefresh();

            _isInitialized = true;
        }

        /// <summary>
        /// Process input from the environment and update cognitive state
        /// </summary>
        public async Task ProcessInputAsync(string input, Dictionary<string, string> metadata = null)
        {
            if (!_isInitialized)
                await InitializeAsync();

            _lastActivityTime = DateTime.Now;

            // Convert input to vector and store it
            var vectorCollection = await _vectorStore.GetCollectionAsync(_vectorCollectionId);
            if (vectorCollection != null)
            {
                var vectorItem = await vectorCollection.AddVectorAsync(input, metadata ?? new Dictionary<string, string>());

                // Store input as a chunk too
                var chunkCollection = await _chunkStore.GetCollectionAsync(_chunkCollectionId);
                var chunk = new Chunk
                {
                    ChunkType = "Input",
                    Name = $"Input_{DateTime.Now.Ticks}",
                    Vector = vectorItem.Vector,
                    ActivationLevel = 0.0
                };

                // Add metadata to slots
                if (metadata != null)
                {
                    foreach (var kvp in metadata)
                    {
                        chunk.Slots[kvp.Key] = new ModelSlot { Name = kvp.Key, Value = kvp.Value };
                    }
                }

                // Add input text to slot
                chunk.Slots["Text"] = new ModelSlot { Name = "Text", Value = input };

                // Add chunk to storage
                var storedChunk = await chunkCollection.AddChunkAsync(chunk);

                // Activate the chunk to bring it into working memory
                await _memorySystem.ActivateChunkAsync(storedChunk.ID, "UserInput", 1.0);

                // TODO: Perform semantic search to find related chunks
                // TODO: Process the input for intent, entities, etc.
            }
        }

        /// <summary>
        /// Generate a response based on current cognitive state
        /// </summary>
        public async Task<string> GenerateResponseAsync()
        {
            if (!_isInitialized)
                await InitializeAsync();

            // Refresh working memory if needed
            if ((DateTime.Now - _lastActivityTime).TotalSeconds > 1.0)
            {
                await _memorySystem.ManualRefreshCycleAsync();
            }

            // Get current working memory contents
            var workingMemoryContents = await _memorySystem.GetWorkingMemoryContentsAsync();

            // TODO: Implement response generation based on working memory
            // For now, just return a simple representation of working memory

            var response = new System.Text.StringBuilder();
            response.AppendLine("Current working memory contents:");

            foreach (var chunk in workingMemoryContents)
            {
                response.AppendLine($"- {chunk.Name} ({chunk.ChunkType}): Activation = {chunk.ActivationLevel:F2}");

                // Add some slots if present
                if (chunk.Slots.Count > 0)
                {
                    response.AppendLine("  Slots:");
                    foreach (var slot in chunk.Slots)
                    {
                        response.AppendLine($"    {slot.Key}: {slot.Value.Value}");
                    }
                }
            }

            // Add some info about primed chunks
            var primedChunks = await _memorySystem.GetPrimedChunksAsync();
            if (primedChunks.Count > 0)
            {
                response.AppendLine("\nPrimed chunks (ready to enter working memory):");
                foreach (var chunk in primedChunks.Take(3)) // Show top 3
                {
                    response.AppendLine($"- {chunk.Name} ({chunk.ChunkType})");
                }
            }

            return response.ToString();
        }

        /// <summary>
        /// Get a snapshot of the agent's current cognitive state
        /// </summary>
        public async Task<AgentState> GetCurrentStateAsync()
        {
            var workingMemory = await _memorySystem.GetWorkingMemoryContentsAsync();
            var primedChunks = await _memorySystem.GetPrimedChunksAsync();

            return new AgentState
            {
                WorkingMemoryContents = workingMemory,
                PrimedChunks = primedChunks,
                SystemTime = _timeManager.GetCurrentTime(),
                LastActivityTime = _lastActivityTime
            };
        }

        /// <summary>
        /// Save the agent's state to persistent storage
        /// </summary>
        public void SaveState()
        {
            _timeManager.SaveState();
            // TODO: Save other state as needed
        }

        /// <summary>
        /// Clean up resources
        /// </summary>
        public void Dispose()
        {
            _memorySystem.Dispose();
            SaveState();
        }

        /// <summary>
        /// Represents a snapshot of the agent's cognitive state
        /// </summary>
        public class AgentState
        {
            public List<Chunk> WorkingMemoryContents { get; set; } = new List<Chunk>();
            public List<Chunk> PrimedChunks { get; set; } = new List<Chunk>();
            public double SystemTime { get; set; }
            public DateTime LastActivityTime { get; set; }
        }
    }
}