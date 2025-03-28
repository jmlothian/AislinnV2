using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Aislinn.ChunkStorage.Interfaces;
using Aislinn.Core.Models;
using Aislinn.Core.Services;
using Aislinn.Core.Relationships;

namespace Aislinn.Core.Goals
{
    /// <summary>
    /// Service for managing goals, goal templates, and instantiation within the cognitive architecture
    /// </summary>
    public class GoalManagementService
    {
        private readonly IChunkStore _chunkStore;
        private readonly IAssociationStore _associationStore;
        private readonly ChunkActivationService _activationService;
        private readonly RelationshipTraversalService _relationshipService;
        private readonly GoalRelationshipMatcher _relationshipMatcher;
        private readonly string _chunkCollectionId;
        private readonly string _associationCollectionId;

        // Constants for slot names used in goal chunks
        public static class GoalSlots
        {
            // Common Goal Properties
            public const string Type = "ChunkType";
            public const string Name = "Name";
            public const string Priority = "Priority";
            public const string Status = "Status";
            public const string ParentGoal = "ParentGoal";
            public const string SubGoals = "SubGoals";
            public const string Dependencies = "Dependencies";
            public const string CompletionCriteria = "CompletionCriteria";

            // Template-specific Properties
            public const string IsTemplate = "IsTemplate";
            public const string RequiredParameters = "RequiredParameters";
            public const string OptionalParameters = "OptionalParameters";
            public const string ParameterDefaults = "ParameterDefaults";
            public const string ParameterConstraints = "ParameterConstraints";

            // Goal status values
            public static class StatusValues
            {
                public const string Pending = "Pending";
                public const string Active = "Active";
                public const string Blocked = "Blocked";
                public const string InProgress = "InProgress";
                public const string Completed = "Completed";
                public const string Failed = "Failed";
                public const string Abandoned = "Abandoned";
            }
        }

        public GoalManagementService(
            IChunkStore chunkStore,
            IAssociationStore associationStore,
            ChunkActivationService activationService,
            RelationshipTraversalService relationshipService,
            string chunkCollectionId = "default",
            string associationCollectionId = "default")
        {
            _chunkStore = chunkStore ?? throw new ArgumentNullException(nameof(chunkStore));
            _associationStore = associationStore ?? throw new ArgumentNullException(nameof(associationStore));
            _activationService = activationService ?? throw new ArgumentNullException(nameof(activationService));
            _relationshipService = relationshipService ?? throw new ArgumentNullException(nameof(relationshipService));
            _chunkCollectionId = chunkCollectionId;
            _associationCollectionId = associationCollectionId;

            // Initialize the relationship matcher
            _relationshipMatcher = new GoalRelationshipMatcher(
                _chunkStore,
                _relationshipService,
                _chunkCollectionId);
        }

        /// <summary>
        /// Creates a new goal template with parameter specifications
        /// </summary>
        public async Task<Chunk> CreateGoalTemplateAsync(
            string name,
            double defaultPriority,
            List<string> requiredParameters,
            List<string> optionalParameters = null,
            Dictionary<string, object> parameterDefaults = null,
            Dictionary<string, string> parameterConstraints = null)
        {
            var chunkCollection = await _chunkStore.GetCollectionAsync(_chunkCollectionId);
            if (chunkCollection == null)
                throw new InvalidOperationException($"Chunk collection '{_chunkCollectionId}' not found");

            // Create the template goal chunk
            var templateChunk = new Chunk
            {
                ChunkType = "Goal",
                Name = name,
                ActivationLevel = 0.0 // Templates start with zero activation
            };

            // Add standard goal slots
            templateChunk.Slots[GoalSlots.IsTemplate] = new ModelSlot { Name = GoalSlots.IsTemplate, Value = true };
            templateChunk.Slots[GoalSlots.Name] = new ModelSlot { Name = GoalSlots.Name, Value = name };
            templateChunk.Slots[GoalSlots.Priority] = new ModelSlot { Name = GoalSlots.Priority, Value = defaultPriority };
            templateChunk.Slots[GoalSlots.Status] = new ModelSlot { Name = GoalSlots.Status, Value = GoalSlots.StatusValues.Pending };

            // Add template-specific slots
            templateChunk.Slots[GoalSlots.RequiredParameters] = new ModelSlot
            {
                Name = GoalSlots.RequiredParameters,
                Value = requiredParameters ?? new List<string>()
            };

            templateChunk.Slots[GoalSlots.OptionalParameters] = new ModelSlot
            {
                Name = GoalSlots.OptionalParameters,
                Value = optionalParameters ?? new List<string>()
            };

            templateChunk.Slots[GoalSlots.ParameterDefaults] = new ModelSlot
            {
                Name = GoalSlots.ParameterDefaults,
                Value = parameterDefaults ?? new Dictionary<string, object>()
            };

            templateChunk.Slots[GoalSlots.ParameterConstraints] = new ModelSlot
            {
                Name = GoalSlots.ParameterConstraints,
                Value = parameterConstraints ?? new Dictionary<string, string>()
            };

            // Add the template to the chunk store
            return await chunkCollection.AddChunkAsync(templateChunk);
        }

