# <a id="RAINA_Services_ConversationManager"></a> Class ConversationManager

Namespace: [RAINA.Services](RAINA.Services.md)  
Assembly: Aislinn.Raina.dll  

```csharp
public class ConversationManager
```

#### Inheritance

object ← 
[ConversationManager](RAINA.Services.ConversationManager.md)

## Constructors

### <a id="RAINA_Services_ConversationManager__ctor_Aislinn_Core_Cognitive_CognitiveMemorySystem_System_String_"></a> ConversationManager\(CognitiveMemorySystem, string\)

```csharp
public ConversationManager(CognitiveMemorySystem memorySystem, string openAIApiKey)
```

#### Parameters

`memorySystem` CognitiveMemorySystem

`openAIApiKey` [string](https://learn.microsoft.com/dotnet/api/system.string)

## Methods

### <a id="RAINA_Services_ConversationManager_GenerateQueryResponseAsync_System_String_RAINA_Intent_System_Collections_Generic_List_Aislinn_Core_Models_Chunk__RAINA_Services_UserContext_"></a> GenerateQueryResponseAsync\(string, Intent, List<Chunk\>, UserContext\)

```csharp
public Task<Response> GenerateQueryResponseAsync(string userInput, Intent intent, List<Chunk> queryResults, UserContext context)
```

#### Parameters

`userInput` [string](https://learn.microsoft.com/dotnet/api/system.string)

`intent` [Intent](RAINA.Intent.md)

`queryResults` [List](https://learn.microsoft.com/dotnet/api/system.collections.generic.list\-1)<Chunk\>

`context` [UserContext](RAINA.Services.UserContext.md)

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[Response](RAINA.Services.Response.md)\>

### <a id="RAINA_Services_ConversationManager_GenerateResponseAsync_System_String_RAINA_Intent_RAINA_Services_UserContext_"></a> GenerateResponseAsync\(string, Intent, UserContext\)

```csharp
public Task<Response> GenerateResponseAsync(string userInput, Intent intent, UserContext context)
```

#### Parameters

`userInput` [string](https://learn.microsoft.com/dotnet/api/system.string)

`intent` [Intent](RAINA.Intent.md)

`context` [UserContext](RAINA.Services.UserContext.md)

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[Response](RAINA.Services.Response.md)\>

### <a id="RAINA_Services_ConversationManager_GetConversationSummaryAsync"></a> GetConversationSummaryAsync\(\)

```csharp
public Task<ConversationSummary> GetConversationSummaryAsync()
```

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[ConversationSummary](RAINA.Services.ConversationSummary.md)\>

### <a id="RAINA_Services_ConversationManager_InitializeConversationAsync_System_String_"></a> InitializeConversationAsync\(string\)

```csharp
public Task<Chunk> InitializeConversationAsync(string conversationId = null)
```

#### Parameters

`conversationId` [string](https://learn.microsoft.com/dotnet/api/system.string)

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<Chunk\>

### <a id="RAINA_Services_ConversationManager_RecordUserInputAsync_System_String_RAINA_Intent_RAINA_Services_UserContext_"></a> RecordUserInputAsync\(string, Intent, UserContext\)

```csharp
public Task<Chunk> RecordUserInputAsync(string userInput, Intent intent, UserContext context)
```

#### Parameters

`userInput` [string](https://learn.microsoft.com/dotnet/api/system.string)

`intent` [Intent](RAINA.Intent.md)

`context` [UserContext](RAINA.Services.UserContext.md)

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<Chunk\>

