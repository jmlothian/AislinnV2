# <a id="RAINA_Modules_Implementations_QueryIntentModule"></a> Class QueryIntentModule

Namespace: [RAINA.Modules.Implementations](RAINA.Modules.Implementations.md)  
Assembly: Aislinn.Raina.dll  

module for handling information retrieval queries

```csharp
public class QueryIntentModule : IIntentModule
```

#### Inheritance

object ← 
[QueryIntentModule](RAINA.Modules.Implementations.QueryIntentModule.md)

#### Implements

[IIntentModule](RAINA.Modules.IIntentModule.md)

## Constructors

### <a id="RAINA_Modules_Implementations_QueryIntentModule__ctor_RAINA_QueryEngine_RAINA_Services_ConversationManager_"></a> QueryIntentModule\(QueryEngine, ConversationManager\)

```csharp
public QueryIntentModule(QueryEngine queryEngine, ConversationManager conversationManager)
```

#### Parameters

`queryEngine` [QueryEngine](RAINA.QueryEngine.md)

`conversationManager` [ConversationManager](RAINA.Services.ConversationManager.md)

## Methods

### <a id="RAINA_Modules_Implementations_QueryIntentModule_GetExpectedEntities"></a> GetExpectedEntities\(\)

Gets the expected entities for this intent type

```csharp
public string[] GetExpectedEntities()
```

#### Returns

 [string](https://learn.microsoft.com/dotnet/api/system.string)\[\]

### <a id="RAINA_Modules_Implementations_QueryIntentModule_GetExpectedParameters"></a> GetExpectedParameters\(\)

Gets the expected parameters for this intent type

```csharp
public string[] GetExpectedParameters()
```

#### Returns

 [string](https://learn.microsoft.com/dotnet/api/system.string)\[\]

### <a id="RAINA_Modules_Implementations_QueryIntentModule_GetIntentType"></a> GetIntentType\(\)

Gets the unique identifier for this intent type

```csharp
public string GetIntentType()
```

#### Returns

 [string](https://learn.microsoft.com/dotnet/api/system.string)

### <a id="RAINA_Modules_Implementations_QueryIntentModule_GetPromptDescription"></a> GetPromptDescription\(\)

Gets the description of this intent for use in the OpenAI prompt

```csharp
public string GetPromptDescription()
```

#### Returns

 [string](https://learn.microsoft.com/dotnet/api/system.string)

### <a id="RAINA_Modules_Implementations_QueryIntentModule_GetPromptExamples"></a> GetPromptExamples\(\)

Gets examples of this intent type for use in the OpenAI prompt

```csharp
public string[] GetPromptExamples()
```

#### Returns

 [string](https://learn.microsoft.com/dotnet/api/system.string)\[\]

### <a id="RAINA_Modules_Implementations_QueryIntentModule_HandleAsync_System_String_RAINA_Intent_RAINA_Services_UserContext_"></a> HandleAsync\(string, Intent, UserContext\)

Handles this specific intent type

```csharp
public Task<Response> HandleAsync(string userInput, Intent intent, UserContext context)
```

#### Parameters

`userInput` [string](https://learn.microsoft.com/dotnet/api/system.string)

`intent` [Intent](RAINA.Intent.md)

`context` [UserContext](RAINA.Services.UserContext.md)

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[Response](RAINA.Services.Response.md)\>