        /// <summary>
        /// Instantiates a goal from a template with specific parameter values
        /// </summary>
        public async Task<Chunk> InstantiateGoalAsync(
            Guid templateId,
            Dictionary<string, object> parameters,
            Guid? parentGoalId = null)
        {
            var chunkCollection = await _chunkStore.GetCollectionAsync(_chunkCollectionId);
            if (chunkCollection == null)
                throw new InvalidOperationException($"Chunk collection '{_chunkCollectionId}' not found");

            // Get the template
            var template = await chunkCollection.GetChunkAsync(templateId);
            if (template == null)
                throw new ArgumentException($"Template with ID {templateId} not found", nameof(templateId));

            // Verify it's a template
            if (!IsGoalTemplate(template))
                throw new ArgumentException($"Chunk with ID {templateId} is not a goal template", nameof(templateId));

            // Validate required parameters
            var requiredParams = GetRequiredParameters(template);
            var missingParams = requiredParams.Where(p => !parameters.ContainsKey(p)).ToList();

            if (missingParams.Any())
                throw new ArgumentException($"Missing required parameters: {string.Join(", ", missingParams)}", nameof(parameters));

            // Apply defaults for optional parameters
            var optionalParams = GetOptionalParameters(template);
            var defaults = GetParameterDefaults(template);

            foreach (var param in optionalParams)
            {
                if (!parameters.ContainsKey(param) && defaults.ContainsKey(param))
                {
                    parameters[param] = defaults[param];
                }
            }

            // Create a new instance
            var instance = new Chunk
            {
                ChunkType = "Goal",
                Name = template.Name,
                ActivationLevel = 0.1 // New goals start with low activation
            };

            // Add standard goal slots
            instance.Slots[GoalSlots.IsTemplate] = new ModelSlot { Name = GoalSlots.IsTemplate, Value = false };
            instance.Slots[GoalSlots.Name] = new ModelSlot { Name = GoalSlots.Name, Value = template.Name };

            // Copy the default priority from the template
            if (template.Slots.TryGetValue(GoalSlots.Priority, out var prioritySlot))
            {
                instance.Slots[GoalSlots.Priority] = new ModelSlot
                {
                    Name = GoalSlots.Priority,
                    Value = prioritySlot.Value
                };
            }

            instance.Slots[GoalSlots.Status] = new ModelSlot
            {
                Name = GoalSlots.Status,
                Value = GoalSlots.StatusValues.Pending
            };

            // Add parent goal reference if provided
            if (parentGoalId.HasValue)
            {
                instance.Slots[GoalSlots.ParentGoal] = new ModelSlot
                {
                    Name = GoalSlots.ParentGoal,
                    Value = parentGoalId.Value
                };
            }

            // Add all provided parameters as slots
            foreach (var param in parameters)
            {
                instance.Slots[param.Key] = new ModelSlot
                {
                    Name = param.Key,
                    Value = param.Value
                };
            }

            // Add the instance to the chunk store
            var savedInstance = await chunkCollection.AddChunkAsync(instance);

            // If this is a subgoal, create association with parent
            if (parentGoalId.HasValue)
            {
                await _associationStore.GetOrCreateCollectionAsync(_associationCollectionId);

                await _activationService.CreateAssociationAsync(
                    parentGoalId.Value,
                    savedInstance.ID,
                    "HasSubgoal",
                    "IsSubgoalOf",
                    0.8, // Parent to child weight
                    0.5  // Child to parent weight
                );
            }

            return savedInstance;
        }

