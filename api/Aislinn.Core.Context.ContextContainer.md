# <a id="Aislinn_Core_Context_ContextContainer"></a> Class ContextContainer

Namespace: [Aislinn.Core.Context](Aislinn.Core.Context.md)  
Assembly: Aislinn.Core.dll  

Maintains situational awareness by tracking and organizing context information
that influences goal selection and execution.

```csharp
public class ContextContainer
```

#### Inheritance

object ← 
[ContextContainer](Aislinn.Core.Context.ContextContainer.md)

## Constructors

### <a id="Aislinn_Core_Context_ContextContainer__ctor_Aislinn_ChunkStorage_Interfaces_IChunkStore_System_String_System_Double_System_Nullable_System_TimeSpan__"></a> ContextContainer\(IChunkStore, string, double, TimeSpan?\)

Initializes a new ContextContainer

```csharp
public ContextContainer(IChunkStore chunkStore, string chunkCollectionId = "default", double significantChangeThreshold = 0.3, TimeSpan? contextRetentionTime = null)
```

#### Parameters

`chunkStore` [IChunkStore](Aislinn.ChunkStorage.Interfaces.IChunkStore.md)

`chunkCollectionId` [string](https://learn.microsoft.com/dotnet/api/system.string)

`significantChangeThreshold` [double](https://learn.microsoft.com/dotnet/api/system.double)

`contextRetentionTime` [TimeSpan](https://learn.microsoft.com/dotnet/api/system.timespan)?

## Methods

### <a id="Aislinn_Core_Context_ContextContainer_AddContextChunk_Aislinn_Core_Context_ContextContainer_ContextCategory_System_Guid_"></a> AddContextChunk\(ContextCategory, Guid\)

Add a chunk to the active context for a category

```csharp
public void AddContextChunk(ContextContainer.ContextCategory category, Guid chunkId)
```

#### Parameters

`category` [ContextContainer](Aislinn.Core.Context.ContextContainer.md).[ContextCategory](Aislinn.Core.Context.ContextContainer.ContextCategory.md)

`chunkId` [Guid](https://learn.microsoft.com/dotnet/api/system.guid)

### <a id="Aislinn_Core_Context_ContextContainer_CalculateContextRelevance_Aislinn_Core_Models_Chunk_"></a> CalculateContextRelevance\(Chunk\)

Calculate a relevance score between context and goal

```csharp
public double CalculateContextRelevance(Chunk goalChunk)
```

#### Parameters

`goalChunk` [Chunk](Aislinn.Core.Models.Chunk.md)

#### Returns

 [double](https://learn.microsoft.com/dotnet/api/system.double)

### <a id="Aislinn_Core_Context_ContextContainer_CleanupExpiredFactors"></a> CleanupExpiredFactors\(\)

Clear expired context factors

```csharp
public void CleanupExpiredFactors()
```

### <a id="Aislinn_Core_Context_ContextContainer_CreateContextSnapshot"></a> CreateContextSnapshot\(\)

Create a context snapshot

```csharp
public Dictionary<ContextContainer.ContextCategory, Dictionary<string, object>> CreateContextSnapshot()
```

#### Returns

 [Dictionary](https://learn.microsoft.com/dotnet/api/system.collections.generic.dictionary\-2)<[ContextContainer](Aislinn.Core.Context.ContextContainer.md).[ContextCategory](Aislinn.Core.Context.ContextContainer.ContextCategory.md), [Dictionary](https://learn.microsoft.com/dotnet/api/system.collections.generic.dictionary\-2)<[string](https://learn.microsoft.com/dotnet/api/system.string), object\>\>

### <a id="Aislinn_Core_Context_ContextContainer_GetActiveContextChunks_Aislinn_Core_Context_ContextContainer_ContextCategory_"></a> GetActiveContextChunks\(ContextCategory\)

Get all active context chunks for a category

```csharp
public List<Guid> GetActiveContextChunks(ContextContainer.ContextCategory category)
```

#### Parameters

`category` [ContextContainer](Aislinn.Core.Context.ContextContainer.md).[ContextCategory](Aislinn.Core.Context.ContextContainer.ContextCategory.md)

#### Returns

 [List](https://learn.microsoft.com/dotnet/api/system.collections.generic.list\-1)<[Guid](https://learn.microsoft.com/dotnet/api/system.guid)\>

### <a id="Aislinn_Core_Context_ContextContainer_GetAllActiveContextChunks"></a> GetAllActiveContextChunks\(\)

Get all active context chunks across all categories

```csharp
public Dictionary<ContextContainer.ContextCategory, List<Guid>> GetAllActiveContextChunks()
```

#### Returns

 [Dictionary](https://learn.microsoft.com/dotnet/api/system.collections.generic.dictionary\-2)<[ContextContainer](Aislinn.Core.Context.ContextContainer.md).[ContextCategory](Aislinn.Core.Context.ContextContainer.ContextCategory.md), [List](https://learn.microsoft.com/dotnet/api/system.collections.generic.list\-1)<[Guid](https://learn.microsoft.com/dotnet/api/system.guid)\>\>

### <a id="Aislinn_Core_Context_ContextContainer_GetCategoryFactors_Aislinn_Core_Context_ContextContainer_ContextCategory_"></a> GetCategoryFactors\(ContextCategory\)

Get all context factors for a category

```csharp
public Dictionary<string, ContextContainer.ContextFactor> GetCategoryFactors(ContextContainer.ContextCategory category)
```

#### Parameters

`category` [ContextContainer](Aislinn.Core.Context.ContextContainer.md).[ContextCategory](Aislinn.Core.Context.ContextContainer.ContextCategory.md)

#### Returns

 [Dictionary](https://learn.microsoft.com/dotnet/api/system.collections.generic.dictionary\-2)<[string](https://learn.microsoft.com/dotnet/api/system.string), [ContextContainer](Aislinn.Core.Context.ContextContainer.md).[ContextFactor](Aislinn.Core.Context.ContextContainer.ContextFactor.md)\>

### <a id="Aislinn_Core_Context_ContextContainer_GetContextFactor_Aislinn_Core_Context_ContextContainer_ContextCategory_System_String_"></a> GetContextFactor\(ContextCategory, string\)

Get a specific context factor

```csharp
public ContextContainer.ContextFactor GetContextFactor(ContextContainer.ContextCategory category, string factorName)
```

#### Parameters

`category` [ContextContainer](Aislinn.Core.Context.ContextContainer.md).[ContextCategory](Aislinn.Core.Context.ContextContainer.ContextCategory.md)

`factorName` [string](https://learn.microsoft.com/dotnet/api/system.string)

#### Returns

 [ContextContainer](Aislinn.Core.Context.ContextContainer.md).[ContextFactor](Aislinn.Core.Context.ContextContainer.ContextFactor.md)

### <a id="Aislinn_Core_Context_ContextContainer_GetContextValue__1_Aislinn_Core_Context_ContextContainer_ContextCategory_System_String___0_"></a> GetContextValue<T\>\(ContextCategory, string, T\)

Get the value of a specific context factor

```csharp
public T GetContextValue<T>(ContextContainer.ContextCategory category, string factorName, T defaultValue = default)
```

#### Parameters

`category` [ContextContainer](Aislinn.Core.Context.ContextContainer.md).[ContextCategory](Aislinn.Core.Context.ContextContainer.ContextCategory.md)

`factorName` [string](https://learn.microsoft.com/dotnet/api/system.string)

`defaultValue` T

#### Returns

 T

#### Type Parameters

`T` 

### <a id="Aislinn_Core_Context_ContextContainer_HasRecentContextFactor_Aislinn_Core_Context_ContextContainer_ContextCategory_System_String_System_Nullable_System_TimeSpan__"></a> HasRecentContextFactor\(ContextCategory, string, TimeSpan?\)

Check if a context factor exists and is recent

```csharp
public bool HasRecentContextFactor(ContextContainer.ContextCategory category, string factorName, TimeSpan? maxAge = null)
```

#### Parameters

`category` [ContextContainer](Aislinn.Core.Context.ContextContainer.md).[ContextCategory](Aislinn.Core.Context.ContextContainer.ContextCategory.md)

`factorName` [string](https://learn.microsoft.com/dotnet/api/system.string)

`maxAge` [TimeSpan](https://learn.microsoft.com/dotnet/api/system.timespan)?

#### Returns

 [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### <a id="Aislinn_Core_Context_ContextContainer_RemoveContextChunk_Aislinn_Core_Context_ContextContainer_ContextCategory_System_Guid_"></a> RemoveContextChunk\(ContextCategory, Guid\)

Remove a chunk from the active context

```csharp
public void RemoveContextChunk(ContextContainer.ContextCategory category, Guid chunkId)
```

#### Parameters

`category` [ContextContainer](Aislinn.Core.Context.ContextContainer.md).[ContextCategory](Aislinn.Core.Context.ContextContainer.ContextCategory.md)

`chunkId` [Guid](https://learn.microsoft.com/dotnet/api/system.guid)

### <a id="Aislinn_Core_Context_ContextContainer_UpdateContextFactor_Aislinn_Core_Context_ContextContainer_ContextCategory_System_String_System_Object_System_Double_System_Double_System_Collections_Generic_Dictionary_System_String_System_Object__"></a> UpdateContextFactor\(ContextCategory, string, object, double, double, Dictionary<string, object\>\)

Updates or adds a context factor

```csharp
public void UpdateContextFactor(ContextContainer.ContextCategory category, string factorName, object value, double importance = 0.5, double confidence = 1, Dictionary<string, object> metadata = null)
```

#### Parameters

`category` [ContextContainer](Aislinn.Core.Context.ContextContainer.md).[ContextCategory](Aislinn.Core.Context.ContextContainer.ContextCategory.md)

`factorName` [string](https://learn.microsoft.com/dotnet/api/system.string)

`value` object

`importance` [double](https://learn.microsoft.com/dotnet/api/system.double)

`confidence` [double](https://learn.microsoft.com/dotnet/api/system.double)

`metadata` [Dictionary](https://learn.microsoft.com/dotnet/api/system.collections.generic.dictionary\-2)<[string](https://learn.microsoft.com/dotnet/api/system.string), object\>

### <a id="Aislinn_Core_Context_ContextContainer_UpdateContextFromWorkingMemoryAsync_System_Collections_Generic_List_Aislinn_Core_Models_Chunk__"></a> UpdateContextFromWorkingMemoryAsync\(List<Chunk\>\)

Update context chunks from working memory

```csharp
public Task UpdateContextFromWorkingMemoryAsync(List<Chunk> workingMemoryChunks)
```

#### Parameters

`workingMemoryChunks` [List](https://learn.microsoft.com/dotnet/api/system.collections.generic.list\-1)<[Chunk](Aislinn.Core.Models.Chunk.md)\>

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task)

### <a id="Aislinn_Core_Context_ContextContainer_SignificantContextChange"></a> SignificantContextChange

```csharp
public event EventHandler<ContextContainer.ContextChangeEventArgs> SignificantContextChange
```

#### Event Type

 [EventHandler](https://learn.microsoft.com/dotnet/api/system.eventhandler\-1)<[ContextContainer](Aislinn.Core.Context.ContextContainer.md).[ContextChangeEventArgs](Aislinn.Core.Context.ContextContainer.ContextChangeEventArgs.md)\>

