# <a id="Aislinn_Core_Procedural_ProcedureMatcher_ProcedureMatcherConfig"></a> Class ProcedureMatcher.ProcedureMatcherConfig

Namespace: [Aislinn.Core.Procedural](Aislinn.Core.Procedural.md)  
Assembly: Aislinn.Core.dll  

Configuration options for procedure matching

```csharp
public class ProcedureMatcher.ProcedureMatcherConfig
```

#### Inheritance

object ← 
[ProcedureMatcher.ProcedureMatcherConfig](Aislinn.Core.Procedural.ProcedureMatcher.ProcedureMatcherConfig.md)

## Properties

### <a id="Aislinn_Core_Procedural_ProcedureMatcher_ProcedureMatcherConfig_ActivationWeight"></a> ActivationWeight

How much activation level affects matching score

```csharp
public double ActivationWeight { get; set; }
```

#### Property Value

 [double](https://learn.microsoft.com/dotnet/api/system.double)

### <a id="Aislinn_Core_Procedural_ProcedureMatcher_ProcedureMatcherConfig_ContextRelevanceWeight"></a> ContextRelevanceWeight

How much context relevance affects matching score

```csharp
public double ContextRelevanceWeight { get; set; }
```

#### Property Value

 [double](https://learn.microsoft.com/dotnet/api/system.double)

### <a id="Aislinn_Core_Procedural_ProcedureMatcher_ProcedureMatcherConfig_EnableCaching"></a> EnableCaching

Whether to cache procedures for faster matching

```csharp
public bool EnableCaching { get; set; }
```

#### Property Value

 [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### <a id="Aislinn_Core_Procedural_ProcedureMatcher_ProcedureMatcherConfig_MaxMatchResults"></a> MaxMatchResults

How many top procedures to return

```csharp
public int MaxMatchResults { get; set; }
```

#### Property Value

 [int](https://learn.microsoft.com/dotnet/api/system.int32)

### <a id="Aislinn_Core_Procedural_ProcedureMatcher_ProcedureMatcherConfig_MinimumMatchScore"></a> MinimumMatchScore

Minimum score for a procedure to be considered

```csharp
public double MinimumMatchScore { get; set; }
```

#### Property Value

 [double](https://learn.microsoft.com/dotnet/api/system.double)

### <a id="Aislinn_Core_Procedural_ProcedureMatcher_ProcedureMatcherConfig_ResourceAvailabilityWeight"></a> ResourceAvailabilityWeight

How much resource availability affects matching score

```csharp
public double ResourceAvailabilityWeight { get; set; }
```

#### Property Value

 [double](https://learn.microsoft.com/dotnet/api/system.double)

### <a id="Aislinn_Core_Procedural_ProcedureMatcher_ProcedureMatcherConfig_SuccessProbabilityWeight"></a> SuccessProbabilityWeight

How much success probability affects matching score

```csharp
public double SuccessProbabilityWeight { get; set; }
```

#### Property Value

 [double](https://learn.microsoft.com/dotnet/api/system.double)

### <a id="Aislinn_Core_Procedural_ProcedureMatcher_ProcedureMatcherConfig_UtilityWeight"></a> UtilityWeight

How much estimated utility affects matching score

```csharp
public double UtilityWeight { get; set; }
```

#### Property Value

 [double](https://learn.microsoft.com/dotnet/api/system.double)

