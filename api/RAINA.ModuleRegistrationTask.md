# <a id="RAINA_ModuleRegistrationTask"></a> Class ModuleRegistrationTask

Namespace: [RAINA](RAINA.md)  
Assembly: Aislinn.Raina.dll  

Task to register intent modules with the intent processor

```csharp
public class ModuleRegistrationTask : IStartupTask
```

#### Inheritance

object ← 
[ModuleRegistrationTask](RAINA.ModuleRegistrationTask.md)

#### Implements

[IStartupTask](RAINA.IStartupTask.md)

## Constructors

### <a id="RAINA_ModuleRegistrationTask__ctor_RAINA_IntentProcessor_System_Collections_Generic_List_System_Type__"></a> ModuleRegistrationTask\(IntentProcessor, List<Type\>\)

```csharp
public ModuleRegistrationTask(IntentProcessor intentProcessor, List<Type> moduleTypes)
```

#### Parameters

`intentProcessor` [IntentProcessor](RAINA.IntentProcessor.md)

`moduleTypes` [List](https://learn.microsoft.com/dotnet/api/system.collections.generic.list\-1)<[Type](https://learn.microsoft.com/dotnet/api/system.type)\>

## Methods

### <a id="RAINA_ModuleRegistrationTask_Execute_System_IServiceProvider_"></a> Execute\(IServiceProvider\)

```csharp
public void Execute(IServiceProvider serviceProvider)
```

#### Parameters

`serviceProvider` [IServiceProvider](https://learn.microsoft.com/dotnet/api/system.iserviceprovider)

