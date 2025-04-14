# <a id="Aislinn_VectorStorage_Interfaces_IVectorStore"></a> Interface IVectorStore

Namespace: [Aislinn.VectorStorage.Interfaces](Aislinn.VectorStorage.Interfaces.md)  
Assembly: Aislinn.VectorStorage.dll  

```csharp
public interface IVectorStore
```

## Methods

### <a id="Aislinn_VectorStorage_Interfaces_IVectorStore_AddVectorAsync_System_String_System_String_System_Collections_Generic_Dictionary_System_String_System_String__"></a> AddVectorAsync\(string, string, Dictionary<string, string\>\)

```csharp
Task AddVectorAsync(string chunkId, string text, Dictionary<string, string> metadata)
```

#### Parameters

`chunkId` [string](https://learn.microsoft.com/dotnet/api/system.string)

`text` [string](https://learn.microsoft.com/dotnet/api/system.string)

`metadata` [Dictionary](https://learn.microsoft.com/dotnet/api/system.collections.generic.dictionary\-2)<[string](https://learn.microsoft.com/dotnet/api/system.string), [string](https://learn.microsoft.com/dotnet/api/system.string)\>

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task)

### <a id="Aislinn_VectorStorage_Interfaces_IVectorStore_DeleteVectorAsync_System_String_"></a> DeleteVectorAsync\(string\)

```csharp
Task<bool> DeleteVectorAsync(string chunkId)
```

#### Parameters

`chunkId` [string](https://learn.microsoft.com/dotnet/api/system.string)

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[bool](https://learn.microsoft.com/dotnet/api/system.boolean)\>

### <a id="Aislinn_VectorStorage_Interfaces_IVectorStore_GetMetadataAsync_System_String_"></a> GetMetadataAsync\(string\)

```csharp
Task<Dictionary<string, string>> GetMetadataAsync(string chunkId)
```

#### Parameters

`chunkId` [string](https://learn.microsoft.com/dotnet/api/system.string)

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[Dictionary](https://learn.microsoft.com/dotnet/api/system.collections.generic.dictionary\-2)<[string](https://learn.microsoft.com/dotnet/api/system.string), [string](https://learn.microsoft.com/dotnet/api/system.string)\>\>

### <a id="Aislinn_VectorStorage_Interfaces_IVectorStore_GetVectorAsync_System_String_"></a> GetVectorAsync\(string\)

```csharp
Task<VectorItem> GetVectorAsync(string chunkId)
```

#### Parameters

`chunkId` [string](https://learn.microsoft.com/dotnet/api/system.string)

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[VectorItem](Aislinn.VectorStorage.Models.VectorItem.md)\>

### <a id="Aislinn_VectorStorage_Interfaces_IVectorStore_GetVectorCountAsync"></a> GetVectorCountAsync\(\)

```csharp
Task<int> GetVectorCountAsync()
```

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[int](https://learn.microsoft.com/dotnet/api/system.int32)\>

### <a id="Aislinn_VectorStorage_Interfaces_IVectorStore_SearchVectorsAsync_System_String_System_Int32_System_Double_"></a> SearchVectorsAsync\(string, int, double\)

```csharp
Task<List<SearchResult>> SearchVectorsAsync(string query, int topN, double minSimilarity = 0)
```

#### Parameters

`query` [string](https://learn.microsoft.com/dotnet/api/system.string)

`topN` [int](https://learn.microsoft.com/dotnet/api/system.int32)

`minSimilarity` [double](https://learn.microsoft.com/dotnet/api/system.double)

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[List](https://learn.microsoft.com/dotnet/api/system.collections.generic.list\-1)<[SearchResult](Aislinn.VectorStorage.Models.SearchResult.md)\>\>

### <a id="Aislinn_VectorStorage_Interfaces_IVectorStore_SearchVectorsAsync_System_Double___System_Int32_System_Double_"></a> SearchVectorsAsync\(double\[\], int, double\)

```csharp
Task<List<SearchResult>> SearchVectorsAsync(double[] queryVector, int topN, double minSimilarity = 0)
```

#### Parameters

`queryVector` [double](https://learn.microsoft.com/dotnet/api/system.double)\[\]

`topN` [int](https://learn.microsoft.com/dotnet/api/system.int32)

`minSimilarity` [double](https://learn.microsoft.com/dotnet/api/system.double)

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[List](https://learn.microsoft.com/dotnet/api/system.collections.generic.list\-1)<[SearchResult](Aislinn.VectorStorage.Models.SearchResult.md)\>\>

