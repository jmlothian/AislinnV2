# <a id="Aislinn_VectorStorage_Storage_SimpleVectorizer"></a> Class SimpleVectorizer

Namespace: [Aislinn.VectorStorage.Storage](Aislinn.VectorStorage.Storage.md)  
Assembly: Aislinn.VectorStorage.dll  

A simple implementation of IVectorizer that creates vectors based on character frequencies
For a real implementation, you would use a proper NLP embedding model

```csharp
public class SimpleVectorizer : IVectorizer
```

#### Inheritance

object ← 
[SimpleVectorizer](Aislinn.VectorStorage.Storage.SimpleVectorizer.md)

#### Implements

[IVectorizer](Aislinn.VectorStorage.Interfaces.IVectorizer.md)

## Constructors

### <a id="Aislinn_VectorStorage_Storage_SimpleVectorizer__ctor_System_Int32_"></a> SimpleVectorizer\(int\)

```csharp
public SimpleVectorizer(int dimensions = 128)
```

#### Parameters

`dimensions` [int](https://learn.microsoft.com/dotnet/api/system.int32)

## Properties

### <a id="Aislinn_VectorStorage_Storage_SimpleVectorizer_Dimensions"></a> Dimensions

Gets the dimension of vectors created by this vectorizer

```csharp
public int Dimensions { get; }
```

#### Property Value

 [int](https://learn.microsoft.com/dotnet/api/system.int32)

## Methods

### <a id="Aislinn_VectorStorage_Storage_SimpleVectorizer_StringToVectorAsync_System_String_"></a> StringToVectorAsync\(string\)

Converts a string to a vector representation asynchronously

```csharp
public Task<double[]> StringToVectorAsync(string text)
```

#### Parameters

`text` [string](https://learn.microsoft.com/dotnet/api/system.string)

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[double](https://learn.microsoft.com/dotnet/api/system.double)\[\]\>

