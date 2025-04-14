# <a id="Aislinn_ChunkStorage_Interfaces_IChunkCollection"></a> Interface IChunkCollection

Namespace: [Aislinn.ChunkStorage.Interfaces](Aislinn.ChunkStorage.Interfaces.md)  
Assembly: Aislinn.Core.dll  

```csharp
public interface IChunkCollection
```

## Properties

### <a id="Aislinn_ChunkStorage_Interfaces_IChunkCollection_Id"></a> Id

```csharp
string Id { get; }
```

#### Property Value

 [string](https://learn.microsoft.com/dotnet/api/system.string)

## Methods

### <a id="Aislinn_ChunkStorage_Interfaces_IChunkCollection_AddChunkAsync_Aislinn_Core_Models_Chunk_"></a> AddChunkAsync\(Chunk\)

```csharp
Task<Chunk> AddChunkAsync(Chunk chunk)
```

#### Parameters

`chunk` [Chunk](Aislinn.Core.Models.Chunk.md)

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[Chunk](Aislinn.Core.Models.Chunk.md)\>

### <a id="Aislinn_ChunkStorage_Interfaces_IChunkCollection_DeleteChunkAsync_System_Guid_"></a> DeleteChunkAsync\(Guid\)

```csharp
Task<bool> DeleteChunkAsync(Guid chunkId)
```

#### Parameters

`chunkId` [Guid](https://learn.microsoft.com/dotnet/api/system.guid)

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[bool](https://learn.microsoft.com/dotnet/api/system.boolean)\>

### <a id="Aislinn_ChunkStorage_Interfaces_IChunkCollection_GetAllChunksAsync"></a> GetAllChunksAsync\(\)

```csharp
Task<List<Chunk>> GetAllChunksAsync()
```

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[List](https://learn.microsoft.com/dotnet/api/system.collections.generic.list\-1)<[Chunk](Aislinn.Core.Models.Chunk.md)\>\>

### <a id="Aislinn_ChunkStorage_Interfaces_IChunkCollection_GetChunkAsync_System_Guid_"></a> GetChunkAsync\(Guid\)

```csharp
Task<Chunk> GetChunkAsync(Guid chunkId)
```

#### Parameters

`chunkId` [Guid](https://learn.microsoft.com/dotnet/api/system.guid)

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[Chunk](Aislinn.Core.Models.Chunk.md)\>

### <a id="Aislinn_ChunkStorage_Interfaces_IChunkCollection_GetChunkCountAsync"></a> GetChunkCountAsync\(\)

```csharp
Task<int> GetChunkCountAsync()
```

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[int](https://learn.microsoft.com/dotnet/api/system.int32)\>

### <a id="Aislinn_ChunkStorage_Interfaces_IChunkCollection_UpdateChunkAsync_Aislinn_Core_Models_Chunk_"></a> UpdateChunkAsync\(Chunk\)

```csharp
Task<bool> UpdateChunkAsync(Chunk chunk)
```

#### Parameters

`chunk` [Chunk](Aislinn.Core.Models.Chunk.md)

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[bool](https://learn.microsoft.com/dotnet/api/system.boolean)\>

