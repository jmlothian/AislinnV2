using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Aislinn.Core.Models;
using Aislinn.Configuration;
using RAINA.Modules;
using RAINA.Services;
using RAINA.Events;

namespace RAINA
{
    public class IntentProcessor
    {
        private readonly HttpClient _httpClient;
        private readonly string _openAIApiKey;
        private readonly ConversationManager _conversationManager;
        private readonly ContextDetector _contextDetector;
        public static event EventHandler<IntentClassifiedEventArgs> IntentClassified;


        private readonly Dictionary<string, IIntentModule> _modules = new Dictionary<string, IIntentModule>();

        private readonly McpServerManager _mcpServerManager;
        public IntentProcessor(
            RainaServices rainaServices,
            ContextDetector contextDetector,
            AislinnConfiguration config)
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {config.OpenAIApiKey}");

            _conversationManager = rainaServices.ConversationManager;
            _contextDetector = contextDetector ?? throw new ArgumentNullException(nameof(contextDetector));
        }

        /// <summary>
        /// Register an intent module
        /// </summary>
        public void RegisterModule(IIntentModule module)
        {
            if (module == null) throw new ArgumentNullException(nameof(module));

            string intentType = module.GetServerName();
            _modules[intentType] = module;
        }

        /// <summary>
        /// Get all registered modules
        /// </summary>
        public IEnumerable<IIntentModule> GetRegisteredModules()
        {
            return _modules.Values;
        }

        /// <summary>
        /// Main entry point for processing user input
        /// </summary>
        public async Task<Response> ProcessInputAsync(string userInput, UserContext context)
        {
            // Classify intent using OpenAI
            var intent = await ClassifyIntentAsync(userInput, context);
            //return new Response() { Message = "Processing intent: " + intent.IntentType };
            OnIntentClassified(userInput, intent, context);


            //add input to conversation manager....

            // Update context based on input... should probably do this
            // not sure we need a separate class for this, it kind of fits in here.

            // revisit context - do we pull out more entities? or leave that to intents?
            // update conversation context (utterances, summaries, etc.)
            // newly created summaries should have lower activation than utterances
            // update topic
            // update intent
            // activate/prime memories (entities, tasks, etc.)
            // load relevant chunks (e.g. memories, tasks, etc.) into context
            // we should build a "Context" text block via LLM that takes the highest activated items and summariezes them.  For Entities, we should look for where they're
            // associated with other conversation memories or highly activated items, and include those.
            // Context:
            //  Recent Conversation Hisory
            //  Current Topic
            //  Current Intent
            //  Relevant Entities
            //    relevant summaries
            //        //relevant utterances?

            //handle intent below, should consider context in generating response or adding tasks


            await _contextDetector.UpdateContextAsync(userInput, intent);


            // Route to appropriate module
            Console.WriteLine("Intent: " + intent.IntentType);
            // Get the server/module for this intent
            string serverName = intent.IntentType;

            // Call the appropriate tool if one was specified
            if (!string.IsNullOrEmpty(intent.ToolName))
            {
                // Convert intent parameters and entities to arguments
                var baseArguments = ConvertParametersToArguments(intent);

                // Call MCP server with context extraction
                var toolResult = await _mcpServerManager.CallToolAsync(
                    serverName,
                    intent.ToolName,
                    baseArguments,
                    context,
                    intent
                );

                if (toolResult.IsError)
                {
                    return new Response
                    {
                        Message = $"Error: {toolResult.Content}",
                        Success = false,
                        //Error = toolResult.Content
                    };
                }

                return new Response
                {
                    Message = toolResult.Content,
                    Success = true
                };
            }
            // Default to conversation handler if no specific module is registered
            return await _conversationManager.GenerateResponseAsync(userInput, intent, context);
        }

