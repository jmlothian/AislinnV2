# <a id="Aislinn_Core_Context_ContextContainer_ContextFactor"></a> Class ContextContainer.ContextFactor

Namespace: [Aislinn.Core.Context](Aislinn.Core.Context.md)  
Assembly: Aislinn.Core.dll  

Represents a single context factor with value, timestamp, and metadata

```csharp
public class ContextContainer.ContextFactor
```

#### Inheritance

object ← 
[ContextContainer.ContextFactor](Aislinn.Core.Context.ContextContainer.ContextFactor.md)

## Constructors

### <a id="Aislinn_Core_Context_ContextContainer_ContextFactor__ctor_System_String_System_Object_"></a> ContextFactor\(string, object\)

```csharp
public ContextFactor(string name, object value)
```

#### Parameters

`name` [string](https://learn.microsoft.com/dotnet/api/system.string)

`value` object

## Properties

### <a id="Aislinn_Core_Context_ContextContainer_ContextFactor_Confidence"></a> Confidence

```csharp
public double Confidence { get; set; }
```

#### Property Value

 [double](https://learn.microsoft.com/dotnet/api/system.double)

### <a id="Aislinn_Core_Context_ContextContainer_ContextFactor_Importance"></a> Importance

```csharp
public double Importance { get; set; }
```

#### Property Value

 [double](https://learn.microsoft.com/dotnet/api/system.double)

### <a id="Aislinn_Core_Context_ContextContainer_ContextFactor_Metadata"></a> Metadata

```csharp
public Dictionary<string, object> Metadata { get; set; }
```

#### Property Value

 [Dictionary](https://learn.microsoft.com/dotnet/api/system.collections.generic.dictionary\-2)<[string](https://learn.microsoft.com/dotnet/api/system.string), object\>

### <a id="Aislinn_Core_Context_ContextContainer_ContextFactor_Name"></a> Name

```csharp
public string Name { get; set; }
```

#### Property Value

 [string](https://learn.microsoft.com/dotnet/api/system.string)

### <a id="Aislinn_Core_Context_ContextContainer_ContextFactor_SourceChunkId"></a> SourceChunkId

```csharp
public Guid? SourceChunkId { get; set; }
```

#### Property Value

 [Guid](https://learn.microsoft.com/dotnet/api/system.guid)?

### <a id="Aislinn_Core_Context_ContextContainer_ContextFactor_Timestamp"></a> Timestamp

```csharp
public DateTime Timestamp { get; set; }
```

#### Property Value

 [DateTime](https://learn.microsoft.com/dotnet/api/system.datetime)

### <a id="Aislinn_Core_Context_ContextContainer_ContextFactor_Value"></a> Value

```csharp
public object Value { get; set; }
```

#### Property Value

 object

## Methods

### <a id="Aislinn_Core_Context_ContextContainer_ContextFactor_HasExpired_System_TimeSpan_"></a> HasExpired\(TimeSpan\)

```csharp
public bool HasExpired(TimeSpan retentionTime)
```

#### Parameters

`retentionTime` [TimeSpan](https://learn.microsoft.com/dotnet/api/system.timespan)

#### Returns

 [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

