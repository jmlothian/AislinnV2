using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using RAINA.Events;
using RAINA.Services;
using RAINA.ConsoleEvents;
using Microsoft.Extensions.DependencyInjection;
using Aislinn.Configuration;
using Aislinn.Core;
using Aislinn.Core.Models;
using Aislinn.Core.Query;
using static Aislinn.Core.Context.ContextContainer;
using Aislinn.Core.Services;

namespace RAINA.ConsoleApp
{
    class Program
    {
        private static ConsoleEventSubscriber _eventSubscriber;
        private static IntentProcessor _intentProcessor;
        private static UserContext _userContext;
        private static AislinnCoreServices _coreServices;
        private static RainaServices _rainaServices;
        private static ChunkManager _chunkManager;
        private static WorkingMemoryController _workingMemoryController;
        private static ChunkQueryService _chunkQueryService;
        private static EntityRelationshipExtractionService _entityRelationshipExtractionService;
        private static bool _isRunning = true;

        static async Task Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("=== RAINA Console ===");
            Console.WriteLine("Initializing RAINA - Realtime Adaptive Intelligence Neural Assistant");

            try
            {
                // Initialize RAINA services
                await InitializeRainaAsync();

                // Subscribe to events
                _eventSubscriber = new ConsoleEventSubscriber(showTimestamps: true, useColors: true);
                _eventSubscriber.Subscribe();

                Console.WriteLine("\nRAINA is ready! Type 'help' for commands, 'exit' to quit.\n");

                // Main console loop
                await RunConsoleLoopAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fatal error: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
            finally
            {
                await ShutdownAsync();
            }
        }

        private static async Task InitializeRainaAsync()
        {
            // Get API keys from environment
            string openAIApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
            if (string.IsNullOrEmpty(openAIApiKey))
            {
                Console.WriteLine("Please set the OPENAI_API_KEY environment variable");
                Console.WriteLine("Example: set OPENAI_API_KEY=your-api-key-here");
                return;
            }

            string voyageAPIKey = Environment.GetEnvironmentVariable("VOYAGE_API_KEY");
            if (string.IsNullOrEmpty(voyageAPIKey))
            {
                Console.WriteLine("Please set the VOYAGE_API_KEY environment variable");
                Console.WriteLine("Example: set VOYAGE_API_KEY=your-api-key-here");
                return;
            }

            // Define the database path for chunk storage
            string appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "RAINA");

            // Create directory if it doesn't exist
            if (!Directory.Exists(appDataPath))
            {
                Directory.CreateDirectory(appDataPath);
            }

            // Set up dependency injection
            var services = new ServiceCollection();

            try
            {
                var config = new RainaConfiguration();
                config.ChunkCollectionId = "raina_main";
                config.AssociationCollectionId = "raina_associations";
                config.OpenAIApiKey = openAIApiKey;

                var vectorConfig = new VoyageConfiguration();
                vectorConfig.VoyageApiKey = voyageAPIKey;

                // Configure RAINA with the bootstrapper
                var serviceProvider = new RainaBootstrapper(services)
                    .ConfigureWithSettings(config, vectorConfig)
                    .ConfigureChunkMemorySystem()
                    .ConfigureCore()
                    .RegisterStandardModules()
                    .ConfigureIntegrations()
                    .Build();

                // Get all required services
                _intentProcessor = serviceProvider.GetRequiredService<IntentProcessor>();
                _coreServices = serviceProvider.GetRequiredService<AislinnCoreServices>();
                _rainaServices = serviceProvider.GetRequiredService<RainaServices>();
                _chunkManager = serviceProvider.GetRequiredService<ChunkManager>();
                _workingMemoryController = serviceProvider.GetRequiredService<WorkingMemoryController>();
                _chunkQueryService = serviceProvider.GetRequiredService<ChunkQueryService>();
                _entityRelationshipExtractionService = serviceProvider.GetRequiredService<EntityRelationshipExtractionService>();

                // Initialize conversation manager
                _rainaServices.ConversationManager.Init();

                // Load entity relationship cache
                await _entityRelationshipExtractionService.LoadCacheAsync("relationship_cache.json");

                // Create user context
                _userContext = new UserContext
                {
                    UserId = "console_user",
                    UserName = "Console User",
                    CurrentTopic = "general"
                };

                // Load any active context from memory
                await LoadUserContextAsync(_userContext);

                // Initialize conversation
                await _rainaServices.ConversationManager.InitializeConversationAsync(_userContext);

                Console.WriteLine("RAINA services initialized successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during initialization: {ex.Message}");
                throw;
            }
        }

