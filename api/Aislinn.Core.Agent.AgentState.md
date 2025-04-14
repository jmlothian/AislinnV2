# <a id="Aislinn_Core_Agent_AgentState"></a> Class AgentState

Namespace: [Aislinn.Core.Agent](Aislinn.Core.Agent.md)  
Assembly: Aislinn.Agent.dll  

```csharp
public class AgentState
```

#### Inheritance

object ← 
[AgentState](Aislinn.Core.Agent.AgentState.md)

## Properties

### <a id="Aislinn_Core_Agent_AgentState_CurrentContext"></a> CurrentContext

```csharp
public Dictionary<ContextContainer.ContextCategory, Dictionary<string, object>> CurrentContext { get; set; }
```

#### Property Value

 [Dictionary](https://learn.microsoft.com/dotnet/api/system.collections.generic.dictionary\-2)<ContextContainer.ContextCategory, [Dictionary](https://learn.microsoft.com/dotnet/api/system.collections.generic.dictionary\-2)<[string](https://learn.microsoft.com/dotnet/api/system.string), object\>\>

### <a id="Aislinn_Core_Agent_AgentState_CurrentPrimaryGoal"></a> CurrentPrimaryGoal

```csharp
public Chunk CurrentPrimaryGoal { get; set; }
```

#### Property Value

 Chunk

### <a id="Aislinn_Core_Agent_AgentState_CurrentSecondaryGoals"></a> CurrentSecondaryGoals

```csharp
public List<Chunk> CurrentSecondaryGoals { get; set; }
```

#### Property Value

 [List](https://learn.microsoft.com/dotnet/api/system.collections.generic.list\-1)<Chunk\>

### <a id="Aislinn_Core_Agent_AgentState_ExecutionState"></a> ExecutionState

```csharp
public GoalExecutionService.ExecutionState ExecutionState { get; set; }
```

#### Property Value

 GoalExecutionService.ExecutionState

### <a id="Aislinn_Core_Agent_AgentState_LastActivityTime"></a> LastActivityTime

```csharp
public DateTime LastActivityTime { get; set; }
```

#### Property Value

 [DateTime](https://learn.microsoft.com/dotnet/api/system.datetime)

### <a id="Aislinn_Core_Agent_AgentState_PrimedChunks"></a> PrimedChunks

```csharp
public List<Chunk> PrimedChunks { get; set; }
```

#### Property Value

 [List](https://learn.microsoft.com/dotnet/api/system.collections.generic.list\-1)<Chunk\>

### <a id="Aislinn_Core_Agent_AgentState_SystemTime"></a> SystemTime

```csharp
public double SystemTime { get; set; }
```

#### Property Value

 [double](https://learn.microsoft.com/dotnet/api/system.double)

### <a id="Aislinn_Core_Agent_AgentState_WorkingMemoryContents"></a> WorkingMemoryContents

```csharp
public List<Chunk> WorkingMemoryContents { get; set; }
```

#### Property Value

 [List](https://learn.microsoft.com/dotnet/api/system.collections.generic.list\-1)<Chunk\>

