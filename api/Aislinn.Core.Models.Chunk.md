# <a id="Aislinn_Core_Models_Chunk"></a> Class Chunk

Namespace: [Aislinn.Core.Models](Aislinn.Core.Models.md)  
Assembly: Aislinn.Core.dll  

```csharp
public class Chunk
```

#### Inheritance

object ← 
[Chunk](Aislinn.Core.Models.Chunk.md)

#### Derived

[ProcedureChunk](Aislinn.Core.Procedural.ProcedureChunk.md)

#### Extension Methods

[ChunkExtensions.GetChunkFromSlot\(Chunk, string\)](Aislinn.Core.Extensions.ChunkExtensions.md\#Aislinn\_Core\_Extensions\_ChunkExtensions\_GetChunkFromSlot\_Aislinn\_Core\_Models\_Chunk\_System\_String\_), 
[ChunkExtensions.GetChunkFromSlotAsync\(Chunk, string, string\)](Aislinn.Core.Extensions.ChunkExtensions.md\#Aislinn\_Core\_Extensions\_ChunkExtensions\_GetChunkFromSlotAsync\_Aislinn\_Core\_Models\_Chunk\_System\_String\_System\_String\_), 
[ChunkExtensions.SetChunkReference\(Chunk, string, Chunk\)](Aislinn.Core.Extensions.ChunkExtensions.md\#Aislinn\_Core\_Extensions\_ChunkExtensions\_SetChunkReference\_Aislinn\_Core\_Models\_Chunk\_System\_String\_Aislinn\_Core\_Models\_Chunk\_)

## Constructors

### <a id="Aislinn_Core_Models_Chunk__ctor"></a> Chunk\(\)

```csharp
public Chunk()
```

## Properties

### <a id="Aislinn_Core_Models_Chunk_ActivationHistory"></a> ActivationHistory

```csharp
public List<ActivationHistoryItem> ActivationHistory { get; set; }
```

#### Property Value

 [List](https://learn.microsoft.com/dotnet/api/system.collections.generic.list\-1)<[ActivationHistoryItem](Aislinn.Core.Models.ActivationHistoryItem.md)\>

### <a id="Aislinn_Core_Models_Chunk_ActivationLevel"></a> ActivationLevel

```csharp
public double ActivationLevel { get; set; }
```

#### Property Value

 [double](https://learn.microsoft.com/dotnet/api/system.double)

### <a id="Aislinn_Core_Models_Chunk_ChunkType"></a> ChunkType

```csharp
public string ChunkType { get; set; }
```

#### Property Value

 [string](https://learn.microsoft.com/dotnet/api/system.string)

### <a id="Aislinn_Core_Models_Chunk_CognitiveCategory"></a> CognitiveCategory

```csharp
public string CognitiveCategory { get; set; }
```

#### Property Value

 [string](https://learn.microsoft.com/dotnet/api/system.string)

### <a id="Aislinn_Core_Models_Chunk_ID"></a> ID

```csharp
public Guid ID { get; set; }
```

#### Property Value

 [Guid](https://learn.microsoft.com/dotnet/api/system.guid)

### <a id="Aislinn_Core_Models_Chunk_Name"></a> Name

```csharp
public string Name { get; set; }
```

#### Property Value

 [string](https://learn.microsoft.com/dotnet/api/system.string)

### <a id="Aislinn_Core_Models_Chunk_SemanticType"></a> SemanticType

```csharp
public string SemanticType { get; set; }
```

#### Property Value

 [string](https://learn.microsoft.com/dotnet/api/system.string)

### <a id="Aislinn_Core_Models_Chunk_Slots"></a> Slots

```csharp
public Dictionary<string, ModelSlot> Slots { get; set; }
```

#### Property Value

 [Dictionary](https://learn.microsoft.com/dotnet/api/system.collections.generic.dictionary\-2)<[string](https://learn.microsoft.com/dotnet/api/system.string), [ModelSlot](Aislinn.Core.Models.ModelSlot.md)\>

### <a id="Aislinn_Core_Models_Chunk_Vector"></a> Vector

```csharp
public double[] Vector { get; set; }
```

#### Property Value

 [double](https://learn.microsoft.com/dotnet/api/system.double)\[\]

