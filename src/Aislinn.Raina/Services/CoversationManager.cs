using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Aislinn.ChunkStorage;
using Aislinn.Core;
using Aislinn.Core.Cognitive;
using Aislinn.Core.Context;
using Aislinn.Core.Models;
using Aislinn.Core.Query;
using Aislinn.Core.Services;
using Aislinn.VectorStorage.Interfaces;
using Aislinn.VectorStorage.Models;
using Aislinn.Configuration;
using static Aislinn.Core.Context.ContextContainer;

namespace RAINA.Services;

public class LLMContextResponse
{
    public ContextCategoryFactors Environment { get; set; }
    public ContextCategoryFactors Social { get; set; }
    public ContextCategoryFactors Task { get; set; }
    public ContextCategoryFactors Internal { get; set; }
    public ContextCategoryFactors Temporal { get; set; }
    public ContextCategoryFactors Resource { get; set; }
    public ContextCategoryFactors Communication { get; set; }
    public ContextCategoryFactors Information { get; set; }

}

public class ContextCategoryFactors
{
    public List<ContextFactorData> Factors { get; set; } = new List<ContextFactorData>();
}

public class ContextFactorData
{
    public string Name { get; set; }
    public string Value { get; set; }
    public string Importance { get; set; }
    public string Confidence { get; set; }
}
public class ConversationManager
{
    private readonly HttpClient _httpClient;

    private readonly ContextContainer _contextContainer;

    private readonly CognitiveMemorySystem _memorySystem;
    private readonly ChunkQueryService _chunkQueryService;
    private readonly string _openAIApiKey;

    // Track the current conversation as a Chunk
    private Chunk _currentConversationChunk;

    // Store recent utterance chunks for quick access
    private List<Chunk> _recentUtterances = new List<Chunk>();

    // Maximum number of recent utterances to keep in memory
    private const int MaxRecentUtterances = 10;

    private SummaryService summaryService = new SummaryService();
    private PromptLibrary promptLibrary = new PromptLibrary();
    private EntityRelationshipExtractionService _entityRelationshipExtraction;
    private EntityInstanceManager _entityManager;
    public IVectorCollection _vectorCollection { get; }

    private readonly RainaConfiguration _rainaConfig;
    /// <summary>
    /// Default agent name for the system
    /// </summary>
    private string _agentName = "Raina";

