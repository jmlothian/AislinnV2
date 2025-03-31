using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Aislinn.ChunkStorage;
using Aislinn.ChunkStorage.Interfaces;
using Aislinn.Core.Cognitive;
using Aislinn.Core.Context;
using Aislinn.Core.Goals;
using Aislinn.Core.Goals.Execution;
using Aislinn.Core.Goals.Selection;
using Aislinn.Core.Models;
using Aislinn.Core.Procedural;
using Aislinn.Core.Relationships;
using Aislinn.Core.Services;
using Aislinn.VectorStorage.Interfaces;
using Aislinn.VectorStorage.Storage;

namespace Aislinn.Core.Agent
{
    public class CognitiveEventArgs : EventArgs
    {
        public string EventType { get; set; }
        public Dictionary<string, object> EventData { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
    public class AgentState
    {
        public List<Chunk> WorkingMemoryContents { get; set; } = new List<Chunk>();
        public List<Chunk> PrimedChunks { get; set; } = new List<Chunk>();
        public Chunk CurrentPrimaryGoal { get; set; }
        public List<Chunk> CurrentSecondaryGoals { get; set; } = new List<Chunk>();
        public Dictionary<ContextContainer.ContextCategory, Dictionary<string, object>> CurrentContext { get; set; }
        public double SystemTime { get; set; }
        public DateTime LastActivityTime { get; set; }
        public GoalExecutionService.ExecutionState ExecutionState { get; set; }
    }
    /// <summary>
    /// A cognitive agent that integrates memory, goals, context awareness, and procedural knowledge
    /// to create an autonomous, adaptable intelligent entity.
    /// </summary>
    public class CognitiveAgent : IDisposable
    {
        // Core cognitive systems - injected via constructor
        private readonly CognitiveMemorySystem _memorySystem;
        private readonly CognitiveTimeManager _timeManager;
        private readonly ContextContainer _contextContainer;
        private readonly RelationshipTraversalService _relationshipService;
        private readonly GoalManagementService _goalManagementService;
        private readonly GoalSelectionService _goalSelectionService;
        private readonly GoalExecutionService _goalExecutionService;
        private readonly ProcedureMatcher _procedureMatcher;
        private readonly ChunkActivationService _activationService;
        private readonly IChunkStore _chunkStore;
        private readonly IVectorizer _vectorizer;
        private readonly VectorStore _vectorStore;

        // Configuration
        private readonly string _chunkCollectionId;
        private readonly string _associationCollectionId;
        private readonly string _vectorCollectionId;

        // Agent state
        private bool _isInitialized = false;
        private DateTime _lastActivityTime;
        private bool _isCognitiveProcessingActive = false;
        private System.Timers.Timer _cognitiveProcessingTimer;
        private bool _isDisposed = false;

        /// <summary>
        /// Event fired when a significant cognitive event occurs
        /// </summary>
        public event EventHandler<CognitiveEventArgs> CognitiveEvent;

        /// <summary>
        /// Creates a new cognitive agent with injected dependencies
        /// </summary>
        public CognitiveAgent(
            IChunkStore chunkStore,
            VectorStore vectorStore,
            IVectorizer vectorizer,
            CognitiveTimeManager timeManager,
            CognitiveMemorySystem memorySystem,
            ContextContainer contextContainer,
            RelationshipTraversalService relationshipService,
            GoalManagementService goalManagementService,
            GoalSelectionService goalSelectionService,
            GoalExecutionService goalExecutionService,
            ProcedureMatcher procedureMatcher,
            ChunkActivationService activationService,
            string chunkCollectionId = "default",
            string associationCollectionId = "default",
            string vectorCollectionId = "default")
        {
            _chunkStore = chunkStore ?? throw new ArgumentNullException(nameof(chunkStore));
            _vectorStore = vectorStore ?? throw new ArgumentNullException(nameof(vectorStore));
            _vectorizer = vectorizer ?? throw new ArgumentNullException(nameof(vectorizer));
            _timeManager = timeManager ?? throw new ArgumentNullException(nameof(timeManager));
            _memorySystem = memorySystem ?? throw new ArgumentNullException(nameof(memorySystem));
            _contextContainer = contextContainer ?? throw new ArgumentNullException(nameof(contextContainer));
            _relationshipService = relationshipService ?? throw new ArgumentNullException(nameof(relationshipService));
            _goalManagementService = goalManagementService ?? throw new ArgumentNullException(nameof(goalManagementService));
            _goalSelectionService = goalSelectionService ?? throw new ArgumentNullException(nameof(goalSelectionService));
            _goalExecutionService = goalExecutionService ?? throw new ArgumentNullException(nameof(goalExecutionService));
            _procedureMatcher = procedureMatcher ?? throw new ArgumentNullException(nameof(procedureMatcher));
            _activationService = activationService ?? throw new ArgumentNullException(nameof(activationService));

            _chunkCollectionId = chunkCollectionId;
            _associationCollectionId = associationCollectionId;
            _vectorCollectionId = vectorCollectionId;

            _lastActivityTime = DateTime.Now;

            // Subscribe to goal execution events
            _goalExecutionService.ExecutionCompleted += OnGoalExecutionCompleted;
            _goalExecutionService.ExecutionFailed += OnGoalExecutionFailed;
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
            ChunkContext.Initialize(_chunkStore, _chunkCollectionId);

            // Ensure vector collection exists
            await _vectorStore.GetOrCreateCollectionAsync(_vectorCollectionId, _vectorizer);

            // Start cognitive processes
            _memorySystem.StartWorkingMemoryRefresh();
            _goalSelectionService.StartAutoEvaluation();

            // Initialize cognitive processing timer
            _cognitiveProcessingTimer = new System.Timers.Timer(1000); // 1 second interval
            _cognitiveProcessingTimer.Elapsed += async (sender, e) => await RunCognitiveProcessingCycleAsync();
            _cognitiveProcessingTimer.AutoReset = true;
            _cognitiveProcessingTimer.Start();

            _isInitialized = true;

            // Raise initialization event
            RaiseCognitiveEvent("AgentInitialized", new Dictionary<string, object>
            {
                { "SystemTime", _timeManager.GetCurrentTime() }
            });
        }

        /// <summary>
        /// Process input from the environment and update cognitive state
        /// </summary>
        public async Task ProcessInputAsync(string input, Dictionary<string, object> metadata = null)
        {
            if (!_isInitialized)
                await InitializeAsync();

            _lastActivityTime = DateTime.Now;

            // Process the input
            try
            {
                // Convert input to vector and store it
                var vectorCollection = await _vectorStore.GetCollectionAsync(_vectorCollectionId);
                if (vectorCollection != null)
                {
                    // Convert metadata to format expected by VectorItem
                    var vectorMetadata = metadata != null
                        ? metadata.ToDictionary(kv => kv.Key, kv => kv.Value?.ToString())
                        : new Dictionary<string, string>();

                    var vectorItem = await vectorCollection.AddVectorAsync(input, vectorMetadata);

                    // Store input as a chunk
                    var chunkCollection = await _chunkStore.GetCollectionAsync(_chunkCollectionId);
                    var inputChunk = new Chunk
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
                            inputChunk.Slots[kvp.Key] = new ModelSlot { Name = kvp.Key, Value = kvp.Value };
                        }
                    }

                    // Add input text to slot
                    inputChunk.Slots["Text"] = new ModelSlot { Name = "Text", Value = input };

                    // Add chunk to storage
                    var storedChunk = await chunkCollection.AddChunkAsync(inputChunk);

                    // Activate the chunk to bring it into working memory
                    await _memorySystem.ActivateChunkAsync(storedChunk.ID, "UserInput", 1.0);

                    // Update context with the new input
                    _contextContainer.UpdateContextFactor(
                        ContextContainer.ContextCategory.Environment,
                        "LatestInput",
                        input,
                        importance: 0.8,
                        confidence: 1.0);

                    // Raise input received event
                    RaiseCognitiveEvent("InputReceived", new Dictionary<string, object>
                    {
                        { "Input", input },
                        { "ChunkId", storedChunk.ID }
                    });
                }
            }
            catch (Exception ex)
            {
                // Raise error event
                RaiseCognitiveEvent("InputProcessingError", new Dictionary<string, object>
                {
                    { "Input", input },
                    { "Error", ex.Message }
                });
            }
        }

