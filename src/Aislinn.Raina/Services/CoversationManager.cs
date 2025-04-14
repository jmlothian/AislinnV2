using System.Text;
using Aislinn.Core.Cognitive;
using Aislinn.Core.Models;
using RAINA;

namespace RAINA.Services;
public class ConversationManager
{
    private readonly CognitiveMemorySystem _memorySystem;
    private readonly string _openAIApiKey;

    // Track the current conversation as a Chunk
    private Chunk _currentConversationChunk;

    // Store recent utterance chunks for quick access
    private List<Chunk> _recentUtterances = new List<Chunk>();

    // Maximum number of recent utterances to keep in memory
    private const int MaxRecentUtterances = 10;

    public ConversationManager(CognitiveMemorySystem memorySystem, string openAIApiKey)
    {
        _memorySystem = memorySystem;
        _openAIApiKey = openAIApiKey;
    }

    // Method to initialize or retrieve an existing conversation
    public async Task<Chunk> InitializeConversationAsync(string conversationId = null)
    {
        if (string.IsNullOrEmpty(conversationId))
        {
            // Create a new conversation chunk
            var conversationChunk = new Chunk
            {
                ChunkType = "Conversation",
                Name = $"Conversation_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}",
                Slots = new Dictionary<string, ModelSlot>
                {
                    { "StartTime", new ModelSlot { Name = "StartTime", Value = DateTime.Now } },
                    { "UtteranceCount", new ModelSlot { Name = "UtteranceCount", Value = 0 } }
                }
            };

            // Add to memory system
            _currentConversationChunk = await _memorySystem.AddChunkAsync(conversationChunk);
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
    public async Task<Chunk> RecordUserInputAsync(string userInput, Intent intent, UserContext context)
    {
        // Ensure we have an active conversation
        if (_currentConversationChunk == null)
        {
            await InitializeConversationAsync();
        }

        // Create utterance chunk for user input
        var utteranceChunk = new Chunk
        {
            ChunkType = "Utterance",
            Name = $"UserUtterance_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}",
            Slots = new Dictionary<string, ModelSlot>
            {
                { "Speaker", new ModelSlot { Name = "Speaker", Value = "User" } },
                { "Text", new ModelSlot { Name = "Text", Value = userInput } },
                { "Intent", new ModelSlot { Name = "Intent", Value = intent?.IntentType } },
                { "Timestamp", new ModelSlot { Name = "Timestamp", Value = DateTime.Now } },
                { "CognitiveTimestamp", new ModelSlot { Name = "CognitiveTimestamp", Value = _memorySystem.GetCurrentCognitiveTime() } },
                { "ConversationId", new ModelSlot { Name = "ConversationId", Value = _currentConversationChunk.ID } }
            }
        };

        // Add metadata based on intent if available
        if (intent != null)
        {
            utteranceChunk.Slots["IntentConfidence"] = new ModelSlot { Name = "IntentConfidence", Value = intent.Confidence };

            if (intent.Entities != null && intent.Entities.Any())
            {
                utteranceChunk.Slots["Entities"] = new ModelSlot { Name = "Entities", Value = intent.Entities };
            }
        }

        // Add to memory system
        utteranceChunk = await _memorySystem.AddChunkAsync(utteranceChunk);

        // Increment utterance count in conversation
        int utteranceCount = (int)(_currentConversationChunk.Slots["UtteranceCount"].Value ?? 0);
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

        return utteranceChunk;
    }

    // Generate and record a system response
    public async Task<Response> GenerateResponseAsync(string userInput, Intent intent, UserContext context)
    {
        // Record the user input first
        var userUtterance = await RecordUserInputAsync(userInput, intent, context);

        // Generate response using LLM
        // This would call OpenAI or other LLM to generate a natural language response
        // For now, just create a simple response
        string responseText = $"I understand you want to have a general conversation. You said: '{userInput}'";

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
                { "Intent", new ModelSlot { Name = "Intent", Value = intent?.IntentType } },
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
            UtteranceChunk = responseChunk
        };
    }

    public async Task<Response> GenerateQueryResponseAsync(string userInput, Intent intent, List<Chunk> queryResults, UserContext context)
    {
        // Record the user input first
        var userUtterance = await RecordUserInputAsync(userInput, intent, context);

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