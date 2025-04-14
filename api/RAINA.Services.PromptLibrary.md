# <a id="RAINA_Services_PromptLibrary"></a> Class PromptLibrary

Namespace: [RAINA.Services](RAINA.Services.md)  
Assembly: Aislinn.Raina.dll  

```csharp
public class PromptLibrary
```

#### Inheritance

object ← 
[PromptLibrary](RAINA.Services.PromptLibrary.md)

## Constructors

### <a id="RAINA_Services_PromptLibrary__ctor"></a> PromptLibrary\(\)

```csharp
public PromptLibrary()
```

## Methods

### <a id="RAINA_Services_PromptLibrary_AddPrompt_System_String_System_String_"></a> AddPrompt\(string, string\)

```csharp
public void AddPrompt(string name, string template)
```

#### Parameters

`name` [string](https://learn.microsoft.com/dotnet/api/system.string)

`template` [string](https://learn.microsoft.com/dotnet/api/system.string)

### <a id="RAINA_Services_PromptLibrary_HydratePrompt_System_String_System_Collections_Generic_Dictionary_System_String_System_Object__"></a> HydratePrompt\(string, Dictionary<string, object\>\)

```csharp
public string HydratePrompt(string promptName, Dictionary<string, object> parameters)
```

#### Parameters

`promptName` [string](https://learn.microsoft.com/dotnet/api/system.string)

`parameters` [Dictionary](https://learn.microsoft.com/dotnet/api/system.collections.generic.dictionary\-2)<[string](https://learn.microsoft.com/dotnet/api/system.string), object\>

#### Returns

 [string](https://learn.microsoft.com/dotnet/api/system.string)

### <a id="RAINA_Services_PromptLibrary_Params_System_Object___"></a> Params\(params object\[\]\)

```csharp
public static Dictionary<string, object> Params(params object[] keyValuePairs)
```

#### Parameters

`keyValuePairs` object\[\]

#### Returns

 [Dictionary](https://learn.microsoft.com/dotnet/api/system.collections.generic.dictionary\-2)<[string](https://learn.microsoft.com/dotnet/api/system.string), object\>

