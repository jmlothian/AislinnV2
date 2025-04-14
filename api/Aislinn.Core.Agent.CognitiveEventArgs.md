# <a id="Aislinn_Core_Agent_CognitiveEventArgs"></a> Class CognitiveEventArgs

Namespace: [Aislinn.Core.Agent](Aislinn.Core.Agent.md)  
Assembly: Aislinn.Agent.dll  

```csharp
public class CognitiveEventArgs : EventArgs
```

#### Inheritance

object ← 
[EventArgs](https://learn.microsoft.com/dotnet/api/system.eventargs) ← 
[CognitiveEventArgs](Aislinn.Core.Agent.CognitiveEventArgs.md)

#### Inherited Members

[EventArgs.Empty](https://learn.microsoft.com/dotnet/api/system.eventargs.empty)

## Properties

### <a id="Aislinn_Core_Agent_CognitiveEventArgs_EventData"></a> EventData

```csharp
public Dictionary<string, object> EventData { get; set; }
```

#### Property Value

 [Dictionary](https://learn.microsoft.com/dotnet/api/system.collections.generic.dictionary\-2)<[string](https://learn.microsoft.com/dotnet/api/system.string), object\>

### <a id="Aislinn_Core_Agent_CognitiveEventArgs_EventType"></a> EventType

```csharp
public string EventType { get; set; }
```

#### Property Value

 [string](https://learn.microsoft.com/dotnet/api/system.string)

### <a id="Aislinn_Core_Agent_CognitiveEventArgs_Timestamp"></a> Timestamp

```csharp
public DateTime Timestamp { get; set; }
```

#### Property Value

 [DateTime](https://learn.microsoft.com/dotnet/api/system.datetime)

