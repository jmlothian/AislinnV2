using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Aislinn.ChunkStorage.Interfaces;
using Aislinn.Core.Models;
using Aislinn.Core.Services;

namespace Aislinn.Core.Memory
{
    /// <summary>
    /// Manages the contents of working memory, enforcing human-like capacity limitations,
    /// handling interference, and managing the relationship between working memory and
    /// associative memory.
    /// </summary>
    public class WorkingMemoryManager
    {
        // Working memory subsystems to simulate different capacity pools
        public enum MemorySubsystem
        {
            VisualSpatial,
            Phonological,
            Episodic,
            Semantic,
            Procedural
        }

        // Configuration 
        private readonly int _totalCapacity;
        private readonly double _activationThreshold;
        private readonly double _associativeThreshold;
        private readonly double _interferenceThreshold;
        private readonly double _refreshDecayRate;
        private readonly double _similarityThreshold;

        // Dependencies
        private readonly IChunkStore _chunkStore;
        private readonly IAssociationStore _associationStore;
        private readonly string _chunkCollectionId;
        private readonly string _associationCollectionId;

        // Working memory state
        private Dictionary<MemorySubsystem, List<WorkingMemorySlot>> _workingMemorySlots;
        private Dictionary<Guid, double> _primedChunks;
        private DateTime _lastRefreshTime;

        private System.Timers.Timer _refreshTimer;
        private bool _autoRefreshEnabled = false;
        private double _refreshIntervalMs = 200; // Default 200ms

        // Add these properties
        public bool AutoRefreshEnabled
        {
            get => _autoRefreshEnabled;
            set
            {
                _autoRefreshEnabled = value;
                if (_refreshTimer != null)
                {
                    _refreshTimer.Enabled = value;
                }
            }
        }

        public double RefreshIntervalMs
        {
            get => _refreshIntervalMs;
            set
            {
                _refreshIntervalMs = value;
                if (_refreshTimer != null)
                {
                    _refreshTimer.Interval = value;
                }
            }
        }

        // Add these methods for timer management
        public void StartAutoRefresh(double intervalMs = 200)
        {
            if (_refreshTimer == null)
            {
                _refreshTimer = new System.Timers.Timer(intervalMs);
                _refreshTimer.Elapsed += async (sender, e) => await RefreshCycleAsync();
                _refreshTimer.AutoReset = true;
            }

            _refreshIntervalMs = intervalMs;
            _refreshTimer.Interval = intervalMs;
            _refreshTimer.Enabled = true;
            _autoRefreshEnabled = true;
        }

        public void StopAutoRefresh()
        {
            if (_refreshTimer != null)
            {
                _refreshTimer.Enabled = false;
            }
            _autoRefreshEnabled = false;
        }

        // Add a disposal method to clean up timer resources
        public void Dispose()
        {
            if (_refreshTimer != null)
            {
                _refreshTimer.Dispose();
                _refreshTimer = null;
            }
        }

        /// <summary>
        /// Represents a chunk in working memory with additional metadata about its status
        /// </summary>
        public class WorkingMemorySlot
        {
            public Guid ChunkId { get; set; }
            public double CurrentActivation { get; set; }
            public DateTime EntryTime { get; set; }
            public DateTime LastRefreshTime { get; set; }
            public MemorySubsystem Subsystem { get; set; }
            public double FocusValue { get; set; } // How much attention is being paid to this item (0-1)
            public int RefreshCount { get; set; } // How many times it's been refreshed
        }

