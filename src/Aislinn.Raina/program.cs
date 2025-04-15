using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using RAINA;
using RAINA.Modules.Implementations;
using Aislinn.Core.Models;
using Aislinn.Core.Cognitive;
using Aislinn.Core.Memory;
using RAINA.Services;
using Aislinn.Core.Query;

namespace RAINA;
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Initializing RAINA - Realtime Adaptive Intelligence Neural Assistant");

        // Get API key from environment or config
        string openAIApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        if (string.IsNullOrEmpty(openAIApiKey))
        {
            Console.WriteLine("Please set the OPENAI_API_KEY environment variable");
            Console.WriteLine("Example: set OPENAI_API_KEY=your-api-key-here");
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
            // Configure RAINA with the bootstrapper
            var serviceProvider = new RainaBootstrapper(services)
                .ConfigureChunkMemorySystem()  // Set up the Aislinn memory system
                .SetCollectionIds("raina_main", "raina_associations")  // Set collection IDs
                .ConfigureCore(openAIApiKey)
                .RegisterStandardModules()
                .ConfigureIntegrations()
                .Build();

            // Get the intent processor
            var intentProcessor = serviceProvider.GetRequiredService<IntentProcessor>();

            // Get the chunk manager for direct memory operations
            var chunkManager = serviceProvider.GetRequiredService<ChunkManager>();

            // Get the working memory controller
            var workingMemoryController = serviceProvider.GetRequiredService<WorkingMemoryController>();

            var chunkQueryService = serviceProvider.GetRequiredService<ChunkQueryService>();

            // Create a simple user context
            var userContext = new UserContext
            {
                UserId = "user1",
                UserName = "User",
                CurrentTopic = "general"
            };

            // Load any active context from memory
            await LoadUserContextAsync(userContext, workingMemoryController, chunkManager, chunkQueryService);

            Console.WriteLine("RAINA is ready! Type 'exit' to quit, 'help' for commands.");
            Console.WriteLine();

            // Main interaction loop
            while (true)
            {
                Console.Write("> ");
                string input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input))
                    continue;

                if (input.ToLower() == "exit")
                    break;

                if (input.ToLower() == "help")
                {
                    DisplayHelp();
                    continue;
                }

                if (input.ToLower() == "memory")
                {
                    await DisplayMemoryStatusAsync(workingMemoryController);
                    continue;
                }

                try
                {
                    // Process the input
                    var response = await intentProcessor.ProcessInputAsync(input, userContext);

                    // Display the response
                    Console.WriteLine();
                    Console.WriteLine($"RAINA: {response.Message}");

                    // Display relevant chunks if any
                    if (response.RelevantChunks != null && response.RelevantChunks.Count > 0)
                    {
                        Console.WriteLine();
                        Console.WriteLine("Relevant information:");
                        foreach (var chunk in response.RelevantChunks)
                        {
                            Console.WriteLine($"- {chunk.Name} (Activation: {chunk.ActivationLevel:F2})");
                        }
                    }

                    Console.WriteLine();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    Console.WriteLine(ex.StackTrace);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to initialize RAINA: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }

        Console.WriteLine("Thank you for using RAINA. Goodbye!");
    }

    static void DisplayHelp()
    {
        Console.WriteLine("RAINA Commands:");
        Console.WriteLine("  help    - Display this help message");
        Console.WriteLine("  exit    - Exit the application");
        Console.WriteLine("  memory  - Display working memory status");
        Console.WriteLine();
        Console.WriteLine("Examples of what you can ask RAINA:");
        Console.WriteLine("  - Remember that John's birthday is on May 15th");
        Console.WriteLine("  - Create a task to finish the report by Friday");
        Console.WriteLine("  - What did I tell you about the Johnson project?");
        Console.WriteLine("  - When is John's birthday?");
    }

    static async Task LoadUserContextAsync(UserContext userContext, WorkingMemoryController workingMemoryController, ChunkManager chunkManager, ChunkQueryService queryService)
    {
        // lookup the speaker
        var query = new ChunkQuery
        {
            ChunkType = "Declaritive",
            SemanticType = "entity.person.instance",
            Name = userContext.UserName,  // Variable containing the username
            NameHandling = NameMatchType.ExactMatch,  // Require exact name match
            ExtraSlotsHandling = ExtraSlotsHandling.Ignore,  // Ignore additional slots
            MinimumThreshold = 0.9  // Set high threshold since we're doing exact matching
        };

        var results = await queryService.ExecuteQueryAsync(query);
        if (results.Count == 0)
        {
            var personChunk = await chunkManager.CreateChunkAsync("Declaritive", "entity.person.instance", userContext.UserName, new Dictionary<string, object> { { "Name", userContext.UserName } });
            userContext.UserChunk = personChunk;
            Console.WriteLine($"Created new user chunk for {userContext.UserName}");
        }
        else
        {
            userContext.UserChunk = results.FirstOrDefault().Chunk;
            Console.WriteLine($"Loaded existing user chunk for {userContext.UserName}");
        }

        // Load active chunks into user context
        var activeChunks = await workingMemoryController.GetActiveChunksAsync();
        userContext.ActiveMemoryChunks = activeChunks;

        Console.WriteLine($"Loaded {activeChunks.Count} active memory chunks into context");
    }

    static async Task DisplayMemoryStatusAsync(WorkingMemoryController workingMemoryController)
    {
        var activeChunks = await workingMemoryController.GetActiveChunksAsync();
        var primedChunks = await workingMemoryController.GetPrimedChunksAsync();

        Console.WriteLine();
        Console.WriteLine("RAINA Memory Status:");
        Console.WriteLine($"Working Memory: {activeChunks.Count} active chunks");

        if (activeChunks.Count > 0)
        {
            Console.WriteLine("Active chunks in working memory:");
            foreach (var chunk in activeChunks.OrderByDescending(c => c.ActivationLevel))
            {
                Console.WriteLine($"- {chunk.Name} (Type: {chunk.ChunkType}, Activation: {chunk.ActivationLevel:F2})");
            }
        }

        Console.WriteLine($"Primed Memory: {primedChunks.Count} primed chunks");

        if (primedChunks.Count > 0 && primedChunks.Count <= 5)
        {
            Console.WriteLine("Top primed chunks (just below working memory threshold):");
            foreach (var chunk in primedChunks.Take(5))
            {
                Console.WriteLine($"- {chunk.Name} (Type: {chunk.ChunkType}, Activation: {chunk.ActivationLevel:F2})");
            }
        }

        Console.WriteLine();
    }
}

