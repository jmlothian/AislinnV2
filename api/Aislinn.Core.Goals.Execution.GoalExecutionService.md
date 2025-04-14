# <a id="Aislinn_Core_Goals_Execution_GoalExecutionService"></a> Class GoalExecutionService

Namespace: [Aislinn.Core.Goals.Execution](Aislinn.Core.Goals.Execution.md)  
Assembly: Aislinn.Core.dll  

Handles the execution of goals by matching them with appropriate procedures,
executing the procedures, and monitoring progress.

```csharp
public class GoalExecutionService : IDisposable
```

#### Inheritance

object ← 
[GoalExecutionService](Aislinn.Core.Goals.Execution.GoalExecutionService.md)

#### Implements

[IDisposable](https://learn.microsoft.com/dotnet/api/system.idisposable)

## Constructors

### <a id="Aislinn_Core_Goals_Execution_GoalExecutionService__ctor_Aislinn_ChunkStorage_Interfaces_IChunkStore_Aislinn_ChunkStorage_Interfaces_IAssociationStore_Aislinn_Core_Goals_Selection_GoalSelectionService_Aislinn_Core_Goals_GoalManagementService_Aislinn_Core_Procedural_ProcedureMatcher_Aislinn_Core_Context_ContextContainer_System_String_System_String_Aislinn_Core_Goals_Execution_GoalExecutionService_GoalExecutionConfig_"></a> GoalExecutionService\(IChunkStore, IAssociationStore, GoalSelectionService, GoalManagementService, ProcedureMatcher, ContextContainer, string, string, GoalExecutionConfig\)

Initializes a new instance of the GoalExecutionService

```csharp
public GoalExecutionService(IChunkStore chunkStore, IAssociationStore associationStore, GoalSelectionService goalSelectionService, GoalManagementService goalManagementService, ProcedureMatcher procedureMatcher, ContextContainer contextContainer, string chunkCollectionId = "default", string associationCollectionId = "default", GoalExecutionService.GoalExecutionConfig config = null)
```

#### Parameters

`chunkStore` [IChunkStore](Aislinn.ChunkStorage.Interfaces.IChunkStore.md)

`associationStore` [IAssociationStore](Aislinn.ChunkStorage.Interfaces.IAssociationStore.md)

`goalSelectionService` [GoalSelectionService](Aislinn.Core.Goals.Selection.GoalSelectionService.md)

`goalManagementService` [GoalManagementService](Aislinn.Core.Goals.GoalManagementService.md)

`procedureMatcher` [ProcedureMatcher](Aislinn.Core.Procedural.ProcedureMatcher.md)

`contextContainer` [ContextContainer](Aislinn.Core.Context.ContextContainer.md)

`chunkCollectionId` [string](https://learn.microsoft.com/dotnet/api/system.string)

`associationCollectionId` [string](https://learn.microsoft.com/dotnet/api/system.string)

`config` [GoalExecutionService](Aislinn.Core.Goals.Execution.GoalExecutionService.md).[GoalExecutionConfig](Aislinn.Core.Goals.Execution.GoalExecutionService.GoalExecutionConfig.md)

## Properties

### <a id="Aislinn_Core_Goals_Execution_GoalExecutionService_CurrentGoalId"></a> CurrentGoalId

Gets the ID of the currently executing goal

```csharp
public Guid? CurrentGoalId { get; }
```

#### Property Value

 [Guid](https://learn.microsoft.com/dotnet/api/system.guid)?

### <a id="Aislinn_Core_Goals_Execution_GoalExecutionService_CurrentProcedureId"></a> CurrentProcedureId

Gets the ID of the currently executing procedure

```csharp
public Guid? CurrentProcedureId { get; }
```

#### Property Value

 [Guid](https://learn.microsoft.com/dotnet/api/system.guid)?

### <a id="Aislinn_Core_Goals_Execution_GoalExecutionService_ExecutionContext"></a> ExecutionContext

Gets the current execution context

```csharp
public Dictionary<string, object> ExecutionContext { get; }
```

#### Property Value

 [Dictionary](https://learn.microsoft.com/dotnet/api/system.collections.generic.dictionary\-2)<[string](https://learn.microsoft.com/dotnet/api/system.string), object\>

### <a id="Aislinn_Core_Goals_Execution_GoalExecutionService_ExecutionProgressValue"></a> ExecutionProgressValue

Gets the current execution progress (0-1)

```csharp
public double ExecutionProgressValue { get; }
```

#### Property Value

 [double](https://learn.microsoft.com/dotnet/api/system.double)

### <a id="Aislinn_Core_Goals_Execution_GoalExecutionService_IsPaused"></a> IsPaused

Gets whether execution is currently paused

```csharp
public bool IsPaused { get; }
```

#### Property Value

 [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### <a id="Aislinn_Core_Goals_Execution_GoalExecutionService_State"></a> State

Gets the current execution state

```csharp
public GoalExecutionService.ExecutionState State { get; }
```

#### Property Value

 [GoalExecutionService](Aislinn.Core.Goals.Execution.GoalExecutionService.md).[ExecutionState](Aislinn.Core.Goals.Execution.GoalExecutionService.ExecutionState.md)

## Methods

### <a id="Aislinn_Core_Goals_Execution_GoalExecutionService_CancelExecution"></a> CancelExecution\(\)

Cancels the current execution

```csharp
public bool CancelExecution()
```

#### Returns

 [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### <a id="Aislinn_Core_Goals_Execution_GoalExecutionService_Dispose"></a> Dispose\(\)

Disposes resources

```csharp
public void Dispose()
```

### <a id="Aislinn_Core_Goals_Execution_GoalExecutionService_ExecuteGoalAsync_System_Guid_System_Collections_Generic_Dictionary_System_String_System_Object__"></a> ExecuteGoalAsync\(Guid, Dictionary<string, object\>\)

Executes a goal by finding and running an appropriate procedure

```csharp
public Task<bool> ExecuteGoalAsync(Guid goalId, Dictionary<string, object> initialContext = null)
```

#### Parameters

`goalId` [Guid](https://learn.microsoft.com/dotnet/api/system.guid)

`initialContext` [Dictionary](https://learn.microsoft.com/dotnet/api/system.collections.generic.dictionary\-2)<[string](https://learn.microsoft.com/dotnet/api/system.string), object\>

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[bool](https://learn.microsoft.com/dotnet/api/system.boolean)\>

### <a id="Aislinn_Core_Goals_Execution_GoalExecutionService_ExecuteSelectedGoalAsync"></a> ExecuteSelectedGoalAsync\(\)

Executes the currently selected goal

```csharp
public Task<bool> ExecuteSelectedGoalAsync()
```

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[bool](https://learn.microsoft.com/dotnet/api/system.boolean)\>

### <a id="Aislinn_Core_Goals_Execution_GoalExecutionService_PauseExecution"></a> PauseExecution\(\)

Pauses the current execution

```csharp
public bool PauseExecution()
```

#### Returns

 [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### <a id="Aislinn_Core_Goals_Execution_GoalExecutionService_ResumeExecution"></a> ResumeExecution\(\)

Resumes a paused execution

```csharp
public bool ResumeExecution()
```

#### Returns

 [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### <a id="Aislinn_Core_Goals_Execution_GoalExecutionService_ExecutionCompleted"></a> ExecutionCompleted

```csharp
public event EventHandler<GoalExecutionService.GoalExecutionEventArgs> ExecutionCompleted
```

#### Event Type

 [EventHandler](https://learn.microsoft.com/dotnet/api/system.eventhandler\-1)<[GoalExecutionService](Aislinn.Core.Goals.Execution.GoalExecutionService.md).[GoalExecutionEventArgs](Aislinn.Core.Goals.Execution.GoalExecutionService.GoalExecutionEventArgs.md)\>

### <a id="Aislinn_Core_Goals_Execution_GoalExecutionService_ExecutionFailed"></a> ExecutionFailed

```csharp
public event EventHandler<GoalExecutionService.GoalExecutionEventArgs> ExecutionFailed
```

#### Event Type

 [EventHandler](https://learn.microsoft.com/dotnet/api/system.eventhandler\-1)<[GoalExecutionService](Aislinn.Core.Goals.Execution.GoalExecutionService.md).[GoalExecutionEventArgs](Aislinn.Core.Goals.Execution.GoalExecutionService.GoalExecutionEventArgs.md)\>

### <a id="Aislinn_Core_Goals_Execution_GoalExecutionService_ExecutionPaused"></a> ExecutionPaused

```csharp
public event EventHandler<GoalExecutionService.GoalExecutionEventArgs> ExecutionPaused
```

#### Event Type

 [EventHandler](https://learn.microsoft.com/dotnet/api/system.eventhandler\-1)<[GoalExecutionService](Aislinn.Core.Goals.Execution.GoalExecutionService.md).[GoalExecutionEventArgs](Aislinn.Core.Goals.Execution.GoalExecutionService.GoalExecutionEventArgs.md)\>

### <a id="Aislinn_Core_Goals_Execution_GoalExecutionService_ExecutionProgress"></a> ExecutionProgress

```csharp
public event EventHandler<GoalExecutionService.GoalExecutionProgressEventArgs> ExecutionProgress
```

#### Event Type

 [EventHandler](https://learn.microsoft.com/dotnet/api/system.eventhandler\-1)<[GoalExecutionService](Aislinn.Core.Goals.Execution.GoalExecutionService.md).[GoalExecutionProgressEventArgs](Aislinn.Core.Goals.Execution.GoalExecutionService.GoalExecutionProgressEventArgs.md)\>

### <a id="Aislinn_Core_Goals_Execution_GoalExecutionService_ExecutionResumed"></a> ExecutionResumed

```csharp
public event EventHandler<GoalExecutionService.GoalExecutionEventArgs> ExecutionResumed
```

#### Event Type

 [EventHandler](https://learn.microsoft.com/dotnet/api/system.eventhandler\-1)<[GoalExecutionService](Aislinn.Core.Goals.Execution.GoalExecutionService.md).[GoalExecutionEventArgs](Aislinn.Core.Goals.Execution.GoalExecutionService.GoalExecutionEventArgs.md)\>

### <a id="Aislinn_Core_Goals_Execution_GoalExecutionService_ExecutionStarted"></a> ExecutionStarted

```csharp
public event EventHandler<GoalExecutionService.GoalExecutionEventArgs> ExecutionStarted
```

#### Event Type

 [EventHandler](https://learn.microsoft.com/dotnet/api/system.eventhandler\-1)<[GoalExecutionService](Aislinn.Core.Goals.Execution.GoalExecutionService.md).[GoalExecutionEventArgs](Aislinn.Core.Goals.Execution.GoalExecutionService.GoalExecutionEventArgs.md)\>

