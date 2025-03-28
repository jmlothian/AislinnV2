using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Aislinn.ChunkStorage.Interfaces;
using Aislinn.Core.Context;
using Aislinn.Core.Goals;
using Aislinn.Core.Goals.Selection;
using Aislinn.Core.Models;
using Aislinn.Core.Procedural;
using Aislinn.Core.Services;

namespace Aislinn.Core.Goals.Execution
{
    /// <summary>
    /// Handles the execution of goals by matching them with appropriate procedures,
    /// executing the procedures, and monitoring progress.
    /// </summary>
    public class GoalExecutionService : IDisposable
    {
        // Dependencies
        private readonly IChunkStore _chunkStore;
        private readonly IAssociationStore _associationStore;
        private readonly GoalSelectionService _goalSelectionService;
        private readonly GoalManagementService _goalManagementService;
        private readonly ProcedureMatcher _procedureMatcher;
        private readonly ContextContainer _contextContainer;

        // Configuration
        private readonly string _chunkCollectionId;
        private readonly string _associationCollectionId;
        private readonly GoalExecutionConfig _config;

        // Execution state
        private Guid? _currentGoalId;
        private ProcedureChunk _currentProcedure;
        private ExecutionState _executionState;
        private Dictionary<string, object> _executionContext;
        private CancellationTokenSource _executionCancellationSource;
        private bool _isPaused = false;
        private bool _isDisposed = false;

        // Execution events
        private TaskCompletionSource<bool> _executionCompleted;
        private DateTime _executionStartTime;
        private DateTime _lastProgressUpdate;
        private double _executionProgress = 0;

        // Execution monitoring timer
        private System.Timers.Timer _monitoringTimer;

        // Events
        public event EventHandler<GoalExecutionEventArgs> ExecutionStarted;
        public event EventHandler<GoalExecutionEventArgs> ExecutionCompleted;
        public event EventHandler<GoalExecutionEventArgs> ExecutionFailed;
        public event EventHandler<GoalExecutionProgressEventArgs> ExecutionProgress;
        public event EventHandler<GoalExecutionEventArgs> ExecutionPaused;
        public event EventHandler<GoalExecutionEventArgs> ExecutionResumed;

        /// <summary>
        /// Execution state of a goal/procedure
        /// </summary>
        public enum ExecutionState
        {
            Idle,
            Preparing,
            Running,
            Paused,
            Completed,
            Failed
        }

        /// <summary>
        /// Event arguments for execution events
        /// </summary>
        public class GoalExecutionEventArgs : EventArgs
        {
            public Guid GoalId { get; set; }
            public string GoalName { get; set; }
            public Guid? ProcedureId { get; set; }
            public string ProcedureName { get; set; }
            public TimeSpan ExecutionTime { get; set; }
            public string Message { get; set; }
            public Exception Error { get; set; }
            public Dictionary<string, object> ExecutionContext { get; set; }
        }

        /// <summary>
        /// Event arguments for execution progress updates
        /// </summary>
        public class GoalExecutionProgressEventArgs : GoalExecutionEventArgs
        {
            public double ProgressPercentage { get; set; }
            public int CurrentStepIndex { get; set; }
            public int TotalSteps { get; set; }
            public string CurrentStepName { get; set; }
        }

        /// <summary>
        /// Configuration options for goal execution
        /// </summary>
        public class GoalExecutionConfig
        {
            /// <summary>
            /// How often to check execution progress (in milliseconds)
            /// </summary>
            public double MonitoringIntervalMs { get; set; } = 500;

            /// <summary>
            /// Maximum execution time for any procedure (in seconds)
            /// </summary>
            public double MaxExecutionTimeSeconds { get; set; } = 60;

            /// <summary>
            /// Whether to automatically retry on failure
            /// </summary>
            public bool AutoRetryOnFailure { get; set; } = true;

            /// <summary>
            /// Maximum number of retry attempts
            /// </summary>
            public int MaxRetryAttempts { get; set; } = 3;

            /// <summary>
            /// Whether to automatically select alternate procedures if one fails
            /// </summary>
            public bool TryAlternateProcedures { get; set; } = true;

            /// <summary>
            /// Whether to update goal activation during execution
            /// </summary>
            public bool UpdateGoalActivation { get; set; } = true;

            /// <summary>
            /// Whether to update context during execution
            /// </summary>
            public bool UpdateContext { get; set; } = true;
        }

        /// <summary>
        /// Initializes a new instance of the GoalExecutionService
        /// </summary>
        public GoalExecutionService(
            IChunkStore chunkStore,
            IAssociationStore associationStore,
            GoalSelectionService goalSelectionService,
            GoalManagementService goalManagementService,
            ProcedureMatcher procedureMatcher,
            ContextContainer contextContainer,
            string chunkCollectionId = "default",
            string associationCollectionId = "default",
            GoalExecutionConfig config = null)
        {
            _chunkStore = chunkStore ?? throw new ArgumentNullException(nameof(chunkStore));
            _associationStore = associationStore ?? throw new ArgumentNullException(nameof(associationStore));
            _goalSelectionService = goalSelectionService ?? throw new ArgumentNullException(nameof(goalSelectionService));
            _goalManagementService = goalManagementService ?? throw new ArgumentNullException(nameof(goalManagementService));
            _procedureMatcher = procedureMatcher ?? throw new ArgumentNullException(nameof(procedureMatcher));
            _contextContainer = contextContainer ?? throw new ArgumentNullException(nameof(contextContainer));

            _chunkCollectionId = chunkCollectionId;
            _associationCollectionId = associationCollectionId;
            _config = config ?? new GoalExecutionConfig();

            _executionState = ExecutionState.Idle;
            _executionContext = new Dictionary<string, object>();

            // Initialize monitoring timer
            _monitoringTimer = new System.Timers.Timer(_config.MonitoringIntervalMs);
            _monitoringTimer.Elapsed += async (sender, e) => await MonitorExecutionAsync();
            _monitoringTimer.AutoReset = true;

            // Subscribe to goal selection changes
            _goalSelectionService.GoalSelectionChanged += OnGoalSelectionChanged;
        }

