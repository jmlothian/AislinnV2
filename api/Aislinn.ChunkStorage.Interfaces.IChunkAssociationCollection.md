# <a id="Aislinn_ChunkStorage_Interfaces_IChunkAssociationCollection"></a> Interface IChunkAssociationCollection

Namespace: [Aislinn.ChunkStorage.Interfaces](Aislinn.ChunkStorage.Interfaces.md)  
Assembly: Aislinn.Core.dll  

```csharp
public interface IChunkAssociationCollection
```

## Methods

### <a id="Aislinn_ChunkStorage_Interfaces_IChunkAssociationCollection_AddAssociationAsync_Aislinn_Core_Models_ChunkAssociation_"></a> AddAssociationAsync\(ChunkAssociation\)

```csharp
Task<ChunkAssociation> AddAssociationAsync(ChunkAssociation association)
```

#### Parameters

`association` [ChunkAssociation](Aislinn.Core.Models.ChunkAssociation.md)

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[ChunkAssociation](Aislinn.Core.Models.ChunkAssociation.md)\>

### <a id="Aislinn_ChunkStorage_Interfaces_IChunkAssociationCollection_DeleteAssociationAsync_System_Guid_System_Guid_"></a> DeleteAssociationAsync\(Guid, Guid\)

```csharp
Task<bool> DeleteAssociationAsync(Guid chunkAId, Guid chunkBId)
```

#### Parameters

`chunkAId` [Guid](https://learn.microsoft.com/dotnet/api/system.guid)

`chunkBId` [Guid](https://learn.microsoft.com/dotnet/api/system.guid)

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[bool](https://learn.microsoft.com/dotnet/api/system.boolean)\>

### <a id="Aislinn_ChunkStorage_Interfaces_IChunkAssociationCollection_GetAssociationAsync_System_Guid_System_Guid_"></a> GetAssociationAsync\(Guid, Guid\)

```csharp
Task<ChunkAssociation> GetAssociationAsync(Guid chunkAId, Guid chunkBId)
```

#### Parameters

`chunkAId` [Guid](https://learn.microsoft.com/dotnet/api/system.guid)

`chunkBId` [Guid](https://learn.microsoft.com/dotnet/api/system.guid)

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[ChunkAssociation](Aislinn.Core.Models.ChunkAssociation.md)\>

### <a id="Aislinn_ChunkStorage_Interfaces_IChunkAssociationCollection_GetAssociationsForChunkAsync_System_Guid_"></a> GetAssociationsForChunkAsync\(Guid\)

```csharp
Task<List<ChunkAssociation>> GetAssociationsForChunkAsync(Guid chunkId)
```

#### Parameters

`chunkId` [Guid](https://learn.microsoft.com/dotnet/api/system.guid)

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[List](https://learn.microsoft.com/dotnet/api/system.collections.generic.list\-1)<[ChunkAssociation](Aislinn.Core.Models.ChunkAssociation.md)\>\>

### <a id="Aislinn_ChunkStorage_Interfaces_IChunkAssociationCollection_UpdateAssociationAsync_Aislinn_Core_Models_ChunkAssociation_"></a> UpdateAssociationAsync\(ChunkAssociation\)

```csharp
Task<bool> UpdateAssociationAsync(ChunkAssociation association)
```

#### Parameters

`association` [ChunkAssociation](Aislinn.Core.Models.ChunkAssociation.md)

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[bool](https://learn.microsoft.com/dotnet/api/system.boolean)\>

