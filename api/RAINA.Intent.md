# <a id="RAINA_Intent"></a> Class Intent

Namespace: [RAINA](RAINA.md)  
Assembly: Aislinn.Raina.dll  

```csharp
public class Intent
```

#### Inheritance

object ← 
[Intent](RAINA.Intent.md)

## Properties

### <a id="RAINA_Intent_Confidence"></a> Confidence

```csharp
[JsonPropertyName("confidence")]
public double Confidence { get; set; }
```

#### Property Value

 [double](https://learn.microsoft.com/dotnet/api/system.double)

### <a id="RAINA_Intent_Entities"></a> Entities

```csharp
[JsonPropertyName("entities")]
public List<Entity> Entities { get; set; }
```

#### Property Value

 [List](https://learn.microsoft.com/dotnet/api/system.collections.generic.list\-1)<[Entity](RAINA.Entity.md)\>

### <a id="RAINA_Intent_IntentType"></a> IntentType

```csharp
[JsonPropertyName("intentType")]
public string IntentType { get; set; }
```

#### Property Value

 [string](https://learn.microsoft.com/dotnet/api/system.string)

### <a id="RAINA_Intent_Parameters"></a> Parameters

```csharp
[JsonPropertyName("parameters")]
public Dictionary<string, string> Parameters { get; set; }
```

#### Property Value

 [Dictionary](https://learn.microsoft.com/dotnet/api/system.collections.generic.dictionary\-2)<[string](https://learn.microsoft.com/dotnet/api/system.string), [string](https://learn.microsoft.com/dotnet/api/system.string)\>