        #region Properties

        /// <summary>
        /// Gets the current execution state
        /// </summary>
        public ExecutionState State => _executionState;

        /// <summary>
        /// Gets the ID of the currently executing goal
        /// </summary>
        public Guid? CurrentGoalId => _currentGoalId;

        /// <summary>
        /// Gets the ID of the currently executing procedure
        /// </summary>
        public Guid? CurrentProcedureId => _currentProcedure?.ID;

        /// <summary>
        /// Gets the current execution progress (0-1)
        /// </summary>
        public double ExecutionProgressValue => _executionProgress;

        /// <summary>
        /// Gets whether execution is currently paused
        /// </summary>
        public bool IsPaused => _isPaused;

        /// <summary>
        /// Gets the current execution context
        /// </summary>
        public Dictionary<string, object> ExecutionContext => new Dictionary<string, object>(_executionContext);

        #endregion

        #region Public Methods

        /// <summary>
        /// Executes a goal by finding and running an appropriate procedure
        /// </summary>
        public async Task<bool> ExecuteGoalAsync(Guid goalId, Dictionary<string, object> initialContext = null)
        {
            // Check if already executing something
            if (_executionState == ExecutionState.Running || _executionState == ExecutionState.Preparing)
            {
                return false;
            }

            try
            {
                // Reset state
                _executionState = ExecutionState.Preparing;
                _isPaused = false;
                _executionProgress = 0;
                _executionStartTime = DateTime.Now;
                _lastProgressUpdate = DateTime.Now;
                _executionCompleted = new TaskCompletionSource<bool>();
                _executionCancellationSource = new CancellationTokenSource();

                // Get the goal
                var chunkCollection = await _chunkStore.GetCollectionAsync(_chunkCollectionId);
                if (chunkCollection == null)
                    throw new InvalidOperationException($"Chunk collection '{_chunkCollectionId}' not found");

                var goal = await chunkCollection.GetChunkAsync(goalId);
                if (goal == null)
                    throw new ArgumentException($"Goal with ID {goalId} not found", nameof(goalId));

                // Prepare execution context
                _executionContext = new Dictionary<string, object>();

                // Add goal information
                _executionContext["Goal.ID"] = goal.ID;
                _executionContext["Goal.Name"] = goal.Name;
                _executionContext["Goal.Type"] = goal.ChunkType;

                foreach (var slot in goal.Slots)
                {
                    _executionContext[$"Goal.{slot.Key}"] = slot.Value?.Value;
                }

                // Add current context
                var contextSnapshot = _contextContainer.CreateContextSnapshot();
                foreach (var category in contextSnapshot.Keys)
                {
                    foreach (var factor in contextSnapshot[category])
                    {
                        _executionContext[$"Context.{category}.{factor.Key}"] = factor.Value;
                    }
                }

                // Add initial context if provided
                if (initialContext != null)
                {
                    foreach (var item in initialContext)
                    {
                        _executionContext[item.Key] = item.Value;
                    }
                }

                // Find appropriate procedures
                var procedureMatches = await _procedureMatcher.FindProceduresForGoalAsync(goal, _executionContext);

                if (!procedureMatches.Any())
                {
                    throw new InvalidOperationException($"No procedures found for goal '{goal.Name}'");
                }

                // Select best procedure
                var bestMatch = procedureMatches.First();
                _currentProcedure = bestMatch.Procedure.CloneForExecution();
                _currentGoalId = goalId;

                // Start monitoring
                _monitoringTimer.Start();

                // Raise execution started event
                RaiseExecutionStarted(goal, _currentProcedure);

                // Update goal status to InProgress
                await _goalManagementService.UpdateGoalStatusAsync(goalId, GoalManagementService.GoalSlots.StatusValues.InProgress);

                // Start executing the procedure
                _executionState = ExecutionState.Running;
                var executionTask = Task.Run(() => ExecuteProcedureAsync(_currentProcedure, _executionCancellationSource.Token));

                // Wait for execution to complete
                bool success = await _executionCompleted.Task;

                // Stop monitoring
                _monitoringTimer.Stop();

                // Update state based on result
                _executionState = success ? ExecutionState.Completed : ExecutionState.Failed;

                // Update goal status based on result
                if (success)
                {
                    await _goalManagementService.UpdateGoalStatusAsync(goalId, GoalManagementService.GoalSlots.StatusValues.Completed);

                    // Raise execution completed event
                    RaiseExecutionCompleted(goal, _currentProcedure);
                }
                else
                {
                    // If execution failed, goal stays InProgress for possible retry
                    RaiseExecutionFailed(goal, _currentProcedure, null);
                }

                // Update procedure statistics
                _currentProcedure.UpdateStatistics(
                    success,
                    DateTime.Now - _executionStartTime);

                // Save updated procedure
                await _procedureMatcher.UpdateProcedureAsync(_currentProcedure);

                return success;
            }
            catch (Exception ex)
            {
                _executionState = ExecutionState.Failed;

                // Stop monitoring
                _monitoringTimer.Stop();

                // Get the goal if possible
                Chunk goal = null;
                try
                {
                    var chunkCollection = await _chunkStore.GetCollectionAsync(_chunkCollectionId);
                    goal = await chunkCollection.GetChunkAsync(goalId);
                }
                catch { }

                // Raise execution failed event
                RaiseExecutionFailed(goal, _currentProcedure, ex);

                return false;
            }
            finally
            {
                // Clean up
                _executionCancellationSource?.Dispose();
                _executionCancellationSource = null;
            }
        }

