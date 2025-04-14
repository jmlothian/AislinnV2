# <a id="Aislinn_Core_Memory_WorkingMemoryManager"></a> Class WorkingMemoryManager

Namespace: [Aislinn.Core.Memory](Aislinn.Core.Memory.md)  
Assembly: Aislinn.Core.dll  

Manages the contents of working memory, enforcing human-like capacity limitations,
handling interference, and managing the relationship between working memory and
associative memory.

```csharp
public class WorkingMemoryManager
```

#### Inheritance

object ← 
[WorkingMemoryManager](Aislinn.Core.Memory.WorkingMemoryManager.md)

## Constructors

### <a id="Aislinn_Core_Memory_WorkingMemoryManager__ctor_Aislinn_ChunkStorage_Interfaces_IChunkStore_Aislinn_ChunkStorage_Interfaces_IAssociationStore_System_Int32_System_Double_System_Double_System_String_System_String_"></a> WorkingMemoryManager\(IChunkStore, IAssociationStore, int, double, double, string, string\)

Initializes a new instance of the WorkingMemoryManager

```csharp
public WorkingMemoryManager(IChunkStore chunkStore, IAssociationStore associationStore, int totalCapacity = 5, double activationThreshold = 0.7, double associativeThreshold = 0.3, string chunkCollectionId = "default", string associationCollectionId = "default")
```

#### Parameters

`chunkStore` [IChunkStore](Aislinn.ChunkStorage.Interfaces.IChunkStore.md)

The chunk store to use

`associationStore` [IAssociationStore](Aislinn.ChunkStorage.Interfaces.IAssociationStore.md)

The association store to use

`totalCapacity` [int](https://learn.microsoft.com/dotnet/api/system.int32)

Maximum number of chunks in working memory (default 5)

`activationThreshold` [double](https://learn.microsoft.com/dotnet/api/system.double)

Minimum activation for working memory inclusion (default 0.7)

`associativeThreshold` [double](https://learn.microsoft.com/dotnet/api/system.double)

Activation threshold for primed/associated items (default 0.3)

`chunkCollectionId` [string](https://learn.microsoft.com/dotnet/api/system.string)

Collection ID for chunks

`associationCollectionId` [string](https://learn.microsoft.com/dotnet/api/system.string)

Collection ID for associations

## Properties

### <a id="Aislinn_Core_Memory_WorkingMemoryManager_AutoRefreshEnabled"></a> AutoRefreshEnabled

```csharp
public bool AutoRefreshEnabled { get; set; }
```

#### Property Value

 [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### <a id="Aislinn_Core_Memory_WorkingMemoryManager_RefreshIntervalMs"></a> RefreshIntervalMs

```csharp
public double RefreshIntervalMs { get; set; }
```

#### Property Value

 [double](https://learn.microsoft.com/dotnet/api/system.double)

## Methods

### <a id="Aislinn_Core_Memory_WorkingMemoryManager_ClearWorkingMemory"></a> ClearWorkingMemory\(\)

Clears working memory entirely

```csharp
public void ClearWorkingMemory()
```

### <a id="Aislinn_Core_Memory_WorkingMemoryManager_Dispose"></a> Dispose\(\)

```csharp
public void Dispose()
```

### <a id="Aislinn_Core_Memory_WorkingMemoryManager_GetPrimedChunksAsync"></a> GetPrimedChunksAsync\(\)

Returns all primed chunks (partially activated but not in working memory)

```csharp
public Task<List<Chunk>> GetPrimedChunksAsync()
```

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[List](https://learn.microsoft.com/dotnet/api/system.collections.generic.list\-1)<[Chunk](Aislinn.Core.Models.Chunk.md)\>\>

### <a id="Aislinn_Core_Memory_WorkingMemoryManager_GetPrimedChunksCount"></a> GetPrimedChunksCount\(\)

Gets the number of primed chunks

```csharp
public int GetPrimedChunksCount()
```

#### Returns

 [int](https://learn.microsoft.com/dotnet/api/system.int32)

### <a id="Aislinn_Core_Memory_WorkingMemoryManager_GetWorkingMemoryContentsAsync"></a> GetWorkingMemoryContentsAsync\(\)

Returns all chunks currently in working memory in order of activation

```csharp
public Task<List<Chunk>> GetWorkingMemoryContentsAsync()
```

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[List](https://learn.microsoft.com/dotnet/api/system.collections.generic.list\-1)<[Chunk](Aislinn.Core.Models.Chunk.md)\>\>

### <a id="Aislinn_Core_Memory_WorkingMemoryManager_GetWorkingMemoryUsage"></a> GetWorkingMemoryUsage\(\)

Gets the current working memory capacity usage statistics

```csharp
public Dictionary<WorkingMemoryManager.MemorySubsystem, int> GetWorkingMemoryUsage()
```

#### Returns

 [Dictionary](https://learn.microsoft.com/dotnet/api/system.collections.generic.dictionary\-2)<[WorkingMemoryManager](Aislinn.Core.Memory.WorkingMemoryManager.md).[MemorySubsystem](Aislinn.Core.Memory.WorkingMemoryManager.MemorySubsystem.md), [int](https://learn.microsoft.com/dotnet/api/system.int32)\>

### <a id="Aislinn_Core_Memory_WorkingMemoryManager_RefreshCycleAsync_System_Collections_Generic_List_System_Guid__"></a> RefreshCycleAsync\(List<Guid\>\)

Simulates natural decay of working memory items and refreshes focused items

```csharp
public Task RefreshCycleAsync(List<Guid> focusedChunkIds = null)
```

#### Parameters

`focusedChunkIds` [List](https://learn.microsoft.com/dotnet/api/system.collections.generic.list\-1)<[Guid](https://learn.microsoft.com/dotnet/api/system.guid)\>

IDs of chunks currently being focused on

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task)

### <a id="Aislinn_Core_Memory_WorkingMemoryManager_RemoveFromWorkingMemory_System_Guid_"></a> RemoveFromWorkingMemory\(Guid\)

Forces a chunk out of working memory

```csharp
public bool RemoveFromWorkingMemory(Guid chunkId)
```

#### Parameters

`chunkId` [Guid](https://learn.microsoft.com/dotnet/api/system.guid)

#### Returns

 [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### <a id="Aislinn_Core_Memory_WorkingMemoryManager_StartAutoRefresh_System_Double_"></a> StartAutoRefresh\(double\)

```csharp
public void StartAutoRefresh(double intervalMs = 200)
```

#### Parameters

`intervalMs` [double](https://learn.microsoft.com/dotnet/api/system.double)

### <a id="Aislinn_Core_Memory_WorkingMemoryManager_StopAutoRefresh"></a> StopAutoRefresh\(\)

```csharp
public void StopAutoRefresh()
```

### <a id="Aislinn_Core_Memory_WorkingMemoryManager_UpdateWorkingMemoryAsync_Aislinn_Core_Models_Chunk_System_Boolean_"></a> UpdateWorkingMemoryAsync\(Chunk, bool\)

Updates working memory based on new chunk activations

```csharp
public Task<bool> UpdateWorkingMemoryAsync(Chunk chunk, bool forceEntry = false)
```

#### Parameters

`chunk` [Chunk](Aislinn.Core.Models.Chunk.md)

The recently activated chunk

`forceEntry` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

Whether to force entry into working memory even if full

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[bool](https://learn.microsoft.com/dotnet/api/system.boolean)\>

Whether the chunk entered working memory

