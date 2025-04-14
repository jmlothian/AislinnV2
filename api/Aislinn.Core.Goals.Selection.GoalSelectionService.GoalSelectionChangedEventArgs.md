# <a id="Aislinn_Core_Goals_Selection_GoalSelectionService_GoalSelectionChangedEventArgs"></a> Class GoalSelectionService.GoalSelectionChangedEventArgs

Namespace: [Aislinn.Core.Goals.Selection](Aislinn.Core.Goals.Selection.md)  
Assembly: Aislinn.Core.dll  

Event args for goal selection changes

```csharp
public class GoalSelectionService.GoalSelectionChangedEventArgs : EventArgs
```

#### Inheritance

object ← 
[EventArgs](https://learn.microsoft.com/dotnet/api/system.eventargs) ← 
[GoalSelectionService.GoalSelectionChangedEventArgs](Aislinn.Core.Goals.Selection.GoalSelectionService.GoalSelectionChangedEventArgs.md)

#### Inherited Members

[EventArgs.Empty](https://learn.microsoft.com/dotnet/api/system.eventargs.empty)

## Properties

### <a id="Aislinn_Core_Goals_Selection_GoalSelectionService_GoalSelectionChangedEventArgs_ChangeReason"></a> ChangeReason

```csharp
public string ChangeReason { get; set; }
```

#### Property Value

 [string](https://learn.microsoft.com/dotnet/api/system.string)

### <a id="Aislinn_Core_Goals_Selection_GoalSelectionService_GoalSelectionChangedEventArgs_NewPrimaryGoalId"></a> NewPrimaryGoalId

```csharp
public Guid? NewPrimaryGoalId { get; set; }
```

#### Property Value

 [Guid](https://learn.microsoft.com/dotnet/api/system.guid)?

### <a id="Aislinn_Core_Goals_Selection_GoalSelectionService_GoalSelectionChangedEventArgs_NewSecondaryGoalIds"></a> NewSecondaryGoalIds

```csharp
public List<Guid> NewSecondaryGoalIds { get; set; }
```

#### Property Value

 [List](https://learn.microsoft.com/dotnet/api/system.collections.generic.list\-1)<[Guid](https://learn.microsoft.com/dotnet/api/system.guid)\>

### <a id="Aislinn_Core_Goals_Selection_GoalSelectionService_GoalSelectionChangedEventArgs_PreviousPrimaryGoalId"></a> PreviousPrimaryGoalId

```csharp
public Guid? PreviousPrimaryGoalId { get; set; }
```

#### Property Value

 [Guid](https://learn.microsoft.com/dotnet/api/system.guid)?

### <a id="Aislinn_Core_Goals_Selection_GoalSelectionService_GoalSelectionChangedEventArgs_PreviousSecondaryGoalIds"></a> PreviousSecondaryGoalIds

```csharp
public List<Guid> PreviousSecondaryGoalIds { get; set; }
```

#### Property Value

 [List](https://learn.microsoft.com/dotnet/api/system.collections.generic.list\-1)<[Guid](https://learn.microsoft.com/dotnet/api/system.guid)\>

### <a id="Aislinn_Core_Goals_Selection_GoalSelectionService_GoalSelectionChangedEventArgs_Timestamp"></a> Timestamp

```csharp
public DateTime Timestamp { get; set; }
```

#### Property Value

 [DateTime](https://learn.microsoft.com/dotnet/api/system.datetime)