        /// <summary>
        /// Executes the currently selected goal
        /// </summary>
        public async Task<bool> ExecuteSelectedGoalAsync()
        {
            var primaryGoal = await _goalSelectionService.GetPrimaryGoalAsync();
            if (primaryGoal == null)
                return false;

            return await ExecuteGoalAsync(primaryGoal.ID);
        }

        /// <summary>
        /// Pauses the current execution
        /// </summary>
        public bool PauseExecution()
        {
            if (_executionState != ExecutionState.Running || _isPaused)
                return false;

            _isPaused = true;
            _executionState = ExecutionState.Paused;

            // Raise execution paused event
            RaiseExecutionPaused();

            return true;
        }

        /// <summary>
        /// Resumes a paused execution
        /// </summary>
        public bool ResumeExecution()
        {
            if (_executionState != ExecutionState.Paused || !_isPaused)
                return false;

            _isPaused = false;
            _executionState = ExecutionState.Running;

            // Raise execution resumed event
            RaiseExecutionResumed();

            return true;
        }

        /// <summary>
        /// Cancels the current execution
        /// </summary>
        public bool CancelExecution()
        {
            if (_executionState != ExecutionState.Running && _executionState != ExecutionState.Paused)
                return false;

            _executionCancellationSource?.Cancel();
            _executionState = ExecutionState.Failed;

            // Raise execution failed event
            RaiseExecutionFailed(null, _currentProcedure, new OperationCanceledException("Execution was cancelled"));

            return true;
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles goal selection changes
        /// </summary>
        private async void OnGoalSelectionChanged(object sender, GoalSelectionService.GoalSelectionChangedEventArgs e)
        {
            // If currently executing a goal that's no longer the primary goal,
            // we might want to reconsider execution (e.g., pause or cancel)
            // This depends on the specific requirements

            // For now, we'll just check if we're idle and auto-execute the new primary goal
            if (_executionState == ExecutionState.Idle && e.NewPrimaryGoalId.HasValue)
            {
                // Auto-execution could be controlled by a configuration option
                // await ExecuteGoalAsync(e.NewPrimaryGoalId.Value);
            }
        }

        #endregion

        #region Procedure Execution

        /// <summary>
        /// Executes a procedure
        /// </summary>
        private async Task<bool> ExecuteProcedureAsync(ProcedureChunk procedure, CancellationToken cancellationToken)
        {
            if (procedure == null)
                return false;

            try
            {
                // Reset procedure state
                procedure.Reset();
                procedure.Status = ProcedureChunk.ExecutionStatus.InProgress;

                // Execute based on procedure type
                bool success = false;

                switch (procedure.Type)
                {
                    case ProcedureChunk.ProcedureType.Action:
                        success = await ExecuteActionProcedureAsync(procedure, cancellationToken);
                        break;

                    case ProcedureChunk.ProcedureType.Sequence:
                        success = await ExecuteSequenceProcedureAsync(procedure, cancellationToken);
                        break;

                    case ProcedureChunk.ProcedureType.Conditional:
                        success = await ExecuteConditionalProcedureAsync(procedure, cancellationToken);
                        break;

                    case ProcedureChunk.ProcedureType.Iterative:
                        success = await ExecuteIterativeProcedureAsync(procedure, cancellationToken);
                        break;

                    case ProcedureChunk.ProcedureType.Hierarchical:
                        success = await ExecuteHierarchicalProcedureAsync(procedure, cancellationToken);
                        break;

                    case ProcedureChunk.ProcedureType.Reactive:
                        success = await ExecuteReactiveProcedureAsync(procedure, cancellationToken);
                        break;

                    default:
                        throw new NotSupportedException($"Procedure type '{procedure.Type}' is not supported");
                }

                // Update procedure status
                procedure.Status = success ? ProcedureChunk.ExecutionStatus.Completed : ProcedureChunk.ExecutionStatus.Failed;

                // Signal completion
                _executionCompleted?.TrySetResult(success);

                return success;
            }
            catch (OperationCanceledException)
            {
                // Execution was cancelled
                procedure.Status = ProcedureChunk.ExecutionStatus.Aborted;
                _executionCompleted?.TrySetResult(false);
                return false;
            }
            catch (Exception ex)
            {
                // Execution failed with exception
                procedure.Status = ProcedureChunk.ExecutionStatus.Failed;
                _executionCompleted?.TrySetException(ex);
                throw;
            }
        }

        /// <summary>
        /// Executes a simple action procedure
        /// </summary>
        private async Task<bool> ExecuteActionProcedureAsync(ProcedureChunk procedure, CancellationToken cancellationToken)
        {
            // For a simple action, treat the first step as the action to perform
            if (procedure.Steps.Count == 0)
                return false;

            ProcedureStep step = procedure.Steps[0];

            // Check if the step can be executed
            if (!step.CanExecute(procedure.ExecutionContext))
                return false;

            // Execute the step
            bool stepSuccess = await ExecuteStepAsync(step, procedure.ExecutionContext, cancellationToken);

            return stepSuccess;
        }

        /// <summary>
        /// Executes a sequence procedure
        /// </summary>
        private async Task<bool> ExecuteSequenceProcedureAsync(ProcedureChunk procedure, CancellationToken cancellationToken)
        {
            if (procedure.Steps.Count == 0)
                return true; // Empty sequence is successful

            // Execute steps in sequence
            int currentStepIndex = 0;
            string nextStepId = procedure.Steps[0].Id;

            while (!string.IsNullOrEmpty(nextStepId) && currentStepIndex < procedure.Steps.Count)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Find the step with this ID
                ProcedureStep currentStep = procedure.Steps.FirstOrDefault(s => s.Id == nextStepId);
                if (currentStep == null)
                    break;

                // Update current step index
                procedure.CurrentStepIndex = currentStepIndex;

                // Check for pause
                while (_isPaused)
                {
                    await Task.Delay(100, cancellationToken);
                }

                // Update progress
                _executionProgress = (double)currentStepIndex / procedure.Steps.Count;
                RaiseExecutionProgress(procedure, currentStepIndex);

                // Check if the step can be executed
                if (!currentStep.CanExecute(procedure.ExecutionContext))
                {
                    // Skip this step
                    currentStep.Status = ProcedureStep.StepStatus.Skipped;
                    nextStepId = currentStep.NextStepId;
                    currentStepIndex++;
                    continue;
                }

                // Execute the step
                bool stepSuccess = await ExecuteStepAsync(currentStep, procedure.ExecutionContext, cancellationToken);

                // Determine next step
                nextStepId = currentStep.GetNextStepId(stepSuccess, procedure.ExecutionContext);

                // If no next step and step failed, return failure
                if (string.IsNullOrEmpty(nextStepId) && !stepSuccess)
                    return false;

                currentStepIndex++;
            }

            // Update final progress
            _executionProgress = 1.0;
            RaiseExecutionProgress(procedure, procedure.Steps.Count);

            return true;
        }

