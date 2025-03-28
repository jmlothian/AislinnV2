using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Aislinn.ChunkStorage.Interfaces;
using Aislinn.Core.Models;
using Aislinn.Core.Services;

namespace Aislinn.Core.Procedural
{
    /// <summary>
    /// Matches goals with appropriate procedures based on goal type, context,
    /// and other factors. Also handles procedure activation and selection.
    /// </summary>
    public class ProcedureMatcher
    {
        private readonly IChunkStore _chunkStore;
        private readonly ChunkActivationService _activationService;
        private readonly string _chunkCollectionId;

        // Cache of procedure chunks by ID for quick access
        private Dictionary<Guid, ProcedureChunk> _procedureCache;

        // Cache last updated time
        private DateTime _lastCacheUpdate;

        // Cache expiration time
        private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(5);

        // Configuration
        private readonly ProcedureMatcherConfig _config;

        /// <summary>
        /// Configuration options for procedure matching
        /// </summary>
        public class ProcedureMatcherConfig
        {
            /// <summary>
            /// How much context relevance affects matching score
            /// </summary>
            public double ContextRelevanceWeight { get; set; } = 0.3;

            /// <summary>
            /// How much activation level affects matching score
            /// </summary>
            public double ActivationWeight { get; set; } = 0.25;

            /// <summary>
            /// How much success probability affects matching score
            /// </summary>
            public double SuccessProbabilityWeight { get; set; } = 0.2;

            /// <summary>
            /// How much resource availability affects matching score
            /// </summary>
            public double ResourceAvailabilityWeight { get; set; } = 0.15;

            /// <summary>
            /// How much estimated utility affects matching score
            /// </summary>
            public double UtilityWeight { get; set; } = 0.1;

            /// <summary>
            /// Minimum score for a procedure to be considered
            /// </summary>
            public double MinimumMatchScore { get; set; } = 0.3;

            /// <summary>
            /// Whether to cache procedures for faster matching
            /// </summary>
            public bool EnableCaching { get; set; } = true;

            /// <summary>
            /// How many top procedures to return
            /// </summary>
            public int MaxMatchResults { get; set; } = 5;
        }

        /// <summary>
        /// Result of a procedure matching operation
        /// </summary>
        public class ProcedureMatchResult
        {
            /// <summary>
            /// The matched procedure
            /// </summary>
            public ProcedureChunk Procedure { get; set; }

            /// <summary>
            /// Match score (0-1)
            /// </summary>
            public double Score { get; set; }

            /// <summary>
            /// Components that make up the score
            /// </summary>
            public Dictionary<string, double> ScoreComponents { get; set; } = new Dictionary<string, double>();

            /// <summary>
            /// Whether this procedure is feasible given the current context
            /// </summary>
            public bool IsFeasible { get; set; }

            /// <summary>
            /// If not feasible, reasons why
            /// </summary>
            public List<string> InfeasibilityReasons { get; set; } = new List<string>();
        }

        /// <summary>
        /// Initializes a new instance of the ProcedureMatcher
        /// </summary>
        public ProcedureMatcher(
            IChunkStore chunkStore,
            ChunkActivationService activationService,
            string chunkCollectionId = "default",
            ProcedureMatcherConfig config = null)
        {
            _chunkStore = chunkStore ?? throw new ArgumentNullException(nameof(chunkStore));
            _activationService = activationService ?? throw new ArgumentNullException(nameof(activationService));
            _chunkCollectionId = chunkCollectionId;
            _config = config ?? new ProcedureMatcherConfig();

            _procedureCache = new Dictionary<Guid, ProcedureChunk>();
            _lastCacheUpdate = DateTime.MinValue;
        }

