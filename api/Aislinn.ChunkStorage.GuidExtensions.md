# <a id="Aislinn_ChunkStorage_GuidExtensions"></a> Class GuidExtensions

Namespace: [Aislinn.ChunkStorage](Aislinn.ChunkStorage.md)  
Assembly: Aislinn.Core.dll  

```csharp
public static class GuidExtensions
```

#### Inheritance

object ← 
[GuidExtensions](Aislinn.ChunkStorage.GuidExtensions.md)

## Methods

### <a id="Aislinn_ChunkStorage_GuidExtensions_ToChunk_System_Guid_System_String_"></a> ToChunk\(Guid, string\)

```csharp
public static Chunk ToChunk(this Guid chunkId, string collectionId = null)
```

#### Parameters

`chunkId` [Guid](https://learn.microsoft.com/dotnet/api/system.guid)

`collectionId` [string](https://learn.microsoft.com/dotnet/api/system.string)

#### Returns

 [Chunk](Aislinn.Core.Models.Chunk.md)

### <a id="Aislinn_ChunkStorage_GuidExtensions_ToChunkAsync_System_Guid_System_String_"></a> ToChunkAsync\(Guid, string\)

```csharp
public static Task<Chunk> ToChunkAsync(this Guid chunkId, string collectionId = null)
```

#### Parameters

`chunkId` [Guid](https://learn.microsoft.com/dotnet/api/system.guid)

`collectionId` [string](https://learn.microsoft.com/dotnet/api/system.string)

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[Chunk](Aislinn.Core.Models.Chunk.md)\>

