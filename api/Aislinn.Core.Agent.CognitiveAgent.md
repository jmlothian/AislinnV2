# <a id="Aislinn_Core_Agent_CognitiveAgent"></a> Class CognitiveAgent

Namespace: [Aislinn.Core.Agent](Aislinn.Core.Agent.md)  
Assembly: Aislinn.Agent.dll  

A cognitive agent that integrates memory, goals, context awareness, and procedural knowledge
to create an autonomous, adaptable intelligent entity.

```csharp
public class CognitiveAgent : IDisposable
```

#### Inheritance

object ← 
[CognitiveAgent](Aislinn.Core.Agent.CognitiveAgent.md)

#### Implements

[IDisposable](https://learn.microsoft.com/dotnet/api/system.idisposable)

## Constructors

### <a id="Aislinn_Core_Agent_CognitiveAgent__ctor_Aislinn_ChunkStorage_Interfaces_IChunkStore_Aislinn_VectorStorage_Storage_VectorStore_Aislinn_VectorStorage_Interfaces_IVectorizer_Aislinn_Core_Services_CognitiveTimeManager_Aislinn_Core_Cognitive_CognitiveMemorySystem_Aislinn_Core_Context_ContextContainer_Aislinn_Core_Relationships_RelationshipTraversalService_Aislinn_Core_Goals_GoalManagementService_Aislinn_Core_Goals_Selection_GoalSelectionService_Aislinn_Core_Goals_Execution_GoalExecutionService_Aislinn_Core_Procedural_ProcedureMatcher_Aislinn_Core_Services_ChunkActivationService_System_String_System_String_System_String_"></a> CognitiveAgent\(IChunkStore, VectorStore, IVectorizer, CognitiveTimeManager, CognitiveMemorySystem, ContextContainer, RelationshipTraversalService, GoalManagementService, GoalSelectionService, GoalExecutionService, ProcedureMatcher, ChunkActivationService, string, string, string\)

Creates a new cognitive agent with injected dependencies

```csharp
public CognitiveAgent(IChunkStore chunkStore, VectorStore vectorStore, IVectorizer vectorizer, CognitiveTimeManager timeManager, CognitiveMemorySystem memorySystem, ContextContainer contextContainer, RelationshipTraversalService relationshipService, GoalManagementService goalManagementService, GoalSelectionService goalSelectionService, GoalExecutionService goalExecutionService, ProcedureMatcher procedureMatcher, ChunkActivationService activationService, string chunkCollectionId = "default", string associationCollectionId = "default", string vectorCollectionId = "default")
```

#### Parameters

`chunkStore` IChunkStore

`vectorStore` VectorStore

`vectorizer` IVectorizer

`timeManager` CognitiveTimeManager

`memorySystem` CognitiveMemorySystem

`contextContainer` ContextContainer

`relationshipService` RelationshipTraversalService

`goalManagementService` GoalManagementService

`goalSelectionService` GoalSelectionService

`goalExecutionService` GoalExecutionService

`procedureMatcher` ProcedureMatcher

`activationService` ChunkActivationService

`chunkCollectionId` [string](https://learn.microsoft.com/dotnet/api/system.string)

`associationCollectionId` [string](https://learn.microsoft.com/dotnet/api/system.string)

`vectorCollectionId` [string](https://learn.microsoft.com/dotnet/api/system.string)

## Methods

### <a id="Aislinn_Core_Agent_CognitiveAgent_CreateGoalAsync_System_Guid_System_Collections_Generic_Dictionary_System_String_System_Object__System_Nullable_System_Guid__"></a> CreateGoalAsync\(Guid, Dictionary<string, object\>, Guid?\)

Create a new goal from a template

```csharp
public Task<Chunk> CreateGoalAsync(Guid templateId, Dictionary<string, object> parameters, Guid? parentGoalId = null)
```

#### Parameters

`templateId` [Guid](https://learn.microsoft.com/dotnet/api/system.guid)

`parameters` [Dictionary](https://learn.microsoft.com/dotnet/api/system.collections.generic.dictionary\-2)<[string](https://learn.microsoft.com/dotnet/api/system.string), object\>

`parentGoalId` [Guid](https://learn.microsoft.com/dotnet/api/system.guid)?

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<Chunk\>

### <a id="Aislinn_Core_Agent_CognitiveAgent_Dispose"></a> Dispose\(\)

Clean up resources

```csharp
public void Dispose()
```

### <a id="Aislinn_Core_Agent_CognitiveAgent_ForcePrimaryGoalAsync_System_Guid_"></a> ForcePrimaryGoalAsync\(Guid\)

Force a specific goal to be the primary goal

```csharp
public Task<bool> ForcePrimaryGoalAsync(Guid goalId)
```

#### Parameters

`goalId` [Guid](https://learn.microsoft.com/dotnet/api/system.guid)

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[bool](https://learn.microsoft.com/dotnet/api/system.boolean)\>

### <a id="Aislinn_Core_Agent_CognitiveAgent_GenerateResponseAsync"></a> GenerateResponseAsync\(\)

Generate a response based on current cognitive state

```csharp
public Task<string> GenerateResponseAsync()
```

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[string](https://learn.microsoft.com/dotnet/api/system.string)\>

### <a id="Aislinn_Core_Agent_CognitiveAgent_GetCurrentStateAsync"></a> GetCurrentStateAsync\(\)

Get a snapshot of the agent's current cognitive state

```csharp
public Task<AgentState> GetCurrentStateAsync()
```

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[AgentState](Aislinn.Core.Agent.AgentState.md)\>

### <a id="Aislinn_Core_Agent_CognitiveAgent_InitializeAsync"></a> InitializeAsync\(\)

Initialize the agent and its subsystems

```csharp
public Task InitializeAsync()
```

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task)

### <a id="Aislinn_Core_Agent_CognitiveAgent_ProcessInputAsync_System_String_System_Collections_Generic_Dictionary_System_String_System_Object__"></a> ProcessInputAsync\(string, Dictionary<string, object\>\)

Process input from the environment and update cognitive state

```csharp
public Task ProcessInputAsync(string input, Dictionary<string, object> metadata = null)
```

#### Parameters

`input` [string](https://learn.microsoft.com/dotnet/api/system.string)

`metadata` [Dictionary](https://learn.microsoft.com/dotnet/api/system.collections.generic.dictionary\-2)<[string](https://learn.microsoft.com/dotnet/api/system.string), object\>

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task)

### <a id="Aislinn_Core_Agent_CognitiveAgent_SaveState"></a> SaveState\(\)

Save the agent's state to persistent storage

```csharp
public void SaveState()
```

### <a id="Aislinn_Core_Agent_CognitiveAgent_UpdateContext_Aislinn_Core_Context_ContextContainer_ContextCategory_System_String_System_Object_System_Double_System_Double_"></a> UpdateContext\(ContextCategory, string, object, double, double\)

Update the agent's context with environmental information

```csharp
public void UpdateContext(ContextContainer.ContextCategory category, string factorName, object value, double importance = 0.5, double confidence = 1)
```

#### Parameters

`category` ContextContainer.ContextCategory

`factorName` [string](https://learn.microsoft.com/dotnet/api/system.string)

`value` object

`importance` [double](https://learn.microsoft.com/dotnet/api/system.double)

`confidence` [double](https://learn.microsoft.com/dotnet/api/system.double)

### <a id="Aislinn_Core_Agent_CognitiveAgent_CognitiveEvent"></a> CognitiveEvent

Event fired when a significant cognitive event occurs

```csharp
public event EventHandler<CognitiveEventArgs> CognitiveEvent
```

#### Event Type

 [EventHandler](https://learn.microsoft.com/dotnet/api/system.eventhandler\-1)<[CognitiveEventArgs](Aislinn.Core.Agent.CognitiveEventArgs.md)\>

