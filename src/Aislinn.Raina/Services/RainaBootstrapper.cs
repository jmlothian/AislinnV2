using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using RAINA.Modules;
using RAINA.Modules.Implementations;
using Aislinn.ChunkStorage;
using Aislinn.ChunkStorage.Interfaces;
using Aislinn.ChunkStorage.Storage;
using Aislinn.Core.Activation;
using Aislinn.Core.Services;
using Aislinn.Core.Memory;
using Aislinn.Models.Activation;
using Aislinn.VectorStorage.Storage;
using Aislinn.Storage.AssociationStore;
using Aislinn.Core.Cognitive;
using Microsoft.Extensions.Logging;
using RAINA.Services;

namespace RAINA
{
    /// <summary>
    /// Bootstrapper for RAINA - Realtime Adaptive Intelligence Neural Assistant
    /// </summary>
    public class RainaBootstrapper
    {
        private readonly IServiceCollection _services;
        private readonly List<Type> _moduleTypes = new List<Type>();
        private string _defaultChunkCollectionId = "default";
        private string _defaultAssociationCollectionId = "default";

        public RainaBootstrapper(IServiceCollection services)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
        }

        /// <summary>
        /// Configure the Aislinn chunk-based memory system
        /// </summary>
        public RainaBootstrapper ConfigureChunkMemorySystem()
        {
            // Register Aislinn storage providers
            _services.AddSingleton<IChunkStore>(sp =>
                new ChunkStore());

            _services.AddSingleton<IAssociationStore>(sp =>
                new AssociationStore());

            // Register activation model components
            _services.AddSingleton<ActivationParametersRegistry>();

            _services.AddSingleton<IActivationModel>(sp =>
            {
                var timeManager = sp.GetRequiredService<CognitiveTimeManager>();
                var parametersRegistry = sp.GetRequiredService<ActivationParametersRegistry>();
                return new ActRActivationModel(timeManager, parametersRegistry);
            });

            // Register cognitive time manager
            _services.AddSingleton<CognitiveTimeManager>();

            // Register chunk activation service
            _services.AddSingleton<ChunkActivationService>(sp =>
            {
                var chunkStore = sp.GetRequiredService<IChunkStore>();
                var associationStore = sp.GetRequiredService<IAssociationStore>();
                var activationModel = sp.GetRequiredService<IActivationModel>();
                var parametersRegistry = sp.GetRequiredService<ActivationParametersRegistry>();

                return new ChunkActivationService(
                    chunkStore,
                    associationStore,
                    activationModel,
                    parametersRegistry,
                    _defaultChunkCollectionId,
                    _defaultAssociationCollectionId);
            });

            // Register working memory manager
            _services.AddSingleton<WorkingMemoryManager>(sp =>
            {
                var chunkStore = sp.GetRequiredService<IChunkStore>();
                var associationStore = sp.GetRequiredService<IAssociationStore>();

                return new WorkingMemoryManager(
                    chunkStore,
                    associationStore,
                    totalCapacity: 7, // Miller's Law - 7±2 items
                    chunkCollectionId: _defaultChunkCollectionId,
                    associationCollectionId: _defaultAssociationCollectionId);
            });

            // Register cognitive memory system
            _services.AddSingleton<CognitiveMemorySystem>(sp =>
            {
                var chunkStore = sp.GetRequiredService<IChunkStore>();
                var associationStore = sp.GetRequiredService<IAssociationStore>();
                var activationModel = sp.GetRequiredService<IActivationModel>();
                var timeManager = sp.GetRequiredService<CognitiveTimeManager>();
                var parametersRegistry = sp.GetRequiredService<ActivationParametersRegistry>();

                return new CognitiveMemorySystem(
                    chunkStore,
                    associationStore,
                    activationModel,
                    timeManager, parametersRegistry,
                    _defaultChunkCollectionId,
                    _defaultAssociationCollectionId);
            });

            return this;
        }

        /// <summary>
        /// Configure the core services for RAINA
        /// </summary>
        public RainaBootstrapper ConfigureCore(string openAIApiKey)
        {
            // Register intent processor
            _services.AddSingleton<IntentProcessor>(sp =>
            {
                var conversationManager = sp.GetRequiredService<ConversationManager>();
                var contextDetector = sp.GetRequiredService<ContextDetector>();

                return new IntentProcessor(
                    openAIApiKey,
                    conversationManager,
                    contextDetector);
            });

            // Register core services that interface with memory system
            _services.AddSingleton<ConversationManager>(sp =>
            {
                var memorySystem = sp.GetRequiredService<CognitiveMemorySystem>();
                return new ConversationManager(memorySystem, openAIApiKey);
            });

            _services.AddSingleton<ContextDetector>(sp =>
            {
                var memorySystem = sp.GetRequiredService<CognitiveMemorySystem>();
                return new ContextDetector(memorySystem);
            });

            _services.AddSingleton<ChunkManager>(sp =>
            {
                var memorySystem = sp.GetRequiredService<CognitiveMemorySystem>();
                return new ChunkManager(memorySystem);
            });

            _services.AddSingleton<WorkingMemoryController>(sp =>
            {
                var workingMemory = sp.GetRequiredService<WorkingMemoryManager>();
                var memorySystem = sp.GetRequiredService<CognitiveMemorySystem>();
                return new WorkingMemoryController(workingMemory, memorySystem);
            });

            _services.AddSingleton<QueryEngine>(sp =>
            {
                var memorySystem = sp.GetRequiredService<CognitiveMemorySystem>();
                return new QueryEngine(memorySystem);
            });

            _services.AddSingleton<TaskManager>(sp =>
            {
                var memorySystem = sp.GetRequiredService<CognitiveMemorySystem>();
                return new TaskManager(memorySystem);
            });

            return this;
        }