    public ConversationManager(
        AislinnCoreServices coreServices,
        EntityInstanceManager entityManager,
        EntityRelationshipExtractionService entityExtractionService,
        RainaConfiguration config,
        IVectorCollection vectorCollection
)
    {
        _memorySystem = coreServices.MemorySystem;
        _chunkQueryService = coreServices.QueryService;
        _openAIApiKey = config.OpenAIApiKey;
        _agentName = config.AgentName ?? _agentName;
        _contextContainer = coreServices.ContextContainer;
        _httpClient = new HttpClient();
        _entityRelationshipExtraction = entityExtractionService;
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_openAIApiKey}");
        _entityManager = entityManager;
        _vectorCollection = vectorCollection;
        _rainaConfig = config;

    }
    public void Shutdown()
    {
        summaryService.SaveToJson(_agentName + ".json");
    }
    public void Init()
    {
        summaryService.LoadFromJson(_agentName + ".json");
    }
    // Method to initialize or retrieve an existing conversation
    public async Task<Chunk> InitializeConversationAsync(UserContext context, string conversationId = null)
    {
        if (string.IsNullOrEmpty(conversationId))
        {
            // Create a new conversation chunk
            var conversationChunk = new Chunk
            {
                ChunkType = "Declarative",
                SemanticType = "Conversation",
                Name = $"Conversation_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}",
                Slots = new Dictionary<string, ModelSlot>
                {
                    { "SpeakerName", new ModelSlot { Name = "Speaker", Value = context.UserName } },
                    { "ListenerName", new ModelSlot { Name = "Listener", Value = _agentName } },
                    { "Speaker", new ModelSlot { Name = "Speaker", Value = context.UserChunk } },
                    { "Listener", new ModelSlot { Name = "Listener", Value = context.RainaChunk } },
                    { "Entities", new ModelSlot { Name = "Entities", Value = new List<Entity>() { new Entity { Type = "entity.person.instance", Name=context.UserName } } } }
                }
            };


            // Add to memory system
            _currentConversationChunk = await _memorySystem.AddChunkAsync(conversationChunk);

            // re-initialize summary service
            //summaryService.Initialize(_currentConversationChunk.ID);


            //for any entity, we need to add an assocation to the conversation chunk, same with utterances (also, activate)
            //_memorySystem.FindSimilarChunksAsync("chunktype == cognitive && SemanticType=='entity.person' && slot['name'] == username");
        }
        else
        {
            // Retrieve existing conversation
            _currentConversationChunk = await _memorySystem.GetChunkAsync(Guid.Parse(conversationId));

            // Load recent utterances
            await LoadRecentUtterancesAsync(_currentConversationChunk.ID);
        }

        return _currentConversationChunk;
    }

    // Record user input as a chunk and link to conversation
    public async Task<(Chunk chunk, ExtractionResult extractionResult)> RecordUserInputAsync(string userInput, Intent intent, UserContext context)
    {
        // Ensure we have an active conversation
        if (_currentConversationChunk == null)
        {
            await InitializeConversationAsync(context);
        }

        // Create utterance chunk for user input
        var utteranceChunk = new Chunk
        {
            ChunkType = "Declarative",
            SemanticType = "Utterance",
            Name = $"UserUtterance_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}",
            Slots = new Dictionary<string, ModelSlot>
            {
                { "SpeakerName", new ModelSlot { Name = "Speaker", Value = context.UserName } },
                { "ListenerName", new ModelSlot { Name = "Listener", Value = _agentName } },
                { "Speaker", new ModelSlot { Name = "Speaker", Value = context.UserChunk } },
                { "Listener", new ModelSlot { Name = "Listener", Value = context.RainaChunk } },
                { "Text", new ModelSlot { Name = "Text", Value = userInput } },
                { "Intent", new ModelSlot { Name = "Intent", Value = intent?.IntentType } },
                { "ConversationId", new ModelSlot { Name = "ConversationId", Value = _currentConversationChunk.ID } }
            },

        };
        var extractionResult = await _entityRelationshipExtraction.ExtractEntitiesAndRelationshipsAsync(userInput);
        // Process entities and attach to utterance
        await _entityManager.AttachEntitiesToUtteranceAsync(utteranceChunk, extractionResult.Entities);

        // Handle special person entity processing (for speaker/listener slots)
        await _entityManager.ProcessPersonEntitiesAsync(utteranceChunk, extractionResult.Entities);

        // Create relationship associations between entities
        await _entityManager.CreateRelationshipAssociationsAsync(extractionResult.Relationships, extractionResult.Entities);


        // Add metadata based on intent if available
        if (intent != null)
        {
            utteranceChunk.Slots["IntentConfidence"] = new ModelSlot { Name = "IntentConfidence", Value = intent.Confidence };

            if (intent.Entities != null && intent.Entities.Any())
            {
                //consider adding the speaker by default as well, we have this on the conversation chunk
                utteranceChunk.Slots["Entities"] = new ModelSlot { Name = "Entities", Value = intent.Entities };
            }
        }

        //create vector
        var vectorText = $"[{DateTime.Now.ToString("F")}] {context.UserName}: {userInput}";
        var vectorMeta = new Dictionary<string, string>()
        {
            {"DataType", "Utterance"},
            {"Intent", intent.IntentType },
            {"ConversationID", _currentConversationChunk.ID.ToString() },
            {"SpeakerID", context.UserChunk.ID.ToString() },
            {"ListenerID", context.RainaChunk.ID.ToString() }
        };
        utteranceChunk.Vector = (await _vectorCollection.AddVectorAsync(vectorText, utteranceChunk.ID.ToString(), vectorMeta)).Vector;

        // Add to memory system
        utteranceChunk = await _memorySystem.AddChunkAsync(utteranceChunk);
        summaryService.AddItem($"[{DateTime.Now.ToString("F")}] {context.UserName}: " + userInput, 0, utteranceChunk.ID);
        // Increment utterance count in conversation
        int utteranceCount = _currentConversationChunk.Slots.ContainsKey("UtteranceCount") ? (int)(_currentConversationChunk.Slots["UtteranceCount"].Value ?? 0) : 0;
        _currentConversationChunk.Slots["UtteranceCount"] = new ModelSlot { Name = "UtteranceCount", Value = utteranceCount + 1 };
        await _memorySystem.UpdateChunkAsync(_currentConversationChunk);

        // Create association between conversation and utterance
        await _memorySystem.CreateAssociationAsync(
            _currentConversationChunk.ID,
            utteranceChunk.ID,
            "Contains",
            "PartOf",
            0.9,
            0.9
        );

        // Add to recent utterances list
        _recentUtterances.Add(utteranceChunk);
        if (_recentUtterances.Count > MaxRecentUtterances)
        {
            _recentUtterances.RemoveAt(0);
        }

        // Update context with this utterance
        context.AddUtterance(utteranceChunk);
        context.CurrentUtterance = utteranceChunk;

        // Activate the utterance chunk in memory
        await _memorySystem.ActivateChunkAsync(utteranceChunk.ID);

        return (utteranceChunk, extractionResult);
    }
    public async Task UpdateContextFromLLMResponse(string llmResponseJson)
    {
        try
        {
            var response = JsonSerializer.Deserialize<LLMContextResponse>(llmResponseJson);

            await ProcessCategoryFactors(ContextCategory.Environment, response.Environment);
            await ProcessCategoryFactors(ContextCategory.Social, response.Social);
            await ProcessCategoryFactors(ContextCategory.Task, response.Task);
            await ProcessCategoryFactors(ContextCategory.Internal, response.Internal);
            await ProcessCategoryFactors(ContextCategory.Temporal, response.Temporal);
            await ProcessCategoryFactors(ContextCategory.Resource, response.Resource);
            await ProcessCategoryFactors(ContextCategory.Communication, response.Communication);
            await ProcessCategoryFactors(ContextCategory.Information, response.Information);

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating context: {ex.Message}");
        }
    }

    private async Task ProcessCategoryFactors(ContextCategory category, ContextCategoryFactors categoryFactors)
    {
        if (categoryFactors?.Factors == null) return;

        foreach (var factor in categoryFactors.Factors)
        {
            // Convert category to chunk type using reverse of CategorizeChunk logic
            string semanticType = "Context." + GetChunkTypeFromCategory(category) + "." + factor.Name;

            // Search for existing chunk
            var existingChunk = await this._memorySystem.FindChunkBySemanticTypeAndName(semanticType, factor.Name);

            if (existingChunk != null)
            {
                // Update existing chunk with new value
                existingChunk.Slots["Value"] = new ModelSlot { Name = "Value", Value = factor.Value };
                existingChunk.Slots["Confidence"] = new ModelSlot { Name = "Confidence", Value = factor.Confidence };
                existingChunk.Slots["LastUpdated"] = new ModelSlot { Name = "LastUpdated", Value = DateTime.Now };

                await _memorySystem.UpdateChunkAsync(existingChunk);

                // Activate it to bring into working memory
                await _memorySystem.ActivateChunkAsync(existingChunk.ID, null, 0.8);
                //manually import into context...
                _contextContainer.AddContextChunk(category, existingChunk.ID);
                _contextContainer.ExtractContextFactorsFromChunk(category, existingChunk);
            }
            else
            {
                // Create new context chunk
                var newChunk = new Chunk
                {
                    ChunkType = semanticType,
                    Name = factor.Name,
                    Slots = new Dictionary<string, ModelSlot>
                {
                    { "Value", new ModelSlot { Name = "Value", Value = factor.Value } },
                    { "Confidence", new ModelSlot { Name = "Confidence", Value = factor.Confidence } },
                    { "Importance", new ModelSlot { Name = "Importance", Value = factor.Importance } },
                    { "Category", new ModelSlot { Name = "Category", Value = category.ToString() } },
                    { "ExtractedFromLLM", new ModelSlot { Name = "ExtractedFromLLM", Value = true } },
                    { "CreatedTimestamp", new ModelSlot { Name = "CreatedTimestamp", Value = DateTime.Now } },
                    { "LastUpdated",  new ModelSlot { Name = "LastUpdated", Value = DateTime.Now } }
                }
                };

                // Add to memory system
                var savedChunk = await _memorySystem.AddChunkAsync(newChunk);

                // Activate it to bring into working memory
                await _memorySystem.ActivateChunkAsync(savedChunk.ID, null, 0.8);
                //manually import into context...
                _contextContainer.AddContextChunk(category, savedChunk.ID);
                _contextContainer.ExtractContextFactorsFromChunk(category, savedChunk);
            }
        }
    }
    private string GetChunkTypeFromCategory(ContextCategory category)
    {
        // Reverse the logic from CategorizeChunk()
        return category switch
        {
            ContextCategory.Environment => "Environment",
            ContextCategory.Internal => "Emotion", // or "Internal"
            ContextCategory.Social => "Social",
            ContextCategory.Task => "Task",
            ContextCategory.Temporal => "Temporal",
            ContextCategory.Resource => "Resource",
            ContextCategory.Communication => "Communication",
            ContextCategory.Information => "Information",
            _ => "Context"
        };
    }


    private async Task<string> GenerateContextualResponse(string userInput, Intent intent)
    {

        return "";
    }
    private async Task<OpenAIResponse> CallOpenAIAsync(string systemprompt, string prompt, bool json, double temperature = 0.7)
    {
        StringContent content;
        if (json)
        {
            var requestBody = new
            {
                model = "gpt-4o",
                messages = new[]
                {
                    new { role = "system", content = systemprompt },
                    new { role = "user", content = prompt }
                },
                temperature = temperature,
                max_tokens = 6000,
                response_format = new
                {
                    type = "json_object"
                }
            };

            content = new StringContent(
               JsonSerializer.Serialize(requestBody),
               Encoding.UTF8,
               "application/json");
        }
        else
        {
            var requestBody = new
            {
                model = "gpt-4o",
                messages = new[]
                {
                    new { role = "system", content = systemprompt },
                    new { role = "user", content = prompt }
                },
                temperature = temperature,
                max_tokens = 6000
            };

            content = new StringContent(
               JsonSerializer.Serialize(requestBody),
               Encoding.UTF8,
               "application/json");
        }
        var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
        response.EnsureSuccessStatusCode();

        var responseString = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<OpenAIResponse>(responseString);
    }
    private async Task<string> GenerateContextualResponse(string userInput, string conversationHistoryText, string contextSummary, Intent intent, List<Chunk> workingMemoryChunks)
    {
        // Format intent information
        var intentType = intent?.IntentType ?? "Unknown";
        var intentConfidence = intent?.Confidence.ToString("P1") ?? "Unknown";

        // Build working memory summary
        var workingMemoryItems = string.Join("\n", workingMemoryChunks
            .Where(c => c.ChunkType != "ContextSummary") // Exclude the context summary we already show
            .Select(c => $"- {c.ChunkType}: {c.Name}"));

        // Get user profile info
        // var userProfile = context.UserChunk != null ?
        //     $"Name: {context.UserName}" :
        //     "No user profile available.";

        // Build the full prompt
        var prompt = promptLibrary.HydratePrompt("response.contextual", new Dictionary<string, object>
        {
            ["recentConversation"] = conversationHistoryText,
            ["contextSummary"] = contextSummary,
            ["intentType"] = intentType,
            ["intentConfidence"] = intentConfidence,
            ["workingMemoryItems"] = workingMemoryItems,
            //["userProfile"] = userProfile,
            ["userInput"] = userInput
        });

        // Generate response
        var response = await CallOpenAIAsync(
            "You are Raina (she/her), an intelligent conversational AI. Generate a natural, contextually appropriate response based on the conversation history, current context, and user input. Do your best to talk like a person.",
            prompt,
            false);

        return response.Choices[0].Message.Content;
    }
    // Generate and record a system response
    public async Task<Response> GenerateResponseAsync(string userInput, Intent intent, UserContext context)
    {
        // Record the user input first
        var (userUtterance, extractionResult) = await RecordUserInputAsync(userInput, intent, context);

        // update context from current chat state
        var conversationHistoryText = "";

        var contextUtterances = summaryService.GetContextForPrompt();
        var recentConversation = contextUtterances.Where(u => u.Depth == 0)
            .Select(u => new { text = u.Text, tokenCount = u.TokenCount });
        var summaries = contextUtterances.Where(u => u.Depth > 0)
            .Select(u => new { text = u.Text, tokenCount = u.TokenCount });
        if (summaries.Any())
        {
            conversationHistoryText += "## Background Summary\n";
            conversationHistoryText += string.Join("\n", summaries.Select(s => s.text));
            conversationHistoryText += "\n\n";
        }

        // Add recent conversation (immediate context)
        if (recentConversation.Any())
        {
            conversationHistoryText += "## Recent Conversation\n";
            conversationHistoryText += string.Join("\n", recentConversation.Select(c => c.text));
        }
        var input = new
        {
            recentConversation = recentConversation,
            summaries = summaries
        };


        //convert conversation and summarizes into context
        var summaryJson = JsonSerializer.Serialize(input, new JsonSerializerOptions { WriteIndented = true });
        var prompt = promptLibrary.HydratePrompt("context.extract", new Dictionary<string, object>() { ["summaryData"] = summaryJson, ["agentName"] = "Raina" });
        var resp = await CallOpenAIAsync("You are part of Raina (she/her), an intelligent conversational AI. You are a helpful assistant specialized in conversational context extraction for her. Please respond in first person as her.", prompt, true);
        //Console.WriteLine(prompt);
        Console.WriteLine(resp.Choices[0].Message.Content);
        await this.UpdateContextFromLLMResponse(resp.Choices[0].Message.Content);

        //convert context snapshot back into text
        var contextSnapshot = _contextContainer.CreateContextSnapshot();
        var snapshotJSON = JsonSerializer.Serialize(contextSnapshot, new JsonSerializerOptions { WriteIndented = true });
        prompt = promptLibrary.HydratePrompt("context.createcontextsummary", new Dictionary<string, object>() { ["contextSnapshot"] = snapshotJSON });
        resp = await CallOpenAIAsync("You are part of Raina (she/her), an intelligent conversational AI. You are a helpful assistant specialized in conversational context summarization for her. Please respond in first person as her.", prompt, false);
        //Console.WriteLine(prompt);
        Console.WriteLine(resp.Choices[0].Message.Content);
        var contextSummary = resp.Choices[0].Message.Content;
        var newChunk = new Chunk
        {
            ChunkType = "ContextSummary",
            Name = "Summary",
            Slots = new Dictionary<string, ModelSlot>
                {
                    { "Text", new ModelSlot { Name = "Value", Value = contextSummary } },
                    { "CreatedTimestamp", new ModelSlot { Name = "CreatedTimestamp", Value = DateTime.Now } }
                }
        };
        var savedChunk = await _memorySystem.AddChunkAsync(newChunk);
        await _memorySystem.CreateAssociationAsync(
            _currentConversationChunk.ID,
            savedChunk.ID,
            "Association",
            "Association",
            0.7,
            0.7
        );


        // NEW: Perform contextual vector searches and activate relevant chunks FIRST
        var searchBoosts = await PerformContextualVectorSearchAsync(userInput, extractionResult.Entities, contextSummary, context);
        if (searchBoosts.Any())
        {
            // Activate chunks found through vector search
            await _memorySystem.ActivateChunksAsync(searchBoosts, "contextual_search");
        }

        // THEN do manual refresh to bring relevant chunks into working memory
        // Focus on the new utterance and let spreading activation do its work
        await _memorySystem.ManualRefreshCycleAsync(new List<Guid> { userUtterance.ID, _currentConversationChunk.ID, context.UserChunk.ID });

        // Get top activated chunks and push some into working memory
        var topActivatedChunks = await _memorySystem.GetTopActivatedChunksAsync(10, excludeWorkingMemory: true);
        var chunkIdsToPush = topActivatedChunks.Take(5).Select(c => c.ID).ToList();

        if (chunkIdsToPush.Any())
        {
            await _memorySystem.PushChunksToWorkingMemoryAsync(chunkIdsToPush);
        }


        // Activate it to bring into working memory
        await _memorySystem.ActivateChunkAsync(savedChunk.ID, null, 0.8);

        // Manual refresh to bring relevant chunks into working memory
        // Focus on the new utterance and let spreading activation do its work
        await _memorySystem.ManualRefreshCycleAsync(new List<Guid> { userUtterance.ID, _currentConversationChunk.ID, context.UserChunk.ID });

        // NOW extract context from what's actively in working memory
        var workingMemoryChunks = await _memorySystem.GetWorkingMemoryContentsAsync();
        await _contextContainer.UpdateContextFromWorkingMemoryAsync(workingMemoryChunks);

        // Use context for response generation
        //You are Raina, an intelligent conversational AI. Generate a natural, contextually appropriate response based on the conversation history, current context, and user input.
        //resp = await CallOpenAIAsync("You are Raina (she/her), an intelligent conversational AI. Generate a natural, contextually appropriate response based on the conversation history, current context, and user input.",
        //prompt,
        //false);

        string responseText = await GenerateContextualResponse(userInput, conversationHistoryText, contextSummary, intent, workingMemoryChunks);
        // Generate response using LLM
        // This would call OpenAI or other LLM to generate a natural language response
        // For now, just create a simple response
        //string responseText = $"I understand you want to have a general conversation. You said: '{userInput}'";

        // Create utterance chunk for system response
        var currentTime = DateTime.Now;
        var strTimestamp = currentTime.ToString("yyyyMMdd_HHmmss");
        var responseChunk = new Chunk
        {
            ChunkType = "Declarative",
            SemanticType = "Utterance",
            Name = $"SystemUtterance_{strTimestamp}",
            Slots = new Dictionary<string, ModelSlot>
            {
                { "SpeakerName", new ModelSlot { Name = "SpeakerName", Value = _agentName } },
                { "Text", new ModelSlot { Name = "Text", Value = responseText } },
                { "ResponseToUtterance", new ModelSlot { Name = "ResponseToUtterance", Value = userUtterance.ID } },
                { "ListenerName", new ModelSlot { Name = "ListenerName", Value = context.UserName } },
                { "Intent", new ModelSlot { Name = "Intent", Value = intent?.IntentType } },
                { "Timestamp", new ModelSlot { Name = "Timestamp", Value = currentTime } },
                { "ConversationId", new ModelSlot { Name = "ConversationId", Value = _currentConversationChunk.ID } }
            }
        };
        var vectorText = $"[{DateTime.Now.ToString("F")}] {_agentName}: {userInput}";
        var vectorMeta = new Dictionary<string, string>()
        {
            {"DataType", "Utterance"},
            {"Intent", intent.IntentType },
            {"ConversationID", _currentConversationChunk.ID.ToString() },
            {"ListenerID", context.UserChunk.ID.ToString() },
            {"SpeakerID", context.RainaChunk.ID.ToString() }
        };
        responseChunk.Vector = (await _vectorCollection.AddVectorAsync(vectorText, responseChunk.ID.ToString(), vectorMeta)).Vector;

        // update cognitive time, 150ms for now
        _memorySystem._timeManager.AdvanceStep(150);

        // Add to memory system
        responseChunk = await _memorySystem.AddChunkAsync(responseChunk);

        // Increment utterance count in conversation
        int utteranceCount = (int)(_currentConversationChunk.Slots["UtteranceCount"].Value ?? 0);
        _currentConversationChunk.Slots["UtteranceCount"] = new ModelSlot { Name = "UtteranceCount", Value = utteranceCount + 1 };
        await _memorySystem.UpdateChunkAsync(_currentConversationChunk);

        // Create associations
        // Link to conversation
        await _memorySystem.CreateAssociationAsync(
            _currentConversationChunk.ID,
            responseChunk.ID,
            "Contains",
            "PartOf",
            0.9,
            0.9
        );

        // Link to user utterance (response relationship)
        await _memorySystem.CreateAssociationAsync(
            responseChunk.ID,
            userUtterance.ID,
            "ResponseTo",
            "HasResponse",
            0.9,
            0.9
        );

        // Add to recent utterances
        _recentUtterances.Add(responseChunk);
        if (_recentUtterances.Count > MaxRecentUtterances)
        {
            _recentUtterances.RemoveAt(0);
        }
        var strTimestamp2 = currentTime.ToString("F");
        summaryService.AddItem("[" + strTimestamp2 + "] " + responseChunk.Slots["SpeakerName"].Value + ": " + responseText);
        // Update context
        context.AddUtterance(responseChunk);
        context.LastSystemUtterance = responseChunk;

        // Activate the response chunk in memory
        await _memorySystem.ActivateChunkAsync(responseChunk.ID);


        // Return response object
        return new Response
        {
            Message = responseText,
            UtteranceChunk = responseChunk
        };
    }

    public async Task<Response> GenerateQueryResponseAsync(string userInput, Intent intent, List<Chunk> queryResults, UserContext context)
    {
        // Record the user input first
        var (userUtterance, _) = await RecordUserInputAsync(userInput, intent, context);

        // Generate response based on query results
        string responseText;
        if (queryResults == null || queryResults.Count == 0)
        {
            responseText = "I couldn't find any information about that in my memory.";
        }
        else
        {
            responseText = $"I found {queryResults.Count} items related to your query. Here's what I know...";
        }

        // Create utterance chunk for system response
        var responseChunk = new Chunk
        {
            ChunkType = "Utterance",
            Name = $"SystemUtterance_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}",
            Slots = new Dictionary<string, ModelSlot>
            {
                { "Speaker", new ModelSlot { Name = "Speaker", Value = "System" } },
                { "Text", new ModelSlot { Name = "Text", Value = responseText } },
                { "ResponseToUtterance", new ModelSlot { Name = "ResponseToUtterance", Value = userUtterance.ID } },
                { "QueryResponseType", new ModelSlot { Name = "QueryResponseType", Value = true } },
                { "Timestamp", new ModelSlot { Name = "Timestamp", Value = DateTime.Now } },
                { "ConversationId", new ModelSlot { Name = "ConversationId", Value = _currentConversationChunk.ID } }
            }
        };

        // Add to memory system
        responseChunk = await _memorySystem.AddChunkAsync(responseChunk);

        // Increment utterance count in conversation
        int utteranceCount = (int)(_currentConversationChunk.Slots["UtteranceCount"].Value ?? 0);
        _currentConversationChunk.Slots["UtteranceCount"] = new ModelSlot { Name = "UtteranceCount", Value = utteranceCount + 1 };
        await _memorySystem.UpdateChunkAsync(_currentConversationChunk);

        // Create associations
        // Link to conversation
        await _memorySystem.CreateAssociationAsync(
            _currentConversationChunk.ID,
            responseChunk.ID,
            "Contains",
            "PartOf",
            0.9,
            0.9
        );

        // Link to user utterance (response relationship)
        await _memorySystem.CreateAssociationAsync(
            responseChunk.ID,
            userUtterance.ID,
            "ResponseTo",
            "HasResponse",
            0.9,
            0.9
        );

        // Link each query result to the response
        if (queryResults != null)
        {
            foreach (var resultChunk in queryResults)
            {
                await _memorySystem.CreateAssociationAsync(
                    responseChunk.ID,
                    resultChunk.ID,
                    "References",
                    "ReferencedBy",
                    0.7,
                    0.5
                );
            }
        }

        // Add to recent utterances
        _recentUtterances.Add(responseChunk);
        if (_recentUtterances.Count > MaxRecentUtterances)
        {
            _recentUtterances.RemoveAt(0);
        }

        // Update context
        context.AddUtterance(responseChunk);
        context.LastSystemUtterance = responseChunk;

        // Activate the response chunk in memory
        await _memorySystem.ActivateChunkAsync(responseChunk.ID);

        // Return response object
        return new Response
        {
            Message = responseText,
            UtteranceChunk = responseChunk,
            RelevantChunks = queryResults
        };
    }

    // Helper method to load recent utterances for an existing conversation
    private async Task LoadRecentUtterancesAsync(Guid conversationId)
    {
        _recentUtterances.Clear();

        // This would use your association system to find utterances linked to this conversation
        // For now, a simplified approach
        var associationCollection = await _memorySystem.GetAssociationCollectionAsync();
        var associations = await associationCollection.GetAssociationsForChunkAsync(conversationId);

        // Find chunks that are part of this conversation
        var utteranceIds = associations
            .Where(a => a.RelationAtoB == "Contains" && a.ChunkAId == conversationId)
            .Select(a => a.ChunkBId)
            .ToList();

        // Load all the utterance chunks
        var utteranceChunks = new List<Chunk>();
        foreach (var id in utteranceIds)
        {
            var chunk = await _memorySystem.GetChunkAsync(id);
            if (chunk != null && chunk.Slots.ContainsKey("Timestamp") && chunk.Slots["Timestamp"].Value is DateTime)
            {
                utteranceChunks.Add(chunk);
            }
        }

        // Sort by timestamp and take most recent
        _recentUtterances = utteranceChunks
            .OrderByDescending(c => (DateTime)c.Slots["Timestamp"].Value)
            .Take(MaxRecentUtterances)
            .ToList();
    }

    // Get conversation summary
    public async Task<ConversationSummary> GetConversationSummaryAsync()
    {
        if (_currentConversationChunk == null)
            return null;

        // Prepare conversation summary
        var summary = new ConversationSummary
        {
            ConversationId = _currentConversationChunk.ID,
            ConversationChunk = _currentConversationChunk,
            StartTime = (DateTime)_currentConversationChunk.Slots["StartTime"].Value,
            UtteranceCount = (int)_currentConversationChunk.Slots["UtteranceCount"].Value,
            RecentUtterances = _recentUtterances
        };

        return summary;
    }


    private async Task<Dictionary<Guid, double>> PerformContextualVectorSearchAsync(
   string utteranceText,
   List<Entity> entities,
   string contextSummary,
   UserContext context)
    {
        var searchResults = new Dictionary<Guid, List<(double similarity, int rank, string searchType, double baseBoost)>>();

        // Configuration - these should eventually move to config
        var config = new
        {
            UtteranceBoost = _rainaConfig.VectorSearchUtteranceBoost,
            EntityBoost = _rainaConfig.VectorSearchEntityBoost,
            ContextBoost = _rainaConfig.VectorSearchContextBoost,
            RecentHistoryBoost = _rainaConfig.VectorSearchRecentHistoryBoost,
            BackgroundBoost = _rainaConfig.VectorSearchBackgroundBoost,
            MaxResults = _rainaConfig.VectorSearchMaxResults,
            MinSimilarity = _rainaConfig.VectorSearchMinSimilarity,
            DiminishingReturns = _rainaConfig.VectorSearchDiminishingReturns
        };

        // 1. Search for utterance text
        var utteranceResults = await _vectorCollection.SearchVectorsAsync(utteranceText, config.MaxResults, config.MinSimilarity);
        AddSearchResults(searchResults, utteranceResults, "Utterance", config.UtteranceBoost);

        // 2. Search for each entity
        foreach (var entity in entities)
        {
            var entityResults = await _vectorCollection.SearchVectorsAsync(entity.Name, config.MaxResults, config.MinSimilarity);
            AddSearchResults(searchResults, entityResults, $"Entity:{entity.Name}", config.EntityBoost);
        }

        // 3. Search for context summary
        if (!string.IsNullOrEmpty(contextSummary))
        {
            var contextResults = await _vectorCollection.SearchVectorsAsync(contextSummary, config.MaxResults, config.MinSimilarity);
            AddSearchResults(searchResults, contextResults, "Context", config.ContextBoost);
        }

        // 4. Search for recent conversation history
        var recentHistory = summaryService.GetContextForPrompt().Where(u => u.Depth == 0)
            .Select(u => u.Text).Take(5); // Last 5 recent items
        if (recentHistory.Any())
        {
            var recentText = string.Join(" ", recentHistory);
            var recentResults = await _vectorCollection.SearchVectorsAsync(recentText, config.MaxResults, config.MinSimilarity);
            AddSearchResults(searchResults, recentResults, "RecentHistory", config.RecentHistoryBoost);
        }

        // 5. Search for background summaries
        var summaries = summaryService.GetContextForPrompt().Where(u => u.Depth > 0)
            .Select(u => u.Text);
        if (summaries.Any())
        {
            var summaryText = string.Join(" ", summaries);
            var summaryResults = await _vectorCollection.SearchVectorsAsync(summaryText, config.MaxResults, config.MinSimilarity);
            AddSearchResults(searchResults, summaryResults, "BackgroundSummary", config.BackgroundBoost);
        }

        // Calculate final boosts with diminishing returns for duplicates
        var finalBoosts = new Dictionary<Guid, double>();

        foreach (var kvp in searchResults)
        {
            var chunkId = kvp.Key;
            var appearances = kvp.Value.OrderByDescending(a => a.similarity).ToList();

            double totalBoost = 0;
            for (int i = 0; i < appearances.Count; i++)
            {
                var appearance = appearances[i];
                var rankFactor = 1.0 / Math.Sqrt(appearance.rank + 1); // Higher rank = more boost
                var diminishingFactor = i < config.DiminishingReturns.Length ? config.DiminishingReturns[i] : 0.01;

                totalBoost += appearance.baseBoost * appearance.similarity * rankFactor * diminishingFactor;
            }

            finalBoosts[chunkId] = totalBoost;
        }

        return finalBoosts;
    }

    private void AddSearchResults(
       Dictionary<Guid, List<(double similarity, int rank, string searchType, double baseBoost)>> searchResults,
       List<SearchResult> results,
       string searchType,
       double baseBoost)
    {
        for (int i = 0; i < results.Count; i++)
        {
            var result = results[i];
            if (Guid.TryParse(result.Value.ID, out var chunkId))
            {
                if (!searchResults.ContainsKey(chunkId))
                    searchResults[chunkId] = new List<(double, int, string, double)>();

                searchResults[chunkId].Add((result.Similarity, i + 1, searchType, baseBoost));
            }
        }
    }
}


// New supporting classes
public class ConversationSummary
{
    public Guid ConversationId { get; set; }
    public Chunk ConversationChunk { get; set; }
    public DateTime StartTime { get; set; }
    public int UtteranceCount { get; set; }
    public List<Chunk> RecentUtterances { get; set; } = new List<Chunk>();
}

public class Response
{
    public string Message { get; set; }
    public Chunk UtteranceChunk { get; set; }
    public List<Chunk> RelevantChunks { get; set; }
}