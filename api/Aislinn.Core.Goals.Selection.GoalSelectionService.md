# <a id="Aislinn_Core_Goals_Selection_GoalSelectionService"></a> Class GoalSelectionService

Namespace: [Aislinn.Core.Goals.Selection](Aislinn.Core.Goals.Selection.md)  
Assembly: Aislinn.Core.dll  

Service responsible for selecting which goals should be actively pursued
based on multiple factors including priority, activation, and context.

```csharp
public class GoalSelectionService : IDisposable
```

#### Inheritance

object ← 
[GoalSelectionService](Aislinn.Core.Goals.Selection.GoalSelectionService.md)

#### Implements

[IDisposable](https://learn.microsoft.com/dotnet/api/system.idisposable)

## Constructors

### <a id="Aislinn_Core_Goals_Selection_GoalSelectionService__ctor_Aislinn_ChunkStorage_Interfaces_IChunkStore_Aislinn_ChunkStorage_Interfaces_IAssociationStore_Aislinn_Core_Goals_GoalManagementService_Aislinn_Core_Services_ChunkActivationService_Aislinn_Core_Context_ContextContainer_System_String_System_String_Aislinn_Core_Goals_Selection_GoalSelectionService_GoalSelectionConfig_"></a> GoalSelectionService\(IChunkStore, IAssociationStore, GoalManagementService, ChunkActivationService, ContextContainer, string, string, GoalSelectionConfig\)

Initializes a new instance of the GoalSelectionService

```csharp
public GoalSelectionService(IChunkStore chunkStore, IAssociationStore associationStore, GoalManagementService goalManagementService, ChunkActivationService activationService, ContextContainer contextContainer, string chunkCollectionId = "default", string associationCollectionId = "default", GoalSelectionService.GoalSelectionConfig config = null)
```

#### Parameters

`chunkStore` [IChunkStore](Aislinn.ChunkStorage.Interfaces.IChunkStore.md)

`associationStore` [IAssociationStore](Aislinn.ChunkStorage.Interfaces.IAssociationStore.md)

`goalManagementService` [GoalManagementService](Aislinn.Core.Goals.GoalManagementService.md)

`activationService` [ChunkActivationService](Aislinn.Core.Services.ChunkActivationService.md)

`contextContainer` [ContextContainer](Aislinn.Core.Context.ContextContainer.md)

`chunkCollectionId` [string](https://learn.microsoft.com/dotnet/api/system.string)

`associationCollectionId` [string](https://learn.microsoft.com/dotnet/api/system.string)

`config` [GoalSelectionService](Aislinn.Core.Goals.Selection.GoalSelectionService.md).[GoalSelectionConfig](Aislinn.Core.Goals.Selection.GoalSelectionService.GoalSelectionConfig.md)

## Methods

### <a id="Aislinn_Core_Goals_Selection_GoalSelectionService_Dispose"></a> Dispose\(\)

Disposes resources

```csharp
public void Dispose()
```

### <a id="Aislinn_Core_Goals_Selection_GoalSelectionService_EvaluateGoalsAsync_System_Collections_Generic_Dictionary_System_String_System_Object__System_String_"></a> EvaluateGoalsAsync\(Dictionary<string, object\>, string\)

Evaluates all eligible goals and selects which to pursue

```csharp
public Task<List<GoalSelectionService.GoalEvaluationResult>> EvaluateGoalsAsync(Dictionary<string, object> additionalContext = null, string contextChangeReason = null)
```

#### Parameters

`additionalContext` [Dictionary](https://learn.microsoft.com/dotnet/api/system.collections.generic.dictionary\-2)<[string](https://learn.microsoft.com/dotnet/api/system.string), object\>

`contextChangeReason` [string](https://learn.microsoft.com/dotnet/api/system.string)

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[List](https://learn.microsoft.com/dotnet/api/system.collections.generic.list\-1)<[GoalSelectionService](Aislinn.Core.Goals.Selection.GoalSelectionService.md).[GoalEvaluationResult](Aislinn.Core.Goals.Selection.GoalSelectionService.GoalEvaluationResult.md)\>\>

### <a id="Aislinn_Core_Goals_Selection_GoalSelectionService_EvaluateSpecificGoalsAsync_System_Collections_Generic_List_System_Guid__System_Collections_Generic_Dictionary_System_String_System_Object__"></a> EvaluateSpecificGoalsAsync\(List<Guid\>, Dictionary<string, object\>\)

Evaluates a specific set of goals

```csharp
public Task<List<GoalSelectionService.GoalEvaluationResult>> EvaluateSpecificGoalsAsync(List<Guid> goalIds, Dictionary<string, object> additionalContext = null)
```

#### Parameters

`goalIds` [List](https://learn.microsoft.com/dotnet/api/system.collections.generic.list\-1)<[Guid](https://learn.microsoft.com/dotnet/api/system.guid)\>

`additionalContext` [Dictionary](https://learn.microsoft.com/dotnet/api/system.collections.generic.dictionary\-2)<[string](https://learn.microsoft.com/dotnet/api/system.string), object\>

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[List](https://learn.microsoft.com/dotnet/api/system.collections.generic.list\-1)<[GoalSelectionService](Aislinn.Core.Goals.Selection.GoalSelectionService.md).[GoalEvaluationResult](Aislinn.Core.Goals.Selection.GoalSelectionService.GoalEvaluationResult.md)\>\>

### <a id="Aislinn_Core_Goals_Selection_GoalSelectionService_ForceGoalSelectionAsync_System_Guid_"></a> ForceGoalSelectionAsync\(Guid\)

Forces a specific goal to be the primary goal

```csharp
public Task<bool> ForceGoalSelectionAsync(Guid goalId)
```

#### Parameters

`goalId` [Guid](https://learn.microsoft.com/dotnet/api/system.guid)

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[bool](https://learn.microsoft.com/dotnet/api/system.boolean)\>

### <a id="Aislinn_Core_Goals_Selection_GoalSelectionService_GetLastEvaluationResults"></a> GetLastEvaluationResults\(\)

Gets the last evaluation results

```csharp
public List<GoalSelectionService.GoalEvaluationResult> GetLastEvaluationResults()
```

#### Returns

 [List](https://learn.microsoft.com/dotnet/api/system.collections.generic.list\-1)<[GoalSelectionService](Aislinn.Core.Goals.Selection.GoalSelectionService.md).[GoalEvaluationResult](Aislinn.Core.Goals.Selection.GoalSelectionService.GoalEvaluationResult.md)\>

### <a id="Aislinn_Core_Goals_Selection_GoalSelectionService_GetPrimaryGoalAsync"></a> GetPrimaryGoalAsync\(\)

Gets the currently selected primary goal

```csharp
public Task<Chunk> GetPrimaryGoalAsync()
```

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[Chunk](Aislinn.Core.Models.Chunk.md)\>

### <a id="Aislinn_Core_Goals_Selection_GoalSelectionService_GetSecondaryGoalsAsync"></a> GetSecondaryGoalsAsync\(\)

Gets the currently selected secondary goals

```csharp
public Task<List<Chunk>> GetSecondaryGoalsAsync()
```

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[List](https://learn.microsoft.com/dotnet/api/system.collections.generic.list\-1)<[Chunk](Aislinn.Core.Models.Chunk.md)\>\>

### <a id="Aislinn_Core_Goals_Selection_GoalSelectionService_StartAutoEvaluation_System_Double_"></a> StartAutoEvaluation\(double\)

Starts automatic goal evaluation on a timer

```csharp
public void StartAutoEvaluation(double intervalMs = 1000)
```

#### Parameters

`intervalMs` [double](https://learn.microsoft.com/dotnet/api/system.double)

### <a id="Aislinn_Core_Goals_Selection_GoalSelectionService_StopAutoEvaluation"></a> StopAutoEvaluation\(\)

Stops automatic goal evaluation

```csharp
public void StopAutoEvaluation()
```

### <a id="Aislinn_Core_Goals_Selection_GoalSelectionService_GoalSelectionChanged"></a> GoalSelectionChanged

Event raised when goal selection changes

```csharp
public event EventHandler<GoalSelectionService.GoalSelectionChangedEventArgs> GoalSelectionChanged
```

#### Event Type

 [EventHandler](https://learn.microsoft.com/dotnet/api/system.eventhandler\-1)<[GoalSelectionService](Aislinn.Core.Goals.Selection.GoalSelectionService.md).[GoalSelectionChangedEventArgs](Aislinn.Core.Goals.Selection.GoalSelectionService.GoalSelectionChangedEventArgs.md)\>

