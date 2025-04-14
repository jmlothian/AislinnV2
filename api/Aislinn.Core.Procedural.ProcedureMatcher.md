# <a id="Aislinn_Core_Procedural_ProcedureMatcher"></a> Class ProcedureMatcher

Namespace: [Aislinn.Core.Procedural](Aislinn.Core.Procedural.md)  
Assembly: Aislinn.Core.dll  

Matches goals with appropriate procedures based on goal type, context,
and other factors. Also handles procedure activation and selection.

```csharp
public class ProcedureMatcher
```

#### Inheritance

object ← 
[ProcedureMatcher](Aislinn.Core.Procedural.ProcedureMatcher.md)

## Constructors

### <a id="Aislinn_Core_Procedural_ProcedureMatcher__ctor_Aislinn_ChunkStorage_Interfaces_IChunkStore_Aislinn_Core_Services_ChunkActivationService_System_String_Aislinn_Core_Procedural_ProcedureMatcher_ProcedureMatcherConfig_"></a> ProcedureMatcher\(IChunkStore, ChunkActivationService, string, ProcedureMatcherConfig\)

Initializes a new instance of the ProcedureMatcher

```csharp
public ProcedureMatcher(IChunkStore chunkStore, ChunkActivationService activationService, string chunkCollectionId = "default", ProcedureMatcher.ProcedureMatcherConfig config = null)
```

#### Parameters

`chunkStore` [IChunkStore](Aislinn.ChunkStorage.Interfaces.IChunkStore.md)

`activationService` [ChunkActivationService](Aislinn.Core.Services.ChunkActivationService.md)

`chunkCollectionId` [string](https://learn.microsoft.com/dotnet/api/system.string)

`config` [ProcedureMatcher](Aislinn.Core.Procedural.ProcedureMatcher.md).[ProcedureMatcherConfig](Aislinn.Core.Procedural.ProcedureMatcher.ProcedureMatcherConfig.md)

## Methods

### <a id="Aislinn_Core_Procedural_ProcedureMatcher_CreateProcedureAsync_Aislinn_Core_Procedural_ProcedureChunk_"></a> CreateProcedureAsync\(ProcedureChunk\)

Creates a new procedure and adds it to the store

```csharp
public Task<ProcedureChunk> CreateProcedureAsync(ProcedureChunk procedure)
```

#### Parameters

`procedure` [ProcedureChunk](Aislinn.Core.Procedural.ProcedureChunk.md)

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[ProcedureChunk](Aislinn.Core.Procedural.ProcedureChunk.md)\>

### <a id="Aislinn_Core_Procedural_ProcedureMatcher_FindProceduresForGoalAsync_Aislinn_Core_Models_Chunk_System_Collections_Generic_Dictionary_System_String_System_Object__"></a> FindProceduresForGoalAsync\(Chunk, Dictionary<string, object\>\)

Finds procedures that can achieve a specific goal

```csharp
public Task<List<ProcedureMatcher.ProcedureMatchResult>> FindProceduresForGoalAsync(Chunk goalChunk, Dictionary<string, object> context = null)
```

#### Parameters

`goalChunk` [Chunk](Aislinn.Core.Models.Chunk.md)

`context` [Dictionary](https://learn.microsoft.com/dotnet/api/system.collections.generic.dictionary\-2)<[string](https://learn.microsoft.com/dotnet/api/system.string), object\>

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[List](https://learn.microsoft.com/dotnet/api/system.collections.generic.list\-1)<[ProcedureMatcher](Aislinn.Core.Procedural.ProcedureMatcher.md).[ProcedureMatchResult](Aislinn.Core.Procedural.ProcedureMatcher.ProcedureMatchResult.md)\>\>

### <a id="Aislinn_Core_Procedural_ProcedureMatcher_GetProcedureByIdAsync_System_Guid_"></a> GetProcedureByIdAsync\(Guid\)

Gets a procedure by ID

```csharp
public Task<ProcedureChunk> GetProcedureByIdAsync(Guid procedureId)
```

#### Parameters

`procedureId` [Guid](https://learn.microsoft.com/dotnet/api/system.guid)

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[ProcedureChunk](Aislinn.Core.Procedural.ProcedureChunk.md)\>

### <a id="Aislinn_Core_Procedural_ProcedureMatcher_UpdateProcedureAsync_Aislinn_Core_Procedural_ProcedureChunk_"></a> UpdateProcedureAsync\(ProcedureChunk\)

Updates an existing procedure

```csharp
public Task<bool> UpdateProcedureAsync(ProcedureChunk procedure)
```

#### Parameters

`procedure` [ProcedureChunk](Aislinn.Core.Procedural.ProcedureChunk.md)

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[bool](https://learn.microsoft.com/dotnet/api/system.boolean)\>

