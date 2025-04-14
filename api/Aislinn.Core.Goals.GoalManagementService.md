# <a id="Aislinn_Core_Goals_GoalManagementService"></a> Class GoalManagementService

Namespace: [Aislinn.Core.Goals](Aislinn.Core.Goals.md)  
Assembly: Aislinn.Core.dll  

Service for managing goals, goal templates, and instantiation within the cognitive architecture

```csharp
public class GoalManagementService
```

#### Inheritance

object ← 
[GoalManagementService](Aislinn.Core.Goals.GoalManagementService.md)

#### Extension Methods

[GoalManagementServiceExtensions.EvaluateExpressionWithRelationshipsAsync\(GoalManagementService, string, Chunk, GoalRelationshipMatcher\)](Aislinn.Core.Goals.GoalManagementServiceExtensions.md\#Aislinn\_Core\_Goals\_GoalManagementServiceExtensions\_EvaluateExpressionWithRelationshipsAsync\_Aislinn\_Core\_Goals\_GoalManagementService\_System\_String\_Aislinn\_Core\_Models\_Chunk\_Aislinn\_Core\_Goals\_GoalRelationshipMatcher\_)

## Constructors

### <a id="Aislinn_Core_Goals_GoalManagementService__ctor_Aislinn_ChunkStorage_Interfaces_IChunkStore_Aislinn_ChunkStorage_Interfaces_IAssociationStore_Aislinn_Core_Services_ChunkActivationService_Aislinn_Core_Relationships_RelationshipTraversalService_System_String_System_String_"></a> GoalManagementService\(IChunkStore, IAssociationStore, ChunkActivationService, RelationshipTraversalService, string, string\)

```csharp
public GoalManagementService(IChunkStore chunkStore, IAssociationStore associationStore, ChunkActivationService activationService, RelationshipTraversalService relationshipService, string chunkCollectionId = "default", string associationCollectionId = "default")
```

#### Parameters

`chunkStore` [IChunkStore](Aislinn.ChunkStorage.Interfaces.IChunkStore.md)

`associationStore` [IAssociationStore](Aislinn.ChunkStorage.Interfaces.IAssociationStore.md)

`activationService` [ChunkActivationService](Aislinn.Core.Services.ChunkActivationService.md)

`relationshipService` [RelationshipTraversalService](Aislinn.Core.Relationships.RelationshipTraversalService.md)

`chunkCollectionId` [string](https://learn.microsoft.com/dotnet/api/system.string)

`associationCollectionId` [string](https://learn.microsoft.com/dotnet/api/system.string)

## Methods

### <a id="Aislinn_Core_Goals_GoalManagementService_CreateGoalDependencyAsync_System_Guid_System_Guid_"></a> CreateGoalDependencyAsync\(Guid, Guid\)

Creates a dependency relationship between goals

```csharp
public Task CreateGoalDependencyAsync(Guid dependentGoalId, Guid prerequisiteGoalId)
```

#### Parameters

`dependentGoalId` [Guid](https://learn.microsoft.com/dotnet/api/system.guid)

`prerequisiteGoalId` [Guid](https://learn.microsoft.com/dotnet/api/system.guid)

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task)

### <a id="Aislinn_Core_Goals_GoalManagementService_CreateGoalTemplateAsync_System_String_System_Double_System_Collections_Generic_List_System_String__System_Collections_Generic_List_System_String__System_Collections_Generic_Dictionary_System_String_System_Object__System_Collections_Generic_Dictionary_System_String_System_String__"></a> CreateGoalTemplateAsync\(string, double, List<string\>, List<string\>, Dictionary<string, object\>, Dictionary<string, string\>\)

Creates a new goal template with parameter specifications

```csharp
public Task<Chunk> CreateGoalTemplateAsync(string name, double defaultPriority, List<string> requiredParameters, List<string> optionalParameters = null, Dictionary<string, object> parameterDefaults = null, Dictionary<string, string> parameterConstraints = null)
```

#### Parameters

`name` [string](https://learn.microsoft.com/dotnet/api/system.string)

`defaultPriority` [double](https://learn.microsoft.com/dotnet/api/system.double)

`requiredParameters` [List](https://learn.microsoft.com/dotnet/api/system.collections.generic.list\-1)<[string](https://learn.microsoft.com/dotnet/api/system.string)\>

`optionalParameters` [List](https://learn.microsoft.com/dotnet/api/system.collections.generic.list\-1)<[string](https://learn.microsoft.com/dotnet/api/system.string)\>

`parameterDefaults` [Dictionary](https://learn.microsoft.com/dotnet/api/system.collections.generic.dictionary\-2)<[string](https://learn.microsoft.com/dotnet/api/system.string), object\>

`parameterConstraints` [Dictionary](https://learn.microsoft.com/dotnet/api/system.collections.generic.dictionary\-2)<[string](https://learn.microsoft.com/dotnet/api/system.string), [string](https://learn.microsoft.com/dotnet/api/system.string)\>

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[Chunk](Aislinn.Core.Models.Chunk.md)\>

### <a id="Aislinn_Core_Goals_GoalManagementService_CreateMultiPathGoalAsync_System_String_System_Double_System_Collections_Generic_List_System_Object__"></a> CreateMultiPathGoalAsync\(string, double, List<object\>\)

Creates a goal with multiple possible fulfillment paths

```csharp
public Task<Chunk> CreateMultiPathGoalAsync(string name, double priority, List<object> completionPaths)
```

#### Parameters

`name` [string](https://learn.microsoft.com/dotnet/api/system.string)

`priority` [double](https://learn.microsoft.com/dotnet/api/system.double)

`completionPaths` [List](https://learn.microsoft.com/dotnet/api/system.collections.generic.list\-1)<object\>

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[Chunk](Aislinn.Core.Models.Chunk.md)\>

### <a id="Aislinn_Core_Goals_GoalManagementService_EvaluateGoalActivationsAsync_System_Double_"></a> EvaluateGoalActivationsAsync\(double\)

Evaluates all active goals and updates their activation levels based on priority and context

```csharp
public Task EvaluateGoalActivationsAsync(double contextBoost = 0.2)
```

#### Parameters

`contextBoost` [double](https://learn.microsoft.com/dotnet/api/system.double)

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task)

### <a id="Aislinn_Core_Goals_GoalManagementService_EvaluateGoalCompletionAsync_Aislinn_Core_Models_Chunk_"></a> EvaluateGoalCompletionAsync\(Chunk\)

Evaluates if a goal's completion criteria are met and updates its status accordingly

```csharp
public Task<bool> EvaluateGoalCompletionAsync(Chunk goal)
```

#### Parameters

`goal` [Chunk](Aislinn.Core.Models.Chunk.md)

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[bool](https://learn.microsoft.com/dotnet/api/system.boolean)\>

### <a id="Aislinn_Core_Goals_GoalManagementService_InstantiateGoalAsync_System_Guid_System_Collections_Generic_Dictionary_System_String_System_Object__System_Nullable_System_Guid__"></a> InstantiateGoalAsync\(Guid, Dictionary<string, object\>, Guid?\)

Instantiates a goal from a template with specific parameter values

```csharp
public Task<Chunk> InstantiateGoalAsync(Guid templateId, Dictionary<string, object> parameters, Guid? parentGoalId = null)
```

#### Parameters

`templateId` [Guid](https://learn.microsoft.com/dotnet/api/system.guid)

`parameters` [Dictionary](https://learn.microsoft.com/dotnet/api/system.collections.generic.dictionary\-2)<[string](https://learn.microsoft.com/dotnet/api/system.string), object\>

`parentGoalId` [Guid](https://learn.microsoft.com/dotnet/api/system.guid)?

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[Chunk](Aislinn.Core.Models.Chunk.md)\>

### <a id="Aislinn_Core_Goals_GoalManagementService_UpdateGoalStatusAsync_System_Guid_System_String_"></a> UpdateGoalStatusAsync\(Guid, string\)

Updates the status of a goal and handles dependent goals

```csharp
public Task UpdateGoalStatusAsync(Guid goalId, string newStatus)
```

#### Parameters

`goalId` [Guid](https://learn.microsoft.com/dotnet/api/system.guid)

`newStatus` [string](https://learn.microsoft.com/dotnet/api/system.string)

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task)

