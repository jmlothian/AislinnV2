# <a id="Aislinn_Core_Goals_GoalManagementServiceExtensions"></a> Class GoalManagementServiceExtensions

Namespace: [Aislinn.Core.Goals](Aislinn.Core.Goals.md)  
Assembly: Aislinn.Core.dll  

```csharp
public static class GoalManagementServiceExtensions
```

#### Inheritance

object ← 
[GoalManagementServiceExtensions](Aislinn.Core.Goals.GoalManagementServiceExtensions.md)

## Methods

### <a id="Aislinn_Core_Goals_GoalManagementServiceExtensions_EvaluateExpressionWithRelationshipsAsync_Aislinn_Core_Goals_GoalManagementService_System_String_Aislinn_Core_Models_Chunk_Aislinn_Core_Goals_GoalRelationshipMatcher_"></a> EvaluateExpressionWithRelationshipsAsync\(GoalManagementService, string, Chunk, GoalRelationshipMatcher\)

Evaluates a criteria expression that may contain relationship patterns

```csharp
public static Task<bool> EvaluateExpressionWithRelationshipsAsync(this GoalManagementService goalService, string expression, Chunk goal, GoalRelationshipMatcher relationshipMatcher)
```

#### Parameters

`goalService` [GoalManagementService](Aislinn.Core.Goals.GoalManagementService.md)

`expression` [string](https://learn.microsoft.com/dotnet/api/system.string)

`goal` [Chunk](Aislinn.Core.Models.Chunk.md)

`relationshipMatcher` [GoalRelationshipMatcher](Aislinn.Core.Goals.GoalRelationshipMatcher.md)

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[bool](https://learn.microsoft.com/dotnet/api/system.boolean)\>