        /// <summary>
        /// Executes a conditional procedure
        /// </summary>
        private async Task<bool> ExecuteConditionalProcedureAsync(ProcedureChunk procedure, CancellationToken cancellationToken)
        {
            // Simplified implementation - treat as sequence with conditional branches
            return await ExecuteSequenceProcedureAsync(procedure, cancellationToken);
        }

        /// <summary>
        /// Executes an iterative procedure
        /// </summary>
        private async Task<bool> ExecuteIterativeProcedureAsync(ProcedureChunk procedure, CancellationToken cancellationToken)
        {
            // Implementation would handle loops and iterations
            // For now, treat as a sequence for simplicity
            return await ExecuteSequenceProcedureAsync(procedure, cancellationToken);
        }

        /// <summary>
        /// Executes a hierarchical procedure
        /// </summary>
        private async Task<bool> ExecuteHierarchicalProcedureAsync(ProcedureChunk procedure, CancellationToken cancellationToken)
        {
            // Implementation would handle sub-procedures
            // For now, treat as a sequence for simplicity
            return await ExecuteSequenceProcedureAsync(procedure, cancellationToken);
        }

        /// <summary>
        /// Executes a reactive procedure
        /// </summary>
        private async Task<bool> ExecuteReactiveProcedureAsync(ProcedureChunk procedure, CancellationToken cancellationToken)
        {
            // Implementation would handle event-driven execution
            // For now, treat as a sequence for simplicity
            return await ExecuteSequenceProcedureAsync(procedure, cancellationToken);
        }

        /// <summary>
        /// Executes a single procedure step
        /// </summary>
        private async Task<bool> ExecuteStepAsync(
            ProcedureStep step,
            Dictionary<string, object> context,
            CancellationToken cancellationToken)
        {
            try
            {
                step.Status = ProcedureStep.StepStatus.InProgress;

                // Handle different step types
                bool success = false;

                switch (step.Type)
                {
                    case ProcedureStep.StepType.Action:
                        success = await ExecuteActionStepAsync(step, context, cancellationToken);
                        break;

                    case ProcedureStep.StepType.Perception:
                        success = await ExecutePerceptionStepAsync(step, context, cancellationToken);
                        break;

                    case ProcedureStep.StepType.Calculation:
                        success = ExecuteCalculationStep(step, context);
                        break;

                    case ProcedureStep.StepType.MemoryStore:
                        success = await ExecuteMemoryStoreStepAsync(step, context, cancellationToken);
                        break;

                    case ProcedureStep.StepType.MemoryRetrieve:
                        success = await ExecuteMemoryRetrieveStepAsync(step, context, cancellationToken);
                        break;

                    case ProcedureStep.StepType.GoalCreate:
                        success = await ExecuteGoalCreateStepAsync(step, context, cancellationToken);
                        break;

                    case ProcedureStep.StepType.GoalModify:
                        success = await ExecuteGoalModifyStepAsync(step, context, cancellationToken);
                        break;
                    case ProcedureStep.StepType.Decision:
                        success = ExecuteDecisionStep(step, context);
                        break;

                    case ProcedureStep.StepType.SubProcedure:
                        success = await ExecuteSubProcedureStepAsync(step, context, cancellationToken);
                        break;

                    case ProcedureStep.StepType.Wait:
                        success = await ExecuteWaitStepAsync(step, context, cancellationToken);
                        break;

                    case ProcedureStep.StepType.Event:
                        success = await ExecuteEventStepAsync(step, context, cancellationToken);
                        break;

                    default:
                        throw new NotSupportedException($"Step type '{step.Type}' is not supported");
                }

                // Update step status
                step.Status = success ? ProcedureStep.StepStatus.Completed : ProcedureStep.StepStatus.Failed;

                // Apply effects if successful
                if (success)
                {
                    step.ApplyEffects(context);
                }

                return success;
            }
            catch (Exception)
            {
                step.Status = ProcedureStep.StepStatus.Failed;
                throw;
            }
        }

