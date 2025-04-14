# <a id="RAINA_ChunkDatabaseInitializationTask"></a> Class ChunkDatabaseInitializationTask

Namespace: [RAINA](RAINA.md)  
Assembly: Aislinn.Raina.dll  

Task to initialize the chunk database with required collections

```csharp
public class ChunkDatabaseInitializationTask : IStartupTask
```

#### Inheritance

object ← 
[ChunkDatabaseInitializationTask](RAINA.ChunkDatabaseInitializationTask.md)

#### Implements

[IStartupTask](RAINA.IStartupTask.md)

## Constructors

### <a id="RAINA_ChunkDatabaseInitializationTask__ctor_Aislinn_ChunkStorage_Interfaces_IChunkStore_Aislinn_ChunkStorage_Interfaces_IAssociationStore_System_String_System_String_"></a> ChunkDatabaseInitializationTask\(IChunkStore, IAssociationStore, string, string\)

```csharp
public ChunkDatabaseInitializationTask(IChunkStore chunkStore, IAssociationStore associationStore, string chunkCollectionId, string associationCollectionId)
```

#### Parameters

`chunkStore` IChunkStore

`associationStore` IAssociationStore

`chunkCollectionId` [string](https://learn.microsoft.com/dotnet/api/system.string)

`associationCollectionId` [string](https://learn.microsoft.com/dotnet/api/system.string)

## Methods

### <a id="RAINA_ChunkDatabaseInitializationTask_Execute_System_IServiceProvider_"></a> Execute\(IServiceProvider\)

```csharp
public void Execute(IServiceProvider serviceProvider)
```

#### Parameters

`serviceProvider` [IServiceProvider](https://learn.microsoft.com/dotnet/api/system.iserviceprovider)

