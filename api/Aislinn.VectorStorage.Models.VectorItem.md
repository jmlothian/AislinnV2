# <a id="Aislinn_VectorStorage_Models_VectorItem"></a> Class VectorItem

Namespace: [Aislinn.VectorStorage.Models](Aislinn.VectorStorage.Models.md)  
Assembly: Aislinn.VectorStorage.dll  

```csharp
public class VectorItem
```

#### Inheritance

object ← 
[VectorItem](Aislinn.VectorStorage.Models.VectorItem.md)

## Constructors

### <a id="Aislinn_VectorStorage_Models_VectorItem__ctor_System_String_System_String_System_Double___System_Collections_Generic_Dictionary_System_String_System_String__"></a> VectorItem\(string, string, double\[\], Dictionary<string, string\>\)

```csharp
public VectorItem(string id, string text, double[] vector, Dictionary<string, string> metadata)
```

#### Parameters

`id` [string](https://learn.microsoft.com/dotnet/api/system.string)

`text` [string](https://learn.microsoft.com/dotnet/api/system.string)

`vector` [double](https://learn.microsoft.com/dotnet/api/system.double)\[\]

`metadata` [Dictionary](https://learn.microsoft.com/dotnet/api/system.collections.generic.dictionary\-2)<[string](https://learn.microsoft.com/dotnet/api/system.string), [string](https://learn.microsoft.com/dotnet/api/system.string)\>

## Properties

### <a id="Aislinn_VectorStorage_Models_VectorItem_ID"></a> ID

```csharp
public string ID { get; set; }
```

#### Property Value

 [string](https://learn.microsoft.com/dotnet/api/system.string)

### <a id="Aislinn_VectorStorage_Models_VectorItem_Metadata"></a> Metadata

```csharp
public Dictionary<string, string> Metadata { get; set; }
```

#### Property Value

 [Dictionary](https://learn.microsoft.com/dotnet/api/system.collections.generic.dictionary\-2)<[string](https://learn.microsoft.com/dotnet/api/system.string), [string](https://learn.microsoft.com/dotnet/api/system.string)\>

### <a id="Aislinn_VectorStorage_Models_VectorItem_Text"></a> Text

```csharp
public string Text { get; set; }
```

#### Property Value

 [string](https://learn.microsoft.com/dotnet/api/system.string)

### <a id="Aislinn_VectorStorage_Models_VectorItem_Vector"></a> Vector

```csharp
public double[] Vector { get; set; }
```

#### Property Value

 [double](https://learn.microsoft.com/dotnet/api/system.double)\[\]

## Methods

### <a id="Aislinn_VectorStorage_Models_VectorItem_Clone"></a> Clone\(\)

```csharp
public VectorItem Clone()
```

#### Returns

 [VectorItem](Aislinn.VectorStorage.Models.VectorItem.md)

