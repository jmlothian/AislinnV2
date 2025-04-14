# <a id="Aislinn_Core_Procedural_ProcedureMatcher_ProcedureMatchResult"></a> Class ProcedureMatcher.ProcedureMatchResult

Namespace: [Aislinn.Core.Procedural](Aislinn.Core.Procedural.md)  
Assembly: Aislinn.Core.dll  

Result of a procedure matching operation

```csharp
public class ProcedureMatcher.ProcedureMatchResult
```

#### Inheritance

object ← 
[ProcedureMatcher.ProcedureMatchResult](Aislinn.Core.Procedural.ProcedureMatcher.ProcedureMatchResult.md)

## Properties

### <a id="Aislinn_Core_Procedural_ProcedureMatcher_ProcedureMatchResult_InfeasibilityReasons"></a> InfeasibilityReasons

If not feasible, reasons why

```csharp
public List<string> InfeasibilityReasons { get; set; }
```

#### Property Value

 [List](https://learn.microsoft.com/dotnet/api/system.collections.generic.list\-1)<[string](https://learn.microsoft.com/dotnet/api/system.string)\>

### <a id="Aislinn_Core_Procedural_ProcedureMatcher_ProcedureMatchResult_IsFeasible"></a> IsFeasible

Whether this procedure is feasible given the current context

```csharp
public bool IsFeasible { get; set; }
```

#### Property Value

 [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### <a id="Aislinn_Core_Procedural_ProcedureMatcher_ProcedureMatchResult_Procedure"></a> Procedure

The matched procedure

```csharp
public ProcedureChunk Procedure { get; set; }
```

#### Property Value

 [ProcedureChunk](Aislinn.Core.Procedural.ProcedureChunk.md)

### <a id="Aislinn_Core_Procedural_ProcedureMatcher_ProcedureMatchResult_Score"></a> Score

Match score (0-1)

```csharp
public double Score { get; set; }
```

#### Property Value

 [double](https://learn.microsoft.com/dotnet/api/system.double)

### <a id="Aislinn_Core_Procedural_ProcedureMatcher_ProcedureMatchResult_ScoreComponents"></a> ScoreComponents

Components that make up the score

```csharp
public Dictionary<string, double> ScoreComponents { get; set; }
```

#### Property Value

 [Dictionary](https://learn.microsoft.com/dotnet/api/system.collections.generic.dictionary\-2)<[string](https://learn.microsoft.com/dotnet/api/system.string), [double](https://learn.microsoft.com/dotnet/api/system.double)\>

