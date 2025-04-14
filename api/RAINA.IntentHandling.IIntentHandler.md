# <a id="RAINA_IntentHandling_IIntentHandler"></a> Interface IIntentHandler

Namespace: [RAINA.IntentHandling](RAINA.IntentHandling.md)  
Assembly: Aislinn.Raina.dll  

Interface for intent handlers in RAINA (Realtime Adaptive Intelligence Neural Assistant)

```csharp
public interface IIntentHandler
```

## Methods

### <a id="RAINA_IntentHandling_IIntentHandler_GetDescription"></a> GetDescription\(\)

Gets a description of what this intent handler does

```csharp
string GetDescription()
```

#### Returns

 [string](https://learn.microsoft.com/dotnet/api/system.string)

### <a id="RAINA_IntentHandling_IIntentHandler_GetIntentType"></a> GetIntentType\(\)

Gets the intent type this handler is responsible for

```csharp
string GetIntentType()
```

#### Returns

 [string](https://learn.microsoft.com/dotnet/api/system.string)

### <a id="RAINA_IntentHandling_IIntentHandler_HandleAsync_System_String_RAINA_Intent_RAINA_Services_UserContext_"></a> HandleAsync\(string, Intent, UserContext\)

Handles a specific intent type

```csharp
Task<Response> HandleAsync(string userInput, Intent intent, UserContext context)
```

#### Parameters

`userInput` [string](https://learn.microsoft.com/dotnet/api/system.string)

The original user input text

`intent` [Intent](RAINA.Intent.md)

The parsed intent with entities and parameters

`context` [UserContext](RAINA.Services.UserContext.md)

The current user context

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[Response](RAINA.Services.Response.md)\>

A response to be sent back to the user

