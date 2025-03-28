using System;
using System.Collections.Generic;
using Aislinn.Core.Models;

namespace Aislinn.Core.Procedural
{
    /// <summary>
    /// Represents procedural knowledge as a specialized chunk type containing
    /// execution steps, control flow, and metadata about when and how to apply the procedure.
    /// </summary>
    public class ProcedureChunk : Chunk
    {
        /// <summary>
        /// The type of procedure represented
        /// </summary>
        public enum ProcedureType
        {
            Action,         // Single atomic action
            Sequence,       // Sequential series of steps
            Conditional,    // Contains conditional branching
            Iterative,      // Contains loops or repeated steps
            Hierarchical,   // Contains subtasks that are procedures themselves
            Reactive        // Responds to events during execution
        }

        /// <summary>
        /// The execution status of a procedure
        /// </summary>
        public enum ExecutionStatus
        {
            NotStarted,
            InProgress,
            Paused,
            Completed,
            Failed,
            Aborted
        }

        /// <summary>
        /// Type of procedure
        /// </summary>
        public ProcedureType Type { get; set; } = ProcedureType.Sequence;

        /// <summary>
        /// Goal type(s) this procedure can fulfill
        /// </summary>
        public List<string> ApplicableGoalTypes { get; set; } = new List<string>();

        /// <summary>
        /// Current execution status when this procedure is active
        /// </summary>
        public ExecutionStatus Status { get; set; } = ExecutionStatus.NotStarted;

        /// <summary>
        /// The ordered sequence of steps for this procedure
        /// </summary>
        public List<ProcedureStep> Steps { get; set; } = new List<ProcedureStep>();

        /// <summary>
        /// Index of the current step being executed (when active)
        /// </summary>
        public int CurrentStepIndex { get; set; } = 0;

        /// <summary>
        /// Preconditions that must be satisfied for this procedure to be applicable
        /// </summary>
        public List<ProcedureCondition> Preconditions { get; set; } = new List<ProcedureCondition>();

        /// <summary>
        /// Expected postconditions/effects when this procedure completes successfully
        /// </summary>
        public List<ProcedureEffect> ExpectedEffects { get; set; } = new List<ProcedureEffect>();

        /// <summary>
        /// Resources required to execute this procedure
        /// </summary>
        public Dictionary<string, object> RequiredResources { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Estimated execution time
        /// </summary>
        public TimeSpan EstimatedDuration { get; set; } = TimeSpan.FromSeconds(10);

        /// <summary>
        /// Expected utility/reward when procedure completes successfully
        /// </summary>
        public double ExpectedUtility { get; set; } = 0.5;

        /// <summary>
        /// Estimated cost of execution (resources, effort)
        /// </summary>
        public double EstimatedCost { get; set; } = 0.5;

        /// <summary>
        /// Success probability based on past executions
        /// </summary>
        public double SuccessProbability { get; set; } = 0.8;

        /// <summary>
        /// Execution context for parameter binding and variable storage
        /// </summary>
        public Dictionary<string, object> ExecutionContext { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Failure handling strategy
        /// </summary>
        public string FailureStrategy { get; set; } = "Retry"; // Retry, Alternate, Abort, etc.

        /// <summary>
        /// Total successful executions count
        /// </summary>
        public int SuccessCount { get; set; } = 0;

        /// <summary>
        /// Total failed executions count
        /// </summary>
        public int FailureCount { get; set; } = 0;

        /// <summary>
        /// When this procedure was last executed
        /// </summary>
        public DateTime LastExecutionTime { get; set; } = DateTime.MinValue;

        /// <summary>
        /// Average execution time based on history
        /// </summary>
        public TimeSpan AverageExecutionTime { get; set; } = TimeSpan.Zero;

        /// <summary>
        /// Creates a new procedure chunk
        /// </summary>
        public ProcedureChunk()
        {
            ChunkType = "Procedure";
        }

        /// <summary>
        /// Creates a new procedure chunk with specific name and type
        /// </summary>
        public ProcedureChunk(string name, ProcedureType type = ProcedureType.Sequence)
        {
            ChunkType = "Procedure";
            Name = name;
            Type = type;
        }

        /// <summary>
        /// Resets procedure execution state
        /// </summary>
        public void Reset()
        {
            Status = ExecutionStatus.NotStarted;
            CurrentStepIndex = 0;
            ExecutionContext.Clear();
        }

        /// <summary>
        /// Update the success/failure statistics after execution
        /// </summary>
        public void UpdateStatistics(bool wasSuccessful, TimeSpan executionTime)
        {
            if (wasSuccessful)
            {
                SuccessCount++;
            }
            else
            {
                FailureCount++;
            }

            // Update success probability
            if (SuccessCount + FailureCount > 0)
            {
                SuccessProbability = (double)SuccessCount / (SuccessCount + FailureCount);
            }

            // Update average execution time
            if (AverageExecutionTime == TimeSpan.Zero)
            {
                AverageExecutionTime = executionTime;
            }
            else
            {
                // Weighted average favoring recent executions
                AverageExecutionTime = TimeSpan.FromMilliseconds(
                    (AverageExecutionTime.TotalMilliseconds * 0.7) +
                    (executionTime.TotalMilliseconds * 0.3));
            }

            LastExecutionTime = DateTime.Now;
        }

        /// <summary>
        /// Check if this procedure can achieve a specific goal type
        /// </summary>
        public bool CanAchieveGoal(string goalType)
        {
            return ApplicableGoalTypes.Contains(goalType);
        }

        /// <summary>
        /// Check if all preconditions are satisfied given a context
        /// </summary>
        public bool ArePreconditionsSatisfied(Dictionary<string, object> context)
        {
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
        /// Clone this procedure for a specific execution instance
        /// </summary>
        public ProcedureChunk CloneForExecution()
        {
            var clone = new ProcedureChunk
            {
                ID = ID, // Same ID for reference
                ChunkType = ChunkType,
                Name = Name,
                Type = Type,
                ApplicableGoalTypes = new List<string>(ApplicableGoalTypes),
                Status = ExecutionStatus.NotStarted,
                Steps = new List<ProcedureStep>(),
                Preconditions = new List<ProcedureCondition>(),
                ExpectedEffects = new List<ProcedureEffect>(),
                RequiredResources = new Dictionary<string, object>(RequiredResources),
                EstimatedDuration = EstimatedDuration,
                ExpectedUtility = ExpectedUtility,
                EstimatedCost = EstimatedCost,
                SuccessProbability = SuccessProbability,
                ExecutionContext = new Dictionary<string, object>(),
                FailureStrategy = FailureStrategy,
                SuccessCount = SuccessCount,
                FailureCount = FailureCount,
                LastExecutionTime = LastExecutionTime,
                AverageExecutionTime = AverageExecutionTime
            };

            // Deep copy steps
            foreach (var step in Steps)
            {
                clone.Steps.Add(step.Clone());
            }

            // Deep copy preconditions
            foreach (var condition in Preconditions)
            {
                clone.Preconditions.Add(condition.Clone());
            }

            // Deep copy effects
            foreach (var effect in ExpectedEffects)
            {
                clone.ExpectedEffects.Add(effect.Clone());
            }

            return clone;
        }
    }
}