# <a id="RAINA_ChunkManager"></a> Class ChunkManager

Namespace: [RAINA](RAINA.md)  
Assembly: Aislinn.Raina.dll  

```csharp
public class ChunkManager
```

#### Inheritance

object ← 
[ChunkManager](RAINA.ChunkManager.md)

## Constructors

### <a id="RAINA_ChunkManager__ctor_Aislinn_Core_Cognitive_CognitiveMemorySystem_"></a> ChunkManager\(CognitiveMemorySystem\)

```csharp
public ChunkManager(CognitiveMemorySystem memorySystem)
```

#### Parameters

`memorySystem` CognitiveMemorySystem

## Methods

### <a id="RAINA_ChunkManager_CreateChunkAsync_System_String_System_String_System_Collections_Generic_Dictionary_System_String_System_Object__"></a> CreateChunkAsync\(string, string, Dictionary<string, object\>\)

```csharp
public Task<Chunk> CreateChunkAsync(string chunkType, string name, Dictionary<string, object> slots = null)
```

#### Parameters

`chunkType` [string](https://learn.microsoft.com/dotnet/api/system.string)

`name` [string](https://learn.microsoft.com/dotnet/api/system.string)

`slots` [Dictionary](https://learn.microsoft.com/dotnet/api/system.collections.generic.dictionary\-2)<[string](https://learn.microsoft.com/dotnet/api/system.string), object\>

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<Chunk\>

### <a id="RAINA_ChunkManager_GetChunkAsync_System_Guid_"></a> GetChunkAsync\(Guid\)

```csharp
public Task<Chunk> GetChunkAsync(Guid chunkId)
```

#### Parameters

`chunkId` [Guid](https://learn.microsoft.com/dotnet/api/system.guid)

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<Chunk\>