        /// <summary>
        /// Creates a dependency relationship between goals
        /// </summary>
        public async Task CreateGoalDependencyAsync(Guid dependentGoalId, Guid prerequisiteGoalId)
        {
            var chunkCollection = await _chunkStore.GetCollectionAsync(_chunkCollectionId);
            if (chunkCollection == null)
                throw new InvalidOperationException($"Chunk collection '{_chunkCollectionId}' not found");

            // Get both goals
            var dependentGoal = await chunkCollection.GetChunkAsync(dependentGoalId);
            var prerequisiteGoal = await chunkCollection.GetChunkAsync(prerequisiteGoalId);

            if (dependentGoal == null)
                throw new ArgumentException($"Dependent goal with ID {dependentGoalId} not found", nameof(dependentGoalId));

            if (prerequisiteGoal == null)
                throw new ArgumentException($"Prerequisite goal with ID {prerequisiteGoalId} not found", nameof(prerequisiteGoalId));

            // Add dependency to dependent goal's Dependencies slot
            Dictionary<Guid, bool> dependencies;

            if (dependentGoal.Slots.TryGetValue(GoalSlots.Dependencies, out var depsSlot) &&
                depsSlot.Value is Dictionary<Guid, bool> existingDeps)
            {
                dependencies = existingDeps;
            }
            else
            {
                dependencies = new Dictionary<Guid, bool>();
            }

            // Add or update the dependency (initially unsatisfied)
            dependencies[prerequisiteGoalId] = false;

            dependentGoal.Slots[GoalSlots.Dependencies] = new ModelSlot
            {
                Name = GoalSlots.Dependencies,
                Value = dependencies
            };

            // Update the dependent goal
            await chunkCollection.UpdateChunkAsync(dependentGoal);

            // Create an association between the goals
            await _activationService.CreateAssociationAsync(
                dependentGoalId,
                prerequisiteGoalId,
                "DependsOn",
                "IsPrerequisiteFor",
                0.7, // Strong dependent to prerequisite connection
                0.3  // Weaker prerequisite to dependent connection
            );
        }

        /// <summary>
        /// Updates the status of a goal and handles dependent goals
        /// </summary>
        public async Task UpdateGoalStatusAsync(Guid goalId, string newStatus)
        {
            var chunkCollection = await _chunkStore.GetCollectionAsync(_chunkCollectionId);
            if (chunkCollection == null)
                throw new InvalidOperationException($"Chunk collection '{_chunkCollectionId}' not found");

            var associationCollection = await _associationStore.GetCollectionAsync(_associationCollectionId);
            if (associationCollection == null)
                throw new InvalidOperationException($"Association collection '{_associationCollectionId}' not found");

            // Get the goal
            var goal = await chunkCollection.GetChunkAsync(goalId);
            if (goal == null)
                throw new ArgumentException($"Goal with ID {goalId} not found", nameof(goalId));

            // Update the status
            goal.Slots[GoalSlots.Status] = new ModelSlot { Name = GoalSlots.Status, Value = newStatus };
            await chunkCollection.UpdateChunkAsync(goal);

            // If completed, update dependent goals
            if (newStatus == GoalSlots.StatusValues.Completed)
            {
                // Find goals that depend on this one
                var associations = await associationCollection.GetAssociationsForChunkAsync(goalId);
                foreach (var association in associations)
                {
                    if (association.RelationBtoA == "DependsOn")
                    {
                        // This goal is a prerequisite for another goal
                        Guid dependentGoalId = association.ChunkAId;
                        var dependentGoal = await chunkCollection.GetChunkAsync(dependentGoalId);

                        if (dependentGoal != null)
                        {
                            // Update the dependency status
                            if (dependentGoal.Slots.TryGetValue(GoalSlots.Dependencies, out var depsSlot) &&
                                depsSlot.Value is Dictionary<Guid, bool> dependencies)
                            {
                                dependencies[goalId] = true;
                                dependentGoal.Slots[GoalSlots.Dependencies] = new ModelSlot
                                {
                                    Name = GoalSlots.Dependencies,
                                    Value = dependencies
                                };

                                // Check if all dependencies are satisfied
                                bool allSatisfied = dependencies.Values.All(v => v);

                                // If all dependencies are satisfied and this goal is blocked, activate it
                                if (allSatisfied && dependentGoal.Slots.TryGetValue(GoalSlots.Status, out var statusSlot) &&
                                    statusSlot.Value?.ToString() == GoalSlots.StatusValues.Blocked)
                                {
                                    dependentGoal.Slots[GoalSlots.Status] = new ModelSlot
                                    {
                                        Name = GoalSlots.Status,
                                        Value = GoalSlots.StatusValues.Active
                                    };
                                }

                                await chunkCollection.UpdateChunkAsync(dependentGoal);

                                // Increase activation of the dependent goal now that a dependency is satisfied
                                await _activationService.ActivateChunkAsync(dependentGoalId, null, 0.5);
                            }
                        }
                    }
                }

                // If this goal has a parent, update parent goal's completion progress
                if (goal.Slots.TryGetValue(GoalSlots.ParentGoal, out var parentSlot) &&
                    parentSlot.Value is Guid parentId)
                {
                    await _activationService.ActivateChunkAsync(parentId, null, 0.3);
                    await CheckParentGoalCompletionAsync(parentId);
                }
            }
        }

