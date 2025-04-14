# <a id="Aislinn_Core_Models_ChunkAssociation"></a> Class ChunkAssociation

Namespace: [Aislinn.Core.Models](Aislinn.Core.Models.md)  
Assembly: Aislinn.Core.dll  

```csharp
public class ChunkAssociation
```

#### Inheritance

object ← 
[ChunkAssociation](Aislinn.Core.Models.ChunkAssociation.md)

## Properties

### <a id="Aislinn_Core_Models_ChunkAssociation_ActivationHistory"></a> ActivationHistory

```csharp
public List<ActivationHistoryItem> ActivationHistory { get; set; }
```

#### Property Value

 [List](https://learn.microsoft.com/dotnet/api/system.collections.generic.list\-1)<[ActivationHistoryItem](Aislinn.Core.Models.ActivationHistoryItem.md)\>

### <a id="Aislinn_Core_Models_ChunkAssociation_ChunkAId"></a> ChunkAId

```csharp
public Guid ChunkAId { get; set; }
```

#### Property Value

 [Guid](https://learn.microsoft.com/dotnet/api/system.guid)

### <a id="Aislinn_Core_Models_ChunkAssociation_ChunkBId"></a> ChunkBId

```csharp
public Guid ChunkBId { get; set; }
```

#### Property Value

 [Guid](https://learn.microsoft.com/dotnet/api/system.guid)

### <a id="Aislinn_Core_Models_ChunkAssociation_LastActivated"></a> LastActivated

```csharp
public DateTime LastActivated { get; set; }
```

#### Property Value

 [DateTime](https://learn.microsoft.com/dotnet/api/system.datetime)

### <a id="Aislinn_Core_Models_ChunkAssociation_RelationAtoB"></a> RelationAtoB

```csharp
public string RelationAtoB { get; set; }
```

#### Property Value

 [string](https://learn.microsoft.com/dotnet/api/system.string)

### <a id="Aislinn_Core_Models_ChunkAssociation_RelationBtoA"></a> RelationBtoA

```csharp
public string RelationBtoA { get; set; }
```

#### Property Value

 [string](https://learn.microsoft.com/dotnet/api/system.string)

### <a id="Aislinn_Core_Models_ChunkAssociation_WeightAtoB"></a> WeightAtoB

```csharp
public double WeightAtoB { get; set; }
```

#### Property Value

 [double](https://learn.microsoft.com/dotnet/api/system.double)

### <a id="Aislinn_Core_Models_ChunkAssociation_WeightBtoA"></a> WeightBtoA

```csharp
public double WeightBtoA { get; set; }
```

#### Property Value

 [double](https://learn.microsoft.com/dotnet/api/system.double)

## Methods

### <a id="Aislinn_Core_Models_ChunkAssociation_Strengthen_System_Boolean_System_Double_"></a> Strengthen\(bool, double\)

```csharp
public void Strengthen(bool directionAtoB, double amount = 0.1)
```

#### Parameters

`directionAtoB` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

`amount` [double](https://learn.microsoft.com/dotnet/api/system.double)

