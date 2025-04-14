# <a id="RAINA_QueryEngine"></a> Class QueryEngine

Namespace: [RAINA](RAINA.md)  
Assembly: Aislinn.Raina.dll  

```csharp
public class QueryEngine
```

#### Inheritance

object ← 
[QueryEngine](RAINA.QueryEngine.md)

## Constructors

### <a id="RAINA_QueryEngine__ctor_Aislinn_Core_Cognitive_CognitiveMemorySystem_"></a> QueryEngine\(CognitiveMemorySystem\)

```csharp
public QueryEngine(CognitiveMemorySystem memorySystem)
```

#### Parameters

`memorySystem` CognitiveMemorySystem

## Methods

### <a id="RAINA_QueryEngine_SearchAsync_RAINA_QueryParameters_RAINA_Services_UserContext_"></a> SearchAsync\(QueryParameters, UserContext\)

```csharp
public Task<List<Chunk>> SearchAsync(QueryParameters parameters, UserContext context)
```

#### Parameters

`parameters` [QueryParameters](RAINA.QueryParameters.md)

`context` [UserContext](RAINA.Services.UserContext.md)

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[List](https://learn.microsoft.com/dotnet/api/system.collections.generic.list\-1)<Chunk\>\>

