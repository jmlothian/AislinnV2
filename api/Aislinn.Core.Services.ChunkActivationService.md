# <a id="Aislinn_Core_Services_ChunkActivationService"></a> Class ChunkActivationService

Namespace: [Aislinn.Core.Services](Aislinn.Core.Services.md)  
Assembly: Aislinn.Core.dll  

```csharp
public class ChunkActivationService
```

#### Inheritance

object ← 
[ChunkActivationService](Aislinn.Core.Services.ChunkActivationService.md)

## Constructors

### <a id="Aislinn_Core_Services_ChunkActivationService__ctor_Aislinn_ChunkStorage_Interfaces_IChunkStore_Aislinn_ChunkStorage_Interfaces_IAssociationStore_Aislinn_Core_Activation_IActivationModel_Aislinn_Core_Activation_ActivationParametersRegistry_System_String_System_String_"></a> ChunkActivationService\(IChunkStore, IAssociationStore, IActivationModel, ActivationParametersRegistry, string, string\)

```csharp
public ChunkActivationService(IChunkStore chunkStore, IAssociationStore associationStore, IActivationModel activationModel, ActivationParametersRegistry parametersRegistry = null, string chunkCollectionId = "default", string associationCollectionId = "default")
```

#### Parameters

`chunkStore` [IChunkStore](Aislinn.ChunkStorage.Interfaces.IChunkStore.md)

`associationStore` [IAssociationStore](Aislinn.ChunkStorage.Interfaces.IAssociationStore.md)

`activationModel` [IActivationModel](Aislinn.Core.Activation.IActivationModel.md)

`parametersRegistry` [ActivationParametersRegistry](Aislinn.Core.Activation.ActivationParametersRegistry.md)

`chunkCollectionId` [string](https://learn.microsoft.com/dotnet/api/system.string)

`associationCollectionId` [string](https://learn.microsoft.com/dotnet/api/system.string)

## Methods

### <a id="Aislinn_Core_Services_ChunkActivationService_ActivateChunkAsync_System_Guid_System_String_System_Double_"></a> ActivateChunkAsync\(Guid, string, double\)

Activates a chunk with the specified ID and applies spreading activation

```csharp
public Task<Chunk> ActivateChunkAsync(Guid chunkId, string emotionName = null, double activationBoost = 1)
```

#### Parameters

`chunkId` [Guid](https://learn.microsoft.com/dotnet/api/system.guid)

`emotionName` [string](https://learn.microsoft.com/dotnet/api/system.string)

`activationBoost` [double](https://learn.microsoft.com/dotnet/api/system.double)

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[Chunk](Aislinn.Core.Models.Chunk.md)\>

### <a id="Aislinn_Core_Services_ChunkActivationService_ApplyDecayAsync"></a> ApplyDecayAsync\(\)

Apply activation decay to all chunks in the system

```csharp
public Task ApplyDecayAsync()
```

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task)

### <a id="Aislinn_Core_Services_ChunkActivationService_CreateAssociationAsync_System_Guid_System_Guid_System_String_System_String_System_Double_System_Double_"></a> CreateAssociationAsync\(Guid, Guid, string, string, double, double\)

Create an association between two chunks and update their slots

```csharp
public Task<ChunkAssociation> CreateAssociationAsync(Guid chunkAId, Guid chunkBId, string relationAtoB, string relationBtoA, double initialWeightAtoB = 0.5, double initialWeightBtoA = 0.5)
```

#### Parameters

`chunkAId` [Guid](https://learn.microsoft.com/dotnet/api/system.guid)

`chunkBId` [Guid](https://learn.microsoft.com/dotnet/api/system.guid)

`relationAtoB` [string](https://learn.microsoft.com/dotnet/api/system.string)

`relationBtoA` [string](https://learn.microsoft.com/dotnet/api/system.string)

`initialWeightAtoB` [double](https://learn.microsoft.com/dotnet/api/system.double)

`initialWeightBtoA` [double](https://learn.microsoft.com/dotnet/api/system.double)

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[ChunkAssociation](Aislinn.Core.Models.ChunkAssociation.md)\>

### <a id="Aislinn_Core_Services_ChunkActivationService_GetActiveChunksAsync_System_Double_"></a> GetActiveChunksAsync\(double\)

Get chunks above a certain activation threshold

```csharp
public Task<List<Chunk>> GetActiveChunksAsync(double threshold = 0.1)
```

#### Parameters

`threshold` [double](https://learn.microsoft.com/dotnet/api/system.double)

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[List](https://learn.microsoft.com/dotnet/api/system.collections.generic.list\-1)<[Chunk](Aislinn.Core.Models.Chunk.md)\>\>

