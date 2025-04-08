using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using RAINA.Modules;
using RAINA.Modules.Implementations;

namespace RAINA
{
    /// <summary>
    /// Bootstrapper for RAINA - Realtime Adaptive Intelligence Neural Assistant
    /// </summary>
    public class RainaBootstrapper
    {
        private readonly IServiceCollection _services;
        private readonly List<Type> _moduleTypes = new List<Type>();

        public RainaBootstrapper(IServiceCollection services)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
        }

        /// <summary>
        /// Configure the core services for RAINA
        /// </summary>
        public RainaBootstrapper ConfigureCore(string openAIApiKey)
        {
            // Register core services
            _services.AddSingleton<IntentProcessor>(sp =>
            {
                var conversationManager = sp.GetRequiredService<ConversationManager>();
                var contextDetector = sp.GetRequiredService<ContextDetector>();

                return new IntentProcessor(
                    openAIApiKey,
                    conversationManager,
                    contextDetector);
            });

            _services.AddSingleton<ConversationManager>();
            _services.AddSingleton<ContextDetector>();
            _services.AddSingleton<ChunkManager>();
            _services.AddSingleton<WorkingMemoryController>();
            _services.AddSingleton<QueryEngine>();
            _services.AddSingleton<TaskManager>();

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
                   .RegisterIntentModule<TaskCreationIntentModule>();
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
        /// Build the RAINA system
        /// </summary>
        public ServiceProvider Build()
        {
            // Register the module registration startup task
            _services.AddSingleton<IStartupTask>(sp => new ModuleRegistrationTask(
                sp.GetRequiredService<IntentProcessor>(),
                _moduleTypes
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
                    _intentProcessor.Registermodule(module);
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
}