        /// <summary>
        /// Update the agent's context with environmental information
        /// </summary>
        public void UpdateContext(ContextContainer.ContextCategory category, string factorName, object value,
            double importance = 0.5, double confidence = 1.0)
        {
            _contextContainer.UpdateContextFactor(category, factorName, value, importance, confidence);
        }

        /// <summary>
        /// Run a single cognitive processing cycle
        /// </summary>
        private async Task RunCognitiveProcessingCycleAsync()
        {
            if (!_isInitialized || _isCognitiveProcessingActive || _isDisposed)
                return;

            try
            {
                _isCognitiveProcessingActive = true;

                // Update time
                _timeManager.UpdateTime();

                // Apply decay to chunks
                await _activationService.ApplyDecayAsync();

                // Update working memory contents based on current activations
                await _memorySystem.ManualRefreshCycleAsync();

                // Update context from working memory
                var workingMemoryContents = await _memorySystem.GetWorkingMemoryContentsAsync();
                await _contextContainer.UpdateContextFromWorkingMemoryAsync(workingMemoryContents);

                // Clean up expired context factors
                _contextContainer.CleanupExpiredFactors();

                // Evaluate goals and update activations
                await _goalManagementService.EvaluateGoalActivationsAsync(contextBoost: 0.2);

                // If not currently executing a goal, check if it should start
                if (_goalExecutionService.State == GoalExecutionService.ExecutionState.Idle)
                {
                    var primaryGoal = await _goalSelectionService.GetPrimaryGoalAsync();
                    if (primaryGoal != null && ShouldExecuteGoal(primaryGoal))
                    {
                        await _goalExecutionService.ExecuteGoalAsync(primaryGoal.ID);
                    }
                }

                // Raise cognitive cycle event
                RaiseCognitiveEvent("CognitiveCycle", new Dictionary<string, object>
                {
                    { "SystemTime", _timeManager.GetCurrentTime() },
                    { "WorkingMemoryCount", workingMemoryContents.Count }
                });
            }
            catch (Exception ex)
            {
                // Raise error event
                RaiseCognitiveEvent("CognitiveError", new Dictionary<string, object>
                {
                    { "Error", ex.Message },
                    { "StackTrace", ex.StackTrace }
                });
            }
            finally
            {
                _isCognitiveProcessingActive = false;
            }
        }

