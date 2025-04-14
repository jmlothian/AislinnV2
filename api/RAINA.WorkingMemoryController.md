# <a id="RAINA_WorkingMemoryController"></a> Class WorkingMemoryController

Namespace: [RAINA](RAINA.md)  
Assembly: Aislinn.Raina.dll  

```csharp
public class WorkingMemoryController
```

#### Inheritance

object ← 
[WorkingMemoryController](RAINA.WorkingMemoryController.md)

## Constructors

### <a id="RAINA_WorkingMemoryController__ctor_Aislinn_Core_Memory_WorkingMemoryManager_Aislinn_Core_Cognitive_CognitiveMemorySystem_"></a> WorkingMemoryController\(WorkingMemoryManager, CognitiveMemorySystem\)

```csharp
public WorkingMemoryController(WorkingMemoryManager workingMemory, CognitiveMemorySystem memorySystem)
```

#### Parameters

`workingMemory` WorkingMemoryManager

`memorySystem` CognitiveMemorySystem

## Methods

### <a id="RAINA_WorkingMemoryController_FocusOnChunkAsync_System_Guid_"></a> FocusOnChunkAsync\(Guid\)

```csharp
public Task<bool> FocusOnChunkAsync(Guid chunkId)
```

#### Parameters

`chunkId` [Guid](https://learn.microsoft.com/dotnet/api/system.guid)

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[bool](https://learn.microsoft.com/dotnet/api/system.boolean)\>

### <a id="RAINA_WorkingMemoryController_GetActiveChunksAsync"></a> GetActiveChunksAsync\(\)

```csharp
public Task<List<Chunk>> GetActiveChunksAsync()
```

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[List](https://learn.microsoft.com/dotnet/api/system.collections.generic.list\-1)<Chunk\>\>

### <a id="RAINA_WorkingMemoryController_GetPrimedChunksAsync"></a> GetPrimedChunksAsync\(\)

```csharp
public Task<List<Chunk>> GetPrimedChunksAsync()
```

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[List](https://learn.microsoft.com/dotnet/api/system.collections.generic.list\-1)<Chunk\>\>

