using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Aislinn.ChunkStorage.Interfaces;
using Aislinn.Core.Context;
using Aislinn.Core.Goals;
using Aislinn.Core.Models;
using Aislinn.Core.Services;

namespace Aislinn.Core.Goals.Selection
{
    /// <summary>
    /// Service responsible for selecting which goals should be actively pursued
    /// based on multiple factors including priority, activation, and context.
    /// </summary>
    public class GoalSelectionService : IDisposable
    {
        // Dependencies
        private readonly IChunkStore _chunkStore;
        private readonly IAssociationStore _associationStore;
        private readonly GoalManagementService _goalManagementService;
        private readonly ChunkActivationService _activationService;
        private readonly ContextContainer _contextContainer;

        // Configuration
        private readonly string _chunkCollectionId;
        private readonly string _associationCollectionId;
        private readonly GoalSelectionConfig _config;

        // State
        private List<GoalEvaluationResult> _lastEvaluationResults;
        private DateTime _lastEvaluationTime;
        private bool _isDisposed = false;

        // Selected goals tracking
        private Guid? _primaryGoalId;
        private List<Guid> _secondaryGoalIds = new List<Guid>();
        private Dictionary<Guid, DateTime> _goalSelectionTimes = new Dictionary<Guid, DateTime>();

        // Auto-evaluation timer
        private System.Timers.Timer _evaluationTimer;
        private bool _autoEvaluationEnabled = false;
        private double _evaluationIntervalMs = 1000; // Default 1 second

        /// <summary>
        /// Event raised when goal selection changes
        /// </summary>
        public event EventHandler<GoalSelectionChangedEventArgs> GoalSelectionChanged;

        /// <summary>
        /// Event args for goal selection changes
        /// </summary>
        public class GoalSelectionChangedEventArgs : EventArgs
        {
            public Guid? PreviousPrimaryGoalId { get; set; }
            public Guid? NewPrimaryGoalId { get; set; }
            public List<Guid> PreviousSecondaryGoalIds { get; set; }
            public List<Guid> NewSecondaryGoalIds { get; set; }
            public string ChangeReason { get; set; }
            public DateTime Timestamp { get; set; } = DateTime.Now;
        }

        /// <summary>
        /// Contains the evaluation result for a single goal
        /// </summary>
        public class GoalEvaluationResult
        {
            public Guid GoalId { get; set; }
            public string GoalName { get; set; }
            public string GoalStatus { get; set; }
            public double TotalScore { get; set; }
            public Dictionary<string, double> ScoreComponents { get; set; }
            public Dictionary<string, object> EvaluationContext { get; set; }
            public DateTime EvaluationTime { get; set; }
            public bool IsFeasible { get; set; }
            public List<string> BlockingFactors { get; set; }
            public double EstimatedUtility { get; set; }
            public double EstimatedCost { get; set; }
            public TimeSpan EstimatedDuration { get; set; }
        }

        /// <summary>
        /// Configuration options for goal selection
        /// </summary>
        public class GoalSelectionConfig
        {
            // Weight factors for different components
            public double PriorityWeight { get; set; } = 0.3;
            public double ActivationWeight { get; set; } = 0.25;
            public double ContextRelevanceWeight { get; set; } = 0.2;
            public double UtilityWeight { get; set; } = 0.15;
            public double ProgressWeight { get; set; } = 0.1;

            // Goal switching settings
            public double MinScoreDifferenceForSwitch { get; set; } = 0.2;
            public TimeSpan MinGoalFocusTime { get; set; } = TimeSpan.FromSeconds(5);
            public double SwitchingPenalty { get; set; } = 0.15;

            // Selection thresholds
            public double MinimumActivationForConsideration { get; set; } = 0.1;
            public double MinimumScoreForPrimaryGoal { get; set; } = 0.3;
            public double MinimumScoreForSecondaryGoal { get; set; } = 0.2;

            // Max number of secondary goals
            public int MaxSecondaryGoals { get; set; } = 3;

