using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Aislinn.ChunkStorage.Interfaces;
using Aislinn.Core.Models;

namespace Aislinn.Core.Context
{
    /// <summary>
    /// Maintains situational awareness by tracking and organizing context information
    /// that influences goal selection and execution.
    /// </summary>
    public class ContextContainer
    {
        // Context categories
        public enum ContextCategory
        {
            Environment,   // Physical environment state (location, objects, conditions)
            Internal,      // Agent's internal state (energy, emotions, physiological)
            Social,        // Social environment (present entities, relationships, roles)
            Task,          // Task-related context (current activities, progress, history)
            Temporal,      // Time-related context (time of day, day of week, deadlines)
            Resource       // Available resources (tools, information, capabilities)
        }

        // Main context storage - category -> context factors
        private Dictionary<ContextCategory, Dictionary<string, ContextFactor>> _contextFactors;

        // For tracking active context chunks in memory
        private Dictionary<ContextCategory, List<Guid>> _activeContextChunks;

        // Dependencies
        private readonly IChunkStore _chunkStore;
        private readonly string _chunkCollectionId;

        // Configuration
        private readonly double _significantChangeThreshold;
        private readonly TimeSpan _contextRetentionTime;

        // Event for context change notifications
        public event EventHandler<ContextChangeEventArgs> SignificantContextChange;

        /// <summary>
        /// Represents a single context factor with value, timestamp, and metadata
        /// </summary>
        public class ContextFactor
        {
            public string Name { get; set; }
            public object Value { get; set; }
            public DateTime Timestamp { get; set; }
            public double Confidence { get; set; } = 1.0; // How certain we are of this context
            public double Importance { get; set; } = 0.5; // Base importance of this context factor
            public Guid? SourceChunkId { get; set; } // Associated chunk if applicable
            public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();

            public ContextFactor(string name, object value)
            {
                Name = name;
                Value = value;
                Timestamp = DateTime.Now;
            }

            public bool HasExpired(TimeSpan retentionTime)
            {
                return (DateTime.Now - Timestamp) > retentionTime;
            }
        }

        /// <summary>
        /// Event args for context change notifications
        /// </summary>
        public class ContextChangeEventArgs : EventArgs
        {
            public ContextCategory Category { get; set; }
            public string FactorName { get; set; }
            public object OldValue { get; set; }
            public object NewValue { get; set; }
            public double ChangeSignificance { get; set; }
            public DateTime Timestamp { get; set; } = DateTime.Now;
        }

        /// <summary>
        /// Initializes a new ContextContainer
        /// </summary>
        public ContextContainer(
            IChunkStore chunkStore,
            string chunkCollectionId = "default",
            double significantChangeThreshold = 0.3,
            TimeSpan? contextRetentionTime = null)
        {
            _chunkStore = chunkStore ?? throw new ArgumentNullException(nameof(chunkStore));
            _chunkCollectionId = chunkCollectionId;
            _significantChangeThreshold = significantChangeThreshold;
            _contextRetentionTime = contextRetentionTime ?? TimeSpan.FromHours(1);

            // Initialize storage
            _contextFactors = new Dictionary<ContextCategory, Dictionary<string, ContextFactor>>();
            _activeContextChunks = new Dictionary<ContextCategory, List<Guid>>();

            // Initialize categories
            foreach (ContextCategory category in Enum.GetValues(typeof(ContextCategory)))
            {
                _contextFactors[category] = new Dictionary<string, ContextFactor>();
                _activeContextChunks[category] = new List<Guid>();
            }
        }

