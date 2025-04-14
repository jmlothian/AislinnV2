# <a id="RAINA_TaskManager"></a> Class TaskManager

Namespace: [RAINA](RAINA.md)  
Assembly: Aislinn.Raina.dll  

```csharp
public class TaskManager
```

#### Inheritance

object ← 
[TaskManager](RAINA.TaskManager.md)

## Constructors

### <a id="RAINA_TaskManager__ctor_Aislinn_Core_Cognitive_CognitiveMemorySystem_"></a> TaskManager\(CognitiveMemorySystem\)

```csharp
public TaskManager(CognitiveMemorySystem memorySystem)
```

#### Parameters

`memorySystem` CognitiveMemorySystem

## Methods

### <a id="RAINA_TaskManager_CreateTaskAsync_RAINA_RainaTask_RAINA_Services_UserContext_"></a> CreateTaskAsync\(RainaTask, UserContext\)

```csharp
public Task<RainaTask> CreateTaskAsync(RainaTask task, UserContext context)
```

#### Parameters

`task` [RainaTask](RAINA.RainaTask.md)

`context` [UserContext](RAINA.Services.UserContext.md)

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[RainaTask](RAINA.RainaTask.md)\>