        /// <summary>
        /// Initializes a new instance of the WorkingMemoryManager
        /// </summary>
        /// <param name="chunkStore">The chunk store to use</param>
        /// <param name="associationStore">The association store to use</param>
        /// <param name="totalCapacity">Maximum number of chunks in working memory (default 5)</param>
        /// <param name="activationThreshold">Minimum activation for working memory inclusion (default 0.7)</param>
        /// <param name="associativeThreshold">Activation threshold for primed/associated items (default 0.3)</param>
        /// <param name="chunkCollectionId">Collection ID for chunks</param>
        /// <param name="associationCollectionId">Collection ID for associations</param>
        public WorkingMemoryManager(
            IChunkStore chunkStore,
            IAssociationStore associationStore,
            int totalCapacity = 5,
            double activationThreshold = 0.7,
            double associativeThreshold = 0.3,
            string chunkCollectionId = "default",
            string associationCollectionId = "default")
        {
            _chunkStore = chunkStore ?? throw new ArgumentNullException(nameof(chunkStore));
            _associationStore = associationStore ?? throw new ArgumentNullException(nameof(associationStore));
            _chunkCollectionId = chunkCollectionId;
            _associationCollectionId = associationCollectionId;

            _totalCapacity = totalCapacity;
            _activationThreshold = activationThreshold;
            _associativeThreshold = associativeThreshold;
            _interferenceThreshold = 0.8; // High similarity threshold for interference
            _refreshDecayRate = 0.1; // Rate at which items decay when not refreshed
            _similarityThreshold = 0.7; // Threshold for considering chunks similar

            // Initialize working memory subsystems
            _workingMemorySlots = new Dictionary<MemorySubsystem, List<WorkingMemorySlot>>();
            foreach (MemorySubsystem subsystem in Enum.GetValues(typeof(MemorySubsystem)))
            {
                _workingMemorySlots[subsystem] = new List<WorkingMemorySlot>();
            }

            _primedChunks = new Dictionary<Guid, double>();
            _lastRefreshTime = DateTime.Now;
        }

        /// <summary>
        /// Updates working memory based on new chunk activations
        /// </summary>
        /// <param name="chunk">The recently activated chunk</param>
        /// <param name="forceEntry">Whether to force entry into working memory even if full</param>
        /// <returns>Whether the chunk entered working memory</returns>
        public async Task<bool> UpdateWorkingMemoryAsync(Chunk chunk, bool forceEntry = false)
        {
            if (chunk == null)
                throw new ArgumentNullException(nameof(chunk));

            // Check if activation exceeds threshold for working memory
            if (chunk.ActivationLevel < _activationThreshold && !forceEntry)
            {
                // If below WM threshold but above associative threshold, add to primed list
                if (chunk.ActivationLevel >= _associativeThreshold)
                {
                    _primedChunks[chunk.ID] = chunk.ActivationLevel;
                }
                return false;
            }

            // Determine which subsystem this chunk belongs to
            MemorySubsystem subsystem = DetermineSubsystem(chunk);

            // Check if chunk is already in working memory
            var existingSlot = FindChunkInWorkingMemory(chunk.ID);
            if (existingSlot != null)
            {
                // Update existing slot
                existingSlot.CurrentActivation = chunk.ActivationLevel;
                existingSlot.LastRefreshTime = DateTime.Now;
                existingSlot.RefreshCount++;
                existingSlot.FocusValue = 1.0; // Full focus on refreshed item
                return true;
            }

            // Check for similar chunks already in working memory (to avoid redundancy)
            if (await IsRedundantToWorkingMemoryAsync(chunk))
            {
                // If redundant and not forced, just add to primed list
                if (!forceEntry)
                {
                    _primedChunks[chunk.ID] = chunk.ActivationLevel;
                    return false;
                }
            }

            // Check if working memory is at capacity
            int totalUsed = GetTotalSlotsUsed();
            if (totalUsed >= _totalCapacity)
            {
                if (!forceEntry)
                {
                    // If not forced entry, don't add to working memory
                    _primedChunks[chunk.ID] = chunk.ActivationLevel;
                    return false;
                }

                // Forced entry - need to remove something
                // Find lowest activation item in any subsystem
                var lowestSlot = FindLowestActivationSlot();
                if (lowestSlot != null && lowestSlot.CurrentActivation < chunk.ActivationLevel)
                {
                    // Remove the lowest activation item
                    _workingMemorySlots[lowestSlot.Subsystem].Remove(lowestSlot);

                    // Move it to primed list
                    _primedChunks[lowestSlot.ChunkId] = lowestSlot.CurrentActivation;
                }
                else
                {
                    // New chunk has lower activation than all existing ones
                    _primedChunks[chunk.ID] = chunk.ActivationLevel;
                    return false;
                }
            }

            // Add to working memory
            var newSlot = new WorkingMemorySlot
            {
                ChunkId = chunk.ID,
                CurrentActivation = chunk.ActivationLevel,
                EntryTime = DateTime.Now,
                LastRefreshTime = DateTime.Now,
                Subsystem = subsystem,
                FocusValue = 1.0, // Full focus on new items
                RefreshCount = 1
            };

            _workingMemorySlots[subsystem].Add(newSlot);

            // Remove from primed if it was there
            _primedChunks.Remove(chunk.ID);

            // Process new associations to update primed chunks
            await UpdatePrimedChunksAsync(chunk);

            return true;
        }

