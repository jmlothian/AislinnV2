# <a id="Aislinn_Core_Goals_Execution_GoalExecutionService_GoalExecutionConfig"></a> Class GoalExecutionService.GoalExecutionConfig

Namespace: [Aislinn.Core.Goals.Execution](Aislinn.Core.Goals.Execution.md)  
Assembly: Aislinn.Core.dll  

Configuration options for goal execution

```csharp
public class GoalExecutionService.GoalExecutionConfig
```

#### Inheritance

object ← 
[GoalExecutionService.GoalExecutionConfig](Aislinn.Core.Goals.Execution.GoalExecutionService.GoalExecutionConfig.md)

## Properties

### <a id="Aislinn_Core_Goals_Execution_GoalExecutionService_GoalExecutionConfig_AutoRetryOnFailure"></a> AutoRetryOnFailure

Whether to automatically retry on failure

```csharp
public bool AutoRetryOnFailure { get; set; }
```

#### Property Value

 [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### <a id="Aislinn_Core_Goals_Execution_GoalExecutionService_GoalExecutionConfig_MaxExecutionTimeSeconds"></a> MaxExecutionTimeSeconds

Maximum execution time for any procedure (in seconds)

```csharp
public double MaxExecutionTimeSeconds { get; set; }
```

#### Property Value

 [double](https://learn.microsoft.com/dotnet/api/system.double)

### <a id="Aislinn_Core_Goals_Execution_GoalExecutionService_GoalExecutionConfig_MaxRetryAttempts"></a> MaxRetryAttempts

Maximum number of retry attempts

```csharp
public int MaxRetryAttempts { get; set; }
```

#### Property Value

 [int](https://learn.microsoft.com/dotnet/api/system.int32)

### <a id="Aislinn_Core_Goals_Execution_GoalExecutionService_GoalExecutionConfig_MonitoringIntervalMs"></a> MonitoringIntervalMs

How often to check execution progress (in milliseconds)

```csharp
public double MonitoringIntervalMs { get; set; }
```

#### Property Value

 [double](https://learn.microsoft.com/dotnet/api/system.double)

### <a id="Aislinn_Core_Goals_Execution_GoalExecutionService_GoalExecutionConfig_TryAlternateProcedures"></a> TryAlternateProcedures

Whether to automatically select alternate procedures if one fails

```csharp
public bool TryAlternateProcedures { get; set; }
```

#### Property Value

 [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### <a id="Aislinn_Core_Goals_Execution_GoalExecutionService_GoalExecutionConfig_UpdateContext"></a> UpdateContext

Whether to update context during execution

```csharp
public bool UpdateContext { get; set; }
```

#### Property Value

 [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### <a id="Aislinn_Core_Goals_Execution_GoalExecutionService_GoalExecutionConfig_UpdateGoalActivation"></a> UpdateGoalActivation

Whether to update goal activation during execution

```csharp
public bool UpdateGoalActivation { get; set; }
```

#### Property Value

 [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