        private static async Task LoadUserContextAsync(UserContext userContext)
        {
            try
            {
                // Try to find existing user chunk
                var userChunk = await _coreServices.MemorySystem.FindChunkBySemanticTypeAndName("entity.person.instance", userContext.UserName);
                if (userChunk != null)
                {
                    userContext.UserChunk = userChunk;
                    Console.WriteLine($"Loaded existing user context for: {userContext.UserName}");
                }
                else
                {
                    // Create new user chunk
                    var newUserChunk = new Chunk
                    {
                        ChunkType = "Declarative",
                        CognitiveCategory = "Instance",
                        SemanticType = "entity.person.instance",
                        Name = userContext.UserName,
                        Slots = new Dictionary<string, ModelSlot>
                        {
                            { "EntityName", new ModelSlot { Name = "EntityName", Value = userContext.UserName } },
                            { "Role", new ModelSlot { Name = "Role", Value = "User" } },
                            { "CreatedTimestamp", new ModelSlot { Name = "CreatedTimestamp", Value = DateTime.Now } }
                        }
                    };

                    userContext.UserChunk = await _coreServices.MemorySystem.AddChunkAsync(newUserChunk);
                    Console.WriteLine($"Created new user context for: {userContext.UserName}");
                }

                // Try to find RAINA chunk
                var rainaChunk = await _coreServices.MemorySystem.FindChunkBySemanticTypeAndName("entity.person.instance", "Raina");
                if (rainaChunk == null)
                {
                    var newRainaChunk = new Chunk
                    {
                        ChunkType = "Declarative",
                        CognitiveCategory = "Instance",
                        SemanticType = "entity.person.instance",
                        Name = "Raina",
                        Slots = new Dictionary<string, ModelSlot>
                        {
                            { "EntityName", new ModelSlot { Name = "EntityName", Value = "Raina" } },
                            { "Role", new ModelSlot { Name = "Role", Value = "AI Assistant" } },
                            { "CreatedTimestamp", new ModelSlot { Name = "CreatedTimestamp", Value = DateTime.Now } }
                        }
                    };

                    userContext.RainaChunk = await _coreServices.MemorySystem.AddChunkAsync(newRainaChunk);
                }
                else
                {
                    userContext.RainaChunk = rainaChunk;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading user context: {ex.Message}");
            }
        }

        private static async Task RunConsoleLoopAsync()
        {
            while (_isRunning)
            {
                try
                {
                    Console.Write("User: ");
                    var input = Console.ReadLine();

                    if (string.IsNullOrWhiteSpace(input))
                        continue;

                    // Handle admin commands
                    if (await HandleAdminCommandAsync(input))
                        continue;

                    // Process user input through RAINA
                    Console.WriteLine(); // Add some space before events
                    var response = await _intentProcessor.ProcessInputAsync(input, _userContext);
                    Console.WriteLine(); // Add some space after events

                    Console.WriteLine($"Assistant: {response.Message}");
                    Console.WriteLine();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing input: {ex.Message}");
                }
            }
        }

        private static async Task<bool> HandleAdminCommandAsync(string input)
        {
            var command = input.ToLower().Trim();

            switch (command)
            {
                case "exit":
                case "quit":
                    Console.WriteLine("Shutting down RAINA...");
                    _isRunning = false;
                    return true;

                case "help":
                    ShowHelp();
                    return true;

                case "status":
                    await ShowStatusAsync();
                    return true;

                case "memory":
                    await ShowMemoryStatusAsync();
                    return true;

                case "clear":
                    Console.Clear();
                    Console.WriteLine("=== RAINA Console ===\n");
                    return true;

                case "events":
                    ToggleEvents();
                    return true;

                case "context":
                    await ShowContextAsync();
                    return true;

                case "entities":
                    await ShowEntitiesAsync();
                    return true;

                case "summaries":
                    await ShowSummariesAsync();
                    return true;

                case "chunks":
                    await ShowChunksAsync();
                    return true;

                case "working":
                case "wm":
                    await ShowWorkingMemoryDetailAsync();
                    return true;

                case "primed":
                    await ShowPrimedChunksAsync();
                    return true;

                case "stats":
                    await ShowDetailedStatsAsync();
                    return true;

                default:
                    // Check for parameterized commands
                    if (input.StartsWith("chunks "))
                    {
                        var query = input.Substring(7).Trim();
                        await QueryChunksAsync(query);
                        return true;
                    }
                    if (input.StartsWith("activate "))
                    {
                        var chunkId = input.Substring(9).Trim();
                        await ActivateChunkAsync(chunkId);
                        return true;
                    }
                    if (input.StartsWith("inspect "))
                    {
                        var chunkId = input.Substring(8).Trim();
                        await InspectChunkAsync(chunkId);
                        return true;
                    }

                    return false; // Not an admin command
            }
        }

        private static void ShowHelp()
        {
            Console.WriteLine("\n=== RAINA Console Commands ===");
            Console.WriteLine("Chat Commands:");
            Console.WriteLine("  (type anything) - Chat with RAINA");
            Console.WriteLine();
            Console.WriteLine("System Commands:");
            Console.WriteLine("  exit/quit      - Shutdown RAINA");
            Console.WriteLine("  help           - Show this help");
            Console.WriteLine("  status         - Show basic RAINA status");
            Console.WriteLine("  clear          - Clear screen");
            Console.WriteLine("  events         - Toggle event display");
            Console.WriteLine();
            Console.WriteLine("Memory & Context:");
            Console.WriteLine("  memory         - Show basic memory status");
            Console.WriteLine("  working/wm     - Show working memory details");
            Console.WriteLine("  primed         - Show primed chunks");
            Console.WriteLine("  context        - Show current context");
            Console.WriteLine("  entities       - Show extracted entities");
            Console.WriteLine("  summaries      - Show summary tree");
            Console.WriteLine("  chunks         - Show all chunks (limited)");
            Console.WriteLine("  stats          - Show detailed system stats");
            Console.WriteLine();
            Console.WriteLine("Advanced:");
            Console.WriteLine("  chunks <query>    - Query chunks (e.g., 'chunks goal')");
            Console.WriteLine("  activate <id>     - Manually activate chunk by ID");
            Console.WriteLine("  inspect <id>      - Inspect specific chunk details");
            Console.WriteLine();
        }

        private static async Task ShowStatusAsync()
        {
            Console.WriteLine("\n=== RAINA Status ===");
            Console.WriteLine($"Running: {_isRunning}");
            Console.WriteLine($"Events: {(_eventSubscriber != null ? "Subscribed" : "Not subscribed")}");
            Console.WriteLine($"User: {_userContext?.UserName ?? "Unknown"}");

            if (_coreServices != null)
            {
                try
                {
                    var workingMemory = await _coreServices.MemorySystem.GetWorkingMemoryContentsAsync();
                    var primedChunks = await _coreServices.MemorySystem.GetPrimedChunksAsync();

                    Console.WriteLine($"Working Memory: {workingMemory.Count}/7 slots");
                    Console.WriteLine($"Primed Chunks: {primedChunks.Count}");
                    Console.WriteLine($"Cognitive Steps: {_coreServices.TimeManager.GetCognitiveSteps():N0}");
                    Console.WriteLine($"Agent Time: {_coreServices.TimeManager.GetAgentTime():F1}s");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error getting status: {ex.Message}");
                }
            }

            Console.WriteLine();
        }

        private static async Task ShowMemoryStatusAsync()
        {
            Console.WriteLine("\n=== Memory Status ===");

            if (_coreServices?.MemorySystem == null)
            {
                Console.WriteLine("Memory system not available");
                return;
            }

            try
            {
                var workingMemory = await _coreServices.MemorySystem.GetWorkingMemoryContentsAsync();
                var primedChunks = await _coreServices.MemorySystem.GetPrimedChunksAsync();

                Console.WriteLine($"Working Memory: {workingMemory.Count} chunks");
                foreach (var chunk in workingMemory.Take(5))
                {
                    Console.WriteLine($"  {chunk.Name} ({chunk.ActivationLevel:F2})");
                }
                if (workingMemory.Count > 5)
                    Console.WriteLine($"  ... and {workingMemory.Count - 5} more");

                Console.WriteLine($"\nPrimed Chunks: {primedChunks.Count}");
                foreach (var chunk in primedChunks.Take(3))
                {
                    Console.WriteLine($"  {chunk.Name} ({chunk.ActivationLevel:F2})");
                }
                if (primedChunks.Count > 3)
                    Console.WriteLine($"  ... and {primedChunks.Count - 3} more");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error accessing memory: {ex.Message}");
            }

            Console.WriteLine();
        }

        private static async Task ShowContextAsync()
        {
            Console.WriteLine("\n=== Current Context ===");

            if (_coreServices?.ContextContainer == null)
            {
                Console.WriteLine("Context container not available");
                return;
            }

            try
            {
                var contextSnapshot = _coreServices.ContextContainer.CreateContextSnapshot();

                foreach (var category in contextSnapshot)
                {
                    Console.WriteLine($"\n{category.Key}:");
                    foreach (var factor in category.Value.Take(10))
                    {
                        var valueStr = factor.Value?.ToString();
                        if (valueStr?.Length > 50)
                            valueStr = valueStr.Substring(0, 47) + "...";
                        Console.WriteLine($"  {factor.Key}: {valueStr}");
                    }
                    if (category.Value.Count > 10)
                        Console.WriteLine($"  ... and {category.Value.Count - 10} more factors");
                }

                if (!contextSnapshot.Any())
                    Console.WriteLine("No context factors currently active");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error accessing context: {ex.Message}");
            }

            Console.WriteLine();
        }

        private static async Task ShowEntitiesAsync()
        {
            Console.WriteLine("\n=== Extracted Entities ===");

            if (_rainaServices?.EntityManager == null)
            {
                Console.WriteLine("Entity manager not available");
                return;
            }

            try
            {
                // Show entities by querying for entity instance chunks
                var topChunks = await _coreServices.MemorySystem.GetTopActivatedChunksAsync(50, excludeWorkingMemory: false);
                var entityChunks = topChunks.Where(c =>
                    c.CognitiveCategory == "Instance" &&
                    c.SemanticType?.Contains("entity") == true)
                    .Take(15)
                    .ToList();

                if (entityChunks.Any())
                {
                    Console.WriteLine($"{"Name",-20} {"Type",-25} {"Activation",-10}");
                    Console.WriteLine(new string('-', 65));

                    foreach (var chunk in entityChunks)
                    {
                        var name = chunk.Name?.Length > 20 ? chunk.Name.Substring(0, 17) + "..." : chunk.Name ?? "Unnamed";
                        var type = chunk.SemanticType?.Length > 25 ? chunk.SemanticType.Substring(0, 22) + "..." : chunk.SemanticType ?? "Unknown";

                        Console.WriteLine($"{name,-20} {type,-25} {chunk.ActivationLevel:F3,-10}");
                    }
                }
                else
                {
                    Console.WriteLine("No entity instances found in memory");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error accessing entities: {ex.Message}");
            }

            Console.WriteLine();
        }

        private static async Task ShowSummariesAsync()
        {
            Console.WriteLine("\n=== Summary Tree ===");

            if (_rainaServices?.ConversationManager == null)
            {
                Console.WriteLine("Conversation manager not available");
                return;
            }

            try
            {
                // Look for chunks that represent summaries
                var topChunks = await _coreServices.MemorySystem.GetTopActivatedChunksAsync(100, excludeWorkingMemory: false);
                var summaryChunks = topChunks.Where(c =>
                    c.ChunkType == "ContextSummary" ||
                    c.Name?.Contains("Summary") == true ||
                    c.Slots.ContainsKey("IsSummary"))
                    .Take(10)
                    .ToList();

                if (summaryChunks.Any())
                {
                    Console.WriteLine($"{"Name",-30} {"Activation",-10} {"Created",-20}");
                    Console.WriteLine(new string('-', 70));

                    foreach (var chunk in summaryChunks)
                    {
                        var name = chunk.Name?.Length > 30 ? chunk.Name.Substring(0, 27) + "..." : chunk.Name ?? "Unnamed";
                        var created = chunk.Slots.ContainsKey("CreatedTimestamp") && chunk.Slots["CreatedTimestamp"].Value is DateTime dt
                            ? dt.ToString("HH:mm:ss")
                            : "Unknown";

                        Console.WriteLine($"{name,-30} {chunk.ActivationLevel:F3,-10} {created,-20}");
                    }
                }
                else
                {
                    Console.WriteLine("No summary chunks found in memory");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error accessing summaries: {ex.Message}");
            }

            Console.WriteLine();
        }

        private static async Task ShowChunksAsync()
        {
            Console.WriteLine("\n=== All Chunks (Top 20 by Activation) ===");

            if (_coreServices?.MemorySystem == null)
            {
                Console.WriteLine("Memory system not available");
                return;
            }

            try
            {
                var topChunks = await _coreServices.MemorySystem.GetTopActivatedChunksAsync(20, excludeWorkingMemory: false);

                Console.WriteLine($"{"ID",-8} {"Type",-12} {"Activation",-10} {"Name",-30}");
                Console.WriteLine(new string('-', 70));

                foreach (var chunk in topChunks)
                {
                    var shortId = chunk.ID.ToString().Substring(0, 8);
                    var chunkType = chunk.ChunkType?.Length > 12 ? chunk.ChunkType.Substring(0, 9) + "..." : chunk.ChunkType ?? "Unknown";
                    var name = chunk.Name?.Length > 30 ? chunk.Name.Substring(0, 27) + "..." : chunk.Name ?? "Unnamed";

                    Console.WriteLine($"{shortId,-8} {chunkType,-12} {chunk.ActivationLevel:F3,-10} {name,-30}");
                }

                if (topChunks.Count == 0)
                    Console.WriteLine("No chunks found");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error accessing chunks: {ex.Message}");
            }

            Console.WriteLine();
        }

        private static async Task ShowWorkingMemoryDetailAsync()
        {
            Console.WriteLine("\n=== Working Memory Details ===");

            if (_coreServices?.MemorySystem == null)
            {
                Console.WriteLine("Memory system not available");
                return;
            }

            try
            {
                var workingMemory = await _coreServices.MemorySystem.GetWorkingMemoryContentsAsync();

                Console.WriteLine($"Capacity: {workingMemory.Count}/7 slots");

                if (workingMemory.Any())
                {
                    Console.WriteLine("\nActive Chunks:");
                    Console.WriteLine($"{"ID",-8} {"Activation",-10} {"Type",-12} {"Name",-25}");
                    Console.WriteLine(new string('-', 65));

                    foreach (var chunk in workingMemory)
                    {
                        var shortId = chunk.ID.ToString().Substring(0, 8);
                        var chunkType = chunk.ChunkType?.Length > 12 ? chunk.ChunkType.Substring(0, 9) + "..." : chunk.ChunkType ?? "Unknown";
                        var name = chunk.Name?.Length > 25 ? chunk.Name.Substring(0, 22) + "..." : chunk.Name ?? "Unnamed";

                        Console.WriteLine($"{shortId,-8} {chunk.ActivationLevel:F3,-10} {chunkType,-12} {name,-25}");
                    }
                }
                else
                {
                    Console.WriteLine("Working memory is empty");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error accessing working memory: {ex.Message}");
            }

            Console.WriteLine();
        }

        private static async Task ShowPrimedChunksAsync()
        {
            Console.WriteLine("\n=== Primed Chunks ===");

            if (_coreServices?.MemorySystem == null)
            {
                Console.WriteLine("Memory system not available");
                return;
            }

            try
            {
                var primedChunks = await _coreServices.MemorySystem.GetPrimedChunksAsync();

                Console.WriteLine($"Count: {primedChunks.Count}");

                if (primedChunks.Any())
                {
                    Console.WriteLine($"{"ID",-8} {"Activation",-10} {"Type",-12} {"Name",-25}");
                    Console.WriteLine(new string('-', 65));

                    foreach (var chunk in primedChunks.Take(15))
                    {
                        var shortId = chunk.ID.ToString().Substring(0, 8);
                        var chunkType = chunk.ChunkType?.Length > 12 ? chunk.ChunkType.Substring(0, 9) + "..." : chunk.ChunkType ?? "Unknown";
                        var name = chunk.Name?.Length > 25 ? chunk.Name.Substring(0, 22) + "..." : chunk.Name ?? "Unnamed";

                        Console.WriteLine($"{shortId,-8} {chunk.ActivationLevel:F3,-10} {chunkType,-12} {name,-25}");
                    }

                    if (primedChunks.Count > 15)
                        Console.WriteLine($"... and {primedChunks.Count - 15} more");
                }
                else
                {
                    Console.WriteLine("No primed chunks");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error accessing primed chunks: {ex.Message}");
            }

            Console.WriteLine();
        }

        private static async Task ShowDetailedStatsAsync()
        {
            Console.WriteLine("\n=== Detailed System Statistics ===");

            if (_coreServices == null)
            {
                Console.WriteLine("Core services not available");
                return;
            }

            try
            {
                var workingMemory = await _coreServices.MemorySystem.GetWorkingMemoryContentsAsync();
                var primedChunks = await _coreServices.MemorySystem.GetPrimedChunksAsync();
                var topChunks = await _coreServices.MemorySystem.GetTopActivatedChunksAsync(100, excludeWorkingMemory: false);
                var contextSnapshot = _coreServices.ContextContainer.CreateContextSnapshot();

                Console.WriteLine("Memory System:");
                Console.WriteLine($"  Total Active Chunks: {topChunks.Count:N0}");
                Console.WriteLine($"  Working Memory: {workingMemory.Count}/7");
                Console.WriteLine($"  Primed Chunks: {primedChunks.Count}");
                if (topChunks.Any())
                {
                    Console.WriteLine($"  Average Activation: {topChunks.Average(c => c.ActivationLevel):F3}");
                    Console.WriteLine($"  Highest Activation: {topChunks.Max(c => c.ActivationLevel):F3}");
                }

                Console.WriteLine("\nContext System:");
                Console.WriteLine($"  Active Categories: {contextSnapshot.Count}");
                var totalFactors = contextSnapshot.Sum(c => c.Value.Count);
                Console.WriteLine($"  Total Factors: {totalFactors}");

                Console.WriteLine("\nCognitive Time:");
                Console.WriteLine($"  Steps: {_coreServices.TimeManager.GetCognitiveSteps():N0}");
                Console.WriteLine($"  Agent Time: {_coreServices.TimeManager.GetAgentTime():F1}s");

                if (_userContext != null)
                {
                    Console.WriteLine("\nSession Info:");
                    Console.WriteLine($"  User: {_userContext.UserName}");
                    Console.WriteLine($"  Current Topic: {_userContext.CurrentTopic ?? "None"}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error gathering stats: {ex.Message}");
            }

            Console.WriteLine();
        }

        private static async Task QueryChunksAsync(string query)
        {
            Console.WriteLine($"\n=== Chunk Query: '{query}' ===");

            if (_coreServices?.QueryService == null)
            {
                Console.WriteLine("Query service not available");
                return;
            }

            try
            {
                // Create a simple query - this is basic text matching
                var allChunks = await _coreServices.MemorySystem.GetTopActivatedChunksAsync(100, excludeWorkingMemory: false);
                var matchingChunks = allChunks.Where(c =>
                    (c.Name?.ToLower().Contains(query.ToLower()) == true) ||
                    (c.ChunkType?.ToLower().Contains(query.ToLower()) == true) ||
                    (c.SemanticType?.ToLower().Contains(query.ToLower()) == true))
                    .ToList();

                Console.WriteLine($"Found {matchingChunks.Count} results:");

                if (matchingChunks.Any())
                {
                    Console.WriteLine($"{"ID",-8} {"Score",-6} {"Type",-12} {"Name",-30}");
                    Console.WriteLine(new string('-', 66));

                    foreach (var chunk in matchingChunks.Take(10))
                    {
                        var shortId = chunk.ID.ToString().Substring(0, 8);
                        var chunkType = chunk.ChunkType?.Length > 12 ? chunk.ChunkType.Substring(0, 9) + "..." : chunk.ChunkType ?? "Unknown";
                        var name = chunk.Name?.Length > 30 ? chunk.Name.Substring(0, 27) + "..." : chunk.Name ?? "Unnamed";

                        Console.WriteLine($"{shortId,-8} {chunk.ActivationLevel:F2,-6} {chunkType,-12} {name,-30}");
                    }

                    if (matchingChunks.Count > 10)
                        Console.WriteLine($"... and {matchingChunks.Count - 10} more results");
                }
                else
                {
                    Console.WriteLine("No matching chunks found");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Query error: {ex.Message}");
            }

            Console.WriteLine();
        }

        private static async Task ActivateChunkAsync(string chunkIdStr)
        {
            Console.WriteLine($"\n=== Activating Chunk: {chunkIdStr} ===");

            if (!Guid.TryParse(chunkIdStr, out var chunkId))
            {
                Console.WriteLine("Invalid chunk ID format. Use full GUID.");
                return;
            }

            if (_coreServices?.MemorySystem == null)
            {
                Console.WriteLine("Memory system not available");
                return;
            }

            try
            {
                var chunk = await _coreServices.MemorySystem.ActivateChunkAsync(chunkId, "manual_console", 1.0);
                if (chunk != null)
                {
                    Console.WriteLine($"Successfully activated: {chunk.Name}");
                    Console.WriteLine($"New activation level: {chunk.ActivationLevel:F3}");
                }
                else
                {
                    Console.WriteLine("Chunk not found or could not be activated");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Activation error: {ex.Message}");
            }

            Console.WriteLine();
        }

        private static async Task InspectChunkAsync(string chunkIdStr)
        {
            Console.WriteLine($"\n=== Inspecting Chunk: {chunkIdStr} ===");

            if (!Guid.TryParse(chunkIdStr, out var chunkId))
            {
                Console.WriteLine("Invalid chunk ID format. Use full GUID.");
                return;
            }

            if (_coreServices?.MemorySystem == null)
            {
                Console.WriteLine("Memory system not available");
                return;
            }

            try
            {
                var chunk = await _coreServices.MemorySystem.GetChunkAsync(chunkId);
                if (chunk != null)
                {
                    Console.WriteLine($"ID: {chunk.ID}");
                    Console.WriteLine($"Name: {chunk.Name}");
                    Console.WriteLine($"Type: {chunk.ChunkType}");
                    Console.WriteLine($"Semantic Type: {chunk.SemanticType}");
                    Console.WriteLine($"Cognitive Category: {chunk.CognitiveCategory}");
                    Console.WriteLine($"Activation: {chunk.ActivationLevel:F3}");

                    Console.WriteLine("\nSlots:");
                    foreach (var slot in chunk.Slots.Take(10))
                    {
                        var value = slot.Value.Value?.ToString();
                        if (value?.Length > 50)
                            value = value.Substring(0, 47) + "...";
                        Console.WriteLine($"  {slot.Key}: {value}");
                    }

                    if (chunk.Slots.Count > 10)
                        Console.WriteLine($"  ... and {chunk.Slots.Count - 10} more slots");

                    Console.WriteLine($"\nActivation History: {chunk.ActivationHistory?.Count ?? 0} entries");
                    if (chunk.ActivationHistory?.Any() == true)
                    {
                        var recent = chunk.ActivationHistory.Take(3);
                        foreach (var history in recent)
                        {
                            Console.WriteLine($"  Step {history.ActivationDate}: {history.PreviousValue:F3} -> {history.NewValue:F3} (change: {history.Change:+F3;-F3})");
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Chunk not found");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Inspection error: {ex.Message}");
            }

            Console.WriteLine();
        }

        private static void ToggleEvents()
        {
            if (_eventSubscriber != null)
            {
                _eventSubscriber.Unsubscribe();
                _eventSubscriber = null;
                Console.WriteLine("Event display disabled");
            }
            else
            {
                _eventSubscriber = new ConsoleEventSubscriber(showTimestamps: true, useColors: true);
                _eventSubscriber.Subscribe();
                Console.WriteLine("Event display enabled");
            }
        }

        private static async Task ShutdownAsync()
        {
            Console.WriteLine("Shutting down...");

            // Unsubscribe from events
            _eventSubscriber?.Unsubscribe();

            // Shutdown RAINA services
            if (_rainaServices?.ConversationManager != null)
            {
                try
                {
                    _rainaServices.ConversationManager.Shutdown();
                    Console.WriteLine("Conversation manager shutdown complete");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error shutting down conversation manager: {ex.Message}");
                }
            }

            // Dispose memory system
            if (_coreServices?.MemorySystem != null)
            {
                try
                {
                    _coreServices.MemorySystem.Dispose();
                    Console.WriteLine("Memory system disposed");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error disposing memory system: {ex.Message}");
                }
            }

            Console.WriteLine("Goodbye!");
        }
    }
}