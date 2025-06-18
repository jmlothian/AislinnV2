using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Aislinn.ChunkStorage.Interfaces;
using Aislinn.Core.Models;
using Aislinn.Core.Services;
using Aislinn.Configuration;
using System.Text.Json;

namespace Aislinn.Core.Memory
{

    /// <summary>
    /// Manages the contents of working memory, enforcing human-like capacity limitations,
    /// handling interference, and managing the relationship between working memory and
    /// associative memory.
    /// </summary>
    public class WorkingMemoryManager
    {
        /// <summary>
        /// State persistence class
        /// </summary>
        private class WorkingMemoryState
        {
            public Dictionary<MemorySubsystem, List<WorkingMemorySlot>> WorkingMemorySlots { get; set; } = new();
            public Dictionary<Guid, double> PrimedChunks { get; set; } = new();
            public long LastRefreshTime { get; set; }
        }
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
        private readonly CognitiveTimeManager _cognitiveTimeManager;
        // Working memory state
        private Dictionary<MemorySubsystem, List<WorkingMemorySlot>> _workingMemorySlots;
        private Dictionary<Guid, double> _primedChunks;
        private long _lastRefreshTime;

        private System.Timers.Timer _refreshTimer;
        private bool _autoRefreshEnabled = false;
        private long _refreshIntervalMs = 200; // Default 200ms

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
                Console.WriteLine($"[WM-DEBUG] AutoRefresh set to: {value}");
            }
        }

        public long RefreshIntervalMs
        {
            get => _refreshIntervalMs;
            set
            {
                _refreshIntervalMs = value;
                if (_refreshTimer != null)
                {
                    _refreshTimer.Interval = value;
                }
                Console.WriteLine($"[WM-DEBUG] RefreshInterval set to: {value}ms");
            }
        }

        // Add these methods for timer management
        public void StartAutoRefresh(long intervalMs = 200)
        {
            Console.WriteLine($"[WM-DEBUG] Starting auto refresh with interval: {intervalMs}ms");

            if (_refreshTimer == null)
            {
                _refreshTimer = new System.Timers.Timer(intervalMs);
                _refreshTimer.Elapsed += async (sender, e) =>
                {
                    Console.WriteLine($"[WM-DEBUG] Auto refresh timer triggered");
                    await RefreshCycleAsync();
                };
                _refreshTimer.AutoReset = true;
            }

            _refreshIntervalMs = intervalMs;
            _refreshTimer.Interval = intervalMs;
            _refreshTimer.Enabled = true;
            _autoRefreshEnabled = true;

            Console.WriteLine($"[WM-DEBUG] Auto refresh started successfully");
        }

        public void StopAutoRefresh()
        {
            Console.WriteLine($"[WM-DEBUG] Stopping auto refresh");
            if (_refreshTimer != null)
            {
                _refreshTimer.Enabled = false;
            }
            _autoRefreshEnabled = false;
        }
        /// <summary>
        /// Save working memory state to file
        /// </summary>
        public void SaveState()
        {
            var state = new WorkingMemoryState
            {
                WorkingMemorySlots = _workingMemorySlots.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.ToList()
                ),
                PrimedChunks = new Dictionary<Guid, double>(_primedChunks),
                LastRefreshTime = _lastRefreshTime
            };

            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(state, options);
            File.WriteAllText("./data/working_memory.json", json);
        }
        // Add a disposal method to clean up timer resources
        public void Dispose()
        {
            Console.WriteLine($"[WM-DEBUG] Disposing WorkingMemoryManager");
            SaveState();
            if (_refreshTimer != null)
            {
                _refreshTimer.Dispose();
                _refreshTimer = null;
            }
        }
        /// <summary>
        /// Load working memory state from file
        /// </summary>
        public void LoadState()
        {
            if (File.Exists("./data/working_memory.json"))
            {
                try
                {
                    var json = File.ReadAllText("./data/working_memory.json");
                    var state = JsonSerializer.Deserialize<WorkingMemoryState>(json);

                    if (state != null)
                    {
                        _workingMemorySlots = state.WorkingMemorySlots.ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value.ToList()
                        );
                        _primedChunks = new Dictionary<Guid, double>(state.PrimedChunks);
                        _lastRefreshTime = state.LastRefreshTime;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading working memory state: {ex.Message}");
                    // Continue with empty state
                }
            }
        }
        /// <summary>
        /// Represents a chunk in working memory with additional metadata about its status
        /// </summary>
        public class WorkingMemorySlot
        {
            public Guid ChunkId { get; set; }
            public double CurrentActivation { get; set; }
            //cognitive time, real times will be specified
            public long EntryTime { get; set; }
            public DateTime EntryRealTime { get; set; }
            public long LastRefreshTime { get; set; }
            public DateTime LastRefreshRealTime { get; set; }
            public MemorySubsystem Subsystem { get; set; }
            public double FocusValue { get; set; } // How much attention is being paid to this item (0-1)
            public int RefreshCount { get; set; } // How many times it's been refreshed
        }

        /// <summary>
        /// Initializes a new instance of the WorkingMemoryManager
        /// </summary>
        public WorkingMemoryManager(
            IChunkStore chunkStore,
            IAssociationStore associationStore,
            CognitiveTimeManager timeManager,
            AislinnConfiguration config)
        {
            Console.WriteLine($"[WM-DEBUG] Initializing WorkingMemoryManager:");
            Console.WriteLine($"[WM-DEBUG]   - Total Capacity: {config.WorkingMemoryCapacity}");
            Console.WriteLine($"[WM-DEBUG]   - Activation Threshold: {config.ActivationThreshold}");
            Console.WriteLine($"[WM-DEBUG]   - Associative Threshold: {config.AssociativeThreshold}");
            Console.WriteLine($"[WM-DEBUG]   - Chunk Collection: {config.ChunkCollectionId}");
            Console.WriteLine($"[WM-DEBUG]   - Association Collection: {config.AssociationCollectionId}");

            _chunkStore = chunkStore;
            _associationStore = associationStore;
            _chunkCollectionId = config.ChunkCollectionId;
            _associationCollectionId = config.AssociationCollectionId;
            _cognitiveTimeManager = timeManager;

            _totalCapacity = config.WorkingMemoryCapacity;
            _activationThreshold = config.ActivationThreshold;
            _associativeThreshold = config.AssociativeThreshold;
            _interferenceThreshold = 0.8; // High similarity threshold for interference
            _refreshDecayRate = 0.1 / 1000; // Rate at which items decay when not refreshed, / 1000 to convert to ms
            _similarityThreshold = 0.7; // Threshold for considering chunks similar

            // Initialize working memory subsystems
            _workingMemorySlots = new Dictionary<MemorySubsystem, List<WorkingMemorySlot>>();
            foreach (MemorySubsystem subsystem in Enum.GetValues(typeof(MemorySubsystem)))
            {
                _workingMemorySlots[subsystem] = new List<WorkingMemorySlot>();
                Console.WriteLine($"[WM-DEBUG] Initialized subsystem: {subsystem}");
            }

            _primedChunks = new Dictionary<Guid, double>();
            _lastRefreshTime = _cognitiveTimeManager.GetCognitiveSteps();

            Console.WriteLine($"[WM-DEBUG] WorkingMemoryManager initialization complete");
        }

        /// <summary>
        /// Updates working memory based on new chunk activations
        /// </summary>
        public async Task<bool> UpdateWorkingMemoryAsync(Chunk chunk, bool forceEntry = false)
        {
            if (chunk == null)
            {
                Console.WriteLine($"[WM-DEBUG] UpdateWorkingMemoryAsync called with null chunk");
                throw new ArgumentNullException(nameof(chunk));
            }

            Console.WriteLine($"[WM-DEBUG] === UpdateWorkingMemoryAsync START ===");
            Console.WriteLine($"[WM-DEBUG] Chunk: {chunk.Name} (ID: {chunk.ID})");
            Console.WriteLine($"[WM-DEBUG] Activation Level: {chunk.ActivationLevel:F3}");
            Console.WriteLine($"[WM-DEBUG] Force Entry: {forceEntry}");
            Console.WriteLine($"[WM-DEBUG] Current WM usage: {GetTotalSlotsUsed()}/{_totalCapacity}");
            Console.WriteLine($"[WM-DEBUG] Current primed count: {_primedChunks.Count}");

            // Check if activation exceeds threshold for working memory
            if (chunk.ActivationLevel < _activationThreshold && !forceEntry)
            {
                Console.WriteLine($"[WM-DEBUG] Chunk activation ({chunk.ActivationLevel:F3}) below WM threshold ({_activationThreshold})");

                // If below WM threshold but above associative threshold, add to primed list
                if (chunk.ActivationLevel >= _associativeThreshold)
                {
                    _primedChunks[chunk.ID] = chunk.ActivationLevel;
                    Console.WriteLine($"[WM-DEBUG] Added chunk to primed list (activation: {chunk.ActivationLevel:F3})");
                }
                else
                {
                    Console.WriteLine($"[WM-DEBUG] Chunk activation too low for priming (threshold: {_associativeThreshold})");
                }

                Console.WriteLine($"[WM-DEBUG] === UpdateWorkingMemoryAsync END (NOT ADDED) ===");
                return false;
            }

            // Determine which subsystem this chunk belongs to
            MemorySubsystem subsystem = DetermineSubsystem(chunk);
            Console.WriteLine($"[WM-DEBUG] Determined subsystem: {subsystem}");

            // Check if chunk is already in working memory
            var existingSlot = FindChunkInWorkingMemory(chunk.ID);
            if (existingSlot != null)
            {
                Console.WriteLine($"[WM-DEBUG] Chunk already in WM (subsystem: {existingSlot.Subsystem})");
                Console.WriteLine($"[WM-DEBUG] Previous activation: {existingSlot.CurrentActivation:F3}");
                Console.WriteLine($"[WM-DEBUG] Previous refresh count: {existingSlot.RefreshCount}");

                // Update existing slot
                existingSlot.CurrentActivation = chunk.ActivationLevel;
                existingSlot.LastRefreshTime = _cognitiveTimeManager.GetCognitiveSteps();
                existingSlot.RefreshCount++;
                existingSlot.FocusValue = 1.0; // Full focus on refreshed item

                Console.WriteLine($"[WM-DEBUG] Updated existing slot - new activation: {existingSlot.CurrentActivation:F3}, refresh count: {existingSlot.RefreshCount}");
                Console.WriteLine($"[WM-DEBUG] === UpdateWorkingMemoryAsync END (UPDATED) ===");
                return true;
            }

            // Check for similar chunks already in working memory (to avoid redundancy)
            bool isRedundant = await IsRedundantToWorkingMemoryAsync(chunk);
            Console.WriteLine($"[WM-DEBUG] Redundancy check result: {isRedundant}");

            if (isRedundant)
            {
                // If redundant and not forced, just add to primed list
                if (!forceEntry)
                {
                    _primedChunks[chunk.ID] = chunk.ActivationLevel;
                    Console.WriteLine($"[WM-DEBUG] Chunk is redundant, added to primed list instead");
                    Console.WriteLine($"[WM-DEBUG] === UpdateWorkingMemoryAsync END (REDUNDANT) ===");
                    return false;
                }
                else
                {
                    Console.WriteLine($"[WM-DEBUG] Chunk is redundant but force entry is enabled");
                }
            }

            // Check if working memory is at capacity
            int totalUsed = GetTotalSlotsUsed();
            Console.WriteLine($"[WM-DEBUG] Current capacity usage: {totalUsed}/{_totalCapacity}");

            if (totalUsed >= _totalCapacity)
            {
                Console.WriteLine($"[WM-DEBUG] Working memory at capacity!");

                if (!forceEntry)
                {
                    // If not forced entry, don't add to working memory
                    _primedChunks[chunk.ID] = chunk.ActivationLevel;
                    Console.WriteLine($"[WM-DEBUG] No force entry - added to primed list instead");
                    Console.WriteLine($"[WM-DEBUG] === UpdateWorkingMemoryAsync END (CAPACITY FULL) ===");
                    return false;
                }

                // Forced entry - need to remove something
                Console.WriteLine($"[WM-DEBUG] Force entry enabled - finding chunk to evict");
                var lowestSlot = FindLowestActivationSlot();

                if (lowestSlot != null)
                {
                    Console.WriteLine($"[WM-DEBUG] Found lowest activation slot:");
                    Console.WriteLine($"[WM-DEBUG]   - Chunk ID: {lowestSlot.ChunkId}");
                    Console.WriteLine($"[WM-DEBUG]   - Activation: {lowestSlot.CurrentActivation:F3}");
                    Console.WriteLine($"[WM-DEBUG]   - Subsystem: {lowestSlot.Subsystem}");

                    if (lowestSlot.CurrentActivation < chunk.ActivationLevel)
                    {
                        // Remove the lowest activation item
                        _workingMemorySlots[lowestSlot.Subsystem].Remove(lowestSlot);
                        Console.WriteLine($"[WM-DEBUG] EVICTED chunk from {lowestSlot.Subsystem} subsystem");

                        // Move it to primed list
                        _primedChunks[lowestSlot.ChunkId] = lowestSlot.CurrentActivation;
                        Console.WriteLine($"[WM-DEBUG] Evicted chunk moved to primed list");
                    }
                    else
                    {
                        // New chunk has lower activation than all existing ones
                        _primedChunks[chunk.ID] = chunk.ActivationLevel;
                        Console.WriteLine($"[WM-DEBUG] New chunk has lower activation than all existing - added to primed list");
                        Console.WriteLine($"[WM-DEBUG] === UpdateWorkingMemoryAsync END (LOWER ACTIVATION) ===");
                        return false;
                    }
                }
                else
                {
                    Console.WriteLine($"[WM-DEBUG] ERROR: Could not find lowest activation slot despite capacity being full!");
                }
            }

            // Add to working memory
            var newSlot = new WorkingMemorySlot
            {
                ChunkId = chunk.ID,
                CurrentActivation = chunk.ActivationLevel,
                EntryTime = _cognitiveTimeManager.GetCognitiveSteps(),
                LastRefreshTime = _cognitiveTimeManager.GetCognitiveSteps(),
                Subsystem = subsystem,
                FocusValue = 1.0, // Full focus on new items
                RefreshCount = 1
            };

            _workingMemorySlots[subsystem].Add(newSlot);
            Console.WriteLine($"[WM-DEBUG] ADDED chunk to working memory in {subsystem} subsystem");
            Console.WriteLine($"[WM-DEBUG] New slot details:");
            Console.WriteLine($"[WM-DEBUG]   - Activation: {newSlot.CurrentActivation:F3}");
            Console.WriteLine($"[WM-DEBUG]   - Focus Value: {newSlot.FocusValue}");
            Console.WriteLine($"[WM-DEBUG]   - Entry Time: {newSlot.EntryTime}");

            // Remove from primed if it was there
            if (_primedChunks.Remove(chunk.ID))
            {
                Console.WriteLine($"[WM-DEBUG] Removed chunk from primed list (was previously primed)");
            }

            // Process new associations to update primed chunks
            Console.WriteLine($"[WM-DEBUG] Processing associations to update primed chunks...");
            await UpdatePrimedChunksAsync(chunk);

            Console.WriteLine($"[WM-DEBUG] Final WM usage: {GetTotalSlotsUsed()}/{_totalCapacity}");
            Console.WriteLine($"[WM-DEBUG] Final primed count: {_primedChunks.Count}");
            Console.WriteLine($"[WM-DEBUG] === UpdateWorkingMemoryAsync END (ADDED) ===");
            return true;
        }

        /// <summary>
        /// Returns all chunks currently in working memory in order of activation
        /// </summary>
        public async Task<List<Chunk>> GetWorkingMemoryContentsAsync()
        {
            Console.WriteLine($"[WM-DEBUG] GetWorkingMemoryContentsAsync called");

            var results = new List<Chunk>();
            var chunkCollection = await _chunkStore.GetCollectionAsync(_chunkCollectionId);

            if (chunkCollection == null)
            {
                Console.WriteLine($"[WM-DEBUG] ERROR: Could not get chunk collection '{_chunkCollectionId}'");
                return results;
            }

            // Flatten all subsystems and order by activation
            var allSlots = _workingMemorySlots.Values
                .SelectMany(slots => slots)
                .OrderByDescending(slot => slot.CurrentActivation);

            Console.WriteLine($"[WM-DEBUG] Found {allSlots.Count()} slots in working memory");

            int count = 0;
            foreach (var slot in allSlots)
            {
                var chunk = await chunkCollection.GetChunkAsync(slot.ChunkId);
                if (chunk != null)
                {
                    results.Add(chunk);
                    Console.WriteLine($"[WM-DEBUG] #{++count}: {chunk.Name} (activation: {slot.CurrentActivation:F3}, subsystem: {slot.Subsystem})");
                }
                else
                {
                    Console.WriteLine($"[WM-DEBUG] WARNING: Could not retrieve chunk {slot.ChunkId} from store");
                }
            }

            Console.WriteLine($"[WM-DEBUG] Returning {results.Count} chunks from working memory");
            return results;
        }

        /// <summary>
        /// Returns all primed chunks (partially activated but not in working memory)
        /// </summary>
        public async Task<List<Chunk>> GetPrimedChunksAsync()
        {
            Console.WriteLine($"[WM-DEBUG] GetPrimedChunksAsync called - {_primedChunks.Count} primed chunks");

            var results = new List<Chunk>();
            var chunkCollection = await _chunkStore.GetCollectionAsync(_chunkCollectionId);

            if (chunkCollection == null)
            {
                Console.WriteLine($"[WM-DEBUG] ERROR: Could not get chunk collection '{_chunkCollectionId}'");
                return results;
            }

            int count = 0;
            foreach (var pair in _primedChunks.OrderByDescending(p => p.Value))
            {
                var chunk = await chunkCollection.GetChunkAsync(pair.Key);
                if (chunk != null)
                {
                    results.Add(chunk);
                    Console.WriteLine($"[WM-DEBUG] Primed #{++count}: {chunk.Name} (activation: {pair.Value:F3})");
                }
                else
                {
                    Console.WriteLine($"[WM-DEBUG] WARNING: Could not retrieve primed chunk {pair.Key} from store");
                }
            }

            return results;
        }

        /// <summary>
        /// Simulates natural decay of working memory items and refreshes focused items
        /// </summary>
        public async Task RefreshCycleAsync(List<Guid> focusedChunkIds = null)
        {
            Console.WriteLine($"[WM-DEBUG] === RefreshCycleAsync START ===");

            // Calculate time since last refresh
            var now = _cognitiveTimeManager.GetCognitiveSteps();
            var timeSinceLastRefresh = (now - _lastRefreshTime);
            _lastRefreshTime = now;

            Console.WriteLine($"[WM-DEBUG] Time since last refresh: {timeSinceLastRefresh:F2} seconds");
            Console.WriteLine($"[WM-DEBUG] Decay rate: {_refreshDecayRate}");

            // Default empty list if null
            focusedChunkIds ??= new List<Guid>();
            Console.WriteLine($"[WM-DEBUG] Focused chunks count: {focusedChunkIds.Count}");

            if (focusedChunkIds.Any())
            {
                foreach (var focusedId in focusedChunkIds)
                {
                    Console.WriteLine($"[WM-DEBUG] Focused chunk: {focusedId}");
                }
            }

            int totalSlotsProcessed = 0;
            int totalSlotsRemoved = 0;

            // Apply decay to all working memory items
            foreach (var subsystem in _workingMemorySlots.Keys)
            {
                var slots = _workingMemorySlots[subsystem];
                var slotsToRemove = new List<WorkingMemorySlot>();

                Console.WriteLine($"[WM-DEBUG] Processing {subsystem} subsystem ({slots.Count} slots)");

                foreach (var slot in slots)
                {
                    totalSlotsProcessed++;
                    double oldActivation = slot.CurrentActivation;
                    double oldFocus = slot.FocusValue;


                    // Apply decay based on time since last refresh
                    slot.FocusValue *= 0.9; // Gradual focus decay
                    slot.CurrentActivation *= (1 - (_refreshDecayRate * timeSinceLastRefresh));

                    Console.WriteLine($"[WM-DEBUG]   Chunk {slot.ChunkId}:");
                    Console.WriteLine($"[WM-DEBUG]     Activation: {oldActivation:F3} -> {slot.CurrentActivation:F3}");
                    Console.WriteLine($"[WM-DEBUG]     Focus: {oldFocus:F3} -> {slot.FocusValue:F3}");

                    // If activation falls below threshold, mark for removal
                    if (slot.CurrentActivation < _associativeThreshold)
                    {
                        slotsToRemove.Add(slot);
                        Console.WriteLine($"[WM-DEBUG]     MARKED FOR COMPLETE REMOVAL (below associative threshold {_associativeThreshold})");
                    }
                    else if (slot.CurrentActivation < _activationThreshold)
                    {
                        // Move to primed list if below WM threshold but above associative threshold
                        slotsToRemove.Add(slot);
                        _primedChunks[slot.ChunkId] = slot.CurrentActivation;
                        Console.WriteLine($"[WM-DEBUG]     MARKED FOR PRIMING (below WM threshold {_activationThreshold})");
                    }

                }

                // Remove decayed items
                foreach (var slotToRemove in slotsToRemove)
                {
                    slots.Remove(slotToRemove);
                    totalSlotsRemoved++;
                    Console.WriteLine($"[WM-DEBUG] REMOVED chunk {slotToRemove.ChunkId} from {subsystem}");
                }

                if (slotsToRemove.Count > 0)
                {
                    Console.WriteLine($"[WM-DEBUG] {subsystem} subsystem: removed {slotsToRemove.Count} slots, {slots.Count} remaining");
                }
            }

            // Apply decay to primed chunks
            var primedToRemove = new List<Guid>();
            int primedProcessed = 0;

            Console.WriteLine($"[WM-DEBUG] Processing {_primedChunks.Count} primed chunks");

            foreach (var chunkId in _primedChunks.Keys.ToList())
            {
                primedProcessed++;
                double oldPrimedActivation = _primedChunks[chunkId];
                _primedChunks[chunkId] *= (1 - (_refreshDecayRate * 1.5 * timeSinceLastRefresh)); // Primed decay faster

                Console.WriteLine($"[WM-DEBUG] Primed chunk {chunkId}: {oldPrimedActivation:F3} -> {_primedChunks[chunkId]:F3}");

                if (_primedChunks[chunkId] < _associativeThreshold)
                {
                    primedToRemove.Add(chunkId);
                    Console.WriteLine($"[WM-DEBUG] Primed chunk {chunkId} marked for removal (below threshold)");
                }
            }

            // Remove decayed primed chunks
            foreach (var chunkId in primedToRemove)
            {
                _primedChunks.Remove(chunkId);
                Console.WriteLine($"[WM-DEBUG] REMOVED primed chunk {chunkId}");
            }

            Console.WriteLine($"[WM-DEBUG] Refresh cycle summary:");
            Console.WriteLine($"[WM-DEBUG]   - Processed {totalSlotsProcessed} WM slots");
            Console.WriteLine($"[WM-DEBUG]   - Removed {totalSlotsRemoved} WM slots");
            Console.WriteLine($"[WM-DEBUG]   - Processed {primedProcessed} primed chunks");
            Console.WriteLine($"[WM-DEBUG]   - Removed {primedToRemove.Count} primed chunks");
            Console.WriteLine($"[WM-DEBUG]   - Final WM usage: {GetTotalSlotsUsed()}/{_totalCapacity}");
            Console.WriteLine($"[WM-DEBUG]   - Final primed count: {_primedChunks.Count}");
            Console.WriteLine($"[WM-DEBUG] === RefreshCycleAsync END ===");
        }

        /// <summary>
        /// Clears working memory entirely
        /// </summary>
        public void ClearWorkingMemory()
        {
            Console.WriteLine($"[WM-DEBUG] === ClearWorkingMemory START ===");

            int totalCleared = 0;
            foreach (var subsystem in _workingMemorySlots.Keys)
            {
                int subsystemCount = _workingMemorySlots[subsystem].Count;
                _workingMemorySlots[subsystem].Clear();
                totalCleared += subsystemCount;
                Console.WriteLine($"[WM-DEBUG] Cleared {subsystemCount} chunks from {subsystem}");
            }

            int primedCleared = _primedChunks.Count;
            _primedChunks.Clear();

            Console.WriteLine($"[WM-DEBUG] Cleared {totalCleared} WM chunks and {primedCleared} primed chunks");
            Console.WriteLine($"[WM-DEBUG] === ClearWorkingMemory END ===");
        }

        /// <summary>
        /// Forces a chunk out of working memory
        /// </summary>
        public bool RemoveFromWorkingMemory(Guid chunkId)
        {
            Console.WriteLine($"[WM-DEBUG] RemoveFromWorkingMemory called for chunk: {chunkId}");

            var slot = FindChunkInWorkingMemory(chunkId);
            if (slot != null)
            {
                Console.WriteLine($"[WM-DEBUG] Found chunk in {slot.Subsystem} subsystem, activation: {slot.CurrentActivation:F3}");

                _workingMemorySlots[slot.Subsystem].Remove(slot);
                _primedChunks[chunkId] = slot.CurrentActivation; // Move to primed

                Console.WriteLine($"[WM-DEBUG] REMOVED chunk from WM and moved to primed list");
                return true;
            }
            else
            {
                Console.WriteLine($"[WM-DEBUG] Chunk not found in working memory");
                return false;
            }
        }

        /// <summary>
        /// Determines which subsystem a chunk belongs to based on its properties
        /// </summary>
        private MemorySubsystem DetermineSubsystem(Chunk chunk)
        {
            // Simple heuristic based on chunk type
            if (string.IsNullOrEmpty(chunk.ChunkType))
            {
                Console.WriteLine($"[WM-DEBUG] No chunk type specified, defaulting to Semantic");
                return MemorySubsystem.Semantic;
            }

            string type = chunk.ChunkType.ToLowerInvariant();
            Console.WriteLine($"[WM-DEBUG] Determining subsystem for chunk type: '{type}'");

            if (type.Contains("visual") || type.Contains("spatial") || type.Contains("image"))
            {
                Console.WriteLine($"[WM-DEBUG] Assigned to VisualSpatial subsystem");
                return MemorySubsystem.VisualSpatial;
            }

            if (type.Contains("sound") || type.Contains("audio") || type.Contains("phonological") || type.Contains("verbal"))
            {
                Console.WriteLine($"[WM-DEBUG] Assigned to Phonological subsystem");
                return MemorySubsystem.Phonological;
            }

            if (type.Contains("episode") || type.Contains("event") || type.Contains("memory"))
            {
                Console.WriteLine($"[WM-DEBUG] Assigned to Episodic subsystem");
                return MemorySubsystem.Episodic;
            }

            if (type.Contains("procedure") || type.Contains("action") || type.Contains("skill"))
            {
                Console.WriteLine($"[WM-DEBUG] Assigned to Procedural subsystem");
                return MemorySubsystem.Procedural;
            }

            // Default to semantic for anything else
            Console.WriteLine($"[WM-DEBUG] No specific match, assigned to Semantic subsystem");
            return MemorySubsystem.Semantic;
        }

        /// <summary>
        /// Updates the list of primed chunks based on associations to the given chunk
        /// </summary>
        private async Task UpdatePrimedChunksAsync(Chunk chunk)
        {
            Console.WriteLine($"[WM-DEBUG] UpdatePrimedChunksAsync for chunk: {chunk.Name}");

            var associationCollection = await _associationStore.GetCollectionAsync(_associationCollectionId);
            if (associationCollection == null)
            {
                Console.WriteLine($"[WM-DEBUG] ERROR: Could not get association collection '{_associationCollectionId}'");
                return;
            }

            // Get all associations for this chunk
            var associations = await associationCollection.GetAssociationsForChunkAsync(chunk.ID);
            Console.WriteLine($"[WM-DEBUG] Found {associations.Count()} associations");

            int primedCount = 0;
            int skippedInWM = 0;
            int skippedLowActivation = 0;

            foreach (var association in associations)
            {
                // Determine target chunk and direction
                bool isSourceA = association.ChunkAId == chunk.ID;
                Guid targetChunkId = isSourceA ? association.ChunkBId : association.ChunkAId;

                Console.WriteLine($"[WM-DEBUG] Processing association to chunk: {targetChunkId}");

                // Skip if already in working memory
                if (FindChunkInWorkingMemory(targetChunkId) != null)
                {
                    skippedInWM++;
                    Console.WriteLine($"[WM-DEBUG]   Skipped - already in working memory");
                    continue;
                }

                // Get weight in the correct direction
                double weight = isSourceA ? association.WeightAtoB : association.WeightBtoA;
                Console.WriteLine($"[WM-DEBUG]   Association weight: {weight:F3} (direction: {(isSourceA ? "A->B" : "B->A")})");

                // Calculate priming activation
                double primingActivation = chunk.ActivationLevel * weight * 0.5;
                Console.WriteLine($"[WM-DEBUG]   Calculated priming activation: {primingActivation:F3} (source: {chunk.ActivationLevel:F3} * {weight:F3} * 0.5)");

                // If high enough to be primed, add to primed list
                if (primingActivation >= _associativeThreshold)
                {
                    if (_primedChunks.ContainsKey(targetChunkId))
                    {
                        double oldPriming = _primedChunks[targetChunkId];
                        // Take the higher of existing or new priming
                        _primedChunks[targetChunkId] = Math.Max(_primedChunks[targetChunkId], primingActivation);
                        Console.WriteLine($"[WM-DEBUG]   Updated existing primed chunk: {oldPriming:F3} -> {_primedChunks[targetChunkId]:F3}");
                    }
                    else
                    {
                        _primedChunks[targetChunkId] = primingActivation;
                        Console.WriteLine($"[WM-DEBUG]   ADDED to primed list with activation: {primingActivation:F3}");
                        primedCount++;
                    }
                }
                else
                {
                    skippedLowActivation++;
                    Console.WriteLine($"[WM-DEBUG]   Skipped - priming activation ({primingActivation:F3}) below threshold ({_associativeThreshold})");
                }
            }

            Console.WriteLine($"[WM-DEBUG] Priming summary:");
            Console.WriteLine($"[WM-DEBUG]   - New primed chunks: {primedCount}");
            Console.WriteLine($"[WM-DEBUG]   - Skipped (in WM): {skippedInWM}");
            Console.WriteLine($"[WM-DEBUG]   - Skipped (low activation): {skippedLowActivation}");
        }

        /// <summary>
        /// Checks if a chunk is redundant to what's already in working memory
        /// </summary>
        private async Task<bool> IsRedundantToWorkingMemoryAsync(Chunk candidateChunk)
        {
            Console.WriteLine($"[WM-DEBUG] Checking redundancy for chunk: {candidateChunk.Name}");

            // Get current working memory contents
            var workingMemoryChunks = await GetWorkingMemoryContentsAsync();

            // If empty, nothing to be redundant with
            if (workingMemoryChunks.Count == 0)
            {
                Console.WriteLine($"[WM-DEBUG] No chunks in working memory - not redundant");
                return false;
            }

            Console.WriteLine($"[WM-DEBUG] Checking against {workingMemoryChunks.Count} chunks in working memory");

            // Check for direct associations first
            var associationCollection = await _associationStore.GetCollectionAsync(_associationCollectionId);
            if (associationCollection != null)
            {
                var associations = await associationCollection.GetAssociationsForChunkAsync(candidateChunk.ID);
                Console.WriteLine($"[WM-DEBUG] Found {associations.Count()} associations to check");

                // Check each association against working memory
                foreach (var association in associations)
                {
                    // Get the other chunk ID
                    bool isSourceA = association.ChunkAId == candidateChunk.ID;
                    Guid otherChunkId = isSourceA ? association.ChunkBId : association.ChunkAId;

                    // Get strength
                    double strength = isSourceA ? association.WeightAtoB : association.WeightBtoA;

                    Console.WriteLine($"[WM-DEBUG] Checking association with {otherChunkId}, strength: {strength:F3}");

                    // If strongly associated with something in working memory, it's redundant
                    if (strength > _interferenceThreshold && FindChunkInWorkingMemory(otherChunkId) != null)
                    {
                        Console.WriteLine($"[WM-DEBUG] REDUNDANT - strong association ({strength:F3} > {_interferenceThreshold}) with chunk in WM");
                        return true;
                    }
                }
            }
            else
            {
                Console.WriteLine($"[WM-DEBUG] WARNING: Could not get association collection for redundancy check");
            }

            // If we have vector representations, check similarities
            if (candidateChunk.Vector != null && candidateChunk.Vector.Length > 0)
            {
                Console.WriteLine($"[WM-DEBUG] Checking vector similarity (candidate has {candidateChunk.Vector.Length} dimensions)");

                int vectorChecked = 0;
                foreach (var wmChunk in workingMemoryChunks)
                {
                    if (wmChunk.Vector != null && wmChunk.Vector.Length == candidateChunk.Vector.Length)
                    {
                        vectorChecked++;
                        double similarity = CalculateCosineSimilarity(candidateChunk.Vector, wmChunk.Vector);
                        Console.WriteLine($"[WM-DEBUG] Vector similarity with {wmChunk.Name}: {similarity:F3}");

                        if (similarity > _similarityThreshold)
                        {
                            Console.WriteLine($"[WM-DEBUG] REDUNDANT - high vector similarity ({similarity:F3} > {_similarityThreshold})");
                            return true;
                        }
                    }
                }

                Console.WriteLine($"[WM-DEBUG] Checked vectors for {vectorChecked} chunks, no high similarity found");
            }
            else
            {
                Console.WriteLine($"[WM-DEBUG] No vector data available for similarity check");
            }

            Console.WriteLine($"[WM-DEBUG] Not redundant to working memory");
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
                {
                    Console.WriteLine($"[WM-DEBUG] Found chunk {chunkId} in {subsystem} subsystem");
                    return slot;
                }
            }
            return null;
        }

        /// <summary>
        /// Find the slot with the lowest activation across all subsystems
        /// </summary>
        private WorkingMemorySlot FindLowestActivationSlot()
        {
            Console.WriteLine($"[WM-DEBUG] Finding lowest activation slot across all subsystems");

            WorkingMemorySlot lowestSlot = null;
            double lowestActivation = double.MaxValue;

            foreach (var subsystem in _workingMemorySlots.Keys)
            {
                foreach (var slot in _workingMemorySlots[subsystem])
                {
                    Console.WriteLine($"[WM-DEBUG] Checking {subsystem} slot {slot.ChunkId}: activation {slot.CurrentActivation:F3}");

                    if (slot.CurrentActivation < lowestActivation)
                    {
                        lowestActivation = slot.CurrentActivation;
                        lowestSlot = slot;
                        Console.WriteLine($"[WM-DEBUG] New lowest found: {slot.CurrentActivation:F3}");
                    }
                }
            }

            if (lowestSlot != null)
            {
                Console.WriteLine($"[WM-DEBUG] Lowest activation slot: {lowestSlot.ChunkId} with {lowestActivation:F3}");
            }
            else
            {
                Console.WriteLine($"[WM-DEBUG] No slots found!");
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
            var usage = _workingMemorySlots.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Count
            );

            Console.WriteLine($"[WM-DEBUG] Working memory usage by subsystem:");
            foreach (var kvp in usage)
            {
                Console.WriteLine($"[WM-DEBUG]   {kvp.Key}: {kvp.Value} slots");
            }

            return usage;
        }

        /// <summary>
        /// Gets the number of primed chunks
        /// </summary>
        public int GetPrimedChunksCount()
        {
            int count = _primedChunks.Count;
            Console.WriteLine($"[WM-DEBUG] Current primed chunks count: {count}");
            return count;
        }
    }
}