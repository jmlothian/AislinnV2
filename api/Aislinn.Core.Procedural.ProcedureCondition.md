# <a id="Aislinn_Core_Procedural_ProcedureCondition"></a> Class ProcedureCondition

Namespace: [Aislinn.Core.Procedural](Aislinn.Core.Procedural.md)  
Assembly: Aislinn.Core.dll  

Represents a condition that can be evaluated during procedure execution

```csharp
public class ProcedureCondition
```

#### Inheritance

object ← 
[ProcedureCondition](Aislinn.Core.Procedural.ProcedureCondition.md)

## Properties

### <a id="Aislinn_Core_Procedural_ProcedureCondition_LeftOperand"></a> LeftOperand

For Comparison: left side of comparison
For Existence/TypeCheck: target value reference
For Custom: expression to evaluate

```csharp
public string LeftOperand { get; set; }
```

#### Property Value

 [string](https://learn.microsoft.com/dotnet/api/system.string)

### <a id="Aislinn_Core_Procedural_ProcedureCondition_NegatableCondition"></a> NegatableCondition

For Negation: the condition to negate

```csharp
public ProcedureCondition NegatableCondition { get; set; }
```

#### Property Value

 [ProcedureCondition](Aislinn.Core.Procedural.ProcedureCondition.md)

### <a id="Aislinn_Core_Procedural_ProcedureCondition_Operator"></a> Operator

For Comparison: comparison operator (==, !=, &gt;, &lt;, etc.)
For TypeCheck: expected type name

```csharp
public string Operator { get; set; }
```

#### Property Value

 [string](https://learn.microsoft.com/dotnet/api/system.string)

### <a id="Aislinn_Core_Procedural_ProcedureCondition_Parameters"></a> Parameters

Additional parameters for condition evaluation

```csharp
public Dictionary<string, object> Parameters { get; set; }
```

#### Property Value

 [Dictionary](https://learn.microsoft.com/dotnet/api/system.collections.generic.dictionary\-2)<[string](https://learn.microsoft.com/dotnet/api/system.string), object\>

### <a id="Aislinn_Core_Procedural_ProcedureCondition_RightOperand"></a> RightOperand

For Comparison: right side of comparison

```csharp
public object RightOperand { get; set; }
```

#### Property Value

 object

### <a id="Aislinn_Core_Procedural_ProcedureCondition_Subconditions"></a> Subconditions

For Conjunction/Disjunction: subconditions

```csharp
public List<ProcedureCondition> Subconditions { get; set; }
```

#### Property Value

 [List](https://learn.microsoft.com/dotnet/api/system.collections.generic.list\-1)<[ProcedureCondition](Aislinn.Core.Procedural.ProcedureCondition.md)\>

### <a id="Aislinn_Core_Procedural_ProcedureCondition_Type"></a> Type

Type of this condition

```csharp
public ProcedureCondition.ConditionType Type { get; set; }
```

#### Property Value

 [ProcedureCondition](Aislinn.Core.Procedural.ProcedureCondition.md).[ConditionType](Aislinn.Core.Procedural.ProcedureCondition.ConditionType.md)

## Methods

### <a id="Aislinn_Core_Procedural_ProcedureCondition_Clone"></a> Clone\(\)

Creates a clone of this condition

```csharp
public ProcedureCondition Clone()
```

#### Returns

 [ProcedureCondition](Aislinn.Core.Procedural.ProcedureCondition.md)

### <a id="Aislinn_Core_Procedural_ProcedureCondition_CreateComparison_System_String_System_String_System_Object_"></a> CreateComparison\(string, string, object\)

Creates a simple comparison condition

```csharp
public static ProcedureCondition CreateComparison(string left, string op, object right)
```

#### Parameters

`left` [string](https://learn.microsoft.com/dotnet/api/system.string)

`op` [string](https://learn.microsoft.com/dotnet/api/system.string)

`right` object

#### Returns

 [ProcedureCondition](Aislinn.Core.Procedural.ProcedureCondition.md)

### <a id="Aislinn_Core_Procedural_ProcedureCondition_CreateConjunction_Aislinn_Core_Procedural_ProcedureCondition___"></a> CreateConjunction\(params ProcedureCondition\[\]\)

Creates an AND condition of multiple subconditions

```csharp
public static ProcedureCondition CreateConjunction(params ProcedureCondition[] conditions)
```

#### Parameters

`conditions` [ProcedureCondition](Aislinn.Core.Procedural.ProcedureCondition.md)\[\]

#### Returns

 [ProcedureCondition](Aislinn.Core.Procedural.ProcedureCondition.md)

### <a id="Aislinn_Core_Procedural_ProcedureCondition_CreateDisjunction_Aislinn_Core_Procedural_ProcedureCondition___"></a> CreateDisjunction\(params ProcedureCondition\[\]\)

Creates an OR condition of multiple subconditions

```csharp
public static ProcedureCondition CreateDisjunction(params ProcedureCondition[] conditions)
```

#### Parameters

`conditions` [ProcedureCondition](Aislinn.Core.Procedural.ProcedureCondition.md)\[\]

#### Returns

 [ProcedureCondition](Aislinn.Core.Procedural.ProcedureCondition.md)

### <a id="Aislinn_Core_Procedural_ProcedureCondition_CreateExistenceCheck_System_String_System_Boolean_"></a> CreateExistenceCheck\(string, bool\)

Creates an existence check condition

```csharp
public static ProcedureCondition CreateExistenceCheck(string target, bool shouldExist = true)
```

#### Parameters

`target` [string](https://learn.microsoft.com/dotnet/api/system.string)

`shouldExist` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

#### Returns

 [ProcedureCondition](Aislinn.Core.Procedural.ProcedureCondition.md)

### <a id="Aislinn_Core_Procedural_ProcedureCondition_CreateNegation_Aislinn_Core_Procedural_ProcedureCondition_"></a> CreateNegation\(ProcedureCondition\)

Creates a NOT condition

```csharp
public static ProcedureCondition CreateNegation(ProcedureCondition condition)
```

#### Parameters

`condition` [ProcedureCondition](Aislinn.Core.Procedural.ProcedureCondition.md)

#### Returns

 [ProcedureCondition](Aislinn.Core.Procedural.ProcedureCondition.md)

### <a id="Aislinn_Core_Procedural_ProcedureCondition_CreateTypeCheck_System_String_System_String_"></a> CreateTypeCheck\(string, string\)

Creates a type check condition

```csharp
public static ProcedureCondition CreateTypeCheck(string target, string expectedType)
```

#### Parameters

`target` [string](https://learn.microsoft.com/dotnet/api/system.string)

`expectedType` [string](https://learn.microsoft.com/dotnet/api/system.string)

#### Returns

 [ProcedureCondition](Aislinn.Core.Procedural.ProcedureCondition.md)

### <a id="Aislinn_Core_Procedural_ProcedureCondition_Evaluate_System_Collections_Generic_Dictionary_System_String_System_Object__"></a> Evaluate\(Dictionary<string, object\>\)

Evaluates this condition against the given context

```csharp
public bool Evaluate(Dictionary<string, object> context)
```

#### Parameters

`context` [Dictionary](https://learn.microsoft.com/dotnet/api/system.collections.generic.dictionary\-2)<[string](https://learn.microsoft.com/dotnet/api/system.string), object\>

#### Returns

 [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

