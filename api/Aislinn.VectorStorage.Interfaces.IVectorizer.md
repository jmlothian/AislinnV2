# <a id="Aislinn_VectorStorage_Interfaces_IVectorizer"></a> Interface IVectorizer

Namespace: [Aislinn.VectorStorage.Interfaces](Aislinn.VectorStorage.Interfaces.md)  
Assembly: Aislinn.VectorStorage.dll  

```csharp
public interface IVectorizer
```

## Properties

### <a id="Aislinn_VectorStorage_Interfaces_IVectorizer_Dimensions"></a> Dimensions

```csharp
int Dimensions { get; }
```

#### Property Value

 [int](https://learn.microsoft.com/dotnet/api/system.int32)

## Methods

### <a id="Aislinn_VectorStorage_Interfaces_IVectorizer_StringToVectorAsync_System_String_"></a> StringToVectorAsync\(string\)

```csharp
Task<double[]> StringToVectorAsync(string text)
```

#### Parameters

`text` [string](https://learn.microsoft.com/dotnet/api/system.string)

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[double](https://learn.microsoft.com/dotnet/api/system.double)\[\]\>

