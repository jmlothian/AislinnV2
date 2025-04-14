# <a id="RAINA_Services_UserContext"></a> Class UserContext

Namespace: [RAINA.Services](RAINA.Services.md)  
Assembly: Aislinn.Raina.dll  

```csharp
public class UserContext
```

#### Inheritance

object ← 
[UserContext](RAINA.Services.UserContext.md)

## Properties

### <a id="RAINA_Services_UserContext_ActiveMemoryChunks"></a> ActiveMemoryChunks

```csharp
public List<Chunk> ActiveMemoryChunks { get; set; }
```

#### Property Value

 [List](https://learn.microsoft.com/dotnet/api/system.collections.generic.list\-1)<Chunk\>

### <a id="RAINA_Services_UserContext_ContextVariables"></a> ContextVariables

```csharp
public Dictionary<string, object> ContextVariables { get; set; }
```

#### Property Value

 [Dictionary](https://learn.microsoft.com/dotnet/api/system.collections.generic.dictionary\-2)<[string](https://learn.microsoft.com/dotnet/api/system.string), object\>

### <a id="RAINA_Services_UserContext_CurrentConversationChunk"></a> CurrentConversationChunk

```csharp
public Chunk CurrentConversationChunk { get; set; }
```

#### Property Value

 Chunk

### <a id="RAINA_Services_UserContext_CurrentTopic"></a> CurrentTopic

```csharp
public string CurrentTopic { get; set; }
```

#### Property Value

 [string](https://learn.microsoft.com/dotnet/api/system.string)

### <a id="RAINA_Services_UserContext_CurrentUtterance"></a> CurrentUtterance

```csharp
public Chunk CurrentUtterance { get; set; }
```

#### Property Value

 Chunk

### <a id="RAINA_Services_UserContext_LastSystemUtterance"></a> LastSystemUtterance

```csharp
public Chunk LastSystemUtterance { get; set; }
```

#### Property Value

 Chunk

### <a id="RAINA_Services_UserContext_UserId"></a> UserId

```csharp
public string UserId { get; set; }
```

#### Property Value

 [string](https://learn.microsoft.com/dotnet/api/system.string)

## Methods

### <a id="RAINA_Services_UserContext_AddUtterance_Aislinn_Core_Models_Chunk_"></a> AddUtterance\(Chunk\)

```csharp
public void AddUtterance(Chunk utteranceChunk)
```

#### Parameters

`utteranceChunk` Chunk

### <a id="RAINA_Services_UserContext_GetConversationContext_System_Int32_"></a> GetConversationContext\(int\)

```csharp
public string GetConversationContext(int utteranceCount = 5)
```

#### Parameters

`utteranceCount` [int](https://learn.microsoft.com/dotnet/api/system.int32)

#### Returns

 [string](https://learn.microsoft.com/dotnet/api/system.string)

### <a id="RAINA_Services_UserContext_GetRecentUtterances_System_Int32_"></a> GetRecentUtterances\(int\)

```csharp
public List<Chunk> GetRecentUtterances(int count = 5)
```

#### Parameters

`count` [int](https://learn.microsoft.com/dotnet/api/system.int32)

#### Returns

 [List](https://learn.microsoft.com/dotnet/api/system.collections.generic.list\-1)<Chunk\>

