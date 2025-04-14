# <a id="Aislinn_Core_Activation_IActivationModel"></a> Interface IActivationModel

Namespace: [Aislinn.Core.Activation](Aislinn.Core.Activation.md)  
Assembly: Aislinn.Core.dll  

Interface defining an activation model that can be plugged into the cognitive architecture

```csharp
public interface IActivationModel
```

## Methods

### <a id="Aislinn_Core_Activation_IActivationModel_ApplyDecay_Aislinn_Core_Models_Chunk_System_Double_"></a> ApplyDecay\(Chunk, double\)

Apply activation decay over time

```csharp
double ApplyDecay(Chunk chunk, double timeSinceLastUpdate)
```

#### Parameters

`chunk` [Chunk](Aislinn.Core.Models.Chunk.md)

The chunk to apply decay to

`timeSinceLastUpdate` [double](https://learn.microsoft.com/dotnet/api/system.double)

Time elapsed since last update (in seconds)

#### Returns

 [double](https://learn.microsoft.com/dotnet/api/system.double)

The new activation level after decay

### <a id="Aislinn_Core_Activation_IActivationModel_CalculateActivation_Aislinn_Core_Models_Chunk_System_Nullable_System_DateTime__"></a> CalculateActivation\(Chunk, DateTime?\)

Calculate the activation level for a chunk based on its history and current state

```csharp
double CalculateActivation(Chunk chunk, DateTime? currentTime = null)
```

#### Parameters

`chunk` [Chunk](Aislinn.Core.Models.Chunk.md)

The chunk to calculate activation for

`currentTime` [DateTime](https://learn.microsoft.com/dotnet/api/system.datetime)?

Current time for reference (defaults to DateTime.Now)

#### Returns

 [double](https://learn.microsoft.com/dotnet/api/system.double)

The calculated activation level

### <a id="Aislinn_Core_Activation_IActivationModel_CalculateSpreadingActivation_Aislinn_Core_Models_Chunk_Aislinn_Core_Models_Chunk_System_Double_System_Double_"></a> CalculateSpreadingActivation\(Chunk, Chunk, double, double\)

Calculate spreading activation from source chunk to target chunk

```csharp
double CalculateSpreadingActivation(Chunk sourceChunk, Chunk targetChunk, double associationWeight, double spreadingFactor)
```

#### Parameters

`sourceChunk` [Chunk](Aislinn.Core.Models.Chunk.md)

The source of spreading activation

`targetChunk` [Chunk](Aislinn.Core.Models.Chunk.md)

The target receiving activation

`associationWeight` [double](https://learn.microsoft.com/dotnet/api/system.double)

Weight of association between chunks

`spreadingFactor` [double](https://learn.microsoft.com/dotnet/api/system.double)

Factor controlling spreading intensity

#### Returns

 [double](https://learn.microsoft.com/dotnet/api/system.double)

Amount of activation to spread to target

