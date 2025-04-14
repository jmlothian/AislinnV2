# <a id="Aislinn_ChunkStorage_Storage_InMemoryChunkCollection"></a> Class InMemoryChunkCollection

Namespace: [Aislinn.ChunkStorage.Storage](Aislinn.ChunkStorage.Storage.md)  
Assembly: Aislinn.Core.dll  

```csharp
public class InMemoryChunkCollection : IChunkCollection
```

#### Inheritance

object ← 
[InMemoryChunkCollection](Aislinn.ChunkStorage.Storage.InMemoryChunkCollection.md)

#### Implements

[IChunkCollection](Aislinn.ChunkStorage.Interfaces.IChunkCollection.md)

## Constructors

### <a id="Aislinn_ChunkStorage_Storage_InMemoryChunkCollection__ctor_System_String_"></a> InMemoryChunkCollection\(string\)

```csharp
public InMemoryChunkCollection(string id)
```

#### Parameters

`id` [string](https://learn.microsoft.com/dotnet/api/system.string)

## Properties

### <a id="Aislinn_ChunkStorage_Storage_InMemoryChunkCollection_Id"></a> Id

```csharp
public string Id { get; }
```

#### Property Value

 [string](https://learn.microsoft.com/dotnet/api/system.string)

## Methods

### <a id="Aislinn_ChunkStorage_Storage_InMemoryChunkCollection_AddChunkAsync_Aislinn_Core_Models_Chunk_"></a> AddChunkAsync\(Chunk\)

```csharp
public Task<Chunk> AddChunkAsync(Chunk chunk)
```

#### Parameters

`chunk` [Chunk](Aislinn.Core.Models.Chunk.md)

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[Chunk](Aislinn.Core.Models.Chunk.md)\>

### <a id="Aislinn_ChunkStorage_Storage_InMemoryChunkCollection_DeleteChunkAsync_System_Guid_"></a> DeleteChunkAsync\(Guid\)

```csharp
public Task<bool> DeleteChunkAsync(Guid chunkId)
```

#### Parameters

`chunkId` [Guid](https://learn.microsoft.com/dotnet/api/system.guid)

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[bool](https://learn.microsoft.com/dotnet/api/system.boolean)\>

### <a id="Aislinn_ChunkStorage_Storage_InMemoryChunkCollection_GetAllChunksAsync"></a> GetAllChunksAsync\(\)

```csharp
public Task<List<Chunk>> GetAllChunksAsync()
```

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[List](https://learn.microsoft.com/dotnet/api/system.collections.generic.list\-1)<[Chunk](Aislinn.Core.Models.Chunk.md)\>\>

### <a id="Aislinn_ChunkStorage_Storage_InMemoryChunkCollection_GetChunkAsync_System_Guid_"></a> GetChunkAsync\(Guid\)

```csharp
public Task<Chunk> GetChunkAsync(Guid chunkId)
```

#### Parameters

`chunkId` [Guid](https://learn.microsoft.com/dotnet/api/system.guid)

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[Chunk](Aislinn.Core.Models.Chunk.md)\>

### <a id="Aislinn_ChunkStorage_Storage_InMemoryChunkCollection_GetChunkCountAsync"></a> GetChunkCountAsync\(\)

```csharp
public Task<int> GetChunkCountAsync()
```

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[int](https://learn.microsoft.com/dotnet/api/system.int32)\>

### <a id="Aislinn_ChunkStorage_Storage_InMemoryChunkCollection_UpdateChunkAsync_Aislinn_Core_Models_Chunk_"></a> UpdateChunkAsync\(Chunk\)

```csharp
public Task<bool> UpdateChunkAsync(Chunk chunk)
```

#### Parameters

`chunk` [Chunk](Aislinn.Core.Models.Chunk.md)

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[bool](https://learn.microsoft.com/dotnet/api/system.boolean)\>

