# <a id="Aislinn_Core_Relationships_RelationshipTraversalService"></a> Class RelationshipTraversalService

Namespace: [Aislinn.Core.Relationships](Aislinn.Core.Relationships.md)  
Assembly: Aislinn.Core.dll  

Service for traversing relationships between chunks, supporting transitive relationship checking
and caching for performance optimization.

```csharp
public class RelationshipTraversalService
```

#### Inheritance

object ← 
[RelationshipTraversalService](Aislinn.Core.Relationships.RelationshipTraversalService.md)

## Constructors

### <a id="Aislinn_Core_Relationships_RelationshipTraversalService__ctor_Aislinn_ChunkStorage_Interfaces_IChunkStore_Aislinn_ChunkStorage_Interfaces_IAssociationStore_System_String_System_String_"></a> RelationshipTraversalService\(IChunkStore, IAssociationStore, string, string\)

Initializes a new instance of the RelationshipTraversalService

```csharp
public RelationshipTraversalService(IChunkStore chunkStore, IAssociationStore associationStore, string chunkCollectionId = "default", string associationCollectionId = "default")
```

#### Parameters

`chunkStore` [IChunkStore](Aislinn.ChunkStorage.Interfaces.IChunkStore.md)

`associationStore` [IAssociationStore](Aislinn.ChunkStorage.Interfaces.IAssociationStore.md)

`chunkCollectionId` [string](https://learn.microsoft.com/dotnet/api/system.string)

`associationCollectionId` [string](https://learn.microsoft.com/dotnet/api/system.string)

## Methods

### <a id="Aislinn_Core_Relationships_RelationshipTraversalService_CheckRelationshipAsync_System_Guid_System_String_System_Guid_System_Int32_"></a> CheckRelationshipAsync\(Guid, string, Guid, int\)

Checks if a relationship exists between source and target, potentially through a chain of relationships
Uses bidirectional breadth-first search for efficiency.

```csharp
public Task<bool> CheckRelationshipAsync(Guid sourceId, string relationshipType, Guid targetId, int maxDepth = 5)
```

#### Parameters

`sourceId` [Guid](https://learn.microsoft.com/dotnet/api/system.guid)

`relationshipType` [string](https://learn.microsoft.com/dotnet/api/system.string)

`targetId` [Guid](https://learn.microsoft.com/dotnet/api/system.guid)

`maxDepth` [int](https://learn.microsoft.com/dotnet/api/system.int32)

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[bool](https://learn.microsoft.com/dotnet/api/system.boolean)\>

### <a id="Aislinn_Core_Relationships_RelationshipTraversalService_ClearAllCaches"></a> ClearAllCaches\(\)

Completely clears all caches

```csharp
public void ClearAllCaches()
```

### <a id="Aislinn_Core_Relationships_RelationshipTraversalService_GetDirectlyRelatedChunksAsync_System_Guid_System_String_"></a> GetDirectlyRelatedChunksAsync\(Guid, string\)

Gets all chunks directly related to the source by the specified relationship type

```csharp
public Task<List<Guid>> GetDirectlyRelatedChunksAsync(Guid sourceId, string relationshipType)
```

#### Parameters

`sourceId` [Guid](https://learn.microsoft.com/dotnet/api/system.guid)

`relationshipType` [string](https://learn.microsoft.com/dotnet/api/system.string)

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[List](https://learn.microsoft.com/dotnet/api/system.collections.generic.list\-1)<[Guid](https://learn.microsoft.com/dotnet/api/system.guid)\>\>

### <a id="Aislinn_Core_Relationships_RelationshipTraversalService_HasDirectRelationshipAsync_System_Guid_System_String_System_Guid_"></a> HasDirectRelationshipAsync\(Guid, string, Guid\)

Checks if a direct relationship exists between source and target

```csharp
public Task<bool> HasDirectRelationshipAsync(Guid sourceId, string relationshipType, Guid targetId)
```

#### Parameters

`sourceId` [Guid](https://learn.microsoft.com/dotnet/api/system.guid)

`relationshipType` [string](https://learn.microsoft.com/dotnet/api/system.string)

`targetId` [Guid](https://learn.microsoft.com/dotnet/api/system.guid)

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[bool](https://learn.microsoft.com/dotnet/api/system.boolean)\>

### <a id="Aislinn_Core_Relationships_RelationshipTraversalService_InvalidateCacheForChunk_System_Guid_"></a> InvalidateCacheForChunk\(Guid\)

Invalidates all cached entries related to a specific chunk
Called when relationships for a chunk are modified

```csharp
public void InvalidateCacheForChunk(Guid chunkId)
```

#### Parameters

`chunkId` [Guid](https://learn.microsoft.com/dotnet/api/system.guid)