            // Context change sensitivity (0-1)
            public double ContextChangeSensitivity { get; set; } = 0.4;
        }

        /// <summary>
        /// Initializes a new instance of the GoalSelectionService
        /// </summary>
        public GoalSelectionService(
            IChunkStore chunkStore,
            IAssociationStore associationStore,
            GoalManagementService goalManagementService,
            ChunkActivationService activationService,
            ContextContainer contextContainer,
            string chunkCollectionId = "default",
            string associationCollectionId = "default",
            GoalSelectionConfig config = null)
        {
            _chunkStore = chunkStore ?? throw new ArgumentNullException(nameof(chunkStore));
            _associationStore = associationStore ?? throw new ArgumentNullException(nameof(associationStore));
            _goalManagementService = goalManagementService ?? throw new ArgumentNullException(nameof(goalManagementService));
            _activationService = activationService ?? throw new ArgumentNullException(nameof(activationService));
            _contextContainer = contextContainer ?? throw new ArgumentNullException(nameof(contextContainer));

            _chunkCollectionId = chunkCollectionId;
            _associationCollectionId = associationCollectionId;
            _config = config ?? new GoalSelectionConfig();

            _lastEvaluationResults = new List<GoalEvaluationResult>();
            _lastEvaluationTime = DateTime.MinValue;

            // Subscribe to context changes
            _contextContainer.SignificantContextChange += OnSignificantContextChange;
        }

        #region Evaluation Timer Management

        /// <summary>
        /// Starts automatic goal evaluation on a timer
        /// </summary>
        public void StartAutoEvaluation(double intervalMs = 1000)
        {
            if (_evaluationTimer == null)
            {
                _evaluationTimer = new System.Timers.Timer(intervalMs);
                _evaluationTimer.Elapsed += async (sender, e) => await EvaluateGoalsAsync();
                _evaluationTimer.AutoReset = true;
            }

            _evaluationIntervalMs = intervalMs;
            _evaluationTimer.Interval = intervalMs;
            _evaluationTimer.Enabled = true;
            _autoEvaluationEnabled = true;
        }

        /// <summary>
        /// Stops automatic goal evaluation
        /// </summary>
        public void StopAutoEvaluation()
        {
            if (_evaluationTimer != null)
            {
                _evaluationTimer.Enabled = false;
            }
            _autoEvaluationEnabled = false;
        }

        /// <summary>
        /// Handles significant context changes by potentially triggering goal re-evaluation
        /// </summary>
        private async void OnSignificantContextChange(object sender, ContextContainer.ContextChangeEventArgs e)
        {
            // Only react if the change is significant enough
            if (e.ChangeSignificance >= _config.ContextChangeSensitivity)
            {
                // If auto-evaluation is enabled, it will handle this soon anyway
                if (!_autoEvaluationEnabled)
                {
                    await EvaluateGoalsAsync(contextChangeReason: $"Context change: {e.Category}.{e.FactorName}");
                }
            }
        }

        #endregion

        #region Goal Evaluation

