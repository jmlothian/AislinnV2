# <a id="Aislinn_Core_Procedural"></a> Namespace Aislinn.Core.Procedural

### Classes

 [ProcedureChunk](Aislinn.Core.Procedural.ProcedureChunk.md)

Represents procedural knowledge as a specialized chunk type containing
execution steps, control flow, and metadata about when and how to apply the procedure.

 [ProcedureCondition](Aislinn.Core.Procedural.ProcedureCondition.md)

Represents a condition that can be evaluated during procedure execution

 [ProcedureEffect](Aislinn.Core.Procedural.ProcedureEffect.md)

Represents an effect that occurs when a procedure step is executed

 [ProcedureMatcher.ProcedureMatchResult](Aislinn.Core.Procedural.ProcedureMatcher.ProcedureMatchResult.md)

Result of a procedure matching operation

 [ProcedureMatcher](Aislinn.Core.Procedural.ProcedureMatcher.md)

Matches goals with appropriate procedures based on goal type, context,
and other factors. Also handles procedure activation and selection.

 [ProcedureMatcher.ProcedureMatcherConfig](Aislinn.Core.Procedural.ProcedureMatcher.ProcedureMatcherConfig.md)

Configuration options for procedure matching

 [ProcedureStep](Aislinn.Core.Procedural.ProcedureStep.md)

Represents a single step within a procedure, including the operation to perform,
parameters, and control flow details.

 [StepBranch](Aislinn.Core.Procedural.StepBranch.md)

Represents a conditional branch in procedure control flow

 [StepRetryPolicy](Aislinn.Core.Procedural.StepRetryPolicy.md)

Retry policy for handling step failures

### Enums

 [ProcedureCondition.ConditionType](Aislinn.Core.Procedural.ProcedureCondition.ConditionType.md)

Types of conditions

 [ProcedureEffect.EffectType](Aislinn.Core.Procedural.ProcedureEffect.EffectType.md)

Types of effects

 [ProcedureChunk.ExecutionStatus](Aislinn.Core.Procedural.ProcedureChunk.ExecutionStatus.md)

The execution status of a procedure

 [ProcedureChunk.ProcedureType](Aislinn.Core.Procedural.ProcedureChunk.ProcedureType.md)

The type of procedure represented

 [ProcedureStep.StepStatus](Aislinn.Core.Procedural.ProcedureStep.StepStatus.md)

Execution status of this step

 [ProcedureStep.StepType](Aislinn.Core.Procedural.ProcedureStep.StepType.md)

Types of operations a step can perform

