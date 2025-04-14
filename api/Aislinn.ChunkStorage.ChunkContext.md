# <a id="Aislinn_ChunkStorage_ChunkContext"></a> Class ChunkContext

Namespace: [Aislinn.ChunkStorage](Aislinn.ChunkStorage.md)  
Assembly: Aislinn.Core.dll  

```csharp
public static class ChunkContext
```

#### Inheritance

object ← 
[ChunkContext](Aislinn.ChunkStorage.ChunkContext.md)

## Methods

### <a id="Aislinn_ChunkStorage_ChunkContext_GetChunkAsync_System_Guid_System_String_"></a> GetChunkAsync\(Guid, string\)

```csharp
public static Task<Chunk> GetChunkAsync(Guid chunkId, string collectionId = null)
```

#### Parameters

`chunkId` [Guid](https://learn.microsoft.com/dotnet/api/system.guid)

`collectionId` [string](https://learn.microsoft.com/dotnet/api/system.string)

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[Chunk](Aislinn.Core.Models.Chunk.md)\>

### <a id="Aislinn_ChunkStorage_ChunkContext_Initialize_Aislinn_ChunkStorage_Interfaces_IChunkStore_System_String_"></a> Initialize\(IChunkStore, string\)

```csharp
public static void Initialize(IChunkStore store, string defaultCollectionId = "")
```

#### Parameters

`store` [IChunkStore](Aislinn.ChunkStorage.Interfaces.IChunkStore.md)

`defaultCollectionId` [string](https://learn.microsoft.com/dotnet/api/system.string)

