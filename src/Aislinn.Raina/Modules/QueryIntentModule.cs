using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using RAINA.Models.Mcp;
using RAINA.Services;

namespace RAINA.Modules.Implementations
{
    /// <summary>
    /// Module for handling information retrieval queries
    /// </summary>
    public class QueryIntentModule : IIntentModule
    {
        private readonly QueryEngine _queryEngine;
        private readonly ConversationManager _conversationManager;

        public QueryIntentModule(QueryEngine queryEngine, ConversationManager conversationManager)
        {
            _queryEngine = queryEngine ?? throw new ArgumentNullException(nameof(queryEngine));
            _conversationManager = conversationManager ?? throw new ArgumentNullException(nameof(conversationManager));
        }

        public string GetServerName() => "Query";

        public bool IsInProcess => true;

        public McpServerConnection GetServerConnection() => null; // In-process, no external connection

        public string GetPromptDescription()
        {
            return "Query: User is asking RAINA to recall information, search through memory, or provide stored knowledge";
        }

        public string[] GetPromptExamples()
        {
            return new[]
            {
                "What did I say about the Johnson project last week?",
                "When was my last meeting with Sarah?",
                "Do you remember the website I bookmarked about machine learning?",
                "What was that restaurant we talked about for the team dinner?"
            };
        }

        public string GetToolsPromptSection()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Tools:");
            sb.AppendLine("  - recall: Request information retrieval from memory system");
            sb.AppendLine("    Parameters: keywords (string), timeframe (string), maxResults (string)");
            sb.AppendLine("  - fetch: Request information from an internet search");
            sb.AppendLine("    Parameters: query (string), source (string)");
            sb.AppendLine("  - find: Locate documents or files related to the query");
            sb.AppendLine("    Parameters: searchTerm (string), fileType (string)");
            sb.AppendLine("Expected Entities: topic, person, timeframe, location");
            sb.AppendLine("Expected Parameters: recency, specificity, importance");
            return sb.ToString();
        }

        public Dictionary<string, string> ExtractArgumentsFromContext(UserContext context, Intent intent)
        {
            var args = new Dictionary<string, string>();

            // Add recent conversation context
            if (context.CurrentTopic != null)
            {
                args["currentTopic"] = context.CurrentTopic;
            }

            // Add conversation history for context
            var recentUtterances = context.GetRecentUtterances(5);
            if (recentUtterances.Any())
            {
                args["conversationHistory"] = context.GetConversationContext(5);
            }

            // Add user information
            if (!string.IsNullOrEmpty(context.UserName))
            {
                args["userName"] = context.UserName;
            }

            return args;
        }

        public async Task<IList<McpTool>> ListToolsAsync()
        {
            return new List<McpTool>
            {
                new McpTool
                {
                    Name = "recall",
                    Description = "Request information retrieval from memory system",
                    InputSchema = new
                    {
                        type = "object",
                        properties = new
                        {
                            keywords = new { type = "string", description = "Keywords to search for" },
                            timeframe = new { type = "string", description = "Time period to search within" },
                            maxResults = new { type = "string", description = "Maximum number of results" }
                        },
                        required = new[] { "keywords" }
                    }
                },
                new McpTool
                {
                    Name = "fetch",
                    Description = "Request information from an internet search",
                    InputSchema = new
                    {
                        type = "object",
                        properties = new
                        {
                            query = new { type = "string", description = "Search query" },
                            source = new { type = "string", description = "Search source (optional)" }
                        },
                        required = new[] { "query" }
                    }
                },
                new McpTool
                {
                    Name = "find",
                    Description = "Locate documents or files related to the query",
                    InputSchema = new
                    {
                        type = "object",
                        properties = new
                        {
                            searchTerm = new { type = "string", description = "Term to search for" },
                            fileType = new { type = "string", description = "Type of file to find (optional)" }
                        },
                        required = new[] { "searchTerm" }
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
                    case "recall":
                        return await HandleRecallAsync(arguments);

                    case "fetch":
                        return await HandleFetchAsync(arguments);

                    case "find":
                        return await HandleFindAsync(arguments);

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

        private async Task<McpToolResult> HandleRecallAsync(Dictionary<string, string> arguments)
        {
            var keywords = arguments.GetValueOrDefault("keywords", "");
            var keywordList = keywords.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();

            var queryParameters = new QueryParameters
            {
                Keywords = keywordList,
                RelevanceThreshold = 0.7,
                MaxResults = int.TryParse(arguments.GetValueOrDefault("maxResults", "5"), out var max) ? max : 5
            };

            // TODO: Need to get UserContext here - might need to pass through or store
            // For now, return placeholder
            var results = await _queryEngine.SearchAsync(queryParameters, null);

            return new McpToolResult
            {
                ToolName = "recall",
                Content = JsonSerializer.Serialize(results),
                IsError = false
            };
        }

        private async Task<McpToolResult> HandleFetchAsync(Dictionary<string, string> arguments)
        {
            // TODO: Implement internet search
            return new McpToolResult
            {
                ToolName = "fetch",
                Content = "Internet search not yet implemented",
                IsError = false
            };
        }

        private async Task<McpToolResult> HandleFindAsync(Dictionary<string, string> arguments)
        {
            // TODO: Implement file finding
            return new McpToolResult
            {
                ToolName = "find",
                Content = "File finding not yet implemented",
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
            throw new NotImplementedException("Query module does not expose resources");
        }
    }
}