        /// <summary>
        /// Creates a goal with multiple possible fulfillment paths
        /// </summary>
        public async Task<Chunk> CreateMultiPathGoalAsync(
            string name,
            double priority,
            List<object> completionPaths)
        {
            var chunkCollection = await _chunkStore.GetCollectionAsync(_chunkCollectionId);
            if (chunkCollection == null)
                throw new InvalidOperationException($"Chunk collection '{_chunkCollectionId}' not found");

            // Create the goal chunk
            var goal = new Chunk
            {
                ChunkType = "Goal",
                Name = name,
                ActivationLevel = 0.1 // Start with low activation
            };

            // Add standard goal slots
            goal.Slots[GoalSlots.IsTemplate] = new ModelSlot { Name = GoalSlots.IsTemplate, Value = false };
            goal.Slots[GoalSlots.Name] = new ModelSlot { Name = GoalSlots.Name, Value = name };
            goal.Slots[GoalSlots.Priority] = new ModelSlot { Name = GoalSlots.Priority, Value = priority };
            goal.Slots[GoalSlots.Status] = new ModelSlot { Name = GoalSlots.Status, Value = GoalSlots.StatusValues.Pending };

            // Add completion criteria as a list of alternative paths
            goal.Slots[GoalSlots.CompletionCriteria] = new ModelSlot
            {
                Name = GoalSlots.CompletionCriteria,
                Value = completionPaths
            };

            // Add the goal to the chunk store
            return await chunkCollection.AddChunkAsync(goal);
        }

        /// <summary>
        /// Checks if a parent goal should be marked as completed based on its subgoals
        /// </summary>
        private async Task CheckParentGoalCompletionAsync(Guid parentGoalId)
        {
            var chunkCollection = await _chunkStore.GetCollectionAsync(_chunkCollectionId);
            var associationCollection = await _associationStore.GetCollectionAsync(_associationCollectionId);

            if (chunkCollection == null || associationCollection == null)
                return;

            var parentGoal = await chunkCollection.GetChunkAsync(parentGoalId);
            if (parentGoal == null)
                return;

            // Find all subgoals
            var associations = await associationCollection.GetAssociationsForChunkAsync(parentGoalId);
            var subgoalAssociations = associations.Where(a => a.RelationAtoB == "HasSubgoal").ToList();

            if (!subgoalAssociations.Any())
                return;

            bool allSubgoalsCompleted = true;

            foreach (var association in subgoalAssociations)
            {
                var subgoalId = association.ChunkBId;
                var subgoal = await chunkCollection.GetChunkAsync(subgoalId);

                if (subgoal == null)
                    continue;

                if (!subgoal.Slots.TryGetValue(GoalSlots.Status, out var statusSlot) ||
                    statusSlot.Value?.ToString() != GoalSlots.StatusValues.Completed)
                {
                    allSubgoalsCompleted = false;
                    break;
                }
            }

            // If all subgoals are completed, mark the parent as completed
            if (allSubgoalsCompleted)
            {
                await UpdateGoalStatusAsync(parentGoalId, GoalSlots.StatusValues.Completed);
            }
        }

