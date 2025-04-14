# <a id="Aislinn_VectorStorage_Models_SearchResult"></a> Class SearchResult

Namespace: [Aislinn.VectorStorage.Models](Aislinn.VectorStorage.Models.md)  
Assembly: Aislinn.VectorStorage.dll  

```csharp
public class SearchResult
```

#### Inheritance

object ← 
[SearchResult](Aislinn.VectorStorage.Models.SearchResult.md)

## Constructors

### <a id="Aislinn_VectorStorage_Models_SearchResult__ctor_Aislinn_VectorStorage_Models_VectorItem_System_Double_System_String_"></a> SearchResult\(VectorItem, double, string\)

```csharp
public SearchResult(VectorItem vectorItem, double similarity, string collection)
```

#### Parameters

`vectorItem` [VectorItem](Aislinn.VectorStorage.Models.VectorItem.md)

`similarity` [double](https://learn.microsoft.com/dotnet/api/system.double)

`collection` [string](https://learn.microsoft.com/dotnet/api/system.string)

## Properties

### <a id="Aislinn_VectorStorage_Models_SearchResult_Collection"></a> Collection

```csharp
public string Collection { get; set; }
```

#### Property Value

 [string](https://learn.microsoft.com/dotnet/api/system.string)

### <a id="Aislinn_VectorStorage_Models_SearchResult_Similarity"></a> Similarity

```csharp
public double Similarity { get; set; }
```

#### Property Value

 [double](https://learn.microsoft.com/dotnet/api/system.double)

### <a id="Aislinn_VectorStorage_Models_SearchResult_Value"></a> Value

```csharp
public VectorItem Value { get; set; }
```

#### Property Value

 [VectorItem](Aislinn.VectorStorage.Models.VectorItem.md)

