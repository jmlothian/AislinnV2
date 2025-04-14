# <a id="Aislinn_ChunkStorage_Interfaces_IAssociationStore"></a> Interface IAssociationStore

Namespace: [Aislinn.ChunkStorage.Interfaces](Aislinn.ChunkStorage.Interfaces.md)  
Assembly: Aislinn.Core.dll  

```csharp
public interface IAssociationStore
```

## Methods

### <a id="Aislinn_ChunkStorage_Interfaces_IAssociationStore_CreateCollectionAsync_System_String_"></a> CreateCollectionAsync\(string\)

```csharp
Task<IChunkAssociationCollection> CreateCollectionAsync(string collectionId)
```

#### Parameters

`collectionId` [string](https://learn.microsoft.com/dotnet/api/system.string)

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[IChunkAssociationCollection](Aislinn.ChunkStorage.Interfaces.IChunkAssociationCollection.md)\>

### <a id="Aislinn_ChunkStorage_Interfaces_IAssociationStore_DeleteCollectionAsync_System_String_"></a> DeleteCollectionAsync\(string\)

```csharp
Task<bool> DeleteCollectionAsync(string collectionId)
```

#### Parameters

`collectionId` [string](https://learn.microsoft.com/dotnet/api/system.string)

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[bool](https://learn.microsoft.com/dotnet/api/system.boolean)\>

### <a id="Aislinn_ChunkStorage_Interfaces_IAssociationStore_GetCollectionAsync_System_String_"></a> GetCollectionAsync\(string\)

```csharp
Task<IChunkAssociationCollection> GetCollectionAsync(string collectionId)
```

#### Parameters

`collectionId` [string](https://learn.microsoft.com/dotnet/api/system.string)

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[IChunkAssociationCollection](Aislinn.ChunkStorage.Interfaces.IChunkAssociationCollection.md)\>

### <a id="Aislinn_ChunkStorage_Interfaces_IAssociationStore_GetCollectionIdsAsync"></a> GetCollectionIdsAsync\(\)

```csharp
Task<List<string>> GetCollectionIdsAsync()
```

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[List](https://learn.microsoft.com/dotnet/api/system.collections.generic.list\-1)<[string](https://learn.microsoft.com/dotnet/api/system.string)\>\>

### <a id="Aislinn_ChunkStorage_Interfaces_IAssociationStore_GetOrCreateCollectionAsync_System_String_"></a> GetOrCreateCollectionAsync\(string\)

```csharp
Task<IChunkAssociationCollection> GetOrCreateCollectionAsync(string collectionId)
```

#### Parameters

`collectionId` [string](https://learn.microsoft.com/dotnet/api/system.string)

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[IChunkAssociationCollection](Aislinn.ChunkStorage.Interfaces.IChunkAssociationCollection.md)\>