        /// <summary>
        /// Classify intent using OpenAI API with dynamically constructed prompt based on modules
        /// </summary>
        private async Task<Intent> ClassifyIntentAsync(string userInput, UserContext context)
        {
            // Build the prompt dynamically based on registered modules
            string intentTypesSection = BuildIntentTypesSection();
            string examplesSection = BuildExamplesSection();

            string prompt = $@"
You are an intent classifier for RAINA (Realtime Adaptive Intelligence Neural Assistant).
Analyze the following user input and classify it into one of these intent types:

{intentTypesSection}

{examplesSection}

Where possible, try to include parameters and entities relevant to the intent.

Current context: {context.CurrentTopic ?? "None"}, Time: {DateTime.Now}

Provide your response in JSON format:
{{
  ""intentType"": ""[intent type]"",
  ""toolName"": ""[specific tool if applicable]"",
  ""confidence"": 0.95,
  ""entities"": [
    {{ ""type"": ""person"", ""value"": ""John Smith"" }},
    {{ ""type"": ""date"", ""value"": ""tomorrow"" }}
  ],
  ""parameters"": {{ 
    ""urgency"": ""high"",
    ""action"": ""schedule"" 
  }}
}}

User input: ""{userInput}""
";
            Console.WriteLine(prompt);
            var response = await CallOpenAIAsync(prompt, 0.1);
            Console.WriteLine(response.Choices[0].Message.Content);
            try
            {
                return JsonSerializer.Deserialize<Intent>(response.Choices[0].Message.Content);
            }
            catch (JsonException ex)
            {
                // Fallback if JSON parsing fails
                Console.WriteLine($"JSON parsing error: {ex.Message}. Fallback to default intent classification.");
                Console.WriteLine(response.Choices[0].Message.Content);

                return new Intent { IntentType = "Conversation", Confidence = 0.5 };
            }
        }
        /// <summary>
        /// Convert intent parameters and entities to tool arguments
        /// </summary>
        private Dictionary<string, string> ConvertParametersToArguments(Intent intent)
        {
            var args = new Dictionary<string, string>();

            // Add explicit parameters from intent
            foreach (var param in intent.Parameters)
            {
                args[param.Key] = param.Value;
            }

            // Add entities as parameters (entity type as key, entity value as value)
            foreach (var entity in intent.Entities)
            {
                args[entity.Type] = entity.Name;
            }

            return args;
        }
        /// <summary>
        /// Build the intent types section of the prompt based on registered modules
        /// </summary>
        private string BuildIntentTypesSection()
        {
            if (!_modules.Any())
            {
                return "- Conversation: General conversation or chat";
            }

            var intentDescriptions = _modules.Values.Select(module =>
                $"- {module.GetPromptDescription()}\n{module.GetToolsPromptSection()}"
            );

            return string.Join("\n\n", intentDescriptions) + "\n\n- Conversation: General conversation or chat";
        }

        /// <summary>
        /// Build the examples section of the prompt based on registered modules
        /// </summary>
        private string BuildExamplesSection()
        {
            if (!_modules.Any())
            {
                return "";
            }

            var examplesByType = new StringBuilder("Examples:\n");

            foreach (var module in _modules.Values)
            {
                var examples = module.GetPromptExamples();
                if (examples.Length > 0)
                {
                    examplesByType.AppendLine($"{module.GetServerName()} examples:");
                    foreach (var example in examples.Take(2)) // Limit to 2 examples per type
                    {
                        examplesByType.AppendLine($"- \"{example}\"");
                    }
                    examplesByType.AppendLine();
                }
            }

            return examplesByType.ToString();
        }

        /// <summary>
        /// Call OpenAI API
        /// </summary>
        private async Task<OpenAIResponse> CallOpenAIAsync(string prompt, double temperature = 0.7)
        {
            var requestBody = new
            {
                model = "gpt-4o",
                messages = new[]
                {
                    new { role = "system", content = "You are a helpful assistant specialized in intent classification. Your name is Raina. Your pronouns are she/her." },
                    new { role = "user", content = prompt }
                },
                temperature = temperature,
                max_tokens = 500,
                response_format = new
                {
                    type = "json_object"
                }
            };

            var content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<OpenAIResponse>(responseString);
        }
        private void OnIntentClassified(string userInput, Intent intent, UserContext context)
        {
            IntentClassified?.Invoke(this, new IntentClassifiedEventArgs
            {
                UserInput = userInput,
                Intent = intent,
                Context = context
            });
        }


    }

    /// <summary>
    /// Intent classification result with MCP tool information
    /// </summary>
    public class Intent
    {
        [JsonPropertyName("intentType")]
        public string IntentType { get; set; } // MCP server name

        [JsonPropertyName("toolName")]
        public string ToolName { get; set; } // Specific tool to call

        [JsonPropertyName("confidence")]
        public double Confidence { get; set; }

        [JsonPropertyName("entities")]
        public List<Entity> Entities { get; set; } = new List<Entity>();

        [JsonPropertyName("parameters")]
        public Dictionary<string, string> Parameters { get; set; } = new Dictionary<string, string>();
    }

    // public class Response
    // {
    //     public string Message { get; set; }
    //     public List<Chunk> RelevantChunks { get; set; } = new List<Chunk>();
    //     public bool Success { get; set; } = true;
    //     public string Error { get; set; }
    // }

    public class QueryParameters
    {
        public List<string> Keywords { get; set; } = new List<string>();
        public double RelevanceThreshold { get; set; } = 0.5;
        public int MaxResults { get; set; } = 10;
    }

    public class RainaTask
    {
        public string Title { get; set; }
        public DateTime? DueDate { get; set; }
        public string Priority { get; set; }
        public string Status { get; set; } = "Not Started";
        public string Category { get; set; }
    }

    public class OpenAIResponse
    {
        [JsonPropertyName("choices")]
        public List<Choice> Choices { get; set; } = new List<Choice>();
    }

    public class Choice
    {
        [JsonPropertyName("message")]
        public Message Message { get; set; }
    }

    public class Message
    {
        [JsonPropertyName("content")]
        public string Content { get; set; }
    }
}