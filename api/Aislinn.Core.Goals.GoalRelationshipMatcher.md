# <a id="Aislinn_Core_Goals_GoalRelationshipMatcher"></a> Class GoalRelationshipMatcher

Namespace: [Aislinn.Core.Goals](Aislinn.Core.Goals.md)  
Assembly: Aislinn.Core.dll  

Extends goal evaluation with the ability to match semantic relationships,
including transitive relationships across multiple hops.

```csharp
public class GoalRelationshipMatcher
```

#### Inheritance

object ← 
[GoalRelationshipMatcher](Aislinn.Core.Goals.GoalRelationshipMatcher.md)

## Constructors

### <a id="Aislinn_Core_Goals_GoalRelationshipMatcher__ctor_Aislinn_ChunkStorage_Interfaces_IChunkStore_Aislinn_Core_Relationships_RelationshipTraversalService_System_String_"></a> GoalRelationshipMatcher\(IChunkStore, RelationshipTraversalService, string\)

```csharp
public GoalRelationshipMatcher(IChunkStore chunkStore, RelationshipTraversalService relationshipService, string chunkCollectionId = "default")
```

#### Parameters

`chunkStore` [IChunkStore](Aislinn.ChunkStorage.Interfaces.IChunkStore.md)

`relationshipService` [RelationshipTraversalService](Aislinn.Core.Relationships.RelationshipTraversalService.md)

`chunkCollectionId` [string](https://learn.microsoft.com/dotnet/api/system.string)

## Properties

### <a id="Aislinn_Core_Goals_GoalRelationshipMatcher_EnableVerboseLogging"></a> EnableVerboseLogging

```csharp
public bool EnableVerboseLogging { get; set; }
```

#### Property Value

 [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

## Methods

### <a id="Aislinn_Core_Goals_GoalRelationshipMatcher_EvaluateRelationshipPatternAsync_System_String_Aislinn_Core_Models_Chunk_"></a> EvaluateRelationshipPatternAsync\(string, Chunk\)

Evaluates a relationship pattern within a goal's completion criteria

```csharp
public Task<bool> EvaluateRelationshipPatternAsync(string pattern, Chunk goal)
```

#### Parameters

`pattern` [string](https://learn.microsoft.com/dotnet/api/system.string)

`goal` [Chunk](Aislinn.Core.Models.Chunk.md)

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[bool](https://learn.microsoft.com/dotnet/api/system.boolean)\>