        /// <summary>
        /// Executes an action step
        /// </summary>
        private async Task<bool> ExecuteActionStepAsync(
            ProcedureStep step,
            Dictionary<string, object> context,
            CancellationToken cancellationToken)
        {
            // In a real implementation, this would invoke the appropriate action in the world
            // For this example, we'll simulate execution with a delay

            string operation = step.Operation;
            var parameters = step.Parameters;

            // Resolve parameter values from context
            var resolvedParameters = new Dictionary<string, object>();
            foreach (var param in parameters)
            {
                resolvedParameters[param.Key] = ResolveValue(param.Value, context);
            }

            // Simulate execution time
            int simulatedTimeMs = new Random().Next(100, 500);
            await Task.Delay(simulatedTimeMs, cancellationToken);

            // Simulate success (in a real system, would be based on action result)
            bool success = true;

            // Store result if specified
            if (!string.IsNullOrEmpty(step.ResultReference))
            {
                // Simulate a result
                context[step.ResultReference] = $"Result of {operation}";
            }

            return success;
        }

        /// <summary>
        /// Executes a perception step
        /// </summary>
        private async Task<bool> ExecutePerceptionStepAsync(
            ProcedureStep step,
            Dictionary<string, object> context,
            CancellationToken cancellationToken)
        {
            // In a real implementation, this would get information from the environment
            // For this example, we'll simulate perception with context values

            string operation = step.Operation;

            // Simulate perception time
            int simulatedTimeMs = new Random().Next(50, 200);
            await Task.Delay(simulatedTimeMs, cancellationToken);

            // Determine what to perceive based on the operation
            object perceivedValue = null;

            switch (operation.ToLower())
            {
                case "getlocation":
                    perceivedValue = "current location";
                    break;

                case "getobjectproperties":
                    // Get object to perceive
                    string objectName = step.Parameters.ContainsKey("objectName")
                        ? step.Parameters["objectName"].ToString()
                        : null;

                    if (string.IsNullOrEmpty(objectName))
                        return false;

                    // Simulate perceived properties
                    perceivedValue = new Dictionary<string, object>
                    {
                        { "color", "blue" },
                        { "size", "medium" },
                        { "position", "left" }
                    };
                    break;

                default:
                    // Unknown perception operation
                    return false;
            }

            // Store result if specified
            if (!string.IsNullOrEmpty(step.ResultReference))
            {
                context[step.ResultReference] = perceivedValue;
            }

            return true;
        }

        /// <summary>
        /// Executes a calculation step
        /// </summary>
        private bool ExecuteCalculationStep(
            ProcedureStep step,
            Dictionary<string, object> context)
        {
            // Execute calculation based on operation
            string operation = step.Operation;

            // Resolve parameter values from context
            var resolvedParameters = new Dictionary<string, object>();
            foreach (var param in step.Parameters)
            {
                resolvedParameters[param.Key] = ResolveValue(param.Value, context);
            }

            object result = null;

            switch (operation.ToLower())
            {
                case "add":
                    if (resolvedParameters.ContainsKey("a") && resolvedParameters.ContainsKey("b"))
                    {
                        double a = Convert.ToDouble(resolvedParameters["a"]);
                        double b = Convert.ToDouble(resolvedParameters["b"]);
                        result = a + b;
                    }
                    break;

                case "subtract":
                    if (resolvedParameters.ContainsKey("a") && resolvedParameters.ContainsKey("b"))
                    {
                        double a = Convert.ToDouble(resolvedParameters["a"]);
                        double b = Convert.ToDouble(resolvedParameters["b"]);
                        result = a - b;
                    }
                    break;

                case "multiply":
                    if (resolvedParameters.ContainsKey("a") && resolvedParameters.ContainsKey("b"))
                    {
                        double a = Convert.ToDouble(resolvedParameters["a"]);
                        double b = Convert.ToDouble(resolvedParameters["b"]);
                        result = a * b;
                    }
                    break;

                case "divide":
                    if (resolvedParameters.ContainsKey("a") && resolvedParameters.ContainsKey("b"))
                    {
                        double a = Convert.ToDouble(resolvedParameters["a"]);
                        double b = Convert.ToDouble(resolvedParameters["b"]);

                        if (Math.Abs(b) < 0.00001)
                            return false; // Division by zero

                        result = a / b;
                    }
                    break;

                default:
                    // Unknown calculation operation
                    return false;
            }

            // Store result if specified
            if (!string.IsNullOrEmpty(step.ResultReference) && result != null)
            {
                context[step.ResultReference] = result;
            }

            return result != null;
        }

        /// <summary>
        /// Executes a memory store step
        /// </summary>
        private async Task<bool> ExecuteMemoryStoreStepAsync(
            ProcedureStep step,
            Dictionary<string, object> context,
            CancellationToken cancellationToken)
        {
            // In a real implementation, this would store data in the agent's memory system
            // For this example, we'll simulate memory storage

            if (!step.Parameters.ContainsKey("key") || !step.Parameters.ContainsKey("value"))
                return false;

            string key = step.Parameters["key"].ToString();
            object value = ResolveValue(step.Parameters["value"], context);

            // Simulate memory storage time
            int simulatedTimeMs = new Random().Next(20, 100);
            await Task.Delay(simulatedTimeMs, cancellationToken);

            // Store in context (simulating memory)
            context[$"Memory.{key}"] = value;

            return true;
        }

        /// <summary>
        /// Executes a memory retrieve step
        /// </summary>
        private async Task<bool> ExecuteMemoryRetrieveStepAsync(
            ProcedureStep step,
            Dictionary<string, object> context,
            CancellationToken cancellationToken)
        {
            // In a real implementation, this would retrieve data from the agent's memory system
            // For this example, we'll simulate memory retrieval

            if (!step.Parameters.ContainsKey("key"))
                return false;

            string key = step.Parameters["key"].ToString();

            // Simulate memory retrieval time
            int simulatedTimeMs = new Random().Next(20, 100);
            await Task.Delay(simulatedTimeMs, cancellationToken);

            // Retrieve from context (simulating memory)
            string memoryKey = $"Memory.{key}";
            if (!context.ContainsKey(memoryKey))
                return false;

            object value = context[memoryKey];

            // Store result if specified
            if (!string.IsNullOrEmpty(step.ResultReference))
            {
                context[step.ResultReference] = value;
            }

            return true;
        }