// Extended Implementation classes with Aislinn references
// These would be in their own files in a real implementation



public class ContextDetector
{
    private readonly CognitiveMemorySystem _memorySystem;

    public ContextDetector(CognitiveMemorySystem memorySystem)
    {
        _memorySystem = memorySystem;
    }

    public async Task UpdateContextAsync(string userInput, Intent intent)
    {
        // This would update the detected context based on user input and the classified intent
        // For demonstration purposes
        Console.WriteLine($"[Debug] Context updated based on intent: {intent.IntentType}");
    }
}

public class ChunkManager
{
    private readonly CognitiveMemorySystem _memorySystem;

    public ChunkManager(CognitiveMemorySystem memorySystem)
    {
        _memorySystem = memorySystem;
    }

    public async Task<Chunk> CreateChunkAsync(string chunkType, string semanticType, string name, Dictionary<string, object> slots = null)
    {
        var chunk = new Chunk
        {
            ChunkType = chunkType,
            SemanticType = semanticType,
            Name = name,
            Slots = new Dictionary<string, ModelSlot>()
        };

        if (slots != null)
        {
            foreach (var slot in slots)
            {
                chunk.Slots[slot.Key] = new ModelSlot
                {
                    Name = slot.Key,
                    Value = slot.Value
                };
            }
        }

        return await _memorySystem.AddChunkAsync(chunk);
    }

    public async Task<Chunk> GetChunkAsync(Guid chunkId)
    {
        return await _memorySystem.GetChunkAsync(chunkId);
    }
}

public class WorkingMemoryController
{
    private readonly WorkingMemoryManager _workingMemory;
    private readonly CognitiveMemorySystem _memorySystem;