        /// <summary>
        /// Evaluates all eligible goals and selects which to pursue
        /// </summary>
        public async Task<List<GoalEvaluationResult>> EvaluateGoalsAsync(
            Dictionary<string, object> additionalContext = null,
            string contextChangeReason = null)
        {
            var chunkCollection = await _chunkStore.GetCollectionAsync(_chunkCollectionId);
            if (chunkCollection == null)
                return new List<GoalEvaluationResult>();

            // Get all chunks of type "Goal" that aren't templates and aren't completed/abandoned
            var allChunks = await chunkCollection.GetAllChunksAsync();
            var eligibleGoals = allChunks.Where(c =>
                c.ChunkType == "Goal" &&
                !IsGoalTemplate(c) &&
                !IsGoalCompleted(c) &&
                c.ActivationLevel >= _config.MinimumActivationForConsideration).ToList();

            // Create evaluation context
            var evaluationContext = new Dictionary<string, object>();

            // Add current context snapshot
            var contextSnapshot = _contextContainer.CreateContextSnapshot();
            foreach (var category in contextSnapshot.Keys)
            {
                foreach (var factor in contextSnapshot[category])
                {
                    evaluationContext[$"Context.{category}.{factor.Key}"] = factor.Value;
                }
            }

            // Add additional context if provided
            if (additionalContext != null)
            {
                foreach (var item in additionalContext)
                {
                    evaluationContext[item.Key] = item.Value;
                }
            }

            // Evaluate each goal
            var evaluationResults = new List<GoalEvaluationResult>();
            foreach (var goal in eligibleGoals)
            {
                var result = await EvaluateGoalAsync(goal, evaluationContext);
                evaluationResults.Add(result);
            }

            // Sort by total score (descending)
            evaluationResults = evaluationResults
                .OrderByDescending(r => r.TotalScore)
                .ToList();

            _lastEvaluationResults = evaluationResults;
            _lastEvaluationTime = DateTime.Now;

            // Select goals based on evaluation results
            await SelectGoalsAsync(evaluationResults, contextChangeReason);

            return evaluationResults;
        }

        /// <summary>
        /// Evaluates a specific set of goals
        /// </summary>
        public async Task<List<GoalEvaluationResult>> EvaluateSpecificGoalsAsync(
            List<Guid> goalIds,
            Dictionary<string, object> additionalContext = null)
        {
            if (goalIds == null || !goalIds.Any())
                return new List<GoalEvaluationResult>();

            var chunkCollection = await _chunkStore.GetCollectionAsync(_chunkCollectionId);
            if (chunkCollection == null)
                return new List<GoalEvaluationResult>();

            // Create evaluation context
            var evaluationContext = new Dictionary<string, object>();

            // Add current context snapshot
            var contextSnapshot = _contextContainer.CreateContextSnapshot();
            foreach (var category in contextSnapshot.Keys)
            {
                foreach (var factor in contextSnapshot[category])
                {
                    evaluationContext[$"Context.{category}.{factor.Key}"] = factor.Value;
                }
            }

            // Add additional context if provided
            if (additionalContext != null)
            {
                foreach (var item in additionalContext)
                {
                    evaluationContext[item.Key] = item.Value;
                }
            }

            // Evaluate each goal
            var evaluationResults = new List<GoalEvaluationResult>();
            foreach (var goalId in goalIds)
            {
                var goal = await chunkCollection.GetChunkAsync(goalId);
                if (goal != null && goal.ChunkType == "Goal" && !IsGoalCompleted(goal))
                {
                    var result = await EvaluateGoalAsync(goal, evaluationContext);
                    evaluationResults.Add(result);
                }
            }

            // Sort by total score (descending)
            evaluationResults = evaluationResults
                .OrderByDescending(r => r.TotalScore)
                .ToList();

            return evaluationResults;
        }

