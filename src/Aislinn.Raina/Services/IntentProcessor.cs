using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Aislinn.Core.Models;
using RAINA.Modules;
using RAINA.Services;

namespace RAINA
{
    public class IntentProcessor
    {
        private readonly HttpClient _httpClient;
        private readonly string _openAIApiKey;
        private readonly ConversationManager _conversationManager;
        private readonly ContextDetector _contextDetector;

        private readonly Dictionary<string, IIntentModule> _modules = new Dictionary<string, IIntentModule>();

        public IntentProcessor(
            string openAIApiKey,
            ConversationManager conversationManager,
            ContextDetector contextDetector)
        {
            _openAIApiKey = openAIApiKey ?? throw new ArgumentNullException(nameof(openAIApiKey));
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_openAIApiKey}");

            _conversationManager = conversationManager ?? throw new ArgumentNullException(nameof(conversationManager));
            _contextDetector = contextDetector ?? throw new ArgumentNullException(nameof(contextDetector));
        }

        /// <summary>
        /// Register an intent module
        /// </summary>
        public void RegisterModule(IIntentModule module)
        {
            if (module == null) throw new ArgumentNullException(nameof(module));

            string intentType = module.GetIntentType();
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

            // Update context based on input
            await _contextDetector.UpdateContextAsync(userInput, intent);

            // Route to appropriate module
            Console.WriteLine("Intent: " + intent.IntentType);
            if (_modules.TryGetValue(intent.IntentType, out var module))
            {
                return await module.HandleAsync(userInput, intent, context);
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

Current context: {context.CurrentTopic ?? "None"}, Time: {DateTime.Now}

User input: ""{userInput}""

Provide your response in JSON format:
{{
  ""intentType"": ""[intent type]"",
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
        /// Build the intent types section of the prompt based on registered modules
        /// </summary>
        private string BuildIntentTypesSection()
        {
            if (!_modules.Any())
            {
                return "- Conversation: General conversation or chat";
            }

            var intentDescriptions = _modules.Values.Select(p => $"- {p.GetPromptDescription()}");
            return string.Join("\n", intentDescriptions) + "\n- Conversation: General conversation or chat";
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
                    examplesByType.AppendLine($"{module.GetIntentType()} examples:");
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
    }

    // Helper classes
    public class Intent
    {
        [JsonPropertyName("intentType")]
        public string IntentType { get; set; }
        [JsonPropertyName("confidence")]
        public double Confidence { get; set; }
        [JsonPropertyName("entities")]
        public List<Entity> Entities { get; set; } = new List<Entity>();
        [JsonPropertyName("parameters")]
        public Dictionary<string, string> Parameters { get; set; } = new Dictionary<string, string>();
    }

    public class Entity
    {
        [JsonPropertyName("type")]
        public string EntityType { get; set; }
        [JsonPropertyName("value")]
        public string Value { get; set; }
    }

    public class UserContext
    {
        public string UserId { get; set; }
        public string CurrentTopic { get; set; }
        public List<Chunk> ActiveMemoryChunks { get; set; } = new List<Chunk>();
        public Dictionary<string, object> ContextVariables { get; set; } = new Dictionary<string, object>();
    }

    public class Response
    {
        public string Message { get; set; }
        public List<Chunk> RelevantChunks { get; set; } = new List<Chunk>();
        public bool Success { get; set; } = true;
        public string Error { get; set; }
    }

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