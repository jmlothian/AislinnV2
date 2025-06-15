using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
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
using Aislinn.Core.Query;
using Aislinn.Core.Context;
using Aislinn.Core.Storage.SQLite;
using RAINA.Services.Data;
using Aislinn.Core.Models;
using Aislinn.Core;
using Aislinn.Configuration;
using Aislinn.VectorStorage.Interfaces;
using Aislinn.VectorStorage.Implementations;

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
        /// Configure RAINA with the provided settings
        /// </summary>
        public RainaBootstrapper ConfigureWithSettings(RainaConfiguration config, VoyageConfiguration voyageConfiguration)
        {
            config.Validate();
            _services.AddSingleton(config);
            _services.AddSingleton<AislinnConfiguration>(config);
            _services.AddSingleton(voyageConfiguration);
            return this;
        }

        /// <summary>
        /// Configure the Aislinn chunk-based memory system
        /// </summary>
        public RainaBootstrapper ConfigureChunkMemorySystem()
        {
            // Register storage providers - no more factories!
            _services.AddSingleton<IChunkStore, SQLiteChunkStore>();
            _services.AddSingleton<IAssociationStore, SQLiteAssociationStore>();

            // Register activation system components
            _services.AddSingleton<ActivationParametersRegistry>();
            _services.AddSingleton<CognitiveTimeManager>();
            _services.AddSingleton<IActivationModel, ActRActivationModel>();

            // Register memory system components - all use DI now
            _services.AddSingleton<ChunkActivationService>();
            _services.AddSingleton<WorkingMemoryManager>();
            _services.AddSingleton<CognitiveMemorySystem>();
            _services.AddSingleton<ContextContainer>();
            _services.AddSingleton<ChunkQueryService>();

            // Register the core services container
            _services.AddSingleton<AislinnCoreServices>();

            return this;
        }

        /// <summary>
        /// Configure the core services for RAINA
        /// </summary>
        public RainaBootstrapper ConfigureCore()
        {
            // Register all services with simple DI - no more factories!
            _services.AddSingleton<EntityRelationshipExtractionService>();
            _services.AddSingleton<EntityInstanceManager>();
            _services.AddSingleton<ConversationManager>();
            _services.AddSingleton<RainaServices>();

            // Register remaining services
            _services.AddSingleton<ContextDetector>();
            _services.AddSingleton<ChunkManager>();
            _services.AddSingleton<WorkingMemoryController>();
            _services.AddSingleton<QueryEngine>();
            _services.AddSingleton<TaskManager>();
            _services.AddSingleton<IntentProcessor>();
            _services.AddSingleton<OntologyLoader>();
            _services.AddSingleton<AssociationLoader>();
            _services.AddSingleton<VectorStore>();
            _services.AddSingleton<SummaryService>((sp) =>
            {
                var config = sp.GetRequiredService<RainaConfiguration>();
                var agentName = config.AgentName;
                return SummaryService.FromJson(agentName + ".json", config.ChunkCollectionId);
            });
            _services.AddSingleton<IVectorCollection, FastMemoryVectorCollection>(sp =>
            {
                return new FastMemoryVectorCollection("vector_collection", sp.GetRequiredService<IVectorizer>());
            });
            _services.AddSingleton<IVectorizer, VoyageVectorizer>();
            _services.AddSingleton<IVectorCollection>(sp =>
            {
                var store = sp.GetRequiredService<VectorStore>();
                var vectorizer = sp.GetRequiredService<IVectorizer>();
                var config = sp.GetRequiredService<RainaConfiguration>();

                return store.GetOrCreateCollectionAsync(config.RainaVectorCollection, vectorizer).GetAwaiter().GetResult();
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
        /// Set collection IDs for chunk storage (deprecated - use AislinnConfiguration instead)
        /// </summary>
        [Obsolete("Use AislinnConfiguration instead")]
        public RainaBootstrapper SetCollectionIds(string chunkCollectionId, string associationCollectionId)
        {
            // This method is now obsolete since we use AislinnConfiguration
            return this;
        }

        public void PrintOntologyTree(List<Chunk> chunks)
        {
            // Build hierarchy dictionary: path -> chunk
            var pathToChunk = chunks.ToDictionary(c => c.SemanticType, c => c);

            // Get all unique paths and sort them
            var allPaths = chunks.Select(c => c.SemanticType).OrderBy(p => p).ToList();

            // Build tree structure
            var tree = new Dictionary<string, List<string>>();
            var allNodes = new HashSet<string>();

            foreach (var path in allPaths)
            {
                var parts = path.Split('.');
                allNodes.Add(path);

                // Add intermediate paths if they don't exist as chunks
                for (int i = 1; i <= parts.Length; i++)
                {
                    var intermediatePath = string.Join(".", parts.Take(i));
                    allNodes.Add(intermediatePath);
                }
            }

            // Build parent-child relationships
            foreach (var node in allNodes)
            {
                var parts = node.Split('.');
                if (parts.Length > 1)
                {
                    var parent = string.Join(".", parts.Take(parts.Length - 1));

                    if (!tree.ContainsKey(parent))
                        tree[parent] = new List<string>();

                    if (!tree[parent].Contains(node))
                        tree[parent].Add(node);
                }
            }

            // Sort children for each parent
            foreach (var key in tree.Keys.ToList())
            {
                tree[key] = tree[key].OrderBy(x => x).ToList();
            }

            // Print the tree starting from root
            PrintNode("essence", tree, pathToChunk, "", true);
        }

        private void PrintNode(string currentPath, Dictionary<string, List<string>> tree,
                              Dictionary<string, Chunk> pathToChunk, string prefix, bool isLast)
        {
            // Print current node
            var connector = isLast ? "└── " : "├── ";
            var nodeName = currentPath.Split('.').Last();

            Console.Write(prefix + connector + nodeName);

            // Add chunk details if this path has an actual chunk
            if (pathToChunk.ContainsKey(currentPath))
            {
                var chunk = pathToChunk[currentPath];
                var description = chunk.Slots["Description"].Value?.ToString() ?? "";
                var synonyms = chunk.Slots["Synonyms"].Value as string[] ?? new string[0];

                Console.Write($" - {description}");
                if (synonyms.Length > 0)
                {
                    Console.Write($" [Synonyms: {string.Join(", ", synonyms)}]");
                }
            }

            Console.WriteLine();

            // Print children
            if (tree.ContainsKey(currentPath))
            {
                var children = tree[currentPath];
                for (int i = 0; i < children.Count; i++)
                {
                    var child = children[i];
                    var isLastChild = i == children.Count - 1;
                    var childPrefix = prefix + (isLast ? "    " : "│   ");

                    PrintNode(child, tree, pathToChunk, childPrefix, isLastChild);
                }
            }
        }

        /// <summary>
        /// Build the RAINA system
        /// </summary>
        public ServiceProvider Build()
        {
            var provider = _services.BuildServiceProvider();

            // Get configuration
            var config = provider.GetRequiredService<RainaConfiguration>();

            // Initialize chunk context
            var coreServices = provider.GetRequiredService<AislinnCoreServices>();
            ChunkContext.Initialize(coreServices.ChunkStore, coreServices.ChunkCollectionId);

            // Initialize chunk and association collections
            var chunkCollection = coreServices.ChunkStore.GetOrCreateCollectionAsync(coreServices.ChunkCollectionId).GetAwaiter().GetResult();
            Console.WriteLine($"Initialized chunk collection: {coreServices.ChunkCollectionId}");

            var associationCollection = coreServices.AssociationStore.GetOrCreateCollectionAsync(coreServices.AssociationCollectionId).GetAwaiter().GetResult();
            Console.WriteLine($"Initialized association collection: {coreServices.AssociationCollectionId}");

            // Register intent modules
            var intentProcessor = provider.GetRequiredService<IntentProcessor>();
            foreach (var moduleType in _moduleTypes)
            {
                var module = provider.GetService(moduleType) as IIntentModule;
                if (module != null)
                {
                    intentProcessor.RegisterModule(module);
                    Console.WriteLine($"Registered intent module: {moduleType.Name} for intent type: {module.GetIntentType()}");
                }
            }

            // Output summary of registered modules
            var modules = intentProcessor.GetRegisteredModules().ToList();
            Console.WriteLine($"Total registered modules: {modules.Count}");
            Console.WriteLine("Available intent types:");
            foreach (var module in modules)
            {
                Console.WriteLine($"- {module.GetIntentType()}: {module.GetPromptDescription()}");
            }

            // Load ontology and associations using config paths
            var ontologyLoader = provider.GetRequiredService<OntologyLoader>();
            var associationLoader = provider.GetRequiredService<AssociationLoader>();

            var allLoadedChunks = new List<Chunk>();
            if (Directory.Exists(config.BasicKnowledgeChunksPath))
            {
                foreach (var file in Directory.GetFiles(config.BasicKnowledgeChunksPath, "*.json"))
                {
                    var loadedChunks = ontologyLoader.LoadChunksAsync(File.ReadAllText(file)).Result;
                    allLoadedChunks.AddRange(loadedChunks);
                }
            }

            if (Directory.Exists(config.BasicKnowledgeAssociationsPath))
            {
                foreach (var file in Directory.GetFiles(config.BasicKnowledgeAssociationsPath, "*.json"))
                    associationLoader.LoadAssociationsAsync(File.ReadAllText(file), allLoadedChunks).Wait();
            }

            // Load entity relationship extraction cache
            var entityRelationshipExtractionService = provider.GetRequiredService<EntityRelationshipExtractionService>();
            entityRelationshipExtractionService.LoadCacheAsync(config.RelationshipCachePath).Wait();

            PrintOntologyTree(allLoadedChunks);
            return provider;
        }
    }
}