        /// <summary>
        /// Evaluates a single goal against the current context
        /// </summary>
        private async Task<GoalEvaluationResult> EvaluateGoalAsync(
            Chunk goal,
            Dictionary<string, object> evaluationContext)
        {
            var result = new GoalEvaluationResult
            {
                GoalId = goal.ID,
                GoalName = goal.Name,
                GoalStatus = GetGoalStatus(goal),
                ScoreComponents = new Dictionary<string, double>(),
                EvaluationContext = new Dictionary<string, object>(evaluationContext),
                EvaluationTime = DateTime.Now,
                BlockingFactors = new List<string>(),
                IsFeasible = true
            };

            // Calculate base scores for different factors
            double priorityScore = CalculatePriorityScore(goal);
            double activationScore = CalculateActivationScore(goal);
            double contextRelevanceScore = CalculateContextRelevanceScore(goal);
            double utilityScore = await CalculateUtilityScoreAsync(goal);
            double progressScore = await CalculateProgressScoreAsync(goal);

            // Store individual components
            result.ScoreComponents["Priority"] = priorityScore;
            result.ScoreComponents["Activation"] = activationScore;
            result.ScoreComponents["ContextRelevance"] = contextRelevanceScore;
            result.ScoreComponents["Utility"] = utilityScore;
            result.ScoreComponents["Progress"] = progressScore;

            // Store utility and cost estimates
            result.EstimatedUtility = GetEstimatedUtility(goal);
            result.EstimatedCost = GetEstimatedCost(goal);
            result.EstimatedDuration = GetEstimatedDuration(goal);

            // Check if goal is currently blocked
            if (IsGoalBlocked(goal))
            {
                result.IsFeasible = false;
                result.BlockingFactors.Add("Dependencies not satisfied");
            }

            // Check if goal has necessary resources
            if (!await HasNecessaryResourcesAsync(goal))
            {
                result.IsFeasible = false;
                result.BlockingFactors.Add("Missing required resources");
            }

            // Calculate switching penalty if this would be a goal switch
            double switchingPenalty = 0;
            if (_primaryGoalId.HasValue && _primaryGoalId.Value != goal.ID)
            {
                // Check how long we've been on the current goal
                if (_goalSelectionTimes.TryGetValue(_primaryGoalId.Value, out DateTime selectionTime))
                {
                    TimeSpan timeSinceSelection = DateTime.Now - selectionTime;
                    if (timeSinceSelection < _config.MinGoalFocusTime)
                    {
                        // Apply penalty if we haven't focused on the current goal long enough
                        double remainingRatio = 1.0 - (timeSinceSelection.TotalMilliseconds / _config.MinGoalFocusTime.TotalMilliseconds);
                        switchingPenalty = _config.SwitchingPenalty * remainingRatio;
                        result.ScoreComponents["SwitchingPenalty"] = -switchingPenalty;
                    }
                }
            }

            // Calculate weighted total score
            double totalScore =
                (priorityScore * _config.PriorityWeight) +
                (activationScore * _config.ActivationWeight) +
                (contextRelevanceScore * _config.ContextRelevanceWeight) +
                (utilityScore * _config.UtilityWeight) +
                (progressScore * _config.ProgressWeight) -
                switchingPenalty;

            // Reduce score if not feasible
            if (!result.IsFeasible)
            {
                totalScore *= 0.25; // Significant reduction but not zero
                result.ScoreComponents["FeasibilityPenalty"] = -0.75 * totalScore;
            }

            result.TotalScore = Math.Max(0, Math.Min(1, totalScore)); // Clamp to 0-1

            return result;
        }

        /// <summary>
        /// Calculate priority score based on goal's explicit priority
        /// </summary>
        private double CalculatePriorityScore(Chunk goal)
        {
            double priority = 0.5; // Default middle priority

            if (goal.Slots.TryGetValue(GoalManagementService.GoalSlots.Priority, out var prioritySlot) &&
                prioritySlot.Value != null)
            {
                // Try convert to double
                if (prioritySlot.Value is double doubleValue)
                {
                    priority = doubleValue;
                }
                else if (double.TryParse(prioritySlot.Value.ToString(), out double parsedValue))
                {
                    priority = parsedValue;
                }
            }

            // Normalize to 0-1 range if outside
            return Math.Max(0, Math.Min(1, priority));
        }

        /// <summary>
        /// Calculate activation score based on goal's activation level
        /// </summary>
        private double CalculateActivationScore(Chunk goal)
        {
            // Normalize activation level to 0-1 range
            // Assuming activation is already in a reasonable range
            double normalizedActivation = Math.Min(1, goal.ActivationLevel);

            return normalizedActivation;
        }

        /// <summary>
        /// Calculate context relevance score based on how well the goal fits current context
        /// </summary>
        private double CalculateContextRelevanceScore(Chunk goal)
        {
            // Use context container to calculate relevance
            return _contextContainer.CalculateContextRelevance(goal);
        }

