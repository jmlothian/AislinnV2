# <a id="Aislinn_Core_Models_ActivationHistoryItem"></a> Class ActivationHistoryItem

Namespace: [Aislinn.Core.Models](Aislinn.Core.Models.md)  
Assembly: Aislinn.Core.dll  

```csharp
public class ActivationHistoryItem : IEmotion
```

#### Inheritance

object ← 
[ActivationHistoryItem](Aislinn.Core.Models.ActivationHistoryItem.md)

#### Implements

[IEmotion](Aislinn.Core.Interfaces.IEmotion.md)

## Properties

### <a id="Aislinn_Core_Models_ActivationHistoryItem_ActivatedBy"></a> ActivatedBy

```csharp
public List<Guid> ActivatedBy { get; set; }
```

#### Property Value

 [List](https://learn.microsoft.com/dotnet/api/system.collections.generic.list\-1)<[Guid](https://learn.microsoft.com/dotnet/api/system.guid)\>

### <a id="Aislinn_Core_Models_ActivationHistoryItem_ActivatedByChunk"></a> ActivatedByChunk

```csharp
public Guid ActivatedByChunk { get; set; }
```

#### Property Value

 [Guid](https://learn.microsoft.com/dotnet/api/system.guid)

### <a id="Aislinn_Core_Models_ActivationHistoryItem_ActivationDate"></a> ActivationDate

```csharp
public DateTime ActivationDate { get; set; }
```

#### Property Value

 [DateTime](https://learn.microsoft.com/dotnet/api/system.datetime)

### <a id="Aislinn_Core_Models_ActivationHistoryItem_Change"></a> Change

```csharp
public double Change { get; set; }
```

#### Property Value

 [double](https://learn.microsoft.com/dotnet/api/system.double)

### <a id="Aislinn_Core_Models_ActivationHistoryItem_Coordinates"></a> Coordinates

```csharp
public List<double> Coordinates { get; set; }
```

#### Property Value

 [List](https://learn.microsoft.com/dotnet/api/system.collections.generic.list\-1)<[double](https://learn.microsoft.com/dotnet/api/system.double)\>

### <a id="Aislinn_Core_Models_ActivationHistoryItem_EmotionName"></a> EmotionName

```csharp
public string EmotionName { get; set; }
```

#### Property Value

 [string](https://learn.microsoft.com/dotnet/api/system.string)

### <a id="Aislinn_Core_Models_ActivationHistoryItem_NewValue"></a> NewValue

```csharp
public double NewValue { get; set; }
```

#### Property Value

 [double](https://learn.microsoft.com/dotnet/api/system.double)

### <a id="Aislinn_Core_Models_ActivationHistoryItem_PreviousValue"></a> PreviousValue

```csharp
public double PreviousValue { get; set; }
```

#### Property Value

 [double](https://learn.microsoft.com/dotnet/api/system.double)

### <a id="Aislinn_Core_Models_ActivationHistoryItem_SequenceNumber"></a> SequenceNumber

```csharp
public ulong SequenceNumber { get; set; }
```

#### Property Value

 [ulong](https://learn.microsoft.com/dotnet/api/system.uint64)

## Methods

### <a id="Aislinn_Core_Models_ActivationHistoryItem_FormatElapsedTime"></a> FormatElapsedTime\(\)

```csharp
public string FormatElapsedTime()
```

#### Returns

 [string](https://learn.microsoft.com/dotnet/api/system.string)