        /// <summary>
        /// Updates or adds a context factor
        /// </summary>
        public void UpdateContextFactor(
            ContextCategory category,
            string factorName,
            object value,
            double importance = 0.5,
            double confidence = 1.0,
            Dictionary<string, object> metadata = null)
        {
            if (string.IsNullOrEmpty(factorName))
                throw new ArgumentException("Factor name cannot be null or empty", nameof(factorName));

            object oldValue = null;
            bool isSignificantChange = false;

            // Check if factor already exists
            if (_contextFactors[category].TryGetValue(factorName, out var existingFactor))
            {
                oldValue = existingFactor.Value;

                // Determine if this is a significant change
                isSignificantChange = IsSignificantChange(existingFactor.Value, value, importance);

                // Update existing factor
                existingFactor.Value = value;
                existingFactor.Timestamp = DateTime.Now;
                existingFactor.Importance = importance;
                existingFactor.Confidence = confidence;

                if (metadata != null)
                {
                    foreach (var item in metadata)
                    {
                        existingFactor.Metadata[item.Key] = item.Value;
                    }
                }
            }
            else
            {
                // Create new factor
                var newFactor = new ContextFactor(factorName, value)
                {
                    Importance = importance,
                    Confidence = confidence,
                    Metadata = metadata ?? new Dictionary<string, object>()
                };

                _contextFactors[category][factorName] = newFactor;

                // New factor is always considered a significant change
                isSignificantChange = true;
            }

            // Raise event if significant change occurred
            if (isSignificantChange && SignificantContextChange != null)
            {
                double changeSignificance = CalculateChangeSignificance(oldValue, value, importance);

                SignificantContextChange(this, new ContextChangeEventArgs
                {
                    Category = category,
                    FactorName = factorName,
                    OldValue = oldValue,
                    NewValue = value,
                    ChangeSignificance = changeSignificance
                });
            }
        }

        /// <summary>
        /// Get a specific context factor
        /// </summary>
        public ContextFactor GetContextFactor(ContextCategory category, string factorName)
        {
            if (string.IsNullOrEmpty(factorName))
                throw new ArgumentException("Factor name cannot be null or empty", nameof(factorName));

            if (_contextFactors[category].TryGetValue(factorName, out var factor))
            {
                return factor;
            }

            return null;
        }

        /// <summary>
        /// Get the value of a specific context factor
        /// </summary>
        public T GetContextValue<T>(ContextCategory category, string factorName, T defaultValue = default)
        {
            var factor = GetContextFactor(category, factorName);

            if (factor != null && factor.Value is T typedValue)
            {
                return typedValue;
            }

            return defaultValue;
        }

        /// <summary>
        /// Get all context factors for a category
        /// </summary>
        public Dictionary<string, ContextFactor> GetCategoryFactors(ContextCategory category)
        {
            return new Dictionary<string, ContextFactor>(_contextFactors[category]);
        }

        /// <summary>
        /// Check if a context factor exists and is recent
        /// </summary>
        public bool HasRecentContextFactor(ContextCategory category, string factorName, TimeSpan? maxAge = null)
        {
            var factor = GetContextFactor(category, factorName);

            if (factor == null)
                return false;

            TimeSpan age = DateTime.Now - factor.Timestamp;
            TimeSpan maxAllowedAge = maxAge ?? _contextRetentionTime;

            return age <= maxAllowedAge;
        }

        /// <summary>
        /// Add a chunk to the active context for a category
        /// </summary>
        public void AddContextChunk(ContextCategory category, Guid chunkId)
        {
            if (!_activeContextChunks[category].Contains(chunkId))
            {
                _activeContextChunks[category].Add(chunkId);
            }
        }

        /// <summary>
        /// Remove a chunk from the active context
        /// </summary>
        public void RemoveContextChunk(ContextCategory category, Guid chunkId)
        {
            _activeContextChunks[category].Remove(chunkId);
        }

        /// <summary>
        /// Get all active context chunks for a category
        /// </summary>
        public List<Guid> GetActiveContextChunks(ContextCategory category)
        {
            return new List<Guid>(_activeContextChunks[category]);
        }

        /// <summary>
        /// Get all active context chunks across all categories
        /// </summary>
        public Dictionary<ContextCategory, List<Guid>> GetAllActiveContextChunks()
        {
            var result = new Dictionary<ContextCategory, List<Guid>>();

            foreach (var category in _activeContextChunks.Keys)
            {
                result[category] = new List<Guid>(_activeContextChunks[category]);
            }

            return result;
        }

