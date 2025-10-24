using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Aislinn.Core;
using Aislinn.Core.Models;
using RAINA.Models.Mcp;
using RAINA.Services;

namespace RAINA.Modules.Implementations
{
    public class MemoryExtractionResponse
    {
        [JsonPropertyName("people")]
        public Dictionary<string, string[]> People { get; set; } = new Dictionary<string, string[]>();

        [JsonPropertyName("places")]
        public Dictionary<string, string[]> Places { get; set; } = new Dictionary<string, string[]>();

        [JsonPropertyName("activities")]
        public Dictionary<string, string[]> Activities { get; set; } = new Dictionary<string, string[]>();

        [JsonPropertyName("objects")]
        public Dictionary<string, string[]> Objects { get; set; } = new Dictionary<string, string[]>();

        [JsonPropertyName("events")]
        public Dictionary<string, string[]> Events { get; set; } = new Dictionary<string, string[]>();

        [JsonPropertyName("relationships")]
        public Dictionary<string, string[]> Relationships { get; set; } = new Dictionary<string, string[]>();

        [JsonPropertyName("likes")]
        public Dictionary<string, string[]> Likes { get; set; } = new Dictionary<string, string[]>();

        [JsonPropertyName("dislikes")]
        public Dictionary<string, string[]> Dislikes { get; set; } = new Dictionary<string, string[]>();

        [JsonPropertyName("loves")]
        public Dictionary<string, string[]> Loves { get; set; } = new Dictionary<string, string[]>();

        [JsonPropertyName("hates")]
        public Dictionary<string, string[]> Hates { get; set; } = new Dictionary<string, string[]>();

        [JsonPropertyName("constraints")]
        public Dictionary<string, string[]> Constraints { get; set; } = new Dictionary<string, string[]>();

        [JsonPropertyName("inferences")]
        public string[] Inferences { get; set; } = new string[0];
    }

    /// <summary>
    /// Module for handling memory storage - storing personal information, facts, and experiences
    /// </summary>
    public class RememberIntentModule : IIntentModule
    {
        private readonly QueryEngine _queryEngine;
        private readonly PromptLibrary _promptLibrary;
        private readonly LLMApiService _llmApiService;
        private readonly AislinnCoreServices _coreServices;
        private readonly MemoryChunkConverter _memoryChunkConverter;

        public RememberIntentModule(
            QueryEngine queryEngine,
            LLMApiService llmApiService,
            AislinnCoreServices coreServices,
            MemoryChunkConverter memoryChunkConverter)
        {
            _queryEngine = queryEngine ?? throw new ArgumentNullException(nameof(queryEngine));
            _llmApiService = llmApiService ?? throw new ArgumentNullException(nameof(llmApiService));
            _coreServices = coreServices ?? throw new ArgumentNullException(nameof(coreServices));
            _memoryChunkConverter = memoryChunkConverter ?? throw new ArgumentNullException(nameof(memoryChunkConverter));
            _promptLibrary = new PromptLibrary();
        }

        public string GetServerName() => "Remember";

        public bool IsInProcess => true;

        public McpServerConnection GetServerConnection() => null; // In-process, no external connection

        public string GetPromptDescription()
        {
            return "Remember: User wants to remember/store personal information, facts, or experiences in memory for future reference.";
        }

        public string[] GetPromptExamples()
        {
            return new[]
            {
                "Remember that I have a dentist appointment next Thursday at 2pm.",
                "Can you remember that Sarah's birthday is March 15th and she loves chocolate cake?",
                "My mom is allergic to shellfish - that's really important for when we go to restaurants.",
                "Just so you know, I'm lactose intolerant and my partner David is vegetarian.",
                "My boss mentioned I'm up for a promotion review in December. Also, my daughter Emma starts college in the fall."
            };
        }

        public string GetToolsPromptSection()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Tools:");
            sb.AppendLine("  - store_memory: Store important personal information, facts, or experiences in memory for future reference");
            sb.AppendLine("    Parameters: content (string), importance (string)");
            sb.AppendLine("Expected Entities: people, places, activities, objects, events, relationships, dates, times, likes, dislikes, loves, hates, constraints");
            sb.AppendLine("Expected Parameters: context, specificity, importance");
            return sb.ToString();
        }