        /// <summary>
        /// Calculate utility score based on potential reward/benefit of goal
        /// </summary>
        private async Task<double> CalculateUtilityScoreAsync(Chunk goal)
        {
            // Get estimated utility from goal (or parent goal)
            double utility = GetEstimatedUtility(goal);

            // If no explicit utility, check if there's an association with a utility value
            if (utility <= 0)
            {
                var associationCollection = await _associationStore.GetCollectionAsync(_associationCollectionId);
                if (associationCollection != null)
                {
                    var associations = await associationCollection.GetAssociationsForChunkAsync(goal.ID);

                    // Look for associations with "Utility" in the relation name
                    foreach (var association in associations)
                    {
                        if (association.RelationAtoB?.Contains("Utility") == true)
                        {
                            utility = association.WeightAtoB;
                            break;
                        }
                    }
                }
            }

            return Math.Max(0, Math.Min(1, utility));
        }

        /// <summary>
        /// Calculate progress score based on how far along the goal is
        /// </summary>
        private async Task<double> CalculateProgressScoreAsync(Chunk goal)
        {
            // Check for explicit progress indicator in goal
            if (goal.Slots.TryGetValue("Progress", out var progressSlot) &&
                progressSlot.Value != null)
            {
                if (progressSlot.Value is double doubleValue)
                {
                    return Math.Max(0, Math.Min(1, doubleValue));
                }

                if (double.TryParse(progressSlot.Value.ToString(), out double parsedValue))
                {
                    return Math.Max(0, Math.Min(1, parsedValue));
                }
            }

            // For hierarchical goals, check subgoal completion
            double subgoalProgress = await CalculateSubgoalProgressAsync(goal.ID);
            if (subgoalProgress >= 0)
            {
                return subgoalProgress;
            }

            // Default progress for non-hierarchical goals with no explicit progress
            return 0;
        }

        /// <summary>
        /// Calculate progress based on subgoal completion
        /// </summary>
        private async Task<double> CalculateSubgoalProgressAsync(Guid goalId)
        {
            var associationCollection = await _associationStore.GetCollectionAsync(_associationCollectionId);
            if (associationCollection == null)
                return -1;

            var chunkCollection = await _chunkStore.GetCollectionAsync(_chunkCollectionId);
            if (chunkCollection == null)
                return -1;

            // Find all subgoals
            var associations = await associationCollection.GetAssociationsForChunkAsync(goalId);
            var subgoalAssociations = associations.Where(a => a.RelationAtoB == "HasSubgoal").ToList();

            if (!subgoalAssociations.Any())
                return -1; // No subgoals

            int totalSubgoals = subgoalAssociations.Count;
            int completedSubgoals = 0;

            foreach (var association in subgoalAssociations)
            {
                var subgoalId = association.ChunkBId;
                var subgoal = await chunkCollection.GetChunkAsync(subgoalId);

                if (subgoal != null && subgoal.Slots.TryGetValue(GoalManagementService.GoalSlots.Status, out var statusSlot) &&
                    statusSlot.Value?.ToString() == GoalManagementService.GoalSlots.StatusValues.Completed)
                {
                    completedSubgoals++;
                }
            }

            return (double)completedSubgoals / totalSubgoals;
        }

        /// <summary>
        /// Gets the estimated utility value from a goal
        /// </summary>
        private double GetEstimatedUtility(Chunk goal)
        {
            if (goal.Slots.TryGetValue("EstimatedUtility", out var utilitySlot) &&
                utilitySlot.Value != null)
            {
                if (utilitySlot.Value is double doubleValue)
                {
                    return doubleValue;
                }

                if (double.TryParse(utilitySlot.Value.ToString(), out double parsedValue))
                {
                    return parsedValue;
                }
            }

            return 0.5; // Default medium utility
        }

        /// <summary>
        /// Gets the estimated cost value from a goal
        /// </summary>
        private double GetEstimatedCost(Chunk goal)
        {
            if (goal.Slots.TryGetValue("EstimatedCost", out var costSlot) &&
                costSlot.Value != null)
            {
                if (costSlot.Value is double doubleValue)
                {
                    return doubleValue;
                }

                if (double.TryParse(costSlot.Value.ToString(), out double parsedValue))
                {
                    return parsedValue;
                }
            }

            return 0.5; // Default medium cost
        }

