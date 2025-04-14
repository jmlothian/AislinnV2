# <a id="Aislinn_Core_Activation_ActivationParametersRegistry"></a> Class ActivationParametersRegistry

Namespace: [Aislinn.Core.Activation](Aislinn.Core.Activation.md)  
Assembly: Aislinn.Core.dll  

Registry for managing type-specific activation parameters

```csharp
public class ActivationParametersRegistry
```

#### Inheritance

object ← 
[ActivationParametersRegistry](Aislinn.Core.Activation.ActivationParametersRegistry.md)

## Constructors

### <a id="Aislinn_Core_Activation_ActivationParametersRegistry__ctor"></a> ActivationParametersRegistry\(\)

Creates a new activation parameters registry with default settings

```csharp
public ActivationParametersRegistry()
```

## Methods

### <a id="Aislinn_Core_Activation_ActivationParametersRegistry_GetDefaultParameters"></a> GetDefaultParameters\(\)

Gets the default parameters used when no type-specific ones exist

```csharp
public TypeActivationParameters GetDefaultParameters()
```

#### Returns

 [TypeActivationParameters](Aislinn.Models.Activation.TypeActivationParameters.md)

### <a id="Aislinn_Core_Activation_ActivationParametersRegistry_GetParameters_Aislinn_Core_Models_Chunk_"></a> GetParameters\(Chunk\)

Gets activation parameters for a specific chunk

```csharp
public TypeActivationParameters GetParameters(Chunk chunk)
```

#### Parameters

`chunk` [Chunk](Aislinn.Core.Models.Chunk.md)

#### Returns

 [TypeActivationParameters](Aislinn.Models.Activation.TypeActivationParameters.md)

### <a id="Aislinn_Core_Activation_ActivationParametersRegistry_GetParametersForType_System_String_"></a> GetParametersForType\(string\)

Gets activation parameters for a specific chunk type

```csharp
public TypeActivationParameters GetParametersForType(string chunkType)
```

#### Parameters

`chunkType` [string](https://learn.microsoft.com/dotnet/api/system.string)

#### Returns

 [TypeActivationParameters](Aislinn.Models.Activation.TypeActivationParameters.md)

### <a id="Aislinn_Core_Activation_ActivationParametersRegistry_RegisterTypeParameters_System_String_Aislinn_Models_Activation_TypeActivationParameters_"></a> RegisterTypeParameters\(string, TypeActivationParameters\)

Registers parameters for a specific chunk type

```csharp
public void RegisterTypeParameters(string chunkType, TypeActivationParameters parameters)
```

#### Parameters

`chunkType` [string](https://learn.microsoft.com/dotnet/api/system.string)

`parameters` [TypeActivationParameters](Aislinn.Models.Activation.TypeActivationParameters.md)

### <a id="Aislinn_Core_Activation_ActivationParametersRegistry_SetDefaultParameters_Aislinn_Models_Activation_TypeActivationParameters_"></a> SetDefaultParameters\(TypeActivationParameters\)

Updates the default parameters

```csharp
public void SetDefaultParameters(TypeActivationParameters parameters)
```

#### Parameters

`parameters` [TypeActivationParameters](Aislinn.Models.Activation.TypeActivationParameters.md)

