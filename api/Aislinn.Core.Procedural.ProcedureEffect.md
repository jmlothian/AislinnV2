# <a id="Aislinn_Core_Procedural_ProcedureEffect"></a> Class ProcedureEffect

Namespace: [Aislinn.Core.Procedural](Aislinn.Core.Procedural.md)  
Assembly: Aislinn.Core.dll  

Represents an effect that occurs when a procedure step is executed

```csharp
public class ProcedureEffect
```

#### Inheritance

object ← 
[ProcedureEffect](Aislinn.Core.Procedural.ProcedureEffect.md)

## Properties

### <a id="Aislinn_Core_Procedural_ProcedureEffect_Operation"></a> Operation

Operation to perform (=, +=, -=, etc. for modifications)

```csharp
public string Operation { get; set; }
```

#### Property Value

 [string](https://learn.microsoft.com/dotnet/api/system.string)

### <a id="Aislinn_Core_Procedural_ProcedureEffect_Parameters"></a> Parameters

Additional parameters for effect application

```csharp
public Dictionary<string, object> Parameters { get; set; }
```

#### Property Value

 [Dictionary](https://learn.microsoft.com/dotnet/api/system.collections.generic.dictionary\-2)<[string](https://learn.microsoft.com/dotnet/api/system.string), object\>

### <a id="Aislinn_Core_Procedural_ProcedureEffect_Target"></a> Target

Target of the effect (variable name, entity reference, etc.)

```csharp
public string Target { get; set; }
```

#### Property Value

 [string](https://learn.microsoft.com/dotnet/api/system.string)

### <a id="Aislinn_Core_Procedural_ProcedureEffect_Type"></a> Type

Type of this effect

```csharp
public ProcedureEffect.EffectType Type { get; set; }
```

#### Property Value

 [ProcedureEffect](Aislinn.Core.Procedural.ProcedureEffect.md).[EffectType](Aislinn.Core.Procedural.ProcedureEffect.EffectType.md)

### <a id="Aislinn_Core_Procedural_ProcedureEffect_Value"></a> Value

Value to apply in the effect

```csharp
public object Value { get; set; }
```

#### Property Value

 object

## Methods

### <a id="Aislinn_Core_Procedural_ProcedureEffect_Apply_System_Collections_Generic_Dictionary_System_String_System_Object__"></a> Apply\(Dictionary<string, object\>\)

Applies this effect to the given context

```csharp
public void Apply(Dictionary<string, object> context)
```

#### Parameters

`context` [Dictionary](https://learn.microsoft.com/dotnet/api/system.collections.generic.dictionary\-2)<[string](https://learn.microsoft.com/dotnet/api/system.string), object\>

### <a id="Aislinn_Core_Procedural_ProcedureEffect_Clone"></a> Clone\(\)

Creates a clone of this effect

```csharp
public ProcedureEffect Clone()
```

#### Returns

 [ProcedureEffect](Aislinn.Core.Procedural.ProcedureEffect.md)

### <a id="Aislinn_Core_Procedural_ProcedureEffect_CreateAssignment_System_String_System_Object_"></a> CreateAssignment\(string, object\)

Creates a simple assignment effect

```csharp
public static ProcedureEffect CreateAssignment(string target, object value)
```

#### Parameters

`target` [string](https://learn.microsoft.com/dotnet/api/system.string)

`value` object

#### Returns

 [ProcedureEffect](Aislinn.Core.Procedural.ProcedureEffect.md)

### <a id="Aislinn_Core_Procedural_ProcedureEffect_CreateCreation_System_String_System_String_System_Object_"></a> CreateCreation\(string, string, object\)

Creates a creation effect

```csharp
public static ProcedureEffect CreateCreation(string entityType, string target, object properties)
```

#### Parameters

`entityType` [string](https://learn.microsoft.com/dotnet/api/system.string)

`target` [string](https://learn.microsoft.com/dotnet/api/system.string)

`properties` object

#### Returns

 [ProcedureEffect](Aislinn.Core.Procedural.ProcedureEffect.md)

### <a id="Aislinn_Core_Procedural_ProcedureEffect_CreateDeletion_System_String_"></a> CreateDeletion\(string\)

Creates a deletion effect

```csharp
public static ProcedureEffect CreateDeletion(string target)
```

#### Parameters

`target` [string](https://learn.microsoft.com/dotnet/api/system.string)

#### Returns

 [ProcedureEffect](Aislinn.Core.Procedural.ProcedureEffect.md)

### <a id="Aislinn_Core_Procedural_ProcedureEffect_CreateModification_System_String_System_String_System_Object_"></a> CreateModification\(string, string, object\)

Creates a modification effect

```csharp
public static ProcedureEffect CreateModification(string target, string operation, object value)
```

#### Parameters

`target` [string](https://learn.microsoft.com/dotnet/api/system.string)

`operation` [string](https://learn.microsoft.com/dotnet/api/system.string)

`value` object

#### Returns

 [ProcedureEffect](Aislinn.Core.Procedural.ProcedureEffect.md)

