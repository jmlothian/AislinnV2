# <a id="RAINA_RainaBootstrapper"></a> Class RainaBootstrapper

Namespace: [RAINA](RAINA.md)  
Assembly: Aislinn.Raina.dll  

Bootstrapper for RAINA - Realtime Adaptive Intelligence Neural Assistant

```csharp
public class RainaBootstrapper
```

#### Inheritance

object ← 
[RainaBootstrapper](RAINA.RainaBootstrapper.md)

## Constructors

### <a id="RAINA_RainaBootstrapper__ctor_Microsoft_Extensions_DependencyInjection_IServiceCollection_"></a> RainaBootstrapper\(IServiceCollection\)

```csharp
public RainaBootstrapper(IServiceCollection services)
```

#### Parameters

`services` [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection)

## Methods

### <a id="RAINA_RainaBootstrapper_Build"></a> Build\(\)

Build the RAINA system

```csharp
public ServiceProvider Build()
```

#### Returns

 [ServiceProvider](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.serviceprovider)

### <a id="RAINA_RainaBootstrapper_ConfigureChunkMemorySystem"></a> ConfigureChunkMemorySystem\(\)

Configure the Aislinn chunk-based memory system

```csharp
public RainaBootstrapper ConfigureChunkMemorySystem()
```

#### Returns

 [RainaBootstrapper](RAINA.RainaBootstrapper.md)

### <a id="RAINA_RainaBootstrapper_ConfigureCore_System_String_"></a> ConfigureCore\(string\)

Configure the core services for RAINA

```csharp
public RainaBootstrapper ConfigureCore(string openAIApiKey)
```

#### Parameters

`openAIApiKey` [string](https://learn.microsoft.com/dotnet/api/system.string)

#### Returns

 [RainaBootstrapper](RAINA.RainaBootstrapper.md)

### <a id="RAINA_RainaBootstrapper_ConfigureIntegrations"></a> ConfigureIntegrations\(\)

Configure external integrations

```csharp
public RainaBootstrapper ConfigureIntegrations()
```

#### Returns

 [RainaBootstrapper](RAINA.RainaBootstrapper.md)

### <a id="RAINA_RainaBootstrapper_RegisterIntentModule__1"></a> RegisterIntentModule<T\>\(\)

Register a specific intent module

```csharp
public RainaBootstrapper RegisterIntentModule<T>() where T : class, IIntentModule
```

#### Returns

 [RainaBootstrapper](RAINA.RainaBootstrapper.md)

#### Type Parameters

`T` 

### <a id="RAINA_RainaBootstrapper_RegisterStandardModules"></a> RegisterStandardModules\(\)

Register the standard set of intent modules

```csharp
public RainaBootstrapper RegisterStandardModules()
```

#### Returns

 [RainaBootstrapper](RAINA.RainaBootstrapper.md)

### <a id="RAINA_RainaBootstrapper_SetCollectionIds_System_String_System_String_"></a> SetCollectionIds\(string, string\)

Set collection IDs for chunk storage

```csharp
public RainaBootstrapper SetCollectionIds(string chunkCollectionId, string associationCollectionId)
```

#### Parameters

`chunkCollectionId` [string](https://learn.microsoft.com/dotnet/api/system.string)

`associationCollectionId` [string](https://learn.microsoft.com/dotnet/api/system.string)

#### Returns

 [RainaBootstrapper](RAINA.RainaBootstrapper.md)