        /// <summary>
        /// Decide whether to execute a goal based on current state
        /// </summary>
        private bool ShouldExecuteGoal(Chunk goalChunk)
        {
            // Check if goal status is ready for execution
            if (goalChunk.Slots.TryGetValue(GoalManagementService.GoalSlots.Status, out var statusSlot) &&
                statusSlot.Value is string status)
            {
                // Only execute goals that are Active or Pending
                return status == GoalManagementService.GoalSlots.StatusValues.Active ||
                       status == GoalManagementService.GoalSlots.StatusValues.Pending;
            }

            return false;
        }

        /// <summary>
        /// Handler for goal execution completion
        /// </summary>
        private void OnGoalExecutionCompleted(object sender, GoalExecutionService.GoalExecutionEventArgs e)
        {
            RaiseCognitiveEvent("GoalCompleted", new Dictionary<string, object>
            {
                { "GoalId", e.GoalId },
                { "GoalName", e.GoalName },
                { "ExecutionTime", e.ExecutionTime.TotalSeconds }
            });
        }

        /// <summary>
        /// Handler for goal execution failure
        /// </summary>
        private void OnGoalExecutionFailed(object sender, GoalExecutionService.GoalExecutionEventArgs e)
        {
            RaiseCognitiveEvent("GoalFailed", new Dictionary<string, object>
            {
                { "GoalId", e.GoalId },
                { "GoalName", e.GoalName },
                { "Error", e.Error?.Message },
                { "ExecutionTime", e.ExecutionTime.TotalSeconds }
            });
        }