        public Dictionary<string, string> ExtractArgumentsFromContext(UserContext context, Intent intent)
        {
            var args = new Dictionary<string, string>();

            // Add user information for memory context
            if (!string.IsNullOrEmpty(context.UserName))
            {
                args["userName"] = context.UserName;
            }

            // Add current timestamp
            args["dateTime"] = DateTime.Now.ToString("F");

            // Add agent name
            args["agentName"] = "Raina";

            return args;
        }

        public async Task<IList<McpTool>> ListToolsAsync()
        {
            return new List<McpTool>
            {
                new McpTool
                {
                    Name = "store_memory",
                    Description = "Store important personal information, facts, or experiences in memory for future reference",
                    InputSchema = new
                    {
                        type = "object",
                        properties = new
                        {
                            content = new { type = "string", description = "The content to remember" },
                            importance = new { type = "string", description = "Importance level (optional)" }
                        },
                        required = new[] { "content" }
                    }
                }
            };
        }

        public async Task<McpToolResult> CallToolAsync(string toolName, Dictionary<string, string> arguments)
        {
            try
            {
                switch (toolName)
                {
                    case "store_memory":
                        return await HandleStoreMemoryAsync(arguments);

                    default:
                        return new McpToolResult
                        {
                            ToolName = toolName,
                            Content = $"Unknown tool: {toolName}",
                            IsError = true
                        };
                }
            }
            catch (Exception ex)
            {
                return new McpToolResult
                {
                    ToolName = toolName,
                    Content = $"Error executing {toolName}: {ex.Message}",
                    IsError = true
                };
            }
        }

        private async Task<McpToolResult> HandleStoreMemoryAsync(Dictionary<string, string> arguments)
        {
            var userInput = arguments.GetValueOrDefault("content", "");
            var userName = arguments.GetValueOrDefault("userName", "User");
            var dateTime = arguments.GetValueOrDefault("dateTime", DateTime.Now.ToString("F"));
            var agentName = arguments.GetValueOrDefault("agentName", "Raina");

            // Run memory extraction prompt
            var details = new Dictionary<string, object>
            {
                ["dateTime"] = dateTime,
                ["agentName"] = agentName,
                ["userName"] = userName,
                ["userInput"] = userInput
            };

            var systemPrompt = _promptLibrary.HydratePrompt("memory.extract", details);
            var userPrompt = _promptLibrary.HydratePrompt("memory.extract.userprompt", details);

            var extracted = await _llmApiService.CallLLM(systemPrompt, userPrompt, "Raina", true);
            var data = JsonSerializer.Deserialize<MemoryExtractionResponse>(extracted);

            // Convert to chunks and associations
            var (chunks, associations) = await _memoryChunkConverter.ConvertMemoryExtractionToChunks(data);

            // Add chunks to memory
            foreach (var chunk in chunks)
            {
                var savedChunk = await _coreServices.MemorySystem.AddChunkAsync(chunk);
                chunk.ID = savedChunk.ID; // Ensure ID is updated for association reference
            }

            // Add associations to memory
            foreach (var assoc in associations)
            {
                await _coreServices.MemorySystem.CreateAssociationAsync(
                    assoc.ChunkAId,
                    assoc.ChunkBId,
                    assoc.RelationAtoB,
                    assoc.RelationBtoA,
                    assoc.WeightAtoB,
                    assoc.WeightBtoA,
                    assoc.SubTypeRelationshipAtoB,
                    assoc.SubTypeRelationshipBtoA
                );
            }

            // Activate the newly stored chunks
            foreach (var chunk in chunks)
            {
                await _coreServices.MemorySystem.ActivateChunkAsync(
                    chunk.ID,
                    null,
                    "memory.intent",
                    "RememberIntentModule.HandleStoreMemoryAsync",
                    null,
                    0.005
                );
            }

            return new McpToolResult
            {
                ToolName = "store_memory",
                Content = "The requested or important memories were stored.",
                IsError = false
            };
        }

        public Task<IList<McpResource>> ListResourcesAsync()
        {
            return Task.FromResult<IList<McpResource>>(new List<McpResource>());
        }

        public Task<IList<McpPrompt>> ListPromptsAsync()
        {
            return Task.FromResult<IList<McpPrompt>>(new List<McpPrompt>());
        }

        public Task<McpResourceContent> ReadResourceAsync(string uri)
        {
            throw new NotImplementedException("Remember module does not expose resources");
        }
    }
}