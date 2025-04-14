# <a id="Aislinn_Core_Goals_Selection_GoalSelectionService_GoalEvaluationResult"></a> Class GoalSelectionService.GoalEvaluationResult

Namespace: [Aislinn.Core.Goals.Selection](Aislinn.Core.Goals.Selection.md)  
Assembly: Aislinn.Core.dll  

Contains the evaluation result for a single goal

```csharp
public class GoalSelectionService.GoalEvaluationResult
```

#### Inheritance

object ← 
[GoalSelectionService.GoalEvaluationResult](Aislinn.Core.Goals.Selection.GoalSelectionService.GoalEvaluationResult.md)

## Properties

### <a id="Aislinn_Core_Goals_Selection_GoalSelectionService_GoalEvaluationResult_BlockingFactors"></a> BlockingFactors

```csharp
public List<string> BlockingFactors { get; set; }
```

#### Property Value

 [List](https://learn.microsoft.com/dotnet/api/system.collections.generic.list\-1)<[string](https://learn.microsoft.com/dotnet/api/system.string)\>

### <a id="Aislinn_Core_Goals_Selection_GoalSelectionService_GoalEvaluationResult_EstimatedCost"></a> EstimatedCost

```csharp
public double EstimatedCost { get; set; }
```

#### Property Value

 [double](https://learn.microsoft.com/dotnet/api/system.double)

### <a id="Aislinn_Core_Goals_Selection_GoalSelectionService_GoalEvaluationResult_EstimatedDuration"></a> EstimatedDuration

```csharp
public TimeSpan EstimatedDuration { get; set; }
```

#### Property Value

 [TimeSpan](https://learn.microsoft.com/dotnet/api/system.timespan)

### <a id="Aislinn_Core_Goals_Selection_GoalSelectionService_GoalEvaluationResult_EstimatedUtility"></a> EstimatedUtility

```csharp
public double EstimatedUtility { get; set; }
```

#### Property Value

 [double](https://learn.microsoft.com/dotnet/api/system.double)

### <a id="Aislinn_Core_Goals_Selection_GoalSelectionService_GoalEvaluationResult_EvaluationContext"></a> EvaluationContext

```csharp
public Dictionary<string, object> EvaluationContext { get; set; }
```

#### Property Value

 [Dictionary](https://learn.microsoft.com/dotnet/api/system.collections.generic.dictionary\-2)<[string](https://learn.microsoft.com/dotnet/api/system.string), object\>

### <a id="Aislinn_Core_Goals_Selection_GoalSelectionService_GoalEvaluationResult_EvaluationTime"></a> EvaluationTime

```csharp
public DateTime EvaluationTime { get; set; }
```

#### Property Value

 [DateTime](https://learn.microsoft.com/dotnet/api/system.datetime)

### <a id="Aislinn_Core_Goals_Selection_GoalSelectionService_GoalEvaluationResult_GoalId"></a> GoalId

```csharp
public Guid GoalId { get; set; }
```

#### Property Value

 [Guid](https://learn.microsoft.com/dotnet/api/system.guid)

### <a id="Aislinn_Core_Goals_Selection_GoalSelectionService_GoalEvaluationResult_GoalName"></a> GoalName

```csharp
public string GoalName { get; set; }
```

#### Property Value

 [string](https://learn.microsoft.com/dotnet/api/system.string)

### <a id="Aislinn_Core_Goals_Selection_GoalSelectionService_GoalEvaluationResult_GoalStatus"></a> GoalStatus

```csharp
public string GoalStatus { get; set; }
```

#### Property Value

 [string](https://learn.microsoft.com/dotnet/api/system.string)

### <a id="Aislinn_Core_Goals_Selection_GoalSelectionService_GoalEvaluationResult_IsFeasible"></a> IsFeasible

```csharp
public bool IsFeasible { get; set; }
```

#### Property Value

 [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### <a id="Aislinn_Core_Goals_Selection_GoalSelectionService_GoalEvaluationResult_ScoreComponents"></a> ScoreComponents

```csharp
public Dictionary<string, double> ScoreComponents { get; set; }
```

#### Property Value

 [Dictionary](https://learn.microsoft.com/dotnet/api/system.collections.generic.dictionary\-2)<[string](https://learn.microsoft.com/dotnet/api/system.string), [double](https://learn.microsoft.com/dotnet/api/system.double)\>

### <a id="Aislinn_Core_Goals_Selection_GoalSelectionService_GoalEvaluationResult_TotalScore"></a> TotalScore

```csharp
public double TotalScore { get; set; }
```

#### Property Value

 [double](https://learn.microsoft.com/dotnet/api/system.double)

