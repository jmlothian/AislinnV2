# <a id="Aislinn_Core_Goals_Execution_GoalExecutionService_GoalExecutionProgressEventArgs"></a> Class GoalExecutionService.GoalExecutionProgressEventArgs

Namespace: [Aislinn.Core.Goals.Execution](Aislinn.Core.Goals.Execution.md)  
Assembly: Aislinn.Core.dll  

Event arguments for execution progress updates

```csharp
public class GoalExecutionService.GoalExecutionProgressEventArgs : GoalExecutionService.GoalExecutionEventArgs
```

#### Inheritance

object ← 
[EventArgs](https://learn.microsoft.com/dotnet/api/system.eventargs) ← 
[GoalExecutionService.GoalExecutionEventArgs](Aislinn.Core.Goals.Execution.GoalExecutionService.GoalExecutionEventArgs.md) ← 
[GoalExecutionService.GoalExecutionProgressEventArgs](Aislinn.Core.Goals.Execution.GoalExecutionService.GoalExecutionProgressEventArgs.md)

#### Inherited Members

[GoalExecutionService.GoalExecutionEventArgs.GoalId](Aislinn.Core.Goals.Execution.GoalExecutionService.GoalExecutionEventArgs.md\#Aislinn\_Core\_Goals\_Execution\_GoalExecutionService\_GoalExecutionEventArgs\_GoalId), 
[GoalExecutionService.GoalExecutionEventArgs.GoalName](Aislinn.Core.Goals.Execution.GoalExecutionService.GoalExecutionEventArgs.md\#Aislinn\_Core\_Goals\_Execution\_GoalExecutionService\_GoalExecutionEventArgs\_GoalName), 
[GoalExecutionService.GoalExecutionEventArgs.ProcedureId](Aislinn.Core.Goals.Execution.GoalExecutionService.GoalExecutionEventArgs.md\#Aislinn\_Core\_Goals\_Execution\_GoalExecutionService\_GoalExecutionEventArgs\_ProcedureId), 
[GoalExecutionService.GoalExecutionEventArgs.ProcedureName](Aislinn.Core.Goals.Execution.GoalExecutionService.GoalExecutionEventArgs.md\#Aislinn\_Core\_Goals\_Execution\_GoalExecutionService\_GoalExecutionEventArgs\_ProcedureName), 
[GoalExecutionService.GoalExecutionEventArgs.ExecutionTime](Aislinn.Core.Goals.Execution.GoalExecutionService.GoalExecutionEventArgs.md\#Aislinn\_Core\_Goals\_Execution\_GoalExecutionService\_GoalExecutionEventArgs\_ExecutionTime), 
[GoalExecutionService.GoalExecutionEventArgs.Message](Aislinn.Core.Goals.Execution.GoalExecutionService.GoalExecutionEventArgs.md\#Aislinn\_Core\_Goals\_Execution\_GoalExecutionService\_GoalExecutionEventArgs\_Message), 
[GoalExecutionService.GoalExecutionEventArgs.Error](Aislinn.Core.Goals.Execution.GoalExecutionService.GoalExecutionEventArgs.md\#Aislinn\_Core\_Goals\_Execution\_GoalExecutionService\_GoalExecutionEventArgs\_Error), 
[GoalExecutionService.GoalExecutionEventArgs.ExecutionContext](Aislinn.Core.Goals.Execution.GoalExecutionService.GoalExecutionEventArgs.md\#Aislinn\_Core\_Goals\_Execution\_GoalExecutionService\_GoalExecutionEventArgs\_ExecutionContext), 
[EventArgs.Empty](https://learn.microsoft.com/dotnet/api/system.eventargs.empty)

## Properties

### <a id="Aislinn_Core_Goals_Execution_GoalExecutionService_GoalExecutionProgressEventArgs_CurrentStepIndex"></a> CurrentStepIndex

```csharp
public int CurrentStepIndex { get; set; }
```

#### Property Value

 [int](https://learn.microsoft.com/dotnet/api/system.int32)

### <a id="Aislinn_Core_Goals_Execution_GoalExecutionService_GoalExecutionProgressEventArgs_CurrentStepName"></a> CurrentStepName

```csharp
public string CurrentStepName { get; set; }
```

#### Property Value

 [string](https://learn.microsoft.com/dotnet/api/system.string)

### <a id="Aislinn_Core_Goals_Execution_GoalExecutionService_GoalExecutionProgressEventArgs_ProgressPercentage"></a> ProgressPercentage

```csharp
public double ProgressPercentage { get; set; }
```

#### Property Value

 [double](https://learn.microsoft.com/dotnet/api/system.double)

### <a id="Aislinn_Core_Goals_Execution_GoalExecutionService_GoalExecutionProgressEventArgs_TotalSteps"></a> TotalSteps

```csharp
public int TotalSteps { get; set; }
```

#### Property Value

 [int](https://learn.microsoft.com/dotnet/api/system.int32)