        /// <summary>
        /// Finds procedures that can achieve a specific goal
        /// </summary>
        public async Task<List<ProcedureMatchResult>> FindProceduresForGoalAsync(
            Chunk goalChunk,
            Dictionary<string, object> context = null)
        {
            if (goalChunk == null)
                throw new ArgumentNullException(nameof(goalChunk));

            // Ensure we have up-to-date procedure cache
            if (_config.EnableCaching)
            {
                await UpdateProcedureCacheIfNeededAsync();
            }

            // Get all procedures
            var procedures = await GetAllProceduresAsync();

            // Create evaluation context
            var evaluationContext = new Dictionary<string, object>();

            // Add goal information to context
            AddGoalInfoToContext(goalChunk, evaluationContext);

            // Add additional context if provided
            if (context != null)
            {
                foreach (var item in context)
                {
                    evaluationContext[item.Key] = item.Value;
                }
            }

            // Calculate scores for each procedure
            var results = new List<ProcedureMatchResult>();

            foreach (var procedure in procedures)
            {
                // Skip if doesn't apply to this goal type
                if (!CanProcedureAchieveGoal(procedure, goalChunk))
                    continue;

                var result = EvaluateProcedure(procedure, goalChunk, evaluationContext);

                // Only include if meets minimum score
                if (result.Score >= _config.MinimumMatchScore)
                {
                    results.Add(result);
                }
            }

            // Sort by score (descending) and take top N
            results = results
                .OrderByDescending(r => r.Score)
                .Take(_config.MaxMatchResults)
                .ToList();

            // Boost activation for matched procedures
            await BoostProcedureActivationAsync(results);

            return results;
        }

        /// <summary>
        /// Evaluates a procedure against a goal and context
        /// </summary>
        private ProcedureMatchResult EvaluateProcedure(
            ProcedureChunk procedure,
            Chunk goalChunk,
            Dictionary<string, object> context)
        {
            var result = new ProcedureMatchResult
            {
                Procedure = procedure,
                ScoreComponents = new Dictionary<string, double>(),
                IsFeasible = true
            };

            // Get score components
            double contextRelevance = CalculateContextRelevance(procedure, context);
            double activationScore = CalculateActivationScore(procedure);
            double successProbability = procedure.SuccessProbability;
            double resourceAvailability = CalculateResourceAvailability(procedure, context);
            double utilityScore = procedure.ExpectedUtility;

            // Store score components
            result.ScoreComponents["ContextRelevance"] = contextRelevance;
            result.ScoreComponents["Activation"] = activationScore;
            result.ScoreComponents["SuccessProbability"] = successProbability;
            result.ScoreComponents["ResourceAvailability"] = resourceAvailability;
            result.ScoreComponents["Utility"] = utilityScore;

            // Check if preconditions are satisfied
            bool preconditionsSatisfied = procedure.ArePreconditionsSatisfied(context);
            if (!preconditionsSatisfied)
            {
                result.IsFeasible = false;
                result.InfeasibilityReasons.Add("Preconditions not satisfied");
            }

            // Check resource availability
            if (resourceAvailability < 0.5)
            {
                result.IsFeasible = false;
                result.InfeasibilityReasons.Add("Insufficient resources");
            }

            // Calculate weighted total score
            double totalScore =
                (contextRelevance * _config.ContextRelevanceWeight) +
                (activationScore * _config.ActivationWeight) +
                (successProbability * _config.SuccessProbabilityWeight) +
                (resourceAvailability * _config.ResourceAvailabilityWeight) +
                (utilityScore * _config.UtilityWeight);

            // Reduce score if not feasible
            if (!result.IsFeasible)
            {
                totalScore *= 0.5;
                result.ScoreComponents["FeasibilityPenalty"] = -0.5 * totalScore;
            }

            result.Score = Math.Max(0, Math.Min(1, totalScore));

            return result;
        }

        /// <summary>
        /// Calculates how relevant a procedure is to the current context
        /// </summary>
        private double CalculateContextRelevance(ProcedureChunk procedure, Dictionary<string, object> context)
        {
            // Count how many context factors are relevant to this procedure
            int relevantFactors = 0;
            int matchingFactors = 0;

            // Check preconditions against context
            foreach (var condition in procedure.Preconditions)
            {
                relevantFactors++;
                if (condition.Evaluate(context))
                {
                    matchingFactors++;
                }
            }

            // Check required resources against context
            foreach (var resource in procedure.RequiredResources)
            {
                string resourceKey = $"Resource.{resource.Key}";
                relevantFactors++;

                if (context.ContainsKey(resourceKey) &&
                    CompareValues(context[resourceKey], resource.Value))
                {
                    matchingFactors++;
                }
            }

            if (relevantFactors == 0)
                return 0.5; // Neutral if no relevant factors

            return (double)matchingFactors / relevantFactors;
        }

