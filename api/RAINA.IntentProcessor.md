# <a id="RAINA_IntentProcessor"></a> Class IntentProcessor

Namespace: [RAINA](RAINA.md)  
Assembly: Aislinn.Raina.dll  

```csharp
public class IntentProcessor
```

#### Inheritance

object ← 
[IntentProcessor](RAINA.IntentProcessor.md)

## Constructors

### <a id="RAINA_IntentProcessor__ctor_System_String_RAINA_Services_ConversationManager_RAINA_ContextDetector_"></a> IntentProcessor\(string, ConversationManager, ContextDetector\)

```csharp
public IntentProcessor(string openAIApiKey, ConversationManager conversationManager, ContextDetector contextDetector)
```

#### Parameters

`openAIApiKey` [string](https://learn.microsoft.com/dotnet/api/system.string)

`conversationManager` [ConversationManager](RAINA.Services.ConversationManager.md)

`contextDetector` [ContextDetector](RAINA.ContextDetector.md)

## Methods

### <a id="RAINA_IntentProcessor_GetRegisteredModules"></a> GetRegisteredModules\(\)

Get all registered modules

```csharp
public IEnumerable<IIntentModule> GetRegisteredModules()
```

#### Returns

 [IEnumerable](https://learn.microsoft.com/dotnet/api/system.collections.generic.ienumerable\-1)<[IIntentModule](RAINA.Modules.IIntentModule.md)\>

### <a id="RAINA_IntentProcessor_ProcessInputAsync_System_String_RAINA_Services_UserContext_"></a> ProcessInputAsync\(string, UserContext\)

Main entry point for processing user input

```csharp
public Task<Response> ProcessInputAsync(string userInput, UserContext context)
```

#### Parameters

`userInput` [string](https://learn.microsoft.com/dotnet/api/system.string)

`context` [UserContext](RAINA.Services.UserContext.md)

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[Response](RAINA.Services.Response.md)\>

### <a id="RAINA_IntentProcessor_RegisterModule_RAINA_Modules_IIntentModule_"></a> RegisterModule\(IIntentModule\)

Register an intent module

```csharp
public void RegisterModule(IIntentModule module)
```

#### Parameters

`module` [IIntentModule](RAINA.Modules.IIntentModule.md)

