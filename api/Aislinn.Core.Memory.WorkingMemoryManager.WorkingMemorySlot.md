# <a id="Aislinn_Core_Memory_WorkingMemoryManager_WorkingMemorySlot"></a> Class WorkingMemoryManager.WorkingMemorySlot

Namespace: [Aislinn.Core.Memory](Aislinn.Core.Memory.md)  
Assembly: Aislinn.Core.dll  

Represents a chunk in working memory with additional metadata about its status

```csharp
public class WorkingMemoryManager.WorkingMemorySlot
```

#### Inheritance

object ← 
[WorkingMemoryManager.WorkingMemorySlot](Aislinn.Core.Memory.WorkingMemoryManager.WorkingMemorySlot.md)

## Properties

### <a id="Aislinn_Core_Memory_WorkingMemoryManager_WorkingMemorySlot_ChunkId"></a> ChunkId

```csharp
public Guid ChunkId { get; set; }
```

#### Property Value

 [Guid](https://learn.microsoft.com/dotnet/api/system.guid)

### <a id="Aislinn_Core_Memory_WorkingMemoryManager_WorkingMemorySlot_CurrentActivation"></a> CurrentActivation

```csharp
public double CurrentActivation { get; set; }
```

#### Property Value

 [double](https://learn.microsoft.com/dotnet/api/system.double)

### <a id="Aislinn_Core_Memory_WorkingMemoryManager_WorkingMemorySlot_EntryTime"></a> EntryTime

```csharp
public DateTime EntryTime { get; set; }
```

#### Property Value

 [DateTime](https://learn.microsoft.com/dotnet/api/system.datetime)

### <a id="Aislinn_Core_Memory_WorkingMemoryManager_WorkingMemorySlot_FocusValue"></a> FocusValue

```csharp
public double FocusValue { get; set; }
```

#### Property Value

 [double](https://learn.microsoft.com/dotnet/api/system.double)

### <a id="Aislinn_Core_Memory_WorkingMemoryManager_WorkingMemorySlot_LastRefreshTime"></a> LastRefreshTime

```csharp
public DateTime LastRefreshTime { get; set; }
```

#### Property Value

 [DateTime](https://learn.microsoft.com/dotnet/api/system.datetime)

### <a id="Aislinn_Core_Memory_WorkingMemoryManager_WorkingMemorySlot_RefreshCount"></a> RefreshCount

```csharp
public int RefreshCount { get; set; }
```

#### Property Value

 [int](https://learn.microsoft.com/dotnet/api/system.int32)

### <a id="Aislinn_Core_Memory_WorkingMemoryManager_WorkingMemorySlot_Subsystem"></a> Subsystem

```csharp
public WorkingMemoryManager.MemorySubsystem Subsystem { get; set; }
```

#### Property Value

 [WorkingMemoryManager](Aislinn.Core.Memory.WorkingMemoryManager.md).[MemorySubsystem](Aislinn.Core.Memory.WorkingMemoryManager.MemorySubsystem.md)