        /// <summary>
        /// Calculates activation score based on procedure's activation level
        /// </summary>
        private double CalculateActivationScore(ProcedureChunk procedure)
        {
            // Normalize activation level to 0-1 range
            double normalizedActivation = Math.Min(1, procedure.ActivationLevel);

            return normalizedActivation;
        }

        /// <summary>
        /// Calculates resource availability for a procedure
        /// </summary>
        private double CalculateResourceAvailability(ProcedureChunk procedure, Dictionary<string, object> context)
        {
            if (procedure.RequiredResources.Count == 0)
                return 1.0; // No resources required

            int availableResources = 0;

            foreach (var resource in procedure.RequiredResources)
            {
                string resourceKey = $"Resource.{resource.Key}";

                if (context.ContainsKey(resourceKey))
                {
                    availableResources++;
                }
            }

            return (double)availableResources / procedure.RequiredResources.Count;
        }

        /// <summary>
        /// Adds goal information to evaluation context
        /// </summary>
        private void AddGoalInfoToContext(Chunk goalChunk, Dictionary<string, object> context)
        {
            context["Goal.ID"] = goalChunk.ID;
            context["Goal.Name"] = goalChunk.Name;
            context["Goal.Type"] = goalChunk.ChunkType;

            // Add all goal slots
            foreach (var slot in goalChunk.Slots)
            {
                context[$"Goal.{slot.Key}"] = slot.Value?.Value;
            }
        }

        /// <summary>
        /// Checks if a procedure can achieve a goal based on type matching
        /// </summary>
        private bool CanProcedureAchieveGoal(ProcedureChunk procedure, Chunk goalChunk)
        {
            // Get goal type
            string goalType = goalChunk.Name;

            // Check if procedure has this goal type in its applicable types
            return procedure.ApplicableGoalTypes.Contains(goalType);
        }

        /// <summary>
        /// Boosts activation for matched procedures
        /// </summary>
        private async Task BoostProcedureActivationAsync(List<ProcedureMatchResult> matchResults)
        {
            foreach (var result in matchResults)
            {
                // Boost amount based on match score
                double boost = 0.3 + (result.Score * 0.7);

                await _activationService.ActivateChunkAsync(result.Procedure.ID, null, boost);
            }
        }

        /// <summary>
        /// Updates procedure cache if needed
        /// </summary>
        private async Task UpdateProcedureCacheIfNeededAsync()
        {
            if ((DateTime.Now - _lastCacheUpdate) < _cacheExpiration)
                return;

            var chunkCollection = await _chunkStore.GetCollectionAsync(_chunkCollectionId);
            if (chunkCollection == null)
                return;

            var allChunks = await chunkCollection.GetAllChunksAsync();
            var procedureChunks = allChunks.Where(c => c.ChunkType == "Procedure").ToList();

            // Clear existing cache
            _procedureCache.Clear();

            // Populate cache
            foreach (var chunk in procedureChunks)
            {
                // Convert regular chunk to procedure chunk
                var procedureChunk = ConvertToProcedureChunk(chunk);

                if (procedureChunk != null)
                {
                    _procedureCache[chunk.ID] = procedureChunk;
                }
            }

            _lastCacheUpdate = DateTime.Now;
        }

        /// <summary>
        /// Gets all procedure chunks
        /// </summary>
        private async Task<List<ProcedureChunk>> GetAllProceduresAsync()
        {
            if (_config.EnableCaching)
            {
                return _procedureCache.Values.ToList();
            }
            else
            {
                var chunkCollection = await _chunkStore.GetCollectionAsync(_chunkCollectionId);
                if (chunkCollection == null)
                    return new List<ProcedureChunk>();

                var allChunks = await chunkCollection.GetAllChunksAsync();
                var procedureChunks = allChunks.Where(c => c.ChunkType == "Procedure").ToList();

                // Convert regular chunks to procedure chunks
                return procedureChunks.Select(ConvertToProcedureChunk)
                    .Where(p => p != null)
                    .ToList();
            }
        }