        /// <summary>
        /// Evaluates all active goals and updates their activation levels based on priority and context
        /// </summary>
        public async Task EvaluateGoalActivationsAsync(double contextBoost = 0.2)
        {
            var chunkCollection = await _chunkStore.GetCollectionAsync(_chunkCollectionId);
            if (chunkCollection == null)
                return;

            var allChunks = await chunkCollection.GetAllChunksAsync();
            var goalChunks = allChunks.Where(c => c.ChunkType == "Goal" && !IsGoalTemplate(c)).ToList();

            foreach (var goal in goalChunks)
            {
                // Skip completed or abandoned goals
                if (goal.Slots.TryGetValue(GoalSlots.Status, out var statusSlot))
                {
                    string status = statusSlot.Value?.ToString();
                    if (status == GoalSlots.StatusValues.Completed || status == GoalSlots.StatusValues.Abandoned)
                        continue;
                }

                // Check if the goal is completed before proceeding
                bool isCompleted = await EvaluateGoalCompletionAsync(goal);
                if (isCompleted)
                {
                    continue; // Skip further processing for completed goals
                }

                // Get priority
                double priority = 0.5; // Default priority
                if (goal.Slots.TryGetValue(GoalSlots.Priority, out var prioritySlot) &&
                    prioritySlot.Value is double priorityValue)
                {
                    priority = priorityValue;
                }

                // Check if dependencies are satisfied
                bool dependenciesSatisfied = true;
                if (goal.Slots.TryGetValue(GoalSlots.Dependencies, out var depsSlot) &&
                    depsSlot.Value is Dictionary<Guid, bool> dependencies &&
                    dependencies.Any())
                {
                    dependenciesSatisfied = dependencies.Values.All(v => v);

                    // If blocked by dependencies, update status
                    if (!dependenciesSatisfied &&
                        (!goal.Slots.TryGetValue(GoalSlots.Status, out statusSlot) ||
                         statusSlot.Value?.ToString() != GoalSlots.StatusValues.Blocked))
                    {
                        goal.Slots[GoalSlots.Status] = new ModelSlot
                        {
                            Name = GoalSlots.Status,
                            Value = GoalSlots.StatusValues.Blocked
                        };
                        await chunkCollection.UpdateChunkAsync(goal);
                    }
                }

                // Calculate activation boost based on priority and dependency status
                double boost = priority;

                // Reduce activation if dependencies aren't satisfied
                if (!dependenciesSatisfied)
                {
                    boost *= 0.25; // Significant reduction for blocked goals
                }

                // Apply contextual boost (would be more sophisticated in a full implementation)
                boost += contextBoost;

                // Activate the goal with calculated boost
                if (boost > 0)
                {
                    await _activationService.ActivateChunkAsync(goal.ID, null, boost);
                }
            }
        }

