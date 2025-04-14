# <a id="Aislinn_Core_Converters_ModelSlotExtensions"></a> Class ModelSlotExtensions

Namespace: [Aislinn.Core.Converters](Aislinn.Core.Converters.md)  
Assembly: Aislinn.Core.dll  

```csharp
public static class ModelSlotExtensions
```

#### Inheritance

object ← 
[ModelSlotExtensions](Aislinn.Core.Converters.ModelSlotExtensions.md)

## Methods

### <a id="Aislinn_Core_Converters_ModelSlotExtensions_GetChunk_Aislinn_Core_Models_ModelSlot_"></a> GetChunk\(ModelSlot\)

```csharp
public static Chunk GetChunk(this ModelSlot slot)
```

#### Parameters

`slot` [ModelSlot](Aislinn.Core.Models.ModelSlot.md)

#### Returns

 [Chunk](Aislinn.Core.Models.Chunk.md)

### <a id="Aislinn_Core_Converters_ModelSlotExtensions_GetChunkAsync_Aislinn_Core_Models_ModelSlot_System_String_"></a> GetChunkAsync\(ModelSlot, string\)

```csharp
public static Task<Chunk> GetChunkAsync(this ModelSlot slot, string collectionId = null)
```

#### Parameters

`slot` [ModelSlot](Aislinn.Core.Models.ModelSlot.md)

`collectionId` [string](https://learn.microsoft.com/dotnet/api/system.string)

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[Chunk](Aislinn.Core.Models.Chunk.md)\>