        /// <summary>
        /// Returns all chunks currently in working memory in order of activation
        /// </summary>
        public async Task<List<Chunk>> GetWorkingMemoryContentsAsync()
        {
            var results = new List<Chunk>();
            var chunkCollection = await _chunkStore.GetCollectionAsync(_chunkCollectionId);

            // Flatten all subsystems and order by activation
            var allSlots = _workingMemorySlots.Values
                .SelectMany(slots => slots)
                .OrderByDescending(slot => slot.CurrentActivation);

            foreach (var slot in allSlots)
            {
                var chunk = await chunkCollection.GetChunkAsync(slot.ChunkId);
                if (chunk != null)
                {
                    results.Add(chunk);
                }
            }

            return results;
        }

        /// <summary>
        /// Returns all primed chunks (partially activated but not in working memory)
        /// </summary>
        public async Task<List<Chunk>> GetPrimedChunksAsync()
        {
            var results = new List<Chunk>();
            var chunkCollection = await _chunkStore.GetCollectionAsync(_chunkCollectionId);

            foreach (var pair in _primedChunks.OrderByDescending(p => p.Value))
            {
                var chunk = await chunkCollection.GetChunkAsync(pair.Key);
                if (chunk != null)
                {
                    results.Add(chunk);
                }
            }

            return results;
        }

        /// <summary>
        /// Simulates natural decay of working memory items and refreshes focused items
        /// </summary>
        /// <param name="focusedChunkIds">IDs of chunks currently being focused on</param>
        public async Task RefreshCycleAsync(List<Guid> focusedChunkIds = null)
        {
            // Calculate time since last refresh
            var now = DateTime.Now;
            var timeSinceLastRefresh = (now - _lastRefreshTime).TotalSeconds;
            _lastRefreshTime = now;

            // Default empty list if null
            focusedChunkIds ??= new List<Guid>();

            // Apply decay to all working memory items
            foreach (var subsystem in _workingMemorySlots.Keys)
            {
                var slots = _workingMemorySlots[subsystem];
                var slotsToRemove = new List<WorkingMemorySlot>();

                foreach (var slot in slots)
                {
                    // Refresh focused items
                    if (focusedChunkIds.Contains(slot.ChunkId))
                    {
                        slot.FocusValue = 1.0;
                        slot.LastRefreshTime = now;
                        slot.RefreshCount++;

                        // Focused items decay less
                        slot.CurrentActivation *= (1 - (_refreshDecayRate * 0.2 * timeSinceLastRefresh));
                    }
                    else
                    {
                        // Apply decay based on time since last refresh
                        slot.FocusValue *= 0.9; // Gradual focus decay
                        slot.CurrentActivation *= (1 - (_refreshDecayRate * timeSinceLastRefresh));

                        // If activation falls below threshold, mark for removal
                        if (slot.CurrentActivation < _associativeThreshold)
                        {
                            slotsToRemove.Add(slot);
                        }
                        else if (slot.CurrentActivation < _activationThreshold)
                        {
                            // Move to primed list if below WM threshold but above associative threshold
                            slotsToRemove.Add(slot);
                            _primedChunks[slot.ChunkId] = slot.CurrentActivation;
                        }
                    }
                }

                // Remove decayed items
                foreach (var slotToRemove in slotsToRemove)
                {
                    slots.Remove(slotToRemove);
                }
            }

            // Apply decay to primed chunks
            var primedToRemove = new List<Guid>();
            foreach (var chunkId in _primedChunks.Keys.ToList())
            {
                _primedChunks[chunkId] *= (1 - (_refreshDecayRate * 1.5 * timeSinceLastRefresh)); // Primed decay faster

                if (_primedChunks[chunkId] < _associativeThreshold)
                {
                    primedToRemove.Add(chunkId);
                }
            }

            // Remove decayed primed chunks
            foreach (var chunkId in primedToRemove)
            {
                _primedChunks.Remove(chunkId);
            }
        }