        /// <summary>
        /// Evaluates if a goal's completion criteria are met and updates its status accordingly
        /// </summary>
        public async Task<bool> EvaluateGoalCompletionAsync(Chunk goal)
        {
            if (goal == null || goal.ChunkType != "Goal")
                return false;

            var chunkCollection = await _chunkStore.GetCollectionAsync(_chunkCollectionId);
            if (chunkCollection == null)
                return false;

            // If the goal is already completed, return true
            if (goal.Slots.TryGetValue(GoalSlots.Status, out var statusSlot) &&
                statusSlot.Value?.ToString() == GoalSlots.StatusValues.Completed)
            {
                return true;
            }

            // Get completion criteria
            if (!goal.Slots.TryGetValue(GoalSlots.CompletionCriteria, out var criteriaSlot) ||
                criteriaSlot.Value == null)
            {
                // No explicit completion criteria, so check if all subgoals are completed
                return await CheckSubgoalCompletionAsync(goal.ID);
            }

            // Handle different types of completion criteria
            if (criteriaSlot.Value is Dictionary<string, object> criteriaDictionary)
            {
                bool allCriteriaMet = await EvaluateDictionaryCriteriaAsync(criteriaDictionary, goal);

                if (allCriteriaMet)
                {
                    // Update the status to completed
                    await UpdateGoalStatusAsync(goal.ID, GoalSlots.StatusValues.Completed);
                    return true;
                }
            }
            else if (criteriaSlot.Value is string criteriaExpression)
            {
                // Check if this contains relationship patterns
                bool criteriaMet = await EvaluateExpressionWithRelationshipsAsync(criteriaExpression, goal);

                if (criteriaMet)
                {
                    // Update the status to completed
                    await UpdateGoalStatusAsync(goal.ID, GoalSlots.StatusValues.Completed);
                    return true;
                }
            }
            else if (criteriaSlot.Value is Guid criteriaChunkId)
            {
                // The criteria is a reference to another chunk (like a condition checker)
                bool criteriaMet = await EvaluateChunkCriteriaAsync(criteriaChunkId, goal);

                if (criteriaMet)
                {
                    // Update the status to completed
                    await UpdateGoalStatusAsync(goal.ID, GoalSlots.StatusValues.Completed);
                    return true;
                }
            }
            else if (criteriaSlot.Value is List<object> criteriaList)
            {
                // Handle multi-path fulfillment - where ANY of the criteria can satisfy the goal
                foreach (var criterion in criteriaList)
                {
                    bool criteriaMet = false;

                    if (criterion is Dictionary<string, object> dictCriterion)
                    {
                        criteriaMet = await EvaluateDictionaryCriteriaAsync(dictCriterion, goal);
                    }
                    else if (criterion is string strCriterion)
                    {
                        criteriaMet = await EvaluateExpressionWithRelationshipsAsync(strCriterion, goal);
                    }
                    else if (criterion is Guid guidCriterion)
                    {
                        criteriaMet = await EvaluateChunkCriteriaAsync(guidCriterion, goal);
                    }

                    if (criteriaMet)
                    {
                        // Update the status to completed
                        await UpdateGoalStatusAsync(goal.ID, GoalSlots.StatusValues.Completed);
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Evaluates completion criteria expressed as a dictionary of conditions
        /// </summary>
        private async Task<bool> EvaluateDictionaryCriteriaAsync(Dictionary<string, object> criteria, Chunk goal)
        {
            foreach (var criterion in criteria)
            {
                string slotName = criterion.Key;
                object expectedValue = criterion.Value;

                // Check if the slot exists and matches the expected value
                if (!goal.Slots.TryGetValue(slotName, out var slot) ||
                    slot.Value == null ||
                    !ValueEquals(slot.Value, expectedValue))
                {
                    return false;
                }
            }

            return true; // All criteria met
        }

        /// <summary>
        /// Evaluates a relationship-aware expression
        /// </summary>
        private async Task<bool> EvaluateExpressionWithRelationshipsAsync(string expression, Chunk goal)
        {
            // Handle OR expressions first (highest level of precedence in our parsing)
            if (expression.Contains(" OR "))
            {
                var orExpressions = expression.Split(new[] { " OR " }, StringSplitOptions.None);

                // If any OR expression is true, the whole criteria is met
                foreach (var orExpression in orExpressions)
                {
                    if (await EvaluateExpressionWithRelationshipsAsync(orExpression.Trim(), goal))
                    {
                        return true;
                    }
                }

                return false; // None of the OR conditions were met
            }

            // Process AND expressions
            var conditions = expression.Split(new[] { " AND " }, StringSplitOptions.None);

            foreach (var condition in conditions)
            {
                string trimmedCondition = condition.Trim();

                // Check if this looks like a relationship pattern (has exactly two spaces)
                var parts = trimmedCondition.Split(' ');
                if (parts.Length == 3)
                {
                    // Try to evaluate as a relationship pattern
                    bool relationshipResult = await _relationshipMatcher.EvaluateRelationshipPatternAsync(
                        trimmedCondition, goal);

                    if (!relationshipResult)
                    {
                        // If it failed as a relationship pattern, try as a standard condition
                        bool standardResult = await EvaluateExpressionConditionAsync(trimmedCondition, goal);
                        if (!standardResult)
                            return false;
                    }
                }
                else
                {
                    // Standard condition
                    bool standardResult = await EvaluateExpressionConditionAsync(trimmedCondition, goal);
                    if (!standardResult)
                        return false;
                }
            }

            return true; // All conditions passed
        }

        /// <summary>
        /// Evaluates a chunk-based criteria
        /// </summary>
        private async Task<bool> EvaluateChunkCriteriaAsync(Guid criteriaChunkId, Chunk goal)
        {
            var chunkCollection = await _chunkStore.GetCollectionAsync(_chunkCollectionId);
            if (chunkCollection == null)
                return false;

            var criteriaChunk = await chunkCollection.GetChunkAsync(criteriaChunkId);
            if (criteriaChunk == null)
                return false;

            // The criteria chunk should have a "Check" slot with an evaluation function
            // or a "Condition" slot with a logical expression

            // For now, we'll just check if it has a "Result" slot with a boolean value
            if (criteriaChunk.Slots.TryGetValue("Result", out var resultSlot) &&
                resultSlot.Value is bool result)
            {
                return result;
            }

            // Or it could have a "Condition" slot with an expression
            if (criteriaChunk.Slots.TryGetValue("Condition", out var conditionSlot) &&
                conditionSlot.Value is string conditionExpression)
            {
                return await EvaluateExpressionWithRelationshipsAsync(conditionExpression, goal);
            }

            return false;
        }

        /// <summary>
        /// Checks if all subgoals of a goal are completed
        /// </summary>
        private async Task<bool> CheckSubgoalCompletionAsync(Guid goalId)
        {
            var associationCollection = await _associationStore.GetCollectionAsync(_associationCollectionId);
            if (associationCollection == null)
                return false;

            var chunkCollection = await _chunkStore.GetCollectionAsync(_chunkCollectionId);
            if (chunkCollection == null)
                return false;

            // Find all subgoals
            var associations = await associationCollection.GetAssociationsForChunkAsync(goalId);
            var subgoalAssociations = associations.Where(a => a.RelationAtoB == "HasSubgoal").ToList();

            // If there are no subgoals, return false (no evidence of completion)
            if (!subgoalAssociations.Any())
                return false;

            foreach (var association in subgoalAssociations)
            {
                var subgoalId = association.ChunkBId;
                var subgoal = await chunkCollection.GetChunkAsync(subgoalId);

                if (subgoal == null)
                    continue;

                if (!subgoal.Slots.TryGetValue(GoalSlots.Status, out var statusSlot) ||
                    statusSlot.Value?.ToString() != GoalSlots.StatusValues.Completed)
                {
                    return false; // At least one subgoal is not completed
                }
            }

            // All subgoals are completed, so update the parent goal status
            await UpdateGoalStatusAsync(goalId, GoalSlots.StatusValues.Completed);
            return true;
        }

        /// <summary>
        /// Evaluates a single condition in an expression
        /// </summary>
        private async Task<bool> EvaluateExpressionConditionAsync(string condition, Chunk goal)
        {
            // Handle different comparison operators
            if (condition.Contains(">="))
            {
                var parts = condition.Split(new[] { ">=" }, StringSplitOptions.None);
                if (parts.Length != 2) return false;

                string slotName = parts[0].Trim();
                string valueStr = parts[1].Trim();

                if (!goal.Slots.TryGetValue(slotName, out var slot) || slot.Value == null)
                    return false;

                // Try to convert both to double for comparison
                if (!TryConvertToDouble(slot.Value, out double slotValue) ||
                    !TryConvertToDouble(valueStr, out double comparisonValue) ||
                    !(slotValue >= comparisonValue))
                {
                    return false;
                }
            }
            else if (condition.Contains("<="))
            {
                // Similar implementation for <= operator
                var parts = condition.Split(new[] { "<=" }, StringSplitOptions.None);
                if (parts.Length != 2) return false;

                string slotName = parts[0].Trim();
                string valueStr = parts[1].Trim();

                if (!goal.Slots.TryGetValue(slotName, out var slot) || slot.Value == null)
                    return false;

                if (!TryConvertToDouble(slot.Value, out double slotValue) ||
                    !TryConvertToDouble(valueStr, out double comparisonValue) ||
                    !(slotValue <= comparisonValue))
                {
                    return false;
                }
            }
            else if (condition.Contains("=="))
            {
                var parts = condition.Split(new[] { "==" }, StringSplitOptions.None);
                if (parts.Length != 2) return false;

                string slotName = parts[0].Trim();
                string valueStr = parts[1].Trim();

                if (!goal.Slots.TryGetValue(slotName, out var slot) || slot.Value == null)
                    return false;

                // Handle string comparison with quoted strings
                if (valueStr.StartsWith("\"") && valueStr.EndsWith("\""))
                {
                    string stringValue = valueStr.Substring(1, valueStr.Length - 2); // Remove quotes
                    if (slot.Value.ToString() != stringValue)
                        return false;
                }
                // Handle boolean
                else if (valueStr.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                         valueStr.Equals("false", StringComparison.OrdinalIgnoreCase))
                {
                    bool boolValue = valueStr.Equals("true", StringComparison.OrdinalIgnoreCase);
                    if (!(slot.Value is bool slotBool) || slotBool != boolValue)
                        return false;
                }
                // Handle numeric comparison
                else if (TryConvertToDouble(valueStr, out double numericValue) &&
                         TryConvertToDouble(slot.Value, out double slotNumericValue))
                {
                    if (Math.Abs(slotNumericValue - numericValue) > 0.00001)
                        return false;
                }
                else
                {
                    // Default string comparison
                    if (slot.Value.ToString() != valueStr)
                        return false;
                }
            }
            else if (condition.Contains("!="))
            {
                var parts = condition.Split(new[] { "!=" }, StringSplitOptions.None);
                if (parts.Length != 2) return false;

                string slotName = parts[0].Trim();
                string valueStr = parts[1].Trim();

                if (!goal.Slots.TryGetValue(slotName, out var slot) || slot.Value == null)
                    return true; // If slot doesn't exist, "not equals" is technically true

                // Handle string comparison with quoted strings
                if (valueStr.StartsWith("\"") && valueStr.EndsWith("\""))
                {
                    string stringValue = valueStr.Substring(1, valueStr.Length - 2); // Remove quotes
                    if (slot.Value.ToString() == stringValue)
                        return false;
                }
                // Handle boolean
                else if (valueStr.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                         valueStr.Equals("false", StringComparison.OrdinalIgnoreCase))
                {
                    bool boolValue = valueStr.Equals("true", StringComparison.OrdinalIgnoreCase);
                    if (slot.Value is bool slotBool && slotBool == boolValue)
                        return false;
                }
                // Handle numeric comparison
                else if (TryConvertToDouble(valueStr, out double numericValue) &&
                         TryConvertToDouble(slot.Value, out double slotNumericValue))
                {
                    if (Math.Abs(slotNumericValue - numericValue) <= 0.00001)
                        return false;
                }
                else
                {
                    // Default string comparison
                    if (slot.Value.ToString() == valueStr)
                        return false;
                }
            }
            // Add other operators (>, <) as needed

            return true; // If we get here, the condition is satisfied (or wasn't recognized)
        }

        /// <summary>
        /// Helper to compare two values for equality, handling various types
        /// </summary>
        private bool ValueEquals(object value1, object value2)
        {
            if (value1 == null && value2 == null)
                return true;

            if (value1 == null || value2 == null)
                return false;

            // Convert numeric types for comparison
            if (IsNumeric(value1) && IsNumeric(value2))
            {
                double double1 = Convert.ToDouble(value1);
                double double2 = Convert.ToDouble(value2);
                return Math.Abs(double1 - double2) < 0.00001; // Small epsilon for floating point comparison
            }

            // Compare booleans
            if (value1 is bool && value2 is bool)
                return (bool)value1 == (bool)value2;

            // Compare strings
            return value1.ToString() == value2.ToString();
        }

        /// <summary>
        /// Helper to determine if an object is a numeric type
        /// </summary>
        private bool IsNumeric(object value)
        {
            return value is sbyte || value is byte || value is short || value is ushort ||
                   value is int || value is uint || value is long || value is ulong ||
                   value is float || value is double || value is decimal;
        }

        /// <summary>
        /// Helper to try converting an object to a double
        /// </summary>
        private bool TryConvertToDouble(object value, out double result)
        {
            try
            {
                if (value is double doubleValue)
                {
                    result = doubleValue;
                    return true;
                }

                if (IsNumeric(value))
                {
                    result = Convert.ToDouble(value);
                    return true;
                }

                if (value is string stringValue)
                {
                    return double.TryParse(stringValue, out result);
                }

                result = 0;
                return false;
            }
            catch
            {
                result = 0;
                return false;
            }
        }

        #region Helper Methods

        private bool IsGoalTemplate(Chunk chunk)
        {
            return chunk.ChunkType == "Goal" &&
                   chunk.Slots.TryGetValue(GoalSlots.IsTemplate, out var isTemplateSlot) &&
                   isTemplateSlot.Value is bool isTemplate &&
                   isTemplate;
        }

        private List<string> GetRequiredParameters(Chunk template)
        {
            if (template.Slots.TryGetValue(GoalSlots.RequiredParameters, out var slot) &&
                slot.Value is List<string> parameters)
            {
                return parameters;
            }
            return new List<string>();
        }

        private List<string> GetOptionalParameters(Chunk template)
        {
            if (template.Slots.TryGetValue(GoalSlots.OptionalParameters, out var slot) &&
                slot.Value is List<string> parameters)
            {
                return parameters;
            }
            return new List<string>();
        }

        private Dictionary<string, object> GetParameterDefaults(Chunk template)
        {
            if (template.Slots.TryGetValue(GoalSlots.ParameterDefaults, out var slot) &&
                slot.Value is Dictionary<string, object> defaults)
            {
                return defaults;
            }
            return new Dictionary<string, object>();
        }

        #endregion
    }
}