        /// <summary>
        /// Gets the estimated duration from a goal
        /// </summary>
        private TimeSpan GetEstimatedDuration(Chunk goal)
        {
            if (goal.Slots.TryGetValue("EstimatedDuration", out var durationSlot) &&
                durationSlot.Value != null)
            {
                if (durationSlot.Value is TimeSpan timeSpan)
                {
                    return timeSpan;
                }

                if (durationSlot.Value is double seconds)
                {
                    return TimeSpan.FromSeconds(seconds);
                }

                if (double.TryParse(durationSlot.Value.ToString(), out double parsedSeconds))
                {
                    return TimeSpan.FromSeconds(parsedSeconds);
                }
            }

            return TimeSpan.FromMinutes(5); // Default 5 minutes
        }

        /// <summary>
        /// Check if a goal has all necessary resources for execution
        /// </summary>
        private async Task<bool> HasNecessaryResourcesAsync(Chunk goal)
        {
            // Check if goal has required resources specified
            if (!goal.Slots.TryGetValue("RequiredResources", out var resourcesSlot) ||
                resourcesSlot.Value == null)
            {
                return true; // No resources required
            }

            // Handle different ways resources might be specified
            if (resourcesSlot.Value is List<object> resourceList)
            {
                foreach (var resource in resourceList)
                {
                    // Check if resource is available based on context
                    if (resource is string resourceName)
                    {
                        if (!_contextContainer.HasRecentContextFactor(
                            ContextContainer.ContextCategory.Resource, resourceName))
                        {
                            return false;
                        }
                    }
                }
            }
            else if (resourcesSlot.Value is Dictionary<string, object> resourceDict)
            {
                foreach (var resource in resourceDict)
                {
                    var factor = _contextContainer.GetContextFactor(
                        ContextContainer.ContextCategory.Resource, resource.Key);

                    if (factor == null)
                        return false;

                    // Compare values if specific requirements
                    if (resource.Value != null &&
                        !CompareValues(factor.Value, resource.Value))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        #endregion

        #region Goal Selection

        /// <summary>
        /// Selects which goals to pursue based on evaluation results
        /// </summary>
        private async Task SelectGoalsAsync(List<GoalEvaluationResult> evaluationResults, string contextChangeReason = null)
        {
            if (evaluationResults == null || !evaluationResults.Any())
                return;

            Guid? previousPrimaryGoalId = _primaryGoalId;
            List<Guid> previousSecondaryGoalIds = new List<Guid>(_secondaryGoalIds);
            bool selectionChanged = false;
            string changeReason = contextChangeReason ?? "Regular evaluation";

            // Find highest scoring feasible goal
            var topFeasibleGoals = evaluationResults
                .Where(r => r.IsFeasible)
                .OrderByDescending(r => r.TotalScore)
                .ToList();

            // If no feasible goals, look at non-feasible goals
            if (!topFeasibleGoals.Any())
            {
                topFeasibleGoals = evaluationResults
                    .OrderByDescending(r => r.TotalScore)
                    .ToList();
            }

            // Select primary goal
            if (topFeasibleGoals.Any() && topFeasibleGoals[0].TotalScore >= _config.MinimumScoreForPrimaryGoal)
            {
                var topGoal = topFeasibleGoals[0];

                // Check if this would be a change in primary goal
                if (!_primaryGoalId.HasValue || _primaryGoalId.Value != topGoal.GoalId)
                {
                    // Check if the score difference is significant enough for a switch
                    bool shouldSwitch = !_primaryGoalId.HasValue;

                    if (_primaryGoalId.HasValue)
                    {
                        // Find current primary goal in results
                        var currentGoalResult = evaluationResults.FirstOrDefault(r => r.GoalId == _primaryGoalId.Value);

                        if (currentGoalResult != null)
                        {
                            double scoreDifference = topGoal.TotalScore - currentGoalResult.TotalScore;
                            shouldSwitch = scoreDifference >= _config.MinScoreDifferenceForSwitch;

                            // Also switch if current goal is no longer feasible and new one is
                            if (!currentGoalResult.IsFeasible && topGoal.IsFeasible)
                            {
                                shouldSwitch = true;
                                changeReason = "Current goal no longer feasible";
                            }
                        }
                        else
                        {
                            // Current goal not in results (maybe completed or removed)
                            shouldSwitch = true;
                            changeReason = "Current goal no longer available";
                        }
                    }

                    if (shouldSwitch)
                    {
                        _primaryGoalId = topGoal.GoalId;
                        _goalSelectionTimes[topGoal.GoalId] = DateTime.Now;
                        selectionChanged = true;
                    }
                }
            }
            else if (_primaryGoalId.HasValue)
            {
                // No suitable goals found, clear primary goal
                _primaryGoalId = null;
                selectionChanged = true;
                changeReason = "No suitable primary goal";
            }

            // Select secondary goals
            var newSecondaryGoals = new List<Guid>();

            foreach (var result in topFeasibleGoals.Skip(1).Take(_config.MaxSecondaryGoals))
            {
                if (result.TotalScore >= _config.MinimumScoreForSecondaryGoal)
                {
                    newSecondaryGoals.Add(result.GoalId);

                    // Update selection time if new
                    if (!_secondaryGoalIds.Contains(result.GoalId))
                    {
                        _goalSelectionTimes[result.GoalId] = DateTime.Now;
                    }
                }
            }

            // Check if secondary goals changed
            if (!newSecondaryGoals.SequenceEqual(_secondaryGoalIds))
            {
                _secondaryGoalIds = newSecondaryGoals;
                selectionChanged = true;
            }

            // Raise event if selection changed
            if (selectionChanged && GoalSelectionChanged != null)
            {
                GoalSelectionChanged(this, new GoalSelectionChangedEventArgs
                {
                    PreviousPrimaryGoalId = previousPrimaryGoalId,
                    NewPrimaryGoalId = _primaryGoalId,
                    PreviousSecondaryGoalIds = previousSecondaryGoalIds,
                    NewSecondaryGoalIds = _secondaryGoalIds,
                    ChangeReason = changeReason
                });

                // Boost activation for selected goals
                await BoostSelectedGoalActivationAsync();
            }
        }

        /// <summary>
        /// Boosts activation for selected goals
        /// </summary>
        private async Task BoostSelectedGoalActivationAsync()
        {
            if (_primaryGoalId.HasValue)
            {
                await _activationService.ActivateChunkAsync(_primaryGoalId.Value, null, 1.0);
            }

            foreach (var goalId in _secondaryGoalIds)
            {
                await _activationService.ActivateChunkAsync(goalId, null, 0.5);
            }
        }

        /// <summary>
        /// Gets the currently selected primary goal
        /// </summary>
        public async Task<Chunk> GetPrimaryGoalAsync()
        {
            if (!_primaryGoalId.HasValue)
                return null;

            var chunkCollection = await _chunkStore.GetCollectionAsync(_chunkCollectionId);
            if (chunkCollection == null)
                return null;

            return await chunkCollection.GetChunkAsync(_primaryGoalId.Value);
        }

        /// <summary>
        /// Gets the currently selected secondary goals
        /// </summary>
        public async Task<List<Chunk>> GetSecondaryGoalsAsync()
        {
            var chunkCollection = await _chunkStore.GetCollectionAsync(_chunkCollectionId);
            if (chunkCollection == null)
                return new List<Chunk>();

            var result = new List<Chunk>();

            foreach (var goalId in _secondaryGoalIds)
            {
                var goal = await chunkCollection.GetChunkAsync(goalId);
                if (goal != null)
                {
                    result.Add(goal);
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the last evaluation results
        /// </summary>
        public List<GoalEvaluationResult> GetLastEvaluationResults()
        {
            return new List<GoalEvaluationResult>(_lastEvaluationResults);
        }

        /// <summary>
        /// Forces a specific goal to be the primary goal
        /// </summary>
        public async Task<bool> ForceGoalSelectionAsync(Guid goalId)
        {
            var chunkCollection = await _chunkStore.GetCollectionAsync(_chunkCollectionId);
            if (chunkCollection == null)
                return false;

            var goal = await chunkCollection.GetChunkAsync(goalId);
            if (goal == null || goal.ChunkType != "Goal" || IsGoalCompleted(goal))
                return false;

            Guid? previousPrimaryGoalId = _primaryGoalId;

            _primaryGoalId = goalId;
            _goalSelectionTimes[goalId] = DateTime.Now;

            // Raise event
            if (GoalSelectionChanged != null)
            {
                GoalSelectionChanged(this, new GoalSelectionChangedEventArgs
                {
                    PreviousPrimaryGoalId = previousPrimaryGoalId,
                    NewPrimaryGoalId = goalId,
                    PreviousSecondaryGoalIds = new List<Guid>(_secondaryGoalIds),
                    NewSecondaryGoalIds = _secondaryGoalIds,
                    ChangeReason = "Forced selection"
                });
            }

            // Boost activation
            await _activationService.ActivateChunkAsync(goalId, null, 1.0);

            return true;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Checks if a chunk is a goal template
        /// </summary>
        private bool IsGoalTemplate(Chunk chunk)
        {
            return chunk.ChunkType == "Goal" &&
                   chunk.Slots.TryGetValue(GoalManagementService.GoalSlots.IsTemplate, out var isTemplateSlot) &&
                   isTemplateSlot.Value is bool isTemplate &&
                   isTemplate;
        }

        /// <summary>
        /// Checks if a goal is completed or abandoned
        /// </summary>
        private bool IsGoalCompleted(Chunk goal)
        {
            if (goal.Slots.TryGetValue(GoalManagementService.GoalSlots.Status, out var statusSlot) &&
                statusSlot.Value != null)
            {
                string status = statusSlot.Value.ToString();
                return status == GoalManagementService.GoalSlots.StatusValues.Completed ||
                       status == GoalManagementService.GoalSlots.StatusValues.Abandoned;
            }

            return false;
        }

        /// <summary>
        /// Checks if a goal is blocked by dependencies
        /// </summary>
        private bool IsGoalBlocked(Chunk goal)
        {
            if (goal.Slots.TryGetValue(GoalManagementService.GoalSlots.Status, out var statusSlot) &&
                statusSlot.Value != null)
            {
                string status = statusSlot.Value.ToString();
                return status == GoalManagementService.GoalSlots.StatusValues.Blocked;
            }

            // Check dependencies
            if (goal.Slots.TryGetValue(GoalManagementService.GoalSlots.Dependencies, out var depsSlot) &&
                depsSlot.Value is Dictionary<Guid, bool> dependencies &&
                dependencies.Any())
            {
                // If any dependency is not satisfied, the goal is blocked
                return dependencies.Values.Any(v => !v);
            }

            return false;
        }

        /// <summary>
        /// Gets the status of a goal
        /// </summary>
        private string GetGoalStatus(Chunk goal)
        {
            if (goal.Slots.TryGetValue(GoalManagementService.GoalSlots.Status, out var statusSlot) &&
                statusSlot.Value != null)
            {
                return statusSlot.Value.ToString();
            }

            return GoalManagementService.GoalSlots.StatusValues.Pending;
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

        #endregion

        /// <summary>
        /// Disposes resources
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
                return;

            // Unsubscribe from events
            _contextContainer.SignificantContextChange -= OnSignificantContextChange;

            // Dispose timer
            if (_evaluationTimer != null)
            {
                _evaluationTimer.Dispose();
                _evaluationTimer = null;
            }

            _isDisposed = true;
        }
    }
}