        /// <summary>
        /// Clears working memory entirely
        /// </summary>
        public void ClearWorkingMemory()
        {
            foreach (var subsystem in _workingMemorySlots.Keys)
            {
                _workingMemorySlots[subsystem].Clear();
            }
            _primedChunks.Clear();
        }

        /// <summary>
        /// Forces a chunk out of working memory
        /// </summary>
        public bool RemoveFromWorkingMemory(Guid chunkId)
        {
            var slot = FindChunkInWorkingMemory(chunkId);
            if (slot != null)
            {
                _workingMemorySlots[slot.Subsystem].Remove(slot);
                _primedChunks[chunkId] = slot.CurrentActivation; // Move to primed
                return true;
            }
            return false;
        }

        /// <summary>
        /// Determines which subsystem a chunk belongs to based on its properties
        /// </summary>
        private MemorySubsystem DetermineSubsystem(Chunk chunk)
        {
            // Simple heuristic based on chunk type
            if (string.IsNullOrEmpty(chunk.ChunkType))
                return MemorySubsystem.Semantic;

            string type = chunk.ChunkType.ToLowerInvariant();

            if (type.Contains("visual") || type.Contains("spatial") || type.Contains("image"))
                return MemorySubsystem.VisualSpatial;

            if (type.Contains("sound") || type.Contains("audio") || type.Contains("phonological") || type.Contains("verbal"))
                return MemorySubsystem.Phonological;

            if (type.Contains("episode") || type.Contains("event") || type.Contains("memory"))
                return MemorySubsystem.Episodic;

            if (type.Contains("procedure") || type.Contains("action") || type.Contains("skill"))
                return MemorySubsystem.Procedural;

            // Default to semantic for anything else
            return MemorySubsystem.Semantic;
        }

        /// <summary>
        /// Updates the list of primed chunks based on associations to the given chunk
        /// </summary>
        private async Task UpdatePrimedChunksAsync(Chunk chunk)
        {
            var associationCollection = await _associationStore.GetCollectionAsync(_associationCollectionId);
            if (associationCollection == null)
                return;

            // Get all associations for this chunk
            var associations = await associationCollection.GetAssociationsForChunkAsync(chunk.ID);

            foreach (var association in associations)
            {
                // Determine target chunk and direction
                bool isSourceA = association.ChunkAId == chunk.ID;
                Guid targetChunkId = isSourceA ? association.ChunkBId : association.ChunkAId;

                // Skip if already in working memory
                if (FindChunkInWorkingMemory(targetChunkId) != null)
                    continue;

                // Get weight in the correct direction
                double weight = isSourceA ? association.WeightAtoB : association.WeightBtoA;

                // Calculate priming activation
                double primingActivation = chunk.ActivationLevel * weight * 0.5;

                // If high enough to be primed, add to primed list
                if (primingActivation >= _associativeThreshold)
                {
                    if (_primedChunks.ContainsKey(targetChunkId))
                    {
                        // Take the higher of existing or new priming
                        _primedChunks[targetChunkId] = Math.Max(_primedChunks[targetChunkId], primingActivation);
                    }
                    else
                    {
                        _primedChunks[targetChunkId] = primingActivation;
                    }
                }
            }
        }

