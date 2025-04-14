# <a id="Aislinn_Models_Activation_TypeActivationParameters"></a> Class TypeActivationParameters

Namespace: [Aislinn.Models.Activation](Aislinn.Models.Activation.md)  
Assembly: Aislinn.Core.dll  

Parameters for type-specific activation dynamics

```csharp
public class TypeActivationParameters
```

#### Inheritance

object ← 
[TypeActivationParameters](Aislinn.Models.Activation.TypeActivationParameters.md)

## Constructors

### <a id="Aislinn_Models_Activation_TypeActivationParameters__ctor"></a> TypeActivationParameters\(\)

Creates default activation parameters

```csharp
public TypeActivationParameters()
```

### <a id="Aislinn_Models_Activation_TypeActivationParameters__ctor_System_String_"></a> TypeActivationParameters\(string\)

Creates activation parameters for a specific chunk type

```csharp
public TypeActivationParameters(string chunkType)
```

#### Parameters

`chunkType` [string](https://learn.microsoft.com/dotnet/api/system.string)

## Properties

### <a id="Aislinn_Models_Activation_TypeActivationParameters_ActivationCeiling"></a> ActivationCeiling

Maximum activation level this type can reach

```csharp
public double ActivationCeiling { get; set; }
```

#### Property Value

 [double](https://learn.microsoft.com/dotnet/api/system.double)

### <a id="Aislinn_Models_Activation_TypeActivationParameters_ActivationNoise"></a> ActivationNoise

How much activation noise to apply (0-1)

```csharp
public double ActivationNoise { get; set; }
```

#### Property Value

 [double](https://learn.microsoft.com/dotnet/api/system.double)

### <a id="Aislinn_Models_Activation_TypeActivationParameters_AssociationStrengthIncrement"></a> AssociationStrengthIncrement

```csharp
public double AssociationStrengthIncrement { get; set; }
```

#### Property Value

 [double](https://learn.microsoft.com/dotnet/api/system.double)

### <a id="Aislinn_Models_Activation_TypeActivationParameters_BaseActivationBoost"></a> BaseActivationBoost

Base level of activation added during explicit activation

```csharp
public double BaseActivationBoost { get; set; }
```

#### Property Value

 [double](https://learn.microsoft.com/dotnet/api/system.double)

### <a id="Aislinn_Models_Activation_TypeActivationParameters_ChunkType"></a> ChunkType

The chunk type these parameters apply to

```csharp
public string ChunkType { get; set; }
```

#### Property Value

 [string](https://learn.microsoft.com/dotnet/api/system.string)

### <a id="Aislinn_Models_Activation_TypeActivationParameters_DecayRate"></a> DecayRate

How quickly activation decays over time (0-1)

```csharp
public double DecayRate { get; set; }
```

#### Property Value

 [double](https://learn.microsoft.com/dotnet/api/system.double)

### <a id="Aislinn_Models_Activation_TypeActivationParameters_InitialActivation"></a> InitialActivation

Initial activation level for new chunks of this type

```csharp
public double InitialActivation { get; set; }
```

#### Property Value

 [double](https://learn.microsoft.com/dotnet/api/system.double)

### <a id="Aislinn_Models_Activation_TypeActivationParameters_SpreadingFactor"></a> SpreadingFactor

Factor for spreading activation to other chunks (0-1)

```csharp
public double SpreadingFactor { get; set; }
```

#### Property Value

 [double](https://learn.microsoft.com/dotnet/api/system.double)

### <a id="Aislinn_Models_Activation_TypeActivationParameters_WorkingMemoryPriority"></a> WorkingMemoryPriority

Priority for entering working memory (0-1)

```csharp
public double WorkingMemoryPriority { get; set; }
```

#### Property Value

 [double](https://learn.microsoft.com/dotnet/api/system.double)

