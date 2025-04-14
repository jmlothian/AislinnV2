# <a id="Aislinn_Core_Goals_Execution_GoalExecutionService_GoalExecutionEventArgs"></a> Class GoalExecutionService.GoalExecutionEventArgs

Namespace: [Aislinn.Core.Goals.Execution](Aislinn.Core.Goals.Execution.md)  
Assembly: Aislinn.Core.dll  

Event arguments for execution events

```csharp
public class GoalExecutionService.GoalExecutionEventArgs : EventArgs
```

#### Inheritance

object ← 
[EventArgs](https://learn.microsoft.com/dotnet/api/system.eventargs) ← 
[GoalExecutionService.GoalExecutionEventArgs](Aislinn.Core.Goals.Execution.GoalExecutionService.GoalExecutionEventArgs.md)

#### Derived

[GoalExecutionService.GoalExecutionProgressEventArgs](Aislinn.Core.Goals.Execution.GoalExecutionService.GoalExecutionProgressEventArgs.md)

#### Inherited Members

[EventArgs.Empty](https://learn.microsoft.com/dotnet/api/system.eventargs.empty)

## Properties

### <a id="Aislinn_Core_Goals_Execution_GoalExecutionService_GoalExecutionEventArgs_Error"></a> Error

```csharp
public Exception Error { get; set; }
```

#### Property Value

 [Exception](https://learn.microsoft.com/dotnet/api/system.exception)

### <a id="Aislinn_Core_Goals_Execution_GoalExecutionService_GoalExecutionEventArgs_ExecutionContext"></a> ExecutionContext

```csharp
public Dictionary<string, object> ExecutionContext { get; set; }
```

#### Property Value

 [Dictionary](https://learn.microsoft.com/dotnet/api/system.collections.generic.dictionary\-2)<[string](https://learn.microsoft.com/dotnet/api/system.string), object\>

### <a id="Aislinn_Core_Goals_Execution_GoalExecutionService_GoalExecutionEventArgs_ExecutionTime"></a> ExecutionTime

```csharp
public TimeSpan ExecutionTime { get; set; }
```

#### Property Value

 [TimeSpan](https://learn.microsoft.com/dotnet/api/system.timespan)

### <a id="Aislinn_Core_Goals_Execution_GoalExecutionService_GoalExecutionEventArgs_GoalId"></a> GoalId

```csharp
public Guid GoalId { get; set; }
```

#### Property Value

 [Guid](https://learn.microsoft.com/dotnet/api/system.guid)

### <a id="Aislinn_Core_Goals_Execution_GoalExecutionService_GoalExecutionEventArgs_GoalName"></a> GoalName

```csharp
public string GoalName { get; set; }
```

#### Property Value

 [string](https://learn.microsoft.com/dotnet/api/system.string)

### <a id="Aislinn_Core_Goals_Execution_GoalExecutionService_GoalExecutionEventArgs_Message"></a> Message

```csharp
public string Message { get; set; }
```

#### Property Value

 [string](https://learn.microsoft.com/dotnet/api/system.string)

### <a id="Aislinn_Core_Goals_Execution_GoalExecutionService_GoalExecutionEventArgs_ProcedureId"></a> ProcedureId

```csharp
public Guid? ProcedureId { get; set; }
```

#### Property Value

 [Guid](https://learn.microsoft.com/dotnet/api/system.guid)?

### <a id="Aislinn_Core_Goals_Execution_GoalExecutionService_GoalExecutionEventArgs_ProcedureName"></a> ProcedureName

```csharp
public string ProcedureName { get; set; }
```

#### Property Value

 [string](https://learn.microsoft.com/dotnet/api/system.string)

