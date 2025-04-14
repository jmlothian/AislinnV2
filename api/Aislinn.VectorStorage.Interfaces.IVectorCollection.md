# <a id="Aislinn_VectorStorage_Interfaces_IVectorCollection"></a> Interface IVectorCollection

Namespace: [Aislinn.VectorStorage.Interfaces](Aislinn.VectorStorage.Interfaces.md)  
Assembly: Aislinn.VectorStorage.dll  

```csharp
public interface IVectorCollection
```

## Properties

### <a id="Aislinn_VectorStorage_Interfaces_IVectorCollection_Id"></a> Id

```csharp
string Id { get; }
```

#### Property Value

 [string](https://learn.microsoft.com/dotnet/api/system.string)

## Methods

### <a id="Aislinn_VectorStorage_Interfaces_IVectorCollection_AddVectorAsync_System_String_System_Collections_Generic_Dictionary_System_String_System_String__"></a> AddVectorAsync\(string, Dictionary<string, string\>\)

```csharp
Task<VectorItem> AddVectorAsync(string text, Dictionary<string, string> metadata)
```

#### Parameters

`text` [string](https://learn.microsoft.com/dotnet/api/system.string)

`metadata` [Dictionary](https://learn.microsoft.com/dotnet/api/system.collections.generic.dictionary\-2)<[string](https://learn.microsoft.com/dotnet/api/system.string), [string](https://learn.microsoft.com/dotnet/api/system.string)\>

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[VectorItem](Aislinn.VectorStorage.Models.VectorItem.md)\>

### <a id="Aislinn_VectorStorage_Interfaces_IVectorCollection_AddVectorAsync_System_String_System_String_System_Collections_Generic_Dictionary_System_String_System_String__"></a> AddVectorAsync\(string, string, Dictionary<string, string\>\)

```csharp
Task<VectorItem> AddVectorAsync(string vectorId, string text, Dictionary<string, string> metadata)
```

#### Parameters

`vectorId` [string](https://learn.microsoft.com/dotnet/api/system.string)

`text` [string](https://learn.microsoft.com/dotnet/api/system.string)

`metadata` [Dictionary](https://learn.microsoft.com/dotnet/api/system.collections.generic.dictionary\-2)<[string](https://learn.microsoft.com/dotnet/api/system.string), [string](https://learn.microsoft.com/dotnet/api/system.string)\>

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[VectorItem](Aislinn.VectorStorage.Models.VectorItem.md)\>

### <a id="Aislinn_VectorStorage_Interfaces_IVectorCollection_DeleteVectorAsync_System_String_"></a> DeleteVectorAsync\(string\)

```csharp
Task<bool> DeleteVectorAsync(string vectorId)
```

#### Parameters

`vectorId` [string](https://learn.microsoft.com/dotnet/api/system.string)

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[bool](https://learn.microsoft.com/dotnet/api/system.boolean)\>

### <a id="Aislinn_VectorStorage_Interfaces_IVectorCollection_GetMetadataAsync_System_String_"></a> GetMetadataAsync\(string\)

```csharp
Task<Dictionary<string, string>> GetMetadataAsync(string vectorId)
```

#### Parameters

`vectorId` [string](https://learn.microsoft.com/dotnet/api/system.string)

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[Dictionary](https://learn.microsoft.com/dotnet/api/system.collections.generic.dictionary\-2)<[string](https://learn.microsoft.com/dotnet/api/system.string), [string](https://learn.microsoft.com/dotnet/api/system.string)\>\>

### <a id="Aislinn_VectorStorage_Interfaces_IVectorCollection_GetVectorAsync_System_String_"></a> GetVectorAsync\(string\)

```csharp
Task<VectorItem> GetVectorAsync(string vectorId)
```

#### Parameters

`vectorId` [string](https://learn.microsoft.com/dotnet/api/system.string)

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[VectorItem](Aislinn.VectorStorage.Models.VectorItem.md)\>

### <a id="Aislinn_VectorStorage_Interfaces_IVectorCollection_GetVectorCountAsync"></a> GetVectorCountAsync\(\)

```csharp
Task<int> GetVectorCountAsync()
```

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[int](https://learn.microsoft.com/dotnet/api/system.int32)\>

### <a id="Aislinn_VectorStorage_Interfaces_IVectorCollection_SearchVectorsAsync_System_String_System_Int32_System_Double_"></a> SearchVectorsAsync\(string, int, double\)

```csharp
Task<List<SearchResult>> SearchVectorsAsync(string query, int topN, double minSimilarity = 0)
```

#### Parameters

`query` [string](https://learn.microsoft.com/dotnet/api/system.string)

`topN` [int](https://learn.microsoft.com/dotnet/api/system.int32)

`minSimilarity` [double](https://learn.microsoft.com/dotnet/api/system.double)

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[List](https://learn.microsoft.com/dotnet/api/system.collections.generic.list\-1)<[SearchResult](Aislinn.VectorStorage.Models.SearchResult.md)\>\>

### <a id="Aislinn_VectorStorage_Interfaces_IVectorCollection_SearchVectorsAsync_System_Double___System_Int32_System_Double_"></a> SearchVectorsAsync\(double\[\], int, double\)

```csharp
Task<List<SearchResult>> SearchVectorsAsync(double[] queryVector, int topN, double minSimilarity = 0)
```

#### Parameters

`queryVector` [double](https://learn.microsoft.com/dotnet/api/system.double)\[\]

`topN` [int](https://learn.microsoft.com/dotnet/api/system.int32)

`minSimilarity` [double](https://learn.microsoft.com/dotnet/api/system.double)

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[List](https://learn.microsoft.com/dotnet/api/system.collections.generic.list\-1)<[SearchResult](Aislinn.VectorStorage.Models.SearchResult.md)\>\>