    public WorkingMemoryController(WorkingMemoryManager workingMemory, CognitiveMemorySystem memorySystem)
    {
        _workingMemory = workingMemory;
        _memorySystem = memorySystem;
    }

    public async Task<List<Chunk>> GetActiveChunksAsync()
    {
        return await _workingMemory.GetWorkingMemoryContentsAsync();
    }

    public async Task<List<Chunk>> GetPrimedChunksAsync()
    {
        return await _workingMemory.GetPrimedChunksAsync();
    }

    public async Task<bool> FocusOnChunkAsync(Guid chunkId)
    {
        var chunk = await _memorySystem.GetChunkAsync(chunkId);
        if (chunk == null) return false;

        // Activate the chunk and update working memory
        chunk = await _memorySystem.ActivateChunkAsync(chunkId);
        return chunk != null;
    }
}

public class QueryEngine
{
    private readonly CognitiveMemorySystem _memorySystem;

    public QueryEngine(CognitiveMemorySystem memorySystem)
    {
        _memorySystem = memorySystem;
    }

    public async Task<List<Chunk>> SearchAsync(QueryParameters parameters, UserContext context)
    {
        // In a real implementation, this would use vector search and activation to find relevant chunks
        // For now, we'll simulate finding chunks with high activation

        var activeChunks = await _memorySystem.GetActiveChunksAsync(0.1);

        // Filter by keywords if provided
        if (parameters.Keywords != null && parameters.Keywords.Count > 0)
        {
            activeChunks = activeChunks
                .Where(c => parameters.Keywords.Any(k =>
                    c.Name?.Contains(k, StringComparison.OrdinalIgnoreCase) == true))
                .ToList();
        }

        // Return top results based on activation
        return activeChunks
            .OrderByDescending(c => c.ActivationLevel)
            .Take(parameters.MaxResults)
            .ToList();
    }
}

public class TaskManager
{
    private readonly CognitiveMemorySystem _memorySystem;

    public TaskManager(CognitiveMemorySystem memorySystem)
    {
        _memorySystem = memorySystem;
    }

    public async Task<RainaTask> CreateTaskAsync(RainaTask task, UserContext context)
    {
        //rewrite this, include entity/people relationships - we may want another prompt to extract key details as well.
        // this will also not just create tasks, but handle other updates. so we should branch where needed
        //should we call back into the program here, or should we handle this logic inside the intent module?

        //in addition to the memory system, we also want to make Trello API calls to manage our board
        //we should keep a copy of the board in memory, sync occassionally, and possibly check it to avoid duplicates?

        // Create a task chunks in memory
        // Utterance 
        //   -> Task 
        //     -> Entities (create or associate) (should entities be under the task or the utterance?)
        //       -> People 
        //       -> Dates
        //       -> Projects?
        //       -> Possibly Related Tasks
        //       -> Other Entities
        //   -> Speaker
        //   -> Context (current topic, date, time, tone, etc.)
        //   -> Listener (if we have multiple speakers, we may want to track who said what, Raina in most case for now)

        var taskChunk = new Chunk
        {
            ChunkType = "Declarative",
            SemanticType = "Task",
            Name = task.Title,
            Slots = new Dictionary<string, ModelSlot>
            {
                ["Title"] = new ModelSlot { Name = "Title", Value = task.Title },
                ["Status"] = new ModelSlot { Name = "Status", Value = task.Status },
                ["Priority"] = new ModelSlot { Name = "Priority", Value = task.Priority ?? "Medium" }
                //redo all the details...
            }
        };

        if (task.DueDate.HasValue)
        {
            taskChunk.Slots["DueDate"] = new ModelSlot { Name = "DueDate", Value = task.DueDate.Value };
        }

        // Add the task to memory
        await _memorySystem.AddChunkAsync(taskChunk);

        // Activate the task to make it prominent in memory
        await _memorySystem.ActivateChunkAsync(taskChunk.ID, null, 0.8);

        //handle Trello Update

        //lets write a response to confirm the task creation, and surface any relevant memories that are highly activated


        Console.WriteLine($"[Debug] Task created: {task.Title}");

        return task;
    }
}

public class ExternalIntegrationService
{
    // Would handle integration with external services
}

public class PersistenceService
{
    // Would handle saving and loading the system state
}