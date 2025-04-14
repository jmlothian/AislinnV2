# <a id="Aislinn_Core_Procedural_ProcedureChunk"></a> Class ProcedureChunk

Namespace: [Aislinn.Core.Procedural](Aislinn.Core.Procedural.md)  
Assembly: Aislinn.Core.dll  

Represents procedural knowledge as a specialized chunk type containing
execution steps, control flow, and metadata about when and how to apply the procedure.

```csharp
public class ProcedureChunk : Chunk
```

#### Inheritance

object ← 
[Chunk](Aislinn.Core.Models.Chunk.md) ← 
[ProcedureChunk](Aislinn.Core.Procedural.ProcedureChunk.md)

#### Inherited Members

[Chunk.ID](Aislinn.Core.Models.Chunk.md\#Aislinn\_Core\_Models\_Chunk\_ID), 
[Chunk.ChunkType](Aislinn.Core.Models.Chunk.md\#Aislinn\_Core\_Models\_Chunk\_ChunkType), 
[Chunk.CognitiveCategory](Aislinn.Core.Models.Chunk.md\#Aislinn\_Core\_Models\_Chunk\_CognitiveCategory), 
[Chunk.SemanticType](Aislinn.Core.Models.Chunk.md\#Aislinn\_Core\_Models\_Chunk\_SemanticType), 
[Chunk.Name](Aislinn.Core.Models.Chunk.md\#Aislinn\_Core\_Models\_Chunk\_Name), 
[Chunk.Vector](Aislinn.Core.Models.Chunk.md\#Aislinn\_Core\_Models\_Chunk\_Vector), 
[Chunk.ActivationLevel](Aislinn.Core.Models.Chunk.md\#Aislinn\_Core\_Models\_Chunk\_ActivationLevel), 
[Chunk.Slots](Aislinn.Core.Models.Chunk.md\#Aislinn\_Core\_Models\_Chunk\_Slots), 
[Chunk.ActivationHistory](Aislinn.Core.Models.Chunk.md\#Aislinn\_Core\_Models\_Chunk\_ActivationHistory)

#### Extension Methods

[ChunkExtensions.GetChunkFromSlot\(Chunk, string\)](Aislinn.Core.Extensions.ChunkExtensions.md\#Aislinn\_Core\_Extensions\_ChunkExtensions\_GetChunkFromSlot\_Aislinn\_Core\_Models\_Chunk\_System\_String\_), 
[ChunkExtensions.GetChunkFromSlotAsync\(Chunk, string, string\)](Aislinn.Core.Extensions.ChunkExtensions.md\#Aislinn\_Core\_Extensions\_ChunkExtensions\_GetChunkFromSlotAsync\_Aislinn\_Core\_Models\_Chunk\_System\_String\_System\_String\_), 
[ChunkExtensions.SetChunkReference\(Chunk, string, Chunk\)](Aislinn.Core.Extensions.ChunkExtensions.md\#Aislinn\_Core\_Extensions\_ChunkExtensions\_SetChunkReference\_Aislinn\_Core\_Models\_Chunk\_System\_String\_Aislinn\_Core\_Models\_Chunk\_)

## Constructors

### <a id="Aislinn_Core_Procedural_ProcedureChunk__ctor"></a> ProcedureChunk\(\)

Creates a new procedure chunk

```csharp
public ProcedureChunk()
```

### <a id="Aislinn_Core_Procedural_ProcedureChunk__ctor_System_String_Aislinn_Core_Procedural_ProcedureChunk_ProcedureType_"></a> ProcedureChunk\(string, ProcedureType\)

Creates a new procedure chunk with specific name and type

```csharp
public ProcedureChunk(string name, ProcedureChunk.ProcedureType type = ProcedureType.Sequence)
```

#### Parameters

`name` [string](https://learn.microsoft.com/dotnet/api/system.string)

`type` [ProcedureChunk](Aislinn.Core.Procedural.ProcedureChunk.md).[ProcedureType](Aislinn.Core.Procedural.ProcedureChunk.ProcedureType.md)

## Properties

### <a id="Aislinn_Core_Procedural_ProcedureChunk_ApplicableGoalTypes"></a> ApplicableGoalTypes

Goal type(s) this procedure can fulfill

```csharp
public List<string> ApplicableGoalTypes { get; set; }
```

#### Property Value

 [List](https://learn.microsoft.com/dotnet/api/system.collections.generic.list\-1)<[string](https://learn.microsoft.com/dotnet/api/system.string)\>

### <a id="Aislinn_Core_Procedural_ProcedureChunk_AverageExecutionTime"></a> AverageExecutionTime

Average execution time based on history

```csharp
public TimeSpan AverageExecutionTime { get; set; }
```

#### Property Value

 [TimeSpan](https://learn.microsoft.com/dotnet/api/system.timespan)

### <a id="Aislinn_Core_Procedural_ProcedureChunk_CurrentStepIndex"></a> CurrentStepIndex

Index of the current step being executed (when active)

```csharp
public int CurrentStepIndex { get; set; }
```

#### Property Value

 [int](https://learn.microsoft.com/dotnet/api/system.int32)

### <a id="Aislinn_Core_Procedural_ProcedureChunk_EstimatedCost"></a> EstimatedCost

Estimated cost of execution (resources, effort)

```csharp
public double EstimatedCost { get; set; }
```

#### Property Value

 [double](https://learn.microsoft.com/dotnet/api/system.double)

### <a id="Aislinn_Core_Procedural_ProcedureChunk_EstimatedDuration"></a> EstimatedDuration

Estimated execution time

```csharp
public TimeSpan EstimatedDuration { get; set; }
```

#### Property Value

 [TimeSpan](https://learn.microsoft.com/dotnet/api/system.timespan)

### <a id="Aislinn_Core_Procedural_ProcedureChunk_ExecutionContext"></a> ExecutionContext

Execution context for parameter binding and variable storage

```csharp
public Dictionary<string, object> ExecutionContext { get; set; }
```

#### Property Value

 [Dictionary](https://learn.microsoft.com/dotnet/api/system.collections.generic.dictionary\-2)<[string](https://learn.microsoft.com/dotnet/api/system.string), object\>

### <a id="Aislinn_Core_Procedural_ProcedureChunk_ExpectedEffects"></a> ExpectedEffects

Expected postconditions/effects when this procedure completes successfully

```csharp
public List<ProcedureEffect> ExpectedEffects { get; set; }
```

#### Property Value

 [List](https://learn.microsoft.com/dotnet/api/system.collections.generic.list\-1)<[ProcedureEffect](Aislinn.Core.Procedural.ProcedureEffect.md)\>

### <a id="Aislinn_Core_Procedural_ProcedureChunk_ExpectedUtility"></a> ExpectedUtility

Expected utility/reward when procedure completes successfully

```csharp
public double ExpectedUtility { get; set; }
```

#### Property Value

 [double](https://learn.microsoft.com/dotnet/api/system.double)

### <a id="Aislinn_Core_Procedural_ProcedureChunk_FailureCount"></a> FailureCount

Total failed executions count

```csharp
public int FailureCount { get; set; }
```

#### Property Value

 [int](https://learn.microsoft.com/dotnet/api/system.int32)

### <a id="Aislinn_Core_Procedural_ProcedureChunk_FailureStrategy"></a> FailureStrategy

Failure handling strategy

```csharp
public string FailureStrategy { get; set; }
```

#### Property Value

 [string](https://learn.microsoft.com/dotnet/api/system.string)

### <a id="Aislinn_Core_Procedural_ProcedureChunk_LastExecutionTime"></a> LastExecutionTime

When this procedure was last executed

```csharp
public DateTime LastExecutionTime { get; set; }
```

#### Property Value

 [DateTime](https://learn.microsoft.com/dotnet/api/system.datetime)

### <a id="Aislinn_Core_Procedural_ProcedureChunk_Preconditions"></a> Preconditions

Preconditions that must be satisfied for this procedure to be applicable

```csharp
public List<ProcedureCondition> Preconditions { get; set; }
```

#### Property Value

 [List](https://learn.microsoft.com/dotnet/api/system.collections.generic.list\-1)<[ProcedureCondition](Aislinn.Core.Procedural.ProcedureCondition.md)\>

### <a id="Aislinn_Core_Procedural_ProcedureChunk_RequiredResources"></a> RequiredResources

Resources required to execute this procedure

```csharp
public Dictionary<string, object> RequiredResources { get; set; }
```

#### Property Value

 [Dictionary](https://learn.microsoft.com/dotnet/api/system.collections.generic.dictionary\-2)<[string](https://learn.microsoft.com/dotnet/api/system.string), object\>

### <a id="Aislinn_Core_Procedural_ProcedureChunk_Status"></a> Status

Current execution status when this procedure is active

```csharp
public ProcedureChunk.ExecutionStatus Status { get; set; }
```

#### Property Value

 [ProcedureChunk](Aislinn.Core.Procedural.ProcedureChunk.md).[ExecutionStatus](Aislinn.Core.Procedural.ProcedureChunk.ExecutionStatus.md)

### <a id="Aislinn_Core_Procedural_ProcedureChunk_Steps"></a> Steps

The ordered sequence of steps for this procedure

```csharp
public List<ProcedureStep> Steps { get; set; }
```

#### Property Value

 [List](https://learn.microsoft.com/dotnet/api/system.collections.generic.list\-1)<[ProcedureStep](Aislinn.Core.Procedural.ProcedureStep.md)\>

### <a id="Aislinn_Core_Procedural_ProcedureChunk_SuccessCount"></a> SuccessCount

Total successful executions count

```csharp
public int SuccessCount { get; set; }
```

#### Property Value

 [int](https://learn.microsoft.com/dotnet/api/system.int32)

### <a id="Aislinn_Core_Procedural_ProcedureChunk_SuccessProbability"></a> SuccessProbability

Success probability based on past executions

```csharp
public double SuccessProbability { get; set; }
```

#### Property Value

 [double](https://learn.microsoft.com/dotnet/api/system.double)

### <a id="Aislinn_Core_Procedural_ProcedureChunk_Type"></a> Type

Type of procedure

```csharp
public ProcedureChunk.ProcedureType Type { get; set; }
```

#### Property Value

 [ProcedureChunk](Aislinn.Core.Procedural.ProcedureChunk.md).[ProcedureType](Aislinn.Core.Procedural.ProcedureChunk.ProcedureType.md)

## Methods

### <a id="Aislinn_Core_Procedural_ProcedureChunk_ArePreconditionsSatisfied_System_Collections_Generic_Dictionary_System_String_System_Object__"></a> ArePreconditionsSatisfied\(Dictionary<string, object\>\)

Check if all preconditions are satisfied given a context

```csharp
public bool ArePreconditionsSatisfied(Dictionary<string, object> context)
```

#### Parameters

`context` [Dictionary](https://learn.microsoft.com/dotnet/api/system.collections.generic.dictionary\-2)<[string](https://learn.microsoft.com/dotnet/api/system.string), object\>

#### Returns

 [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### <a id="Aislinn_Core_Procedural_ProcedureChunk_CanAchieveGoal_System_String_"></a> CanAchieveGoal\(string\)

Check if this procedure can achieve a specific goal type

```csharp
public bool CanAchieveGoal(string goalType)
```

#### Parameters

`goalType` [string](https://learn.microsoft.com/dotnet/api/system.string)

#### Returns

 [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### <a id="Aislinn_Core_Procedural_ProcedureChunk_CloneForExecution"></a> CloneForExecution\(\)

Clone this procedure for a specific execution instance

```csharp
public ProcedureChunk CloneForExecution()
```

#### Returns

 [ProcedureChunk](Aislinn.Core.Procedural.ProcedureChunk.md)

### <a id="Aislinn_Core_Procedural_ProcedureChunk_Reset"></a> Reset\(\)

Resets procedure execution state

```csharp
public void Reset()
```

### <a id="Aislinn_Core_Procedural_ProcedureChunk_UpdateStatistics_System_Boolean_System_TimeSpan_"></a> UpdateStatistics\(bool, TimeSpan\)

Update the success/failure statistics after execution

```csharp
public void UpdateStatistics(bool wasSuccessful, TimeSpan executionTime)
```

#### Parameters

`wasSuccessful` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

`executionTime` [TimeSpan](https://learn.microsoft.com/dotnet/api/system.timespan)

