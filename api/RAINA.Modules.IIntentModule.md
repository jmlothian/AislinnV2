# <a id="RAINA_Modules_IIntentModule"></a> Interface IIntentModule

Namespace: [RAINA.Modules](RAINA.Modules.md)  
Assembly: Aislinn.Raina.dll  

Interface for intent modules in RAINA

```csharp
public interface IIntentModule
```

## Methods

### <a id="RAINA_Modules_IIntentModule_GetExpectedEntities"></a> GetExpectedEntities\(\)

Gets the expected entities for this intent type

```csharp
string[] GetExpectedEntities()
```

#### Returns

 [string](https://learn.microsoft.com/dotnet/api/system.string)\[\]

### <a id="RAINA_Modules_IIntentModule_GetExpectedParameters"></a> GetExpectedParameters\(\)

Gets the expected parameters for this intent type

```csharp
string[] GetExpectedParameters()
```

#### Returns

 [string](https://learn.microsoft.com/dotnet/api/system.string)\[\]

### <a id="RAINA_Modules_IIntentModule_GetIntentType"></a> GetIntentType\(\)

Gets the unique identifier for this intent type

```csharp
string GetIntentType()
```

#### Returns

 [string](https://learn.microsoft.com/dotnet/api/system.string)

### <a id="RAINA_Modules_IIntentModule_GetPromptDescription"></a> GetPromptDescription\(\)

Gets the description of this intent for use in the OpenAI prompt

```csharp
string GetPromptDescription()
```

#### Returns

 [string](https://learn.microsoft.com/dotnet/api/system.string)

### <a id="RAINA_Modules_IIntentModule_GetPromptExamples"></a> GetPromptExamples\(\)

Gets examples of this intent type for use in the OpenAI prompt

```csharp
string[] GetPromptExamples()
```

#### Returns

 [string](https://learn.microsoft.com/dotnet/api/system.string)\[\]

### <a id="RAINA_Modules_IIntentModule_HandleAsync_System_String_RAINA_Intent_RAINA_Services_UserContext_"></a> HandleAsync\(string, Intent, UserContext\)

Handles this specific intent type

```csharp
Task<Response> HandleAsync(string userInput, Intent intent, UserContext context)
```

#### Parameters

`userInput` [string](https://learn.microsoft.com/dotnet/api/system.string)

`intent` [Intent](RAINA.Intent.md)

`context` [UserContext](RAINA.Services.UserContext.md)

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[Response](RAINA.Services.Response.md)\>

