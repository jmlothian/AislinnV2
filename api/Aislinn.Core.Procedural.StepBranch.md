# <a id="Aislinn_Core_Procedural_StepBranch"></a> Class StepBranch

Namespace: [Aislinn.Core.Procedural](Aislinn.Core.Procedural.md)  
Assembly: Aislinn.Core.dll  

Represents a conditional branch in procedure control flow

```csharp
public class StepBranch
```

#### Inheritance

object ← 
[StepBranch](Aislinn.Core.Procedural.StepBranch.md)

## Properties

### <a id="Aislinn_Core_Procedural_StepBranch_Condition"></a> Condition

Condition to evaluate for this branch

```csharp
public ProcedureCondition Condition { get; set; }
```

#### Property Value

 [ProcedureCondition](Aislinn.Core.Procedural.ProcedureCondition.md)

### <a id="Aislinn_Core_Procedural_StepBranch_RequiresSuccess"></a> RequiresSuccess

Whether this branch requires the step to have succeeded

```csharp
public bool RequiresSuccess { get; set; }
```

#### Property Value

 [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### <a id="Aislinn_Core_Procedural_StepBranch_TargetStepId"></a> TargetStepId

Target step ID if condition is true

```csharp
public string TargetStepId { get; set; }
```

#### Property Value

 [string](https://learn.microsoft.com/dotnet/api/system.string)

## Methods

### <a id="Aislinn_Core_Procedural_StepBranch_Clone"></a> Clone\(\)

Creates a clone of this branch

```csharp
public StepBranch Clone()
```

#### Returns

 [StepBranch](Aislinn.Core.Procedural.StepBranch.md)

