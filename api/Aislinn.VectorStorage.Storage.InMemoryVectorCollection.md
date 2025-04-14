# <a id="Aislinn_VectorStorage_Storage_InMemoryVectorCollection"></a> Class InMemoryVectorCollection

Namespace: [Aislinn.VectorStorage.Storage](Aislinn.VectorStorage.Storage.md)  
Assembly: Aislinn.VectorStorage.dll  

```csharp
public class InMemoryVectorCollection : IVectorCollection
```

#### Inheritance

object ← 
[InMemoryVectorCollection](Aislinn.VectorStorage.Storage.InMemoryVectorCollection.md)

#### Implements

[IVectorCollection](Aislinn.VectorStorage.Interfaces.IVectorCollection.md)

## Constructors

### <a id="Aislinn_VectorStorage_Storage_InMemoryVectorCollection__ctor_System_String_Aislinn_VectorStorage_Interfaces_IVectorizer_"></a> InMemoryVectorCollection\(string, IVectorizer\)

```csharp
public InMemoryVectorCollection(string id, IVectorizer vectorizer)
```

#### Parameters

`id` [string](https://learn.microsoft.com/dotnet/api/system.string)

`vectorizer` [IVectorizer](Aislinn.VectorStorage.Interfaces.IVectorizer.md)

## Properties

### <a id="Aislinn_VectorStorage_Storage_InMemoryVectorCollection_Id"></a> Id

```csharp
public string Id { get; }
```

#### Property Value

 [string](https://learn.microsoft.com/dotnet/api/system.string)

## Methods

### <a id="Aislinn_VectorStorage_Storage_InMemoryVectorCollection_AddVectorAsync_System_String_System_Collections_Generic_Dictionary_System_String_System_String__"></a> AddVectorAsync\(string, Dictionary<string, string\>\)

```csharp
public Task<VectorItem> AddVectorAsync(string text, Dictionary<string, string> metadata)
```

#### Parameters

`text` [string](https://learn.microsoft.com/dotnet/api/system.string)

`metadata` [Dictionary](https://learn.microsoft.com/dotnet/api/system.collections.generic.dictionary\-2)<[string](https://learn.microsoft.com/dotnet/api/system.string), [string](https://learn.microsoft.com/dotnet/api/system.string)\>

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[VectorItem](Aislinn.VectorStorage.Models.VectorItem.md)\>

### <a id="Aislinn_VectorStorage_Storage_InMemoryVectorCollection_AddVectorAsync_System_String_System_String_System_Collections_Generic_Dictionary_System_String_System_String__"></a> AddVectorAsync\(string, string, Dictionary<string, string\>\)

```csharp
public Task<VectorItem> AddVectorAsync(string vectorId, string text, Dictionary<string, string> metadata)
```

#### Parameters

`vectorId` [string](https://learn.microsoft.com/dotnet/api/system.string)

`text` [string](https://learn.microsoft.com/dotnet/api/system.string)

`metadata` [Dictionary](https://learn.microsoft.com/dotnet/api/system.collections.generic.dictionary\-2)<[string](https://learn.microsoft.com/dotnet/api/system.string), [string](https://learn.microsoft.com/dotnet/api/system.string)\>

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[VectorItem](Aislinn.VectorStorage.Models.VectorItem.md)\>

### <a id="Aislinn_VectorStorage_Storage_InMemoryVectorCollection_DeleteVectorAsync_System_String_"></a> DeleteVectorAsync\(string\)

```csharp
public Task<bool> DeleteVectorAsync(string vectorId)
```

#### Parameters

`vectorId` [string](https://learn.microsoft.com/dotnet/api/system.string)

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[bool](https://learn.microsoft.com/dotnet/api/system.boolean)\>

### <a id="Aislinn_VectorStorage_Storage_InMemoryVectorCollection_GetMetadataAsync_System_String_"></a> GetMetadataAsync\(string\)

```csharp
public Task<Dictionary<string, string>> GetMetadataAsync(string vectorId)
```

#### Parameters

`vectorId` [string](https://learn.microsoft.com/dotnet/api/system.string)

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[Dictionary](https://learn.microsoft.com/dotnet/api/system.collections.generic.dictionary\-2)<[string](https://learn.microsoft.com/dotnet/api/system.string), [string](https://learn.microsoft.com/dotnet/api/system.string)\>\>

### <a id="Aislinn_VectorStorage_Storage_InMemoryVectorCollection_GetVectorAsync_System_String_"></a> GetVectorAsync\(string\)

```csharp
public Task<VectorItem> GetVectorAsync(string vectorId)
```

#### Parameters

`vectorId` [string](https://learn.microsoft.com/dotnet/api/system.string)

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[VectorItem](Aislinn.VectorStorage.Models.VectorItem.md)\>

### <a id="Aislinn_VectorStorage_Storage_InMemoryVectorCollection_GetVectorCountAsync"></a> GetVectorCountAsync\(\)

```csharp
public Task<int> GetVectorCountAsync()
```

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[int](https://learn.microsoft.com/dotnet/api/system.int32)\>

### <a id="Aislinn_VectorStorage_Storage_InMemoryVectorCollection_SearchVectorsAsync_System_String_System_Int32_System_Double_"></a> SearchVectorsAsync\(string, int, double\)

```csharp
public Task<List<SearchResult>> SearchVectorsAsync(string query, int topN, double minSimilarity = 0)
```

#### Parameters

`query` [string](https://learn.microsoft.com/dotnet/api/system.string)

`topN` [int](https://learn.microsoft.com/dotnet/api/system.int32)

`minSimilarity` [double](https://learn.microsoft.com/dotnet/api/system.double)

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[List](https://learn.microsoft.com/dotnet/api/system.collections.generic.list\-1)<[SearchResult](Aislinn.VectorStorage.Models.SearchResult.md)\>\>

### <a id="Aislinn_VectorStorage_Storage_InMemoryVectorCollection_SearchVectorsAsync_System_Double___System_Int32_System_Double_"></a> SearchVectorsAsync\(double\[\], int, double\)

```csharp
public Task<List<SearchResult>> SearchVectorsAsync(double[] queryVector, int topN, double minSimilarity = 0)
```

#### Parameters

`queryVector` [double](https://learn.microsoft.com/dotnet/api/system.double)\[\]

`topN` [int](https://learn.microsoft.com/dotnet/api/system.int32)

`minSimilarity` [double](https://learn.microsoft.com/dotnet/api/system.double)

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[List](https://learn.microsoft.com/dotnet/api/system.collections.generic.list\-1)<[SearchResult](Aislinn.VectorStorage.Models.SearchResult.md)\>\>