        /// <summary>
        /// Update context chunks from working memory
        /// </summary>
        public async Task UpdateContextFromWorkingMemoryAsync(List<Chunk> workingMemoryChunks)
        {
            if (workingMemoryChunks == null)
                return;

            // Clear current active chunks (will be repopulated)
            foreach (var category in _activeContextChunks.Keys)
            {
                _activeContextChunks[category].Clear();
            }

            // Categorize chunks into context categories
            foreach (var chunk in workingMemoryChunks)
            {
                ContextCategory category = CategorizeChunk(chunk);
                AddContextChunk(category, chunk.ID);

                // Extract relevant context factors from chunk slots
                ExtractContextFactorsFromChunk(category, chunk);
            }
        }

        /// <summary>
        /// Clear expired context factors
        /// </summary>
        public void CleanupExpiredFactors()
        {
            foreach (var category in _contextFactors.Keys)
            {
                var expiredFactors = _contextFactors[category]
                    .Where(kv => kv.Value.HasExpired(_contextRetentionTime))
                    .Select(kv => kv.Key)
                    .ToList();

                foreach (var factor in expiredFactors)
                {
                    _contextFactors[category].Remove(factor);
                }
            }
        }

        /// <summary>
        /// Create a context snapshot
        /// </summary>
        public Dictionary<ContextCategory, Dictionary<string, object>> CreateContextSnapshot()
        {
            var snapshot = new Dictionary<ContextCategory, Dictionary<string, object>>();

            foreach (var category in _contextFactors.Keys)
            {
                snapshot[category] = new Dictionary<string, object>();

                foreach (var factor in _contextFactors[category])
                {
                    snapshot[category][factor.Key] = factor.Value.Value;
                }
            }

            return snapshot;
        }

        /// <summary>
        /// Calculate a relevance score between context and goal
        /// </summary>
        public double CalculateContextRelevance(Chunk goalChunk)
        {
            if (goalChunk == null)
                return 0;

            // Example simple implementation - would be enhanced in practice
            double relevanceScore = 0;
            double totalWeight = 0;

            // Check for context requirements in goal slots
            if (goalChunk.Slots.TryGetValue("ContextRequirements", out var requirementsSlot) &&
                requirementsSlot.Value is Dictionary<string, object> requirements)
            {
                foreach (var req in requirements)
                {
                    // Format expected: "Category.FactorName" -> value
                    string[] parts = req.Key.Split('.');
                    if (parts.Length != 2) continue;

                    if (Enum.TryParse<ContextCategory>(parts[0], out var category))
                    {
                        string factorName = parts[1];
                        var factor = GetContextFactor(category, factorName);

                        if (factor != null)
                        {
                            double match = CompareValues(factor.Value, req.Value);
                            double weight = factor.Importance;

                            relevanceScore += match * weight;
                            totalWeight += weight;
                        }
                    }
                }
            }

            return totalWeight > 0 ? relevanceScore / totalWeight : 0;
        }

        #region Helper Methods

        /// <summary>
        /// Determine if a change is significant enough to trigger an event
        /// </summary>
        private bool IsSignificantChange(object oldValue, object newValue, double importance)
        {
            double changeSignificance = CalculateChangeSignificance(oldValue, newValue, importance);
            return changeSignificance >= _significantChangeThreshold;
        }

        /// <summary>
        /// Calculate the significance of a change
        /// </summary>
        private double CalculateChangeSignificance(object oldValue, object newValue, double importance)
        {
            // New values are always significant
            if (oldValue == null && newValue != null)
                return importance;

            // No change
            if ((oldValue == null && newValue == null) ||
                (oldValue != null && oldValue.Equals(newValue)))
                return 0;

            // For numeric values, calculate percentage change
            if (oldValue is IConvertible && newValue is IConvertible)
            {
                try
                {
                    double oldDouble = Convert.ToDouble(oldValue);
                    double newDouble = Convert.ToDouble(newValue);

                    if (Math.Abs(oldDouble) < 0.0001) // Avoid division by zero
                    {
                        return importance;
                    }

                    double percentChange = Math.Abs((newDouble - oldDouble) / oldDouble);
                    return Math.Min(1.0, percentChange) * importance;
                }
                catch { }
            }

            // For boolean, true/false changes are significant
            if (oldValue is bool oldBool && newValue is bool newBool)
            {
                return oldBool != newBool ? importance : 0;
            }

            // For strings, if they're different it's significant
            if (oldValue is string && newValue is string)
            {
                return !oldValue.Equals(newValue) ? importance : 0;
            }

            // Default for other types
            return importance * 0.5;
        }

