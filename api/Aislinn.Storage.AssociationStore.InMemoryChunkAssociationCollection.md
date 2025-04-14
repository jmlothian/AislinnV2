# <a id="Aislinn_Storage_AssociationStore_InMemoryChunkAssociationCollection"></a> Class InMemoryChunkAssociationCollection

Namespace: [Aislinn.Storage.AssociationStore](Aislinn.Storage.AssociationStore.md)  
Assembly: Aislinn.Core.dll  

```csharp
public class InMemoryChunkAssociationCollection : IChunkAssociationCollection
```

#### Inheritance

object ← 
[InMemoryChunkAssociationCollection](Aislinn.Storage.AssociationStore.InMemoryChunkAssociationCollection.md)

#### Implements

[IChunkAssociationCollection](Aislinn.ChunkStorage.Interfaces.IChunkAssociationCollection.md)

## Constructors

### <a id="Aislinn_Storage_AssociationStore_InMemoryChunkAssociationCollection__ctor_System_String_"></a> InMemoryChunkAssociationCollection\(string\)

```csharp
public InMemoryChunkAssociationCollection(string id)
```

#### Parameters

`id` [string](https://learn.microsoft.com/dotnet/api/system.string)

## Properties

### <a id="Aislinn_Storage_AssociationStore_InMemoryChunkAssociationCollection_Id"></a> Id

```csharp
public string Id { get; }
```

#### Property Value

 [string](https://learn.microsoft.com/dotnet/api/system.string)

## Methods

### <a id="Aislinn_Storage_AssociationStore_InMemoryChunkAssociationCollection_AddAssociationAsync_Aislinn_Core_Models_ChunkAssociation_"></a> AddAssociationAsync\(ChunkAssociation\)

```csharp
public Task<ChunkAssociation> AddAssociationAsync(ChunkAssociation association)
```

#### Parameters

`association` [ChunkAssociation](Aislinn.Core.Models.ChunkAssociation.md)

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[ChunkAssociation](Aislinn.Core.Models.ChunkAssociation.md)\>

### <a id="Aislinn_Storage_AssociationStore_InMemoryChunkAssociationCollection_DeleteAssociationAsync_System_Guid_System_Guid_"></a> DeleteAssociationAsync\(Guid, Guid\)

```csharp
public Task<bool> DeleteAssociationAsync(Guid chunkAId, Guid chunkBId)
```

#### Parameters

`chunkAId` [Guid](https://learn.microsoft.com/dotnet/api/system.guid)

`chunkBId` [Guid](https://learn.microsoft.com/dotnet/api/system.guid)

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[bool](https://learn.microsoft.com/dotnet/api/system.boolean)\>

### <a id="Aislinn_Storage_AssociationStore_InMemoryChunkAssociationCollection_GetAssociationAsync_System_Guid_System_Guid_"></a> GetAssociationAsync\(Guid, Guid\)

```csharp
public Task<ChunkAssociation> GetAssociationAsync(Guid chunkAId, Guid chunkBId)
```

#### Parameters

`chunkAId` [Guid](https://learn.microsoft.com/dotnet/api/system.guid)

`chunkBId` [Guid](https://learn.microsoft.com/dotnet/api/system.guid)

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[ChunkAssociation](Aislinn.Core.Models.ChunkAssociation.md)\>

### <a id="Aislinn_Storage_AssociationStore_InMemoryChunkAssociationCollection_GetAssociationCountAsync"></a> GetAssociationCountAsync\(\)

```csharp
public Task<int> GetAssociationCountAsync()
```

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[int](https://learn.microsoft.com/dotnet/api/system.int32)\>

### <a id="Aislinn_Storage_AssociationStore_InMemoryChunkAssociationCollection_GetAssociationsForChunkAsync_System_Guid_"></a> GetAssociationsForChunkAsync\(Guid\)

```csharp
public Task<List<ChunkAssociation>> GetAssociationsForChunkAsync(Guid chunkId)
```

#### Parameters

`chunkId` [Guid](https://learn.microsoft.com/dotnet/api/system.guid)

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[List](https://learn.microsoft.com/dotnet/api/system.collections.generic.list\-1)<[ChunkAssociation](Aislinn.Core.Models.ChunkAssociation.md)\>\>

### <a id="Aislinn_Storage_AssociationStore_InMemoryChunkAssociationCollection_UpdateAssociationAsync_Aislinn_Core_Models_ChunkAssociation_"></a> UpdateAssociationAsync\(ChunkAssociation\)

```csharp
public Task<bool> UpdateAssociationAsync(ChunkAssociation association)
```

#### Parameters

`association` [ChunkAssociation](Aislinn.Core.Models.ChunkAssociation.md)

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[bool](https://learn.microsoft.com/dotnet/api/system.boolean)\>

