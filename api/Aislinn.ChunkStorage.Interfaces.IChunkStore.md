# <a id="Aislinn_ChunkStorage_Interfaces_IChunkStore"></a> Interface IChunkStore

Namespace: [Aislinn.ChunkStorage.Interfaces](Aislinn.ChunkStorage.Interfaces.md)  
Assembly: Aislinn.Core.dll  

```csharp
public interface IChunkStore
```

## Methods

### <a id="Aislinn_ChunkStorage_Interfaces_IChunkStore_CreateCollectionAsync_System_String_"></a> CreateCollectionAsync\(string\)

```csharp
Task<IChunkCollection> CreateCollectionAsync(string collectionId)
```

#### Parameters

`collectionId` [string](https://learn.microsoft.com/dotnet/api/system.string)

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[IChunkCollection](Aislinn.ChunkStorage.Interfaces.IChunkCollection.md)\>

### <a id="Aislinn_ChunkStorage_Interfaces_IChunkStore_DeleteCollectionAsync_System_String_"></a> DeleteCollectionAsync\(string\)

```csharp
Task<bool> DeleteCollectionAsync(string collectionId)
```

#### Parameters

`collectionId` [string](https://learn.microsoft.com/dotnet/api/system.string)

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[bool](https://learn.microsoft.com/dotnet/api/system.boolean)\>

### <a id="Aislinn_ChunkStorage_Interfaces_IChunkStore_GetCollectionAsync_System_String_"></a> GetCollectionAsync\(string\)

```csharp
Task<IChunkCollection> GetCollectionAsync(string collectionId)
```

#### Parameters

`collectionId` [string](https://learn.microsoft.com/dotnet/api/system.string)

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[IChunkCollection](Aislinn.ChunkStorage.Interfaces.IChunkCollection.md)\>

### <a id="Aislinn_ChunkStorage_Interfaces_IChunkStore_GetCollectionIdsAsync"></a> GetCollectionIdsAsync\(\)

```csharp
Task<List<string>> GetCollectionIdsAsync()
```

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[List](https://learn.microsoft.com/dotnet/api/system.collections.generic.list\-1)<[string](https://learn.microsoft.com/dotnet/api/system.string)\>\>

### <a id="Aislinn_ChunkStorage_Interfaces_IChunkStore_GetOrCreateCollectionAsync_System_String_"></a> GetOrCreateCollectionAsync\(string\)

```csharp
Task<IChunkCollection> GetOrCreateCollectionAsync(string collectionId)
```

#### Parameters

`collectionId` [string](https://learn.microsoft.com/dotnet/api/system.string)

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[IChunkCollection](Aislinn.ChunkStorage.Interfaces.IChunkCollection.md)\>

