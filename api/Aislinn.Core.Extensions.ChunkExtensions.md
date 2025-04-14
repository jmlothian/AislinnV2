# <a id="Aislinn_Core_Extensions_ChunkExtensions"></a> Class ChunkExtensions

Namespace: [Aislinn.Core.Extensions](Aislinn.Core.Extensions.md)  
Assembly: Aislinn.Core.dll  

```csharp
public static class ChunkExtensions
```

#### Inheritance

object ← 
[ChunkExtensions](Aislinn.Core.Extensions.ChunkExtensions.md)

## Methods

### <a id="Aislinn_Core_Extensions_ChunkExtensions_GetChunkFromSlot_Aislinn_Core_Models_Chunk_System_String_"></a> GetChunkFromSlot\(Chunk, string\)

```csharp
public static Chunk GetChunkFromSlot(this Chunk chunk, string slotName)
```

#### Parameters

`chunk` [Chunk](Aislinn.Core.Models.Chunk.md)

`slotName` [string](https://learn.microsoft.com/dotnet/api/system.string)

#### Returns

 [Chunk](Aislinn.Core.Models.Chunk.md)

### <a id="Aislinn_Core_Extensions_ChunkExtensions_GetChunkFromSlotAsync_Aislinn_Core_Models_Chunk_System_String_System_String_"></a> GetChunkFromSlotAsync\(Chunk, string, string\)

```csharp
public static Task<Chunk> GetChunkFromSlotAsync(this Chunk chunk, string slotName, string collectionId = null)
```

#### Parameters

`chunk` [Chunk](Aislinn.Core.Models.Chunk.md)

`slotName` [string](https://learn.microsoft.com/dotnet/api/system.string)

`collectionId` [string](https://learn.microsoft.com/dotnet/api/system.string)

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[Chunk](Aislinn.Core.Models.Chunk.md)\>

### <a id="Aislinn_Core_Extensions_ChunkExtensions_SetChunkReference_Aislinn_Core_Models_Chunk_System_String_Aislinn_Core_Models_Chunk_"></a> SetChunkReference\(Chunk, string, Chunk\)

```csharp
public static void SetChunkReference(this Chunk chunk, string slotName, Chunk referencedChunk)
```

#### Parameters

`chunk` [Chunk](Aislinn.Core.Models.Chunk.md)

`slotName` [string](https://learn.microsoft.com/dotnet/api/system.string)

`referencedChunk` [Chunk](Aislinn.Core.Models.Chunk.md)

