# <a id="Aislinn_Core_Procedural_StepRetryPolicy"></a> Class StepRetryPolicy

Namespace: [Aislinn.Core.Procedural](Aislinn.Core.Procedural.md)  
Assembly: Aislinn.Core.dll  

Retry policy for handling step failures

```csharp
public class StepRetryPolicy
```

#### Inheritance

object ← 
[StepRetryPolicy](Aislinn.Core.Procedural.StepRetryPolicy.md)

## Properties

### <a id="Aislinn_Core_Procedural_StepRetryPolicy_CurrentRetryCount"></a> CurrentRetryCount

Current retry count during execution

```csharp
public int CurrentRetryCount { get; set; }
```

#### Property Value

 [int](https://learn.microsoft.com/dotnet/api/system.int32)

### <a id="Aislinn_Core_Procedural_StepRetryPolicy_MaxRetries"></a> MaxRetries

Maximum number of retry attempts

```csharp
public int MaxRetries { get; set; }
```

#### Property Value

 [int](https://learn.microsoft.com/dotnet/api/system.int32)

### <a id="Aislinn_Core_Procedural_StepRetryPolicy_RetryDelay"></a> RetryDelay

Delay between retry attempts

```csharp
public TimeSpan RetryDelay { get; set; }
```

#### Property Value

 [TimeSpan](https://learn.microsoft.com/dotnet/api/system.timespan)

### <a id="Aislinn_Core_Procedural_StepRetryPolicy_UseExponentialBackoff"></a> UseExponentialBackoff

Whether to apply exponential backoff to the retry delay

```csharp
public bool UseExponentialBackoff { get; set; }
```

#### Property Value

 [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

## Methods

### <a id="Aislinn_Core_Procedural_StepRetryPolicy_Clone"></a> Clone\(\)

Creates a clone of this retry policy

```csharp
public StepRetryPolicy Clone()
```

#### Returns

 [StepRetryPolicy](Aislinn.Core.Procedural.StepRetryPolicy.md)

### <a id="Aislinn_Core_Procedural_StepRetryPolicy_GetNextRetryDelay"></a> GetNextRetryDelay\(\)

Calculate delay for next retry attempt

```csharp
public TimeSpan GetNextRetryDelay()
```

#### Returns

 [TimeSpan](https://learn.microsoft.com/dotnet/api/system.timespan)

