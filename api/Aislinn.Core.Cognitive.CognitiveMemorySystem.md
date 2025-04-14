# <a id="Aislinn_Core_Cognitive_CognitiveMemorySystem"></a> Class CognitiveMemorySystem

Namespace: [Aislinn.Core.Cognitive](Aislinn.Core.Cognitive.md)  
Assembly: Aislinn.Core.dll  

Coordinates different memory systems including working memory, long-term memory,
and spreading activation. Serves as the central memory manager for a cognitive agent.

```csharp
public class CognitiveMemorySystem : IDisposable
```

#### Inheritance

object ← 
[CognitiveMemorySystem](Aislinn.Core.Cognitive.CognitiveMemorySystem.md)

#### Implements

[IDisposable](https://learn.microsoft.com/dotnet/api/system.idisposable)

## Constructors

### <a id="Aislinn_Core_Cognitive_CognitiveMemorySystem__ctor_Aislinn_ChunkStorage_Interfaces_IChunkStore_Aislinn_ChunkStorage_Interfaces_IAssociationStore_Aislinn_Core_Activation_IActivationModel_Aislinn_Core_Services_CognitiveTimeManager_Aislinn_Core_Activation_ActivationParametersRegistry_System_String_System_String_"></a> CognitiveMemorySystem\(IChunkStore, IAssociationStore, IActivationModel, CognitiveTimeManager, ActivationParametersRegistry, string, string\)

Creates a new cognitive memory system with working memory and activation components

```csharp
public CognitiveMemorySystem(IChunkStore chunkStore, IAssociationStore associationStore, IActivationModel activationModel, CognitiveTimeManager timeManager, ActivationParametersRegistry parametersRegistry, string chunkCollectionId = "default", string associationCollectionId = "default")
```

#### Parameters

`chunkStore` [IChunkStore](Aislinn.ChunkStorage.Interfaces.IChunkStore.md)

`associationStore` [IAssociationStore](Aislinn.ChunkStorage.Interfaces.IAssociationStore.md)

`activationModel` [IActivationModel](Aislinn.Core.Activation.IActivationModel.md)

`timeManager` [CognitiveTimeManager](Aislinn.Core.Services.CognitiveTimeManager.md)

`parametersRegistry` [ActivationParametersRegistry](Aislinn.Core.Activation.ActivationParametersRegistry.md)

`chunkCollectionId` [string](https://learn.microsoft.com/dotnet/api/system.string)

`associationCollectionId` [string](https://learn.microsoft.com/dotnet/api/system.string)

## Methods

### <a id="Aislinn_Core_Cognitive_CognitiveMemorySystem_ActivateChunkAsync_System_Guid_System_String_System_Double_"></a> ActivateChunkAsync\(Guid, string, double\)

Activate a chunk and optionally bring it into working memory

```csharp
public Task<Chunk> ActivateChunkAsync(Guid chunkId, string emotionName = null, double activationBoost = 1)
```

#### Parameters

`chunkId` [Guid](https://learn.microsoft.com/dotnet/api/system.guid)

`emotionName` [string](https://learn.microsoft.com/dotnet/api/system.string)

`activationBoost` [double](https://learn.microsoft.com/dotnet/api/system.double)

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[Chunk](Aislinn.Core.Models.Chunk.md)\>

### <a id="Aislinn_Core_Cognitive_CognitiveMemorySystem_AddChunkAsync_Aislinn_Core_Models_Chunk_"></a> AddChunkAsync\(Chunk\)

Add a new chunk to memory

```csharp
public Task<Chunk> AddChunkAsync(Chunk chunk)
```

#### Parameters

`chunk` [Chunk](Aislinn.Core.Models.Chunk.md)

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[Chunk](Aislinn.Core.Models.Chunk.md)\>

### <a id="Aislinn_Core_Cognitive_CognitiveMemorySystem_ApplyDecayAsync"></a> ApplyDecayAsync\(\)

Apply activation decay to all chunks in the system

```csharp
public Task ApplyDecayAsync()
```

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task)

### <a id="Aislinn_Core_Cognitive_CognitiveMemorySystem_ClearWorkingMemory"></a> ClearWorkingMemory\(\)

Clear all contents from working memory

```csharp
public void ClearWorkingMemory()
```

### <a id="Aislinn_Core_Cognitive_CognitiveMemorySystem_CreateAssociationAsync_System_Guid_System_Guid_System_String_System_String_System_Double_System_Double_"></a> CreateAssociationAsync\(Guid, Guid, string, string, double, double\)

Create an association between two chunks

```csharp
public Task<ChunkAssociation> CreateAssociationAsync(Guid chunkAId, Guid chunkBId, string relationAtoB, string relationBtoA, double initialWeightAtoB = 0.5, double initialWeightBtoA = 0.5)
```

#### Parameters

`chunkAId` [Guid](https://learn.microsoft.com/dotnet/api/system.guid)

`chunkBId` [Guid](https://learn.microsoft.com/dotnet/api/system.guid)

`relationAtoB` [string](https://learn.microsoft.com/dotnet/api/system.string)

`relationBtoA` [string](https://learn.microsoft.com/dotnet/api/system.string)

`initialWeightAtoB` [double](https://learn.microsoft.com/dotnet/api/system.double)

`initialWeightBtoA` [double](https://learn.microsoft.com/dotnet/api/system.double)

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[ChunkAssociation](Aislinn.Core.Models.ChunkAssociation.md)\>

### <a id="Aislinn_Core_Cognitive_CognitiveMemorySystem_Dispose"></a> Dispose\(\)

Disposes of resources

```csharp
public void Dispose()
```

### <a id="Aislinn_Core_Cognitive_CognitiveMemorySystem_FindSimilarChunksAsync_System_String_System_Int32_System_Double_"></a> FindSimilarChunksAsync\(string, int, double\)

Find chunks that are semantically similar to a text query

```csharp
public Task<List<Chunk>> FindSimilarChunksAsync(string query, int topN = 5, double minSimilarity = 0.6)
```

#### Parameters

`query` [string](https://learn.microsoft.com/dotnet/api/system.string)

`topN` [int](https://learn.microsoft.com/dotnet/api/system.int32)

`minSimilarity` [double](https://learn.microsoft.com/dotnet/api/system.double)

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[List](https://learn.microsoft.com/dotnet/api/system.collections.generic.list\-1)<[Chunk](Aislinn.Core.Models.Chunk.md)\>\>

### <a id="Aislinn_Core_Cognitive_CognitiveMemorySystem_ForceChunkIntoWorkingMemoryAsync_System_Guid_"></a> ForceChunkIntoWorkingMemoryAsync\(Guid\)

Manually move a chunk into working memory

```csharp
public Task<bool> ForceChunkIntoWorkingMemoryAsync(Guid chunkId)
```

#### Parameters

`chunkId` [Guid](https://learn.microsoft.com/dotnet/api/system.guid)

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[bool](https://learn.microsoft.com/dotnet/api/system.boolean)\>

### <a id="Aislinn_Core_Cognitive_CognitiveMemorySystem_GetActiveChunksAsync_System_Double_"></a> GetActiveChunksAsync\(double\)

Get all chunks above an activation threshold

```csharp
public Task<List<Chunk>> GetActiveChunksAsync(double threshold = 0.1)
```

#### Parameters

`threshold` [double](https://learn.microsoft.com/dotnet/api/system.double)

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[List](https://learn.microsoft.com/dotnet/api/system.collections.generic.list\-1)<[Chunk](Aislinn.Core.Models.Chunk.md)\>\>

### <a id="Aislinn_Core_Cognitive_CognitiveMemorySystem_GetAssociationCollectionAsync"></a> GetAssociationCollectionAsync\(\)

```csharp
public Task<IChunkAssociationCollection> GetAssociationCollectionAsync()
```

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[IChunkAssociationCollection](Aislinn.ChunkStorage.Interfaces.IChunkAssociationCollection.md)\>

### <a id="Aislinn_Core_Cognitive_CognitiveMemorySystem_GetChunkAsync_System_Guid_"></a> GetChunkAsync\(Guid\)

Retrieve a chunk from memory

```csharp
public Task<Chunk> GetChunkAsync(Guid chunkId)
```

#### Parameters

`chunkId` [Guid](https://learn.microsoft.com/dotnet/api/system.guid)

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[Chunk](Aislinn.Core.Models.Chunk.md)\>

### <a id="Aislinn_Core_Cognitive_CognitiveMemorySystem_GetCurrentCognitiveTime"></a> GetCurrentCognitiveTime\(\)

```csharp
public double GetCurrentCognitiveTime()
```

#### Returns

 [double](https://learn.microsoft.com/dotnet/api/system.double)

### <a id="Aislinn_Core_Cognitive_CognitiveMemorySystem_GetPrimedChunksAsync"></a> GetPrimedChunksAsync\(\)

Get chunks that are primed but not in working memory

```csharp
public Task<List<Chunk>> GetPrimedChunksAsync()
```

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[List](https://learn.microsoft.com/dotnet/api/system.collections.generic.list\-1)<[Chunk](Aislinn.Core.Models.Chunk.md)\>\>

### <a id="Aislinn_Core_Cognitive_CognitiveMemorySystem_GetWorkingMemoryContentsAsync"></a> GetWorkingMemoryContentsAsync\(\)

Get the current contents of working memory

```csharp
public Task<List<Chunk>> GetWorkingMemoryContentsAsync()
```

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[List](https://learn.microsoft.com/dotnet/api/system.collections.generic.list\-1)<[Chunk](Aislinn.Core.Models.Chunk.md)\>\>

### <a id="Aislinn_Core_Cognitive_CognitiveMemorySystem_ManualRefreshCycleAsync_System_Collections_Generic_List_System_Guid__"></a> ManualRefreshCycleAsync\(List<Guid\>\)

Manually trigger a working memory refresh cycle

```csharp
public Task ManualRefreshCycleAsync(List<Guid> focusedChunks = null)
```

#### Parameters

`focusedChunks` [List](https://learn.microsoft.com/dotnet/api/system.collections.generic.list\-1)<[Guid](https://learn.microsoft.com/dotnet/api/system.guid)\>

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task)

### <a id="Aislinn_Core_Cognitive_CognitiveMemorySystem_RemoveFromWorkingMemory_System_Guid_"></a> RemoveFromWorkingMemory\(Guid\)

Remove a chunk from working memory

```csharp
public bool RemoveFromWorkingMemory(Guid chunkId)
```

#### Parameters

`chunkId` [Guid](https://learn.microsoft.com/dotnet/api/system.guid)

#### Returns

 [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### <a id="Aislinn_Core_Cognitive_CognitiveMemorySystem_StartWorkingMemoryRefresh_System_Double_"></a> StartWorkingMemoryRefresh\(double\)

Start automatic working memory refresh on a timer

```csharp
public void StartWorkingMemoryRefresh(double intervalMs = 200)
```

#### Parameters

`intervalMs` [double](https://learn.microsoft.com/dotnet/api/system.double)

### <a id="Aislinn_Core_Cognitive_CognitiveMemorySystem_StopWorkingMemoryRefresh"></a> StopWorkingMemoryRefresh\(\)

Stop automatic working memory refresh

```csharp
public void StopWorkingMemoryRefresh()
```

### <a id="Aislinn_Core_Cognitive_CognitiveMemorySystem_UpdateChunkAsync_Aislinn_Core_Models_Chunk_"></a> UpdateChunkAsync\(Chunk\)

Update a chunk in memory

```csharp
public Task<bool> UpdateChunkAsync(Chunk chunk)
```

#### Parameters

`chunk` [Chunk](Aislinn.Core.Models.Chunk.md)

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[bool](https://learn.microsoft.com/dotnet/api/system.boolean)\>