        /// <summary>
        /// Executes a goal creation step
        /// </summary>
        private async Task<bool> ExecuteGoalCreateStepAsync(
            ProcedureStep step,
            Dictionary<string, object> context,
            CancellationToken cancellationToken)
        {
            try
            {
                // Get goal template ID
                if (!step.Parameters.ContainsKey("templateId"))
                    return false;

                string templateIdStr = step.Parameters["templateId"].ToString();
                if (!Guid.TryParse(templateIdStr, out Guid templateId))
                    return false;

                // Get goal parameters
                var goalParameters = new Dictionary<string, object>();

                if (step.Parameters.ContainsKey("parameters") &&
                    step.Parameters["parameters"] is Dictionary<string, object> paramsDict)
                {
                    foreach (var param in paramsDict)
                    {
                        goalParameters[param.Key] = ResolveValue(param.Value, context);
                    }
                }

                // Get parent goal if specified
                Guid? parentGoalId = null;
                if (step.Parameters.ContainsKey("parentGoalId"))
                {
                    string parentIdStr = step.Parameters["parentGoalId"].ToString();
                    if (Guid.TryParse(parentIdStr, out Guid parentId))
                    {
                        parentGoalId = parentId;
                    }
                }

                // Create the goal
                var newGoal = await _goalManagementService.InstantiateGoalAsync(
                    templateId,
                    goalParameters,
                    parentGoalId);

                // Store result if specified
                if (!string.IsNullOrEmpty(step.ResultReference) && newGoal != null)
                {
                    context[step.ResultReference] = newGoal.ID;
                }

                return newGoal != null;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Executes a goal modification step
        /// </summary>
        private async Task<bool> ExecuteGoalModifyStepAsync(
            ProcedureStep step,
            Dictionary<string, object> context,
            CancellationToken cancellationToken)
        {
            try
            {
                // Get goal ID
                if (!step.Parameters.ContainsKey("goalId"))
                    return false;

                string goalIdStr = step.Parameters["goalId"].ToString();
                if (!Guid.TryParse(goalIdStr, out Guid goalId))
                    return false;

                // Get the goal
                var chunkCollection = await _chunkStore.GetCollectionAsync(_chunkCollectionId);
                if (chunkCollection == null)
                    return false;

                var goal = await chunkCollection.GetChunkAsync(goalId);
                if (goal == null)
                    return false;

                // Get modification type
                string modificationType = step.Parameters.ContainsKey("modificationType")
                    ? step.Parameters["modificationType"].ToString()
                    : "UpdateStatus";

                switch (modificationType)
                {
                    case "UpdateStatus":
                        // Get new status
                        string newStatus = step.Parameters.ContainsKey("newStatus")
                            ? step.Parameters["newStatus"].ToString()
                            : null;

                        if (string.IsNullOrEmpty(newStatus))
                            return false;

                        // Update goal status
                        await _goalManagementService.UpdateGoalStatusAsync(goalId, newStatus);
                        break;

                    case "UpdatePriority":
                        // Get new priority
                        double newPriority = 0.5;
                        if (step.Parameters.ContainsKey("newPriority"))
                        {
                            try
                            {
                                newPriority = Convert.ToDouble(step.Parameters["newPriority"]);
                            }
                            catch
                            {
                                return false;
                            }
                        }

                        // Update goal priority
                        goal.Slots[GoalManagementService.GoalSlots.Priority] = new ModelSlot
                        {
                            Name = GoalManagementService.GoalSlots.Priority,
                            Value = newPriority
                        };

                        await chunkCollection.UpdateChunkAsync(goal);
                        break;

                    default:
                        // Unsupported modification type
                        return false;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Executes a decision step
        /// </summary>
        private bool ExecuteDecisionStep(
            ProcedureStep step,
            Dictionary<string, object> context)
        {
            // In a real implementation, this would make a decision based on conditions
            // For this example, we'll simulate a simple decision

            if (!step.Parameters.ContainsKey("condition"))
                return false;

            object condition = step.Parameters["condition"];

            if (condition is ProcedureCondition procedureCondition)
            {
                // Evaluate the condition
                bool result = procedureCondition.Evaluate(context);

                // Store result if specified
                if (!string.IsNullOrEmpty(step.ResultReference))
                {
                    context[step.ResultReference] = result;
                }

                return true; // Decision step is successful if condition was evaluated
            }
            else if (condition is string conditionStr)
            {
                // Simple true/false condition
                bool result = conditionStr.Equals("true", StringComparison.OrdinalIgnoreCase);

                // Store result if specified
                if (!string.IsNullOrEmpty(step.ResultReference))
                {
                    context[step.ResultReference] = result;
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Executes a sub-procedure step
        /// </summary>
        private async Task<bool> ExecuteSubProcedureStepAsync(
            ProcedureStep step,
            Dictionary<string, object> context,
            CancellationToken cancellationToken)
        {
            // Get sub-procedure ID
            if (!step.Parameters.ContainsKey("procedureId"))
                return false;

            string procedureIdStr = step.Parameters["procedureId"].ToString();
            if (!Guid.TryParse(procedureIdStr, out Guid procedureId))
                return false;

            // Get the procedure
            var procedure = await _procedureMatcher.GetProcedureByIdAsync(procedureId);
            if (procedure == null)
                return false;

            // Clone for execution
            var procedureInstance = procedure.CloneForExecution();

            // Update execution context for sub-procedure
            foreach (var contextItem in context)
            {
                procedureInstance.ExecutionContext[contextItem.Key] = contextItem.Value;
            }

            // Execute the sub-procedure
            bool success = false;

            switch (procedureInstance.Type)
            {
                case ProcedureChunk.ProcedureType.Action:
                    success = await ExecuteActionProcedureAsync(procedureInstance, cancellationToken);
                    break;

                case ProcedureChunk.ProcedureType.Sequence:
                    success = await ExecuteSequenceProcedureAsync(procedureInstance, cancellationToken);
                    break;

                default:
                    // For simplicity, handle as sequence
                    success = await ExecuteSequenceProcedureAsync(procedureInstance, cancellationToken);
                    break;
            }

            // Copy back any new context values
            foreach (var contextItem in procedureInstance.ExecutionContext)
            {
                context[contextItem.Key] = contextItem.Value;
            }

            return success;
        }

        /// <summary>
        /// Executes a wait step
        /// </summary>
        private async Task<bool> ExecuteWaitStepAsync(
            ProcedureStep step,
            Dictionary<string, object> context,
            CancellationToken cancellationToken)
        {
            // Get wait duration or condition
            if (step.Parameters.ContainsKey("duration"))
            {
                // Wait for duration
                int durationMs;
                try
                {
                    durationMs = Convert.ToInt32(step.Parameters["duration"]);
                }
                catch
                {
                    return false;
                }

                await Task.Delay(durationMs, cancellationToken);
                return true;
            }
            else if (step.Parameters.ContainsKey("condition"))
            {
                // Wait for condition
                object condition = step.Parameters["condition"];

                if (condition is ProcedureCondition procedureCondition)
                {
                    // Get timeout if specified
                    int timeoutMs = step.Parameters.ContainsKey("timeout")
                        ? Convert.ToInt32(step.Parameters["timeout"])
                        : 5000; // Default 5 seconds

                    // Get check interval if specified
                    int checkIntervalMs = step.Parameters.ContainsKey("checkInterval")
                        ? Convert.ToInt32(step.Parameters["checkInterval"])
                        : 100; // Default 100ms

                    // Create a timeout task
                    var timeoutTask = Task.Delay(timeoutMs, cancellationToken);

                    // Wait for condition
                    while (!procedureCondition.Evaluate(context))
                    {
                        // Check for timeout
                        var delayTask = Task.Delay(checkIntervalMs, cancellationToken);
                        var completedTask = await Task.WhenAny(delayTask, timeoutTask);

                        if (completedTask == timeoutTask)
                            return false; // Timed out

                        cancellationToken.ThrowIfCancellationRequested();
                    }

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Executes an event step
        /// </summary>
        private async Task<bool> ExecuteEventStepAsync(
            ProcedureStep step,
            Dictionary<string, object> context,
            CancellationToken cancellationToken)
        {
            // In a real implementation, this would handle events
            // For this example, we'll simulate a simple event handling

            string eventType = step.Parameters.ContainsKey("eventType")
                ? step.Parameters["eventType"].ToString()
                : null;

            if (string.IsNullOrEmpty(eventType))
                return false;

            // Simulate waiting for event
            int simulatedTimeMs = new Random().Next(100, 500);
            await Task.Delay(simulatedTimeMs, cancellationToken);

            // Simulate event data
            var eventData = new Dictionary<string, object>
            {
                { "type", eventType },
                { "timestamp", DateTime.Now },
                { "data", $"Simulated {eventType} event" }
            };

            // Store result if specified
            if (!string.IsNullOrEmpty(step.ResultReference))
            {
                context[step.ResultReference] = eventData;
            }

            return true;
        }

        #endregion

        #region Monitoring and Progress

        /// <summary>
        /// Monitors execution progress and handles timeouts
        /// </summary>
        private async Task MonitorExecutionAsync()
        {
            if (_executionState != ExecutionState.Running && _executionState != ExecutionState.Paused)
                return;

            // Check for timeout
            if ((DateTime.Now - _executionStartTime).TotalSeconds > _config.MaxExecutionTimeSeconds)
            {
                // Execution has timed out
                _executionCancellationSource?.Cancel();
                _executionState = ExecutionState.Failed;

                // Raise execution failed event
                RaiseExecutionFailed(null, _currentProcedure, new TimeoutException("Execution timed out"));

                return;
            }

            // Update context if enabled
            if (_config.UpdateContext && _currentProcedure != null)
            {
                // Only update every second to avoid too many updates
                if ((DateTime.Now - _lastProgressUpdate).TotalSeconds >= 1)
                {
                    // Update context with current execution state
                    _contextContainer.UpdateContextFactor(
                        ContextContainer.ContextCategory.Task,
                        "CurrentGoal",
                        _currentGoalId,
                        importance: 0.8);

                    _contextContainer.UpdateContextFactor(
                        ContextContainer.ContextCategory.Task,
                        "ExecutionProgress",
                        _executionProgress,
                        importance: 0.6);

                    _lastProgressUpdate = DateTime.Now;
                }
            }
        }

        /// <summary>
        /// Raises execution progress event
        /// </summary>
        private void RaiseExecutionProgress(ProcedureChunk procedure, int currentStepIndex)
        {
            if (ExecutionProgress == null)
                return;

            try
            {
                var goal = GetCurrentGoal();
                string currentStepName = null;

                if (procedure != null && procedure.Steps.Count > 0 && currentStepIndex < procedure.Steps.Count)
                {
                    currentStepName = procedure.Steps[currentStepIndex].Name;
                }

                ExecutionProgress(this, new GoalExecutionProgressEventArgs
                {
                    GoalId = _currentGoalId ?? Guid.Empty,
                    GoalName = goal?.Name,
                    ProcedureId = procedure?.ID,
                    ProcedureName = procedure?.Name,
                    ExecutionTime = DateTime.Now - _executionStartTime,
                    ProgressPercentage = _executionProgress * 100,
                    CurrentStepIndex = currentStepIndex,
                    TotalSteps = procedure?.Steps.Count ?? 0,
                    CurrentStepName = currentStepName,
                    ExecutionContext = new Dictionary<string, object>(_executionContext)
                });
            }
            catch
            {
                // Ignore errors in event handlers
            }
        }

        /// <summary>
        /// Raises execution started event
        /// </summary>
        private void RaiseExecutionStarted(Chunk goal, ProcedureChunk procedure)
        {
            if (ExecutionStarted == null)
                return;

            try
            {
                ExecutionStarted(this, new GoalExecutionEventArgs
                {
                    GoalId = goal?.ID ?? Guid.Empty,
                    GoalName = goal?.Name,
                    ProcedureId = procedure?.ID,
                    ProcedureName = procedure?.Name,
                    ExecutionTime = TimeSpan.Zero,
                    Message = $"Starting execution of goal '{goal?.Name}' with procedure '{procedure?.Name}'",
                    ExecutionContext = new Dictionary<string, object>(_executionContext)
                });
            }
            catch
            {
                // Ignore errors in event handlers
            }
        }

        /// <summary>
        /// Raises execution completed event
        /// </summary>
        private void RaiseExecutionCompleted(Chunk goal, ProcedureChunk procedure)
        {
            if (ExecutionCompleted == null)
                return;

            try
            {
                ExecutionCompleted(this, new GoalExecutionEventArgs
                {
                    GoalId = goal?.ID ?? Guid.Empty,
                    GoalName = goal?.Name,
                    ProcedureId = procedure?.ID,
                    ProcedureName = procedure?.Name,
                    ExecutionTime = DateTime.Now - _executionStartTime,
                    Message = $"Successfully completed execution of goal '{goal?.Name}'",
                    ExecutionContext = new Dictionary<string, object>(_executionContext)
                });
            }
            catch
            {
                // Ignore errors in event handlers
            }
        }

        /// <summary>
        /// Raises execution failed event
        /// </summary>
        private void RaiseExecutionFailed(Chunk goal, ProcedureChunk procedure, Exception error)
        {
            if (ExecutionFailed == null)
                return;

            try
            {
                ExecutionFailed(this, new GoalExecutionEventArgs
                {
                    GoalId = goal?.ID ?? Guid.Empty,
                    GoalName = goal?.Name,
                    ProcedureId = procedure?.ID,
                    ProcedureName = procedure?.Name,
                    ExecutionTime = DateTime.Now - _executionStartTime,
                    Message = $"Failed to execute goal '{goal?.Name}'",
                    Error = error,
                    ExecutionContext = new Dictionary<string, object>(_executionContext)
                });
            }
            catch
            {
                // Ignore errors in event handlers
            }
        }

        /// <summary>
        /// Raises execution paused event
        /// </summary>
        private void RaiseExecutionPaused()
        {
            if (ExecutionPaused == null)
                return;

            try
            {
                var goal = GetCurrentGoal();

                ExecutionPaused(this, new GoalExecutionEventArgs
                {
                    GoalId = _currentGoalId ?? Guid.Empty,
                    GoalName = goal?.Name,
                    ProcedureId = _currentProcedure?.ID,
                    ProcedureName = _currentProcedure?.Name,
                    ExecutionTime = DateTime.Now - _executionStartTime,
                    Message = $"Execution of goal '{goal?.Name}' paused",
                    ExecutionContext = new Dictionary<string, object>(_executionContext)
                });
            }
            catch
            {
                // Ignore errors in event handlers
            }
        }

        /// <summary>
        /// Raises execution resumed event
        /// </summary>
        private void RaiseExecutionResumed()
        {
            if (ExecutionResumed == null)
                return;

            try
            {
                var goal = GetCurrentGoal();

                ExecutionResumed(this, new GoalExecutionEventArgs
                {
                    GoalId = _currentGoalId ?? Guid.Empty,
                    GoalName = goal?.Name,
                    ProcedureId = _currentProcedure?.ID,
                    ProcedureName = _currentProcedure?.Name,
                    ExecutionTime = DateTime.Now - _executionStartTime,
                    Message = $"Execution of goal '{goal?.Name}' resumed",
                    ExecutionContext = new Dictionary<string, object>(_executionContext)
                });
            }
            catch
            {
                // Ignore errors in event handlers
            }
        }

        /// <summary>
        /// Gets the current goal
        /// </summary>
        private Chunk GetCurrentGoal()
        {
            if (!_currentGoalId.HasValue)
                return null;

            try
            {
                var chunkCollection = _chunkStore.GetCollectionAsync(_chunkCollectionId).GetAwaiter().GetResult();
                if (chunkCollection == null)
                    return null;

                return chunkCollection.GetChunkAsync(_currentGoalId.Value).GetAwaiter().GetResult();
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Resolves a value from context if it's a variable reference
        /// </summary>
        private object ResolveValue(object value, Dictionary<string, object> context)
        {
            if (value == null)
                return null;

            // If value is a string and looks like a variable reference
            if (value is string stringValue && stringValue.StartsWith("$"))
            {
                string variableName = stringValue.Substring(1);
                return context.ContainsKey(variableName) ? context[variableName] : null;
            }

            return value;
        }

        #endregion

        /// <summary>
        /// Disposes resources
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
                return;

            // Stop monitoring timer
            if (_monitoringTimer != null)
            {
                _monitoringTimer.Dispose();
                _monitoringTimer = null;
            }

            // Cancel any ongoing execution
            if (_executionCancellationSource != null)
            {
                _executionCancellationSource.Cancel();
                _executionCancellationSource.Dispose();
                _executionCancellationSource = null;
            }

            // Unsubscribe from events
            if (_goalSelectionService != null)
            {
                _goalSelectionService.GoalSelectionChanged -= OnGoalSelectionChanged;
            }

            _isDisposed = true;
        }
    }
}