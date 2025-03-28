using System;
using System.Collections.Generic;

namespace Aislinn.Core.Procedural
{
    /// <summary>
    /// Represents a single step within a procedure, including the operation to perform,
    /// parameters, and control flow details.
    /// </summary>
    public class ProcedureStep
    {
        /// <summary>
        /// Types of operations a step can perform
        /// </summary>
        public enum StepType
        {
            Action,         // Perform an action in the world
            Perception,     // Get information from environment
            Calculation,    // Compute a value
            MemoryStore,    // Store something in memory
            MemoryRetrieve, // Retrieve something from memory
            GoalCreate,     // Create a new goal
            GoalModify,     // Modify an existing goal
            Decision,       // Make a decision affecting control flow
            SubProcedure,   // Execute another procedure
            Wait,           // Wait for a condition or time period
            Event           // Handle an event
        }

        /// <summary>
        /// Execution status of this step
        /// </summary>
        public enum StepStatus
        {
            NotStarted,
            InProgress,
            Completed,
            Failed,
            Skipped
        }

        /// <summary>
        /// Unique identifier for this step within the procedure
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Descriptive name of this step
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Type of operation for this step
        /// </summary>
        public StepType Type { get; set; } = StepType.Action;

        /// <summary>
        /// Current execution status
        /// </summary>
        public StepStatus Status { get; set; } = StepStatus.NotStarted;

        /// <summary>
        /// The operation to perform (method name, action name, etc.)
        /// </summary>
        public string Operation { get; set; }

        /// <summary>
        /// Parameters for the operation
        /// </summary>
        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Expected result reference (where to store the result)
        /// </summary>
        public string ResultReference { get; set; }

        /// <summary>
        /// Preconditions specific to this step
        /// </summary>
        public List<ProcedureCondition> Preconditions { get; set; } = new List<ProcedureCondition>();

        /// <summary>
        /// Expected effects of this step
        /// </summary>
        public List<ProcedureEffect> Effects { get; set; } = new List<ProcedureEffect>();

        /// <summary>
        /// Next step ID for unconditional flow
        /// </summary>
        public string NextStepId { get; set; }

        /// <summary>
        /// Conditional branches based on step outcome
        /// </summary>
        public List<StepBranch> Branches { get; set; } = new List<StepBranch>();

        /// <summary>
        /// Optional timeout for this step
        /// </summary>
        public TimeSpan? Timeout { get; set; }

        /// <summary>
        /// Retry policy for failed execution
        /// </summary>
        public StepRetryPolicy RetryPolicy { get; set; }

        /// <summary>
        /// Additional metadata for this step
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Creates a new step with a default ID
        /// </summary>
        public ProcedureStep()
        {
            Id = Guid.NewGuid().ToString("N");
        }

        /// <summary>
        /// Creates a new step with specific name and operation
        /// </summary>
        public ProcedureStep(string name, string operation, StepType type = StepType.Action)
        {
            Id = Guid.NewGuid().ToString("N");
            Name = name;
            Operation = operation;
            Type = type;
        }