        /// <summary>
        /// Compare two values and return a similarity score 0-1
        /// </summary>
        private double CompareValues(object value1, object value2)
        {
            // Exact match
            if ((value1 == null && value2 == null) ||
                (value1 != null && value1.Equals(value2)))
                return 1.0;

            // Numeric comparison
            if (value1 is IConvertible && value2 is IConvertible)
            {
                try
                {
                    double double1 = Convert.ToDouble(value1);
                    double double2 = Convert.ToDouble(value2);

                    double diff = Math.Abs(double1 - double2);
                    double max = Math.Max(Math.Abs(double1), Math.Abs(double2));

                    if (max < 0.0001) return diff < 0.0001 ? 1.0 : 0.0;

                    return Math.Max(0.0, 1.0 - (diff / max));
                }
                catch { }
            }

            // String partial matching
            if (value1 is string str1 && value2 is string str2)
            {
                if (str1.Length == 0 || str2.Length == 0) return 0;

                if (str1.Contains(str2) || str2.Contains(str1))
                {
                    int minLength = Math.Min(str1.Length, str2.Length);
                    int maxLength = Math.Max(str1.Length, str2.Length);
                    return (double)minLength / maxLength;
                }
            }

            // Default for different types/values
            return 0.0;
        }

        /// <summary>
        /// Determine which context category a chunk belongs to
        /// </summary>
        private ContextCategory CategorizeChunk(Chunk chunk)
        {
            if (chunk == null) return ContextCategory.Environment;

            string chunkType = chunk.ChunkType?.ToLowerInvariant() ?? string.Empty;

            if (chunkType.Contains("location") ||
                chunkType.Contains("environment") ||
                chunkType.Contains("physical"))
                return ContextCategory.Environment;

            if (chunkType.Contains("emotion") ||
                chunkType.Contains("internal") ||
                chunkType.Contains("physiological"))
                return ContextCategory.Internal;

            if (chunkType.Contains("social") ||
                chunkType.Contains("person") ||
                chunkType.Contains("relationship"))
                return ContextCategory.Social;

            if (chunkType.Contains("task") ||
                chunkType.Contains("activity") ||
                chunkType.Contains("action"))
                return ContextCategory.Task;

            if (chunkType.Contains("time") ||
                chunkType.Contains("date") ||
                chunkType.Contains("temporal"))
                return ContextCategory.Temporal;

            if (chunkType.Contains("resource") ||
                chunkType.Contains("tool") ||
                chunkType.Contains("capability"))
                return ContextCategory.Resource;

            // Default to environment if no match
            return ContextCategory.Environment;
        }

        /// <summary>
        /// Extract relevant context factors from a chunk's slots
        /// </summary>
        private void ExtractContextFactorsFromChunk(ContextCategory category, Chunk chunk)
        {
            if (chunk == null) return;

            // Extract slots that represent context factors
            foreach (var slot in chunk.Slots)
            {
                // Skip certain system or internal slots
                if (slot.Key == "ID" || slot.Key == "ChunkType" ||
                    slot.Key == "ActivationLevel" || slot.Key == "Vector")
                    continue;

                // Add as context factor with moderate importance
                UpdateContextFactor(
                    category,
                    $"{chunk.Name}.{slot.Key}",
                    slot.Value.Value,
                    importance: 0.4,
                    confidence: 0.8,
                    metadata: new Dictionary<string, object>
                    {
                        { "SourceChunk", chunk.ID },
                        { "SlotName", slot.Key }
                    }
                );
            }
        }

        #endregion
    }
}