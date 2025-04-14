# <a id="Aislinn_Core_Services_CognitiveTimeManager"></a> Class CognitiveTimeManager

Namespace: [Aislinn.Core.Services](Aislinn.Core.Services.md)  
Assembly: Aislinn.Core.dll  

Manages an internal time reference for the cognitive system that persists across sessions

```csharp
public class CognitiveTimeManager
```

#### Inheritance

object ← 
[CognitiveTimeManager](Aislinn.Core.Services.CognitiveTimeManager.md)

## Constructors

### <a id="Aislinn_Core_Services_CognitiveTimeManager__ctor_System_String_"></a> CognitiveTimeManager\(string\)

```csharp
public CognitiveTimeManager(string stateFilePath = "agent_state.json")
```

#### Parameters

`stateFilePath` [string](https://learn.microsoft.com/dotnet/api/system.string)

## Properties

### <a id="Aislinn_Core_Services_CognitiveTimeManager_CurrentTime"></a> CurrentTime

```csharp
public double CurrentTime { get; }
```

#### Property Value

 [double](https://learn.microsoft.com/dotnet/api/system.double)

### <a id="Aislinn_Core_Services_CognitiveTimeManager_LastSaveTime"></a> LastSaveTime

```csharp
public DateTime LastSaveTime { get; }
```

#### Property Value

 [DateTime](https://learn.microsoft.com/dotnet/api/system.datetime)

### <a id="Aislinn_Core_Services_CognitiveTimeManager_SystemStartTime"></a> SystemStartTime

```csharp
public double SystemStartTime { get; }
```

#### Property Value

 [double](https://learn.microsoft.com/dotnet/api/system.double)

## Methods

### <a id="Aislinn_Core_Services_CognitiveTimeManager_ConvertToSystemTime_System_DateTime_"></a> ConvertToSystemTime\(DateTime\)

Convert a real DateTime to internal system time

```csharp
public double ConvertToSystemTime(DateTime dateTime)
```

#### Parameters

`dateTime` [DateTime](https://learn.microsoft.com/dotnet/api/system.datetime)

#### Returns

 [double](https://learn.microsoft.com/dotnet/api/system.double)

### <a id="Aislinn_Core_Services_CognitiveTimeManager_GetCurrentTime"></a> GetCurrentTime\(\)

Gets the current system time, updating it first

```csharp
public double GetCurrentTime()
```

#### Returns

 [double](https://learn.microsoft.com/dotnet/api/system.double)

### <a id="Aislinn_Core_Services_CognitiveTimeManager_LoadState"></a> LoadState\(\)

Load the time state from persistent storage

```csharp
public void LoadState()
```

### <a id="Aislinn_Core_Services_CognitiveTimeManager_SaveState"></a> SaveState\(\)

Save the current time state to persistent storage

```csharp
public void SaveState()
```

### <a id="Aislinn_Core_Services_CognitiveTimeManager_UpdateTime"></a> UpdateTime\(\)

Update the current system time based on elapsed real time

```csharp
public double UpdateTime()
```

#### Returns

 [double](https://learn.microsoft.com/dotnet/api/system.double)