        /// <summary>
        /// Converts a regular chunk to a procedure chunk
        /// </summary>
        private ProcedureChunk ConvertToProcedureChunk(Chunk chunk)
        {
            if (chunk == null || chunk.ChunkType != "Procedure")
                return null;

            // In a real implementation, this would deserialize the chunk's slots 
            // into a proper ProcedureChunk with steps, conditions, etc.
            // For now, we'll create a simple wrapper

            var procedureChunk = new ProcedureChunk
            {
                ID = chunk.ID,
                Name = chunk.Name,
                ChunkType = chunk.ChunkType,
                ActivationLevel = chunk.ActivationLevel,
                Vector = chunk.Vector
            };

            // Copy slots
            foreach (var slot in chunk.Slots)
            {
                procedureChunk.Slots[slot.Key] = slot.Value;
            }

            // Set applicable goal types
            if (chunk.Slots.TryGetValue("ApplicableGoalTypes", out var goalTypesSlot) &&
                goalTypesSlot.Value is List<string> goalTypes)
            {
                procedureChunk.ApplicableGoalTypes = goalTypes;
            }
            else if (chunk.Slots.TryGetValue("ApplicableGoalType", out var goalTypeSlot) &&
                     goalTypeSlot.Value is string goalType)
            {
                procedureChunk.ApplicableGoalTypes.Add(goalType);
            }

            // Set procedure type
            if (chunk.Slots.TryGetValue("ProcedureType", out var typeSlot) &&
                typeSlot.Value is string typeString &&
                Enum.TryParse<ProcedureChunk.ProcedureType>(typeString, out var type))
            {
                procedureChunk.Type = type;
            }

            // Set success probability
            if (chunk.Slots.TryGetValue("SuccessProbability", out var probSlot) &&
                probSlot.Value is double probability)
            {
                procedureChunk.SuccessProbability = probability;
            }

            // Set expected utility
            if (chunk.Slots.TryGetValue("ExpectedUtility", out var utilitySlot) &&
                utilitySlot.Value is double utility)
            {
                procedureChunk.ExpectedUtility = utility;
            }

            // Set estimated cost
            if (chunk.Slots.TryGetValue("EstimatedCost", out var costSlot) &&
                costSlot.Value is double cost)
            {
                procedureChunk.EstimatedCost = cost;
            }

            // In a real implementation, we would also deserialize steps, conditions, etc.
            // This is simplified for now

            return procedureChunk;
        }

        /// <summary>
        /// Compares two values for equality or matching
        /// </summary>
        private bool CompareValues(object value1, object value2)
        {
            if (value1 == null && value2 == null)
                return true;

            if (value1 == null || value2 == null)
                return false;

            // Direct equality
            if (value1.Equals(value2))
                return true;

            // Numeric comparison with tolerance
            if (value1 is IConvertible && value2 is IConvertible)
            {
                try
                {
                    double double1 = Convert.ToDouble(value1);
                    double double2 = Convert.ToDouble(value2);

                    return Math.Abs(double1 - double2) < 0.0001;
                }
                catch { }
            }

            // String comparison
            if (value1 is string str1 && value2 is string str2)
            {
                return string.Equals(str1, str2, StringComparison.OrdinalIgnoreCase);
            }

            // Collection comparison
            if (value1 is IEnumerable<object> list1 && value2 is IEnumerable<object> list2)
            {
                return list1.Count() == list2.Count() &&
                       list1.All(item => list2.Contains(item));
            }

            // Default to string representation comparison
            return value1.ToString() == value2.ToString();
        }