        /// <summary>
        /// Evaluates if this step can be executed given the current context
        /// </summary>
        public bool CanExecute(Dictionary<string, object> context)
        {
            // Check all preconditions
            foreach (var condition in Preconditions)
            {
                if (!condition.Evaluate(context))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Determines the next step ID based on execution result
        /// </summary>
        public string GetNextStepId(bool success, Dictionary<string, object> context)
        {
            // If failed and no branches handle failure, return null (let procedure handle)
            if (!success)
            {
                foreach (var branch in Branches)
                {
                    if (!branch.RequiresSuccess && branch.Condition.Evaluate(context))
                    {
                        return branch.TargetStepId;
                    }
                }
                return null;
            }

            // Check conditional branches
            foreach (var branch in Branches)
            {
                if (branch.RequiresSuccess && branch.Condition.Evaluate(context))
                {
                    return branch.TargetStepId;
                }
            }

            // Default to sequential next step
            return NextStepId;
        }

        /// <summary>
        /// Applies the effects of this step to the context
        /// </summary>
        public void ApplyEffects(Dictionary<string, object> context)
        {
            foreach (var effect in Effects)
            {
                effect.Apply(context);
            }
        }

        /// <summary>
        /// Resets the status of this step
        /// </summary>
        public void Reset()
        {
            Status = StepStatus.NotStarted;
        }

        /// <summary>
        /// Creates a clone of this step
        /// </summary>
        public ProcedureStep Clone()
        {
            var clone = new ProcedureStep
            {
                Id = Id,
                Name = Name,
                Type = Type,
                Status = StepStatus.NotStarted,
                Operation = Operation,
                Parameters = new Dictionary<string, object>(Parameters),
                ResultReference = ResultReference,
                NextStepId = NextStepId,
                Timeout = Timeout,
                Metadata = new Dictionary<string, object>(Metadata)
            };

            // Deep copy preconditions
            foreach (var condition in Preconditions)
            {
                clone.Preconditions.Add(condition.Clone());
            }

            // Deep copy effects
            foreach (var effect in Effects)
            {
                clone.Effects.Add(effect.Clone());
            }

            // Deep copy branches
            foreach (var branch in Branches)
            {
                clone.Branches.Add(branch.Clone());
            }

            // Copy retry policy if exists
            if (RetryPolicy != null)
            {
                clone.RetryPolicy = RetryPolicy.Clone();
            }

            return clone;
        }
    }

    /// <summary>
    /// Represents a conditional branch in procedure control flow
    /// </summary>
    public class StepBranch
    {
        /// <summary>
        /// Condition to evaluate for this branch
        /// </summary>
        public ProcedureCondition Condition { get; set; }

        /// <summary>
        /// Target step ID if condition is true
        /// </summary>
        public string TargetStepId { get; set; }

        /// <summary>
        /// Whether this branch requires the step to have succeeded
        /// </summary>
        public bool RequiresSuccess { get; set; } = true;

        /// <summary>
        /// Creates a clone of this branch
        /// </summary>
        public StepBranch Clone()
        {
            return new StepBranch
            {
                Condition = Condition?.Clone(),
                TargetStepId = TargetStepId,
                RequiresSuccess = RequiresSuccess
            };
        }
    }

    /// <summary>
    /// Retry policy for handling step failures
    /// </summary>
    public class StepRetryPolicy
    {
        /// <summary>
        /// Maximum number of retry attempts
        /// </summary>
        public int MaxRetries { get; set; } = 3;

        /// <summary>
        /// Delay between retry attempts
        /// </summary>
        public TimeSpan RetryDelay { get; set; } = TimeSpan.FromMilliseconds(500);

        /// <summary>
        /// Whether to apply exponential backoff to the retry delay
        /// </summary>
        public bool UseExponentialBackoff { get; set; } = false;

        /// <summary>
        /// Current retry count during execution
        /// </summary>
        public int CurrentRetryCount { get; set; } = 0;

        /// <summary>
        /// Creates a clone of this retry policy
        /// </summary>
        public StepRetryPolicy Clone()
        {
            return new StepRetryPolicy
            {
                MaxRetries = MaxRetries,
                RetryDelay = RetryDelay,
                UseExponentialBackoff = UseExponentialBackoff,
                CurrentRetryCount = 0 // Reset for new execution
            };
        }

        /// <summary>
        /// Calculate delay for next retry attempt
        /// </summary>
        public TimeSpan GetNextRetryDelay()
        {
            if (!UseExponentialBackoff || CurrentRetryCount <= 1)
                return RetryDelay;

            // Apply exponential backoff
            double factor = Math.Pow(2, CurrentRetryCount - 1);
            double delayMs = RetryDelay.TotalMilliseconds * factor;

            // Cap at a reasonable maximum
            const double maxDelayMs = 30000; // 30 seconds
            return TimeSpan.FromMilliseconds(Math.Min(delayMs, maxDelayMs));
        }
    }
}