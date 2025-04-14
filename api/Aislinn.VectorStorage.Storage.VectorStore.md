# <a id="Aislinn_VectorStorage_Storage_VectorStore"></a> Class VectorStore

Namespace: [Aislinn.VectorStorage.Storage](Aislinn.VectorStorage.Storage.md)  
Assembly: Aislinn.VectorStorage.dll  

```csharp
public class VectorStore
```

#### Inheritance

object ← 
[VectorStore](Aislinn.VectorStorage.Storage.VectorStore.md)

## Constructors

### <a id="Aislinn_VectorStorage_Storage_VectorStore__ctor"></a> VectorStore\(\)

```csharp
public VectorStore()
```

## Methods

### <a id="Aislinn_VectorStorage_Storage_VectorStore_CreateCollectionAsync_System_String_Aislinn_VectorStorage_Interfaces_IVectorizer_"></a> CreateCollectionAsync\(string, IVectorizer\)

```csharp
public Task<IVectorCollection> CreateCollectionAsync(string collectionId, IVectorizer vectorizer)
```

#### Parameters

`collectionId` [string](https://learn.microsoft.com/dotnet/api/system.string)

`vectorizer` [IVectorizer](Aislinn.VectorStorage.Interfaces.IVectorizer.md)

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[IVectorCollection](Aislinn.VectorStorage.Interfaces.IVectorCollection.md)\>

### <a id="Aislinn_VectorStorage_Storage_VectorStore_DeleteCollectionAsync_System_String_"></a> DeleteCollectionAsync\(string\)

```csharp
public Task<bool> DeleteCollectionAsync(string collectionId)
```

#### Parameters

`collectionId` [string](https://learn.microsoft.com/dotnet/api/system.string)

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[bool](https://learn.microsoft.com/dotnet/api/system.boolean)\>

### <a id="Aislinn_VectorStorage_Storage_VectorStore_GetCollectionAsync_System_String_"></a> GetCollectionAsync\(string\)

```csharp
public Task<IVectorCollection> GetCollectionAsync(string collectionId)
```

#### Parameters

`collectionId` [string](https://learn.microsoft.com/dotnet/api/system.string)

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[IVectorCollection](Aislinn.VectorStorage.Interfaces.IVectorCollection.md)\>

### <a id="Aislinn_VectorStorage_Storage_VectorStore_GetCollectionIdsAsync"></a> GetCollectionIdsAsync\(\)

```csharp
public Task<List<string>> GetCollectionIdsAsync()
```

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[List](https://learn.microsoft.com/dotnet/api/system.collections.generic.list\-1)<[string](https://learn.microsoft.com/dotnet/api/system.string)\>\>

### <a id="Aislinn_VectorStorage_Storage_VectorStore_GetOrCreateCollectionAsync_System_String_Aislinn_VectorStorage_Interfaces_IVectorizer_"></a> GetOrCreateCollectionAsync\(string, IVectorizer\)

```csharp
public Task<IVectorCollection> GetOrCreateCollectionAsync(string collectionId, IVectorizer vectorizer)
```

#### Parameters

`collectionId` [string](https://learn.microsoft.com/dotnet/api/system.string)

`vectorizer` [IVectorizer](Aislinn.VectorStorage.Interfaces.IVectorizer.md)

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[IVectorCollection](Aislinn.VectorStorage.Interfaces.IVectorCollection.md)\>

### <a id="Aislinn_VectorStorage_Storage_VectorStore_SearchAllCollectionsAsync_System_String_System_Int32_System_Double_"></a> SearchAllCollectionsAsync\(string, int, double\)

```csharp
public Task<List<SearchResult>> SearchAllCollectionsAsync(string query, int topN, double minSimilarity = 0)
```

#### Parameters

`query` [string](https://learn.microsoft.com/dotnet/api/system.string)

`topN` [int](https://learn.microsoft.com/dotnet/api/system.int32)

`minSimilarity` [double](https://learn.microsoft.com/dotnet/api/system.double)

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[List](https://learn.microsoft.com/dotnet/api/system.collections.generic.list\-1)<[SearchResult](Aislinn.VectorStorage.Models.SearchResult.md)\>\>

