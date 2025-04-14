# <a id="Aislinn_Core_Context_ContextContainer_ContextChangeEventArgs"></a> Class ContextContainer.ContextChangeEventArgs

Namespace: [Aislinn.Core.Context](Aislinn.Core.Context.md)  
Assembly: Aislinn.Core.dll  

Event args for context change notifications

```csharp
public class ContextContainer.ContextChangeEventArgs : EventArgs
```

#### Inheritance

object ← 
[EventArgs](https://learn.microsoft.com/dotnet/api/system.eventargs) ← 
[ContextContainer.ContextChangeEventArgs](Aislinn.Core.Context.ContextContainer.ContextChangeEventArgs.md)

#### Inherited Members

[EventArgs.Empty](https://learn.microsoft.com/dotnet/api/system.eventargs.empty)

## Properties

### <a id="Aislinn_Core_Context_ContextContainer_ContextChangeEventArgs_Category"></a> Category

```csharp
public ContextContainer.ContextCategory Category { get; set; }
```

#### Property Value

 [ContextContainer](Aislinn.Core.Context.ContextContainer.md).[ContextCategory](Aislinn.Core.Context.ContextContainer.ContextCategory.md)

### <a id="Aislinn_Core_Context_ContextContainer_ContextChangeEventArgs_ChangeSignificance"></a> ChangeSignificance

```csharp
public double ChangeSignificance { get; set; }
```

#### Property Value

 [double](https://learn.microsoft.com/dotnet/api/system.double)

### <a id="Aislinn_Core_Context_ContextContainer_ContextChangeEventArgs_FactorName"></a> FactorName

```csharp
public string FactorName { get; set; }
```

#### Property Value

 [string](https://learn.microsoft.com/dotnet/api/system.string)

### <a id="Aislinn_Core_Context_ContextContainer_ContextChangeEventArgs_NewValue"></a> NewValue

```csharp
public object NewValue { get; set; }
```

#### Property Value

 object

### <a id="Aislinn_Core_Context_ContextContainer_ContextChangeEventArgs_OldValue"></a> OldValue

```csharp
public object OldValue { get; set; }
```

#### Property Value

 object

### <a id="Aislinn_Core_Context_ContextContainer_ContextChangeEventArgs_Timestamp"></a> Timestamp

```csharp
public DateTime Timestamp { get; set; }
```

#### Property Value

 [DateTime](https://learn.microsoft.com/dotnet/api/system.datetime)

