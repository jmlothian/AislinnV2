# <a id="Aislinn_Storage_AssociationStore_AssociationStore"></a> Class AssociationStore

Namespace: [Aislinn.Storage.AssociationStore](Aislinn.Storage.AssociationStore.md)  
Assembly: Aislinn.Core.dll  

```csharp
public class AssociationStore : IAssociationStore
```

#### Inheritance

object ← 
[AssociationStore](Aislinn.Storage.AssociationStore.AssociationStore.md)

#### Implements

[IAssociationStore](Aislinn.ChunkStorage.Interfaces.IAssociationStore.md)

## Constructors

### <a id="Aislinn_Storage_AssociationStore_AssociationStore__ctor"></a> AssociationStore\(\)

```csharp
public AssociationStore()
```

## Methods

### <a id="Aislinn_Storage_AssociationStore_AssociationStore_CreateCollectionAsync_System_String_"></a> CreateCollectionAsync\(string\)

```csharp
public Task<IChunkAssociationCollection> CreateCollectionAsync(string collectionId)
```

#### Parameters

`collectionId` [string](https://learn.microsoft.com/dotnet/api/system.string)

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[IChunkAssociationCollection](Aislinn.ChunkStorage.Interfaces.IChunkAssociationCollection.md)\>

### <a id="Aislinn_Storage_AssociationStore_AssociationStore_DeleteCollectionAsync_System_String_"></a> DeleteCollectionAsync\(string\)

```csharp
public Task<bool> DeleteCollectionAsync(string collectionId)
```

#### Parameters

`collectionId` [string](https://learn.microsoft.com/dotnet/api/system.string)

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[bool](https://learn.microsoft.com/dotnet/api/system.boolean)\>

### <a id="Aislinn_Storage_AssociationStore_AssociationStore_GetCollectionAsync_System_String_"></a> GetCollectionAsync\(string\)

```csharp
public Task<IChunkAssociationCollection> GetCollectionAsync(string collectionId)
```

#### Parameters

`collectionId` [string](https://learn.microsoft.com/dotnet/api/system.string)

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[IChunkAssociationCollection](Aislinn.ChunkStorage.Interfaces.IChunkAssociationCollection.md)\>

### <a id="Aislinn_Storage_AssociationStore_AssociationStore_GetCollectionIdsAsync"></a> GetCollectionIdsAsync\(\)

```csharp
public Task<List<string>> GetCollectionIdsAsync()
```

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[List](https://learn.microsoft.com/dotnet/api/system.collections.generic.list\-1)<[string](https://learn.microsoft.com/dotnet/api/system.string)\>\>

### <a id="Aislinn_Storage_AssociationStore_AssociationStore_GetOrCreateCollectionAsync_System_String_"></a> GetOrCreateCollectionAsync\(string\)

```csharp
public Task<IChunkAssociationCollection> GetOrCreateCollectionAsync(string collectionId)
```

#### Parameters

`collectionId` [string](https://learn.microsoft.com/dotnet/api/system.string)

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[IChunkAssociationCollection](Aislinn.ChunkStorage.Interfaces.IChunkAssociationCollection.md)\>

