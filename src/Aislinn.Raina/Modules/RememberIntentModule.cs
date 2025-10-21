using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Aislinn.Core;
using Aislinn.Core.Models;
using RAINA.Modules;
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
    /// module for handling information retrieval queries
    /// </summary>
    public class RememberIntentModule : IIntentModule
    {
        private readonly QueryEngine _queryEngine;
        private readonly ConversationManager _conversationManager;
        private readonly PromptLibrary _promptLibrary;
        private LLMApiService _llmApiService;
        PromptLibrary promptLibrary = new PromptLibrary();
        private readonly AislinnCoreServices _coreServices;
        private readonly MemoryChunkConverter _memoryChunkConverter;
        public RememberIntentModule(QueryEngine queryEngine, ConversationManager conversationManager, LLMApiService lLMApiService, AislinnCoreServices coreServices, MemoryChunkConverter memoryChunkConverter)
        {
            _queryEngine = queryEngine ?? throw new ArgumentNullException(nameof(queryEngine));
            _conversationManager = conversationManager ?? throw new ArgumentNullException(nameof(conversationManager));
            _promptLibrary = new PromptLibrary();
            _llmApiService = lLMApiService ?? throw new ArgumentNullException(nameof(lLMApiService));
            _coreServices = coreServices ?? throw new ArgumentNullException(nameof(coreServices));
            _memoryChunkConverter = memoryChunkConverter ?? throw new ArgumentNullException(nameof(memoryChunkConverter));
        }

        public string GetIntentType()
        {
            return "Remember";
        }

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

        public string[] GetExpectedEntities()
        {
            return new[]
            {
                "people",
                "places",
                "activities",
                "objects",
                "events",
                "relationships",
                "constraints",
                "dates",
                "times",
                "likes",
                "dislikes",
                "loves",
                "hates"
            };
        }

        public string[] GetExpectedParameters()
        {
            return new[]
            {
                "context",
                "specificity",
                "importance"
            };
        }

        public async Task<Response> HandleAsync(string userInput, Intent intent, UserContext context)
        {
            //todo - scan memories for entity names - we need to know how important they are to determine
            // what to store about them.

            //todo - use the previously extracted entities - possibly seed prompt and say "heres what we know so far, any others?"

            //Run memory extraction prompt
            var details = new Dictionary<string, object>()
            {
                ["dateTime"] = DateTime.Now.ToString("F"),
                ["agentName"] = "Raina",
                ["userName"] = context.UserName,
                ["userInput"] = userInput
            };
            var systemprompt = _promptLibrary.HydratePrompt("memory.extract", details);
            var userprompt = _promptLibrary.HydratePrompt("memory.extract.userprompt", details);
            var extrated = await _llmApiService.CallLLM(systemprompt, userprompt, "Raina", true);
            var data = JsonSerializer.Deserialize<MemoryExtractionResponse>(extrated);
            //store memories
            (List<Chunk> chunks, List<ChunkAssociation> associations) = await _memoryChunkConverter.ConvertMemoryExtractionToChunks(data);
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
                    "RememberIntentModule.HandleAsync",
                    null,
                    0.005
                );
            }
            context.CurrentIntentActivityMessage = "The requested or important memories were stored.";
            // Generate response using conversation manager

            //return await _conversationManager.GenerateQueryResponseAsync(userInput, intent, queryResults, context);
            return await _conversationManager.GenerateResponseAsync(userInput, intent, context);
        }

        private List<string> ExtractKeywords(Intent intent)
        {
            var keywords = new List<string>();
            foreach (var entity in intent.Entities)
            {
                keywords.Add(entity.Name);
            }
            return keywords;
        }
    }
}