        /// <summary>
        /// Gets a procedure by ID
        /// </summary>
        public async Task<ProcedureChunk> GetProcedureByIdAsync(Guid procedureId)
        {
            // Check cache first
            if (_config.EnableCaching && _procedureCache.TryGetValue(procedureId, out var cachedProcedure))
            {
                return cachedProcedure;
            }

            var chunkCollection = await _chunkStore.GetCollectionAsync(_chunkCollectionId);
            if (chunkCollection == null)
                return null;

            var chunk = await chunkCollection.GetChunkAsync(procedureId);
            if (chunk == null || chunk.ChunkType != "Procedure")
                return null;

            return ConvertToProcedureChunk(chunk);
        }

        /// <summary>
        /// Creates a new procedure and adds it to the store
        /// </summary>
        public async Task<ProcedureChunk> CreateProcedureAsync(ProcedureChunk procedure)
        {
            if (procedure == null)
                throw new ArgumentNullException(nameof(procedure));

            var chunkCollection = await _chunkStore.GetCollectionAsync(_chunkCollectionId);
            if (chunkCollection == null)
                throw new InvalidOperationException($"Chunk collection '{_chunkCollectionId}' not found");

            // Ensure it has the correct type
            procedure.ChunkType = "Procedure";

            // Convert to regular chunk for storage
            var chunk = ConvertToRegularChunk(procedure);

            // Add to store
            var savedChunk = await chunkCollection.AddChunkAsync(chunk);

            // Convert back to procedure chunk
            var savedProcedure = ConvertToProcedureChunk(savedChunk);

            // Update cache if enabled
            if (_config.EnableCaching)
            {
                _procedureCache[savedProcedure.ID] = savedProcedure;
                _lastCacheUpdate = DateTime.Now;
            }

            return savedProcedure;
        }

        /// <summary>
        /// Updates an existing procedure
        /// </summary>
        public async Task<bool> UpdateProcedureAsync(ProcedureChunk procedure)
        {
            if (procedure == null)
                throw new ArgumentNullException(nameof(procedure));

            var chunkCollection = await _chunkStore.GetCollectionAsync(_chunkCollectionId);
            if (chunkCollection == null)
                return false;

            // Convert to regular chunk for storage
            var chunk = ConvertToRegularChunk(procedure);

            // Update in store
            bool success = await chunkCollection.UpdateChunkAsync(chunk);

            // Update cache if enabled and successful
            if (success && _config.EnableCaching)
            {
                _procedureCache[procedure.ID] = procedure;
                _lastCacheUpdate = DateTime.Now;
            }

            return success;
        }

        /// <summary>
        /// Converts a procedure chunk to a regular chunk for storage
        /// </summary>
        private Chunk ConvertToRegularChunk(ProcedureChunk procedure)
        {
            var chunk = new Chunk
            {
                ID = procedure.ID,
                ChunkType = "Procedure",
                Name = procedure.Name,
                ActivationLevel = procedure.ActivationLevel,
                Vector = procedure.Vector,
                Slots = new Dictionary<string, ModelSlot>(procedure.Slots)
            };

            // Store applicable goal types
            chunk.Slots["ApplicableGoalTypes"] = new ModelSlot
            {
                Name = "ApplicableGoalTypes",
                Value = procedure.ApplicableGoalTypes
            };

            // Store procedure type
            chunk.Slots["ProcedureType"] = new ModelSlot
            {
                Name = "ProcedureType",
                Value = procedure.Type.ToString()
            };

            // Store success probability
            chunk.Slots["SuccessProbability"] = new ModelSlot
            {
                Name = "SuccessProbability",
                Value = procedure.SuccessProbability
            };

            // Store expected utility
            chunk.Slots["ExpectedUtility"] = new ModelSlot
            {
                Name = "ExpectedUtility",
                Value = procedure.ExpectedUtility
            };

            // Store estimated cost
            chunk.Slots["EstimatedCost"] = new ModelSlot
            {
                Name = "EstimatedCost",
                Value = procedure.EstimatedCost
            };

            // In a real implementation, we would serialize steps, conditions, etc.
            // This is simplified for now

            return chunk;
        }
    }
}