        /// <summary>
        /// Raise a cognitive event to subscribers
        /// </summary>
        private void RaiseCognitiveEvent(string eventType, Dictionary<string, object> eventData)
        {
            try
            {
                CognitiveEvent?.Invoke(this, new CognitiveEventArgs
                {
                    EventType = eventType,
                    EventData = eventData,
                    Timestamp = DateTime.Now
                });
            }
            catch
            {
                // Ignore errors in event handlers
            }
        }

        /// <summary>
        /// Create a new goal from a template
        /// </summary>
        public async Task<Chunk> CreateGoalAsync(Guid templateId, Dictionary<string, object> parameters, Guid? parentGoalId = null)
        {
            return await _goalManagementService.InstantiateGoalAsync(templateId, parameters, parentGoalId);
        }

        /// <summary>
        /// Force a specific goal to be the primary goal
        /// </summary>
        public async Task<bool> ForcePrimaryGoalAsync(Guid goalId)
        {
            return await _goalSelectionService.ForceGoalSelectionAsync(goalId);
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

            // Get current primary goal
            var primaryGoal = await _goalSelectionService.GetPrimaryGoalAsync();

            // Get current context summary
            var contextSnapshot = _contextContainer.CreateContextSnapshot();

            // Simple response generation based on agent state
            var response = new System.Text.StringBuilder();

            // Add primary goal if present
            if (primaryGoal != null)
            {
                response.AppendLine($"Current goal: {primaryGoal.Name}");

                // Add goal status
                if (primaryGoal.Slots.TryGetValue(GoalManagementService.GoalSlots.Status, out var statusSlot))
                {
                    response.AppendLine($"Goal status: {statusSlot.Value}");
                }
            }
            else
            {
                response.AppendLine("No active goal at the moment.");
            }

            // Add some context information
            if (contextSnapshot.ContainsKey(ContextContainer.ContextCategory.Environment))
            {
                var environmentContext = contextSnapshot[ContextContainer.ContextCategory.Environment];
                if (environmentContext.Count > 0)
                {
                    response.AppendLine("\nEnvironment context:");
                    foreach (var factor in environmentContext.Take(3))
                    {
                        response.AppendLine($"- {factor.Key}: {factor.Value}");
                    }
                }
            }

            // Add working memory contents
            response.AppendLine("\nThinking about:");
            foreach (var chunk in workingMemoryContents.Take(5))
            {
                response.AppendLine($"- {chunk.Name} ({chunk.ActivationLevel:F2})");
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
            var primaryGoal = await _goalSelectionService.GetPrimaryGoalAsync();
            var secondaryGoals = await _goalSelectionService.GetSecondaryGoalsAsync();
            var contextSnapshot = _contextContainer.CreateContextSnapshot();

            return new AgentState
            {
                WorkingMemoryContents = workingMemory,
                PrimedChunks = primedChunks,
                SystemTime = _timeManager.GetCurrentTime(),
                LastActivityTime = _lastActivityTime,
                CurrentPrimaryGoal = primaryGoal,
                CurrentSecondaryGoals = secondaryGoals,
                CurrentContext = contextSnapshot,
                ExecutionState = _goalExecutionService.State
            };
        }

        /// <summary>
        /// Save the agent's state to persistent storage
        /// </summary>
        public void SaveState()
        {
            _timeManager.SaveState();
            // Could add more state saving here
        }

        /// <summary>
        /// Clean up resources
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
                return;

            _cognitiveProcessingTimer?.Stop();
            _cognitiveProcessingTimer?.Dispose();

            // Unsubscribe from events
            _goalExecutionService.ExecutionCompleted -= OnGoalExecutionCompleted;
            _goalExecutionService.ExecutionFailed -= OnGoalExecutionFailed;

            // Allow the injected services to handle their own disposal
            // This avoids direct disposal responsibility from the agent

            SaveState();
            _isDisposed = true;
        }
    }
}