        /// <summary>
        /// Register a specific intent module
        /// </summary>
        public RainaBootstrapper RegisterIntentModule<T>() where T : class, IIntentModule
        {
            _services.AddSingleton<T>();
            _moduleTypes.Add(typeof(T));
            return this;
        }

        /// <summary>
        /// Register the standard set of intent modules
        /// </summary>
        public RainaBootstrapper RegisterStandardModules()
        {
            return RegisterIntentModule<QueryIntentModule>()
                   .RegisterIntentModule<TaskManagementIntentModule>();
            // Add more standard modules as they're implemented
        }

        /// <summary>
        /// Configure external integrations
        /// </summary>
        public RainaBootstrapper ConfigureIntegrations()
        {
            // Register integration services
            _services.AddSingleton<ExternalIntegrationService>();
            _services.AddSingleton<PersistenceService>();

            return this;
        }

        /// <summary>
        /// Set collection IDs for chunk storage
        /// </summary>
        public RainaBootstrapper SetCollectionIds(string chunkCollectionId, string associationCollectionId)
        {
            _defaultChunkCollectionId = chunkCollectionId;
            _defaultAssociationCollectionId = associationCollectionId;
            return this;
        }

        /// <summary>
        /// Build the RAINA system
        /// </summary>
        public ServiceProvider Build()
        {
            // Register the module registration startup task
            _services.AddSingleton<IStartupTask>(sp => new ModuleRegistrationTask(
                sp.GetRequiredService<IntentProcessor>(),
                _moduleTypes
            ));

            // Register chunk database initialization task
            _services.AddSingleton<IStartupTask>(sp => new ChunkDatabaseInitializationTask(
                sp.GetRequiredService<IChunkStore>(),
                sp.GetRequiredService<IAssociationStore>(),
                _defaultChunkCollectionId,
                _defaultAssociationCollectionId
            ));

            var provider = _services.BuildServiceProvider();

            // Run startup tasks
            var startupTasks = provider.GetServices<IStartupTask>();
            foreach (var task in startupTasks)
            {
                task.Execute(provider);
            }

            return provider;
        }
    }

    /// <summary>
    /// Interface for tasks that need to run during startup
    /// </summary>
    public interface IStartupTask
    {
        void Execute(IServiceProvider serviceProvider);
    }

    /// <summary>
    /// Task to register intent modules with the intent processor
    /// </summary>
    public class ModuleRegistrationTask : IStartupTask
    {
        private readonly IntentProcessor _intentProcessor;
        private readonly List<Type> _moduleTypes;

        public ModuleRegistrationTask(IntentProcessor intentProcessor, List<Type> moduleTypes)
        {
            _intentProcessor = intentProcessor;
            _moduleTypes = moduleTypes;
        }

        public void Execute(IServiceProvider serviceProvider)
        {
            foreach (var moduleType in _moduleTypes)
            {
                var module = serviceProvider.GetService(moduleType) as IIntentModule;
                if (module != null)
                {
                    _intentProcessor.RegisterModule(module);
                    Console.WriteLine($"Registered intent module: {moduleType.Name} for intent type: {module.GetIntentType()}");
                }
            }

            // Output summary of registered modules
            var modules = _intentProcessor.GetRegisteredModules().ToList();
            Console.WriteLine($"Total registered modules: {modules.Count}");
            Console.WriteLine("Available intent types:");
            foreach (var module in modules)
            {
                Console.WriteLine($"- {module.GetIntentType()}: {module.GetPromptDescription()}");
            }
        }
    }

    /// <summary>
    /// Task to initialize the chunk database with required collections
    /// </summary>
    public class ChunkDatabaseInitializationTask : IStartupTask
    {
        private readonly IChunkStore _chunkStore;
        private readonly IAssociationStore _associationStore;
        private readonly string _chunkCollectionId;
        private readonly string _associationCollectionId;

        public ChunkDatabaseInitializationTask(
            IChunkStore chunkStore,
            IAssociationStore associationStore,
            string chunkCollectionId,
            string associationCollectionId)
        {
            _chunkStore = chunkStore;
            _associationStore = associationStore;
            _chunkCollectionId = chunkCollectionId;
            _associationCollectionId = associationCollectionId;
        }

        public void Execute(IServiceProvider serviceProvider)
        {
            //initialize the chunk context
            ChunkContext.Initialize(_chunkStore, _chunkCollectionId);

            // Initialize chunk collection
            var chunkCollection = _chunkStore.GetOrCreateCollectionAsync(_chunkCollectionId).GetAwaiter().GetResult();
            Console.WriteLine($"Initialized chunk collection: {_chunkCollectionId}");

            // Initialize association collection
            var associationCollection = _associationStore.GetOrCreateCollectionAsync(_associationCollectionId).GetAwaiter().GetResult();
            Console.WriteLine($"Initialized association collection: {_associationCollectionId}");
        }
    }
}