        /// <summary>
        /// Checks if a chunk is redundant to what's already in working memory
        /// </summary>
        private async Task<bool> IsRedundantToWorkingMemoryAsync(Chunk candidateChunk)
        {
            // Get current working memory contents
            var workingMemoryChunks = await GetWorkingMemoryContentsAsync();

            // If empty, nothing to be redundant with
            if (workingMemoryChunks.Count == 0)
                return false;

            // Check for direct associations first
            var associationCollection = await _associationStore.GetCollectionAsync(_associationCollectionId);
            if (associationCollection != null)
            {
                var associations = await associationCollection.GetAssociationsForChunkAsync(candidateChunk.ID);

                // Check each association against working memory
                foreach (var association in associations)
                {
                    // Get the other chunk ID
                    bool isSourceA = association.ChunkAId == candidateChunk.ID;
                    Guid otherChunkId = isSourceA ? association.ChunkBId : association.ChunkAId;

                    // Get strength
                    double strength = isSourceA ? association.WeightAtoB : association.WeightBtoA;

                    // If strongly associated with something in working memory, it's redundant
                    if (strength > _interferenceThreshold && FindChunkInWorkingMemory(otherChunkId) != null)
                    {
                        return true;
                    }
                }
            }

            // If we have vector representations, check similarities
            if (candidateChunk.Vector != null && candidateChunk.Vector.Length > 0)
            {
                foreach (var wmChunk in workingMemoryChunks)
                {
                    if (wmChunk.Vector != null && wmChunk.Vector.Length == candidateChunk.Vector.Length)
                    {
                        double similarity = CalculateCosineSimilarity(candidateChunk.Vector, wmChunk.Vector);
                        if (similarity > _similarityThreshold)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Find a chunk in working memory across all subsystems
        /// </summary>
        private WorkingMemorySlot FindChunkInWorkingMemory(Guid chunkId)
        {
            foreach (var subsystem in _workingMemorySlots.Keys)
            {
                var slot = _workingMemorySlots[subsystem].FirstOrDefault(s => s.ChunkId == chunkId);
                if (slot != null)
                    return slot;
            }
            return null;
        }

        /// <summary>
        /// Find the slot with the lowest activation across all subsystems
        /// </summary>
        private WorkingMemorySlot FindLowestActivationSlot()
        {
            WorkingMemorySlot lowestSlot = null;
            double lowestActivation = double.MaxValue;

            foreach (var subsystem in _workingMemorySlots.Keys)
            {
                foreach (var slot in _workingMemorySlots[subsystem])
                {
                    if (slot.CurrentActivation < lowestActivation)
                    {
                        lowestActivation = slot.CurrentActivation;
                        lowestSlot = slot;
                    }
                }
            }

            return lowestSlot;
        }

        /// <summary>
        /// Calculate the total number of slots used across all subsystems
        /// </summary>
        private int GetTotalSlotsUsed()
        {
            int total = 0;
            foreach (var subsystem in _workingMemorySlots.Keys)
            {
                total += _workingMemorySlots[subsystem].Count;
            }
            return total;
        }

        /// <summary>
        /// Calculate cosine similarity between two vectors
        /// </summary>
        private double CalculateCosineSimilarity(double[] vector1, double[] vector2)
        {
            if (vector1.Length != vector2.Length)
                throw new ArgumentException("Vectors must have the same dimensions");

            double dotProduct = 0;
            double magnitude1 = 0;
            double magnitude2 = 0;

            for (int i = 0; i < vector1.Length; i++)
            {
                dotProduct += vector1[i] * vector2[i];
                magnitude1 += vector1[i] * vector1[i];
                magnitude2 += vector2[i] * vector2[i];
            }

            magnitude1 = Math.Sqrt(magnitude1);
            magnitude2 = Math.Sqrt(magnitude2);

            if (magnitude1 == 0 || magnitude2 == 0)
                return 0;

            return dotProduct / (magnitude1 * magnitude2);
        }

        /// <summary>
        /// Gets the current working memory capacity usage statistics
        /// </summary>
        public Dictionary<MemorySubsystem, int> GetWorkingMemoryUsage()
        {
            return _workingMemorySlots.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Count
            );
        }

        /// <summary>
        /// Gets the number of primed chunks
        /// </summary>
        public int GetPrimedChunksCount()
        {
            return _primedChunks.Count;
        }
    }
}