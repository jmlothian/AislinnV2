# <a id="Aislinn_Core_Procedural_ProcedureStep"></a> Class ProcedureStep

Namespace: [Aislinn.Core.Procedural](Aislinn.Core.Procedural.md)  
Assembly: Aislinn.Core.dll  

Represents a single step within a procedure, including the operation to perform,
parameters, and control flow details.

```csharp
public class ProcedureStep
```

#### Inheritance

object ← 
[ProcedureStep](Aislinn.Core.Procedural.ProcedureStep.md)

## Constructors

### <a id="Aislinn_Core_Procedural_ProcedureStep__ctor"></a> ProcedureStep\(\)

Creates a new step with a default ID

```csharp
public ProcedureStep()
```

### <a id="Aislinn_Core_Procedural_ProcedureStep__ctor_System_String_System_String_Aislinn_Core_Procedural_ProcedureStep_StepType_"></a> ProcedureStep\(string, string, StepType\)

Creates a new step with specific name and operation

```csharp
public ProcedureStep(string name, string operation, ProcedureStep.StepType type = StepType.Action)
```

#### Parameters

`name` [string](https://learn.microsoft.com/dotnet/api/system.string)

`operation` [string](https://learn.microsoft.com/dotnet/api/system.string)

`type` [ProcedureStep](Aislinn.Core.Procedural.ProcedureStep.md).[StepType](Aislinn.Core.Procedural.ProcedureStep.StepType.md)

## Properties

### <a id="Aislinn_Core_Procedural_ProcedureStep_Branches"></a> Branches

Conditional branches based on step outcome

```csharp
public List<StepBranch> Branches { get; set; }
```

#### Property Value

 [List](https://learn.microsoft.com/dotnet/api/system.collections.generic.list\-1)<[StepBranch](Aislinn.Core.Procedural.StepBranch.md)\>

### <a id="Aislinn_Core_Procedural_ProcedureStep_Effects"></a> Effects

Expected effects of this step

```csharp
public List<ProcedureEffect> Effects { get; set; }
```

#### Property Value

 [List](https://learn.microsoft.com/dotnet/api/system.collections.generic.list\-1)<[ProcedureEffect](Aislinn.Core.Procedural.ProcedureEffect.md)\>

### <a id="Aislinn_Core_Procedural_ProcedureStep_Id"></a> Id

Unique identifier for this step within the procedure

```csharp
public string Id { get; set; }
```

#### Property Value

 [string](https://learn.microsoft.com/dotnet/api/system.string)

### <a id="Aislinn_Core_Procedural_ProcedureStep_Metadata"></a> Metadata

Additional metadata for this step

```csharp
public Dictionary<string, object> Metadata { get; set; }
```

#### Property Value

 [Dictionary](https://learn.microsoft.com/dotnet/api/system.collections.generic.dictionary\-2)<[string](https://learn.microsoft.com/dotnet/api/system.string), object\>

### <a id="Aislinn_Core_Procedural_ProcedureStep_Name"></a> Name

Descriptive name of this step

```csharp
public string Name { get; set; }
```

#### Property Value

 [string](https://learn.microsoft.com/dotnet/api/system.string)

### <a id="Aislinn_Core_Procedural_ProcedureStep_NextStepId"></a> NextStepId

Next step ID for unconditional flow

```csharp
public string NextStepId { get; set; }
```

#### Property Value

 [string](https://learn.microsoft.com/dotnet/api/system.string)

### <a id="Aislinn_Core_Procedural_ProcedureStep_Operation"></a> Operation

The operation to perform (method name, action name, etc.)

```csharp
public string Operation { get; set; }
```

#### Property Value

 [string](https://learn.microsoft.com/dotnet/api/system.string)

### <a id="Aislinn_Core_Procedural_ProcedureStep_Parameters"></a> Parameters

Parameters for the operation

```csharp
public Dictionary<string, object> Parameters { get; set; }
```

#### Property Value

 [Dictionary](https://learn.microsoft.com/dotnet/api/system.collections.generic.dictionary\-2)<[string](https://learn.microsoft.com/dotnet/api/system.string), object\>

### <a id="Aislinn_Core_Procedural_ProcedureStep_Preconditions"></a> Preconditions

Preconditions specific to this step

```csharp
public List<ProcedureCondition> Preconditions { get; set; }
```

#### Property Value

 [List](https://learn.microsoft.com/dotnet/api/system.collections.generic.list\-1)<[ProcedureCondition](Aislinn.Core.Procedural.ProcedureCondition.md)\>

### <a id="Aislinn_Core_Procedural_ProcedureStep_ResultReference"></a> ResultReference

Expected result reference (where to store the result)

```csharp
public string ResultReference { get; set; }
```

#### Property Value

 [string](https://learn.microsoft.com/dotnet/api/system.string)

### <a id="Aislinn_Core_Procedural_ProcedureStep_RetryPolicy"></a> RetryPolicy

Retry policy for failed execution

```csharp
public StepRetryPolicy RetryPolicy { get; set; }
```

#### Property Value

 [StepRetryPolicy](Aislinn.Core.Procedural.StepRetryPolicy.md)

### <a id="Aislinn_Core_Procedural_ProcedureStep_Status"></a> Status

Current execution status

```csharp
public ProcedureStep.StepStatus Status { get; set; }
```

#### Property Value

 [ProcedureStep](Aislinn.Core.Procedural.ProcedureStep.md).[StepStatus](Aislinn.Core.Procedural.ProcedureStep.StepStatus.md)

### <a id="Aislinn_Core_Procedural_ProcedureStep_Timeout"></a> Timeout

Optional timeout for this step

```csharp
public TimeSpan? Timeout { get; set; }
```

#### Property Value

 [TimeSpan](https://learn.microsoft.com/dotnet/api/system.timespan)?

### <a id="Aislinn_Core_Procedural_ProcedureStep_Type"></a> Type

Type of operation for this step

```csharp
public ProcedureStep.StepType Type { get; set; }
```

#### Property Value

 [ProcedureStep](Aislinn.Core.Procedural.ProcedureStep.md).[StepType](Aislinn.Core.Procedural.ProcedureStep.StepType.md)

## Methods

### <a id="Aislinn_Core_Procedural_ProcedureStep_ApplyEffects_System_Collections_Generic_Dictionary_System_String_System_Object__"></a> ApplyEffects\(Dictionary<string, object\>\)

Applies the effects of this step to the context

```csharp
public void ApplyEffects(Dictionary<string, object> context)
```

#### Parameters

`context` [Dictionary](https://learn.microsoft.com/dotnet/api/system.collections.generic.dictionary\-2)<[string](https://learn.microsoft.com/dotnet/api/system.string), object\>

### <a id="Aislinn_Core_Procedural_ProcedureStep_CanExecute_System_Collections_Generic_Dictionary_System_String_System_Object__"></a> CanExecute\(Dictionary<string, object\>\)

Evaluates if this step can be executed given the current context

```csharp
public bool CanExecute(Dictionary<string, object> context)
```

#### Parameters

`context` [Dictionary](https://learn.microsoft.com/dotnet/api/system.collections.generic.dictionary\-2)<[string](https://learn.microsoft.com/dotnet/api/system.string), object\>

#### Returns

 [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### <a id="Aislinn_Core_Procedural_ProcedureStep_Clone"></a> Clone\(\)

Creates a clone of this step

```csharp
public ProcedureStep Clone()
```

#### Returns

 [ProcedureStep](Aislinn.Core.Procedural.ProcedureStep.md)

### <a id="Aislinn_Core_Procedural_ProcedureStep_GetNextStepId_System_Boolean_System_Collections_Generic_Dictionary_System_String_System_Object__"></a> GetNextStepId\(bool, Dictionary<string, object\>\)

Determines the next step ID based on execution result

```csharp
public string GetNextStepId(bool success, Dictionary<string, object> context)
```

#### Parameters

`success` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

`context` [Dictionary](https://learn.microsoft.com/dotnet/api/system.collections.generic.dictionary\-2)<[string](https://learn.microsoft.com/dotnet/api/system.string), object\>

#### Returns

 [string](https://learn.microsoft.com/dotnet/api/system.string)

### <a id="Aislinn_Core_Procedural_ProcedureStep_Reset"></a> Reset\(\)

Resets the status of this step

```csharp
public void Reset()
```

