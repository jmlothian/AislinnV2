using System.Linq.Expressions;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Aislinn.ChunkStorage;
using Aislinn.Core.Interfaces;
using Aislinn.Core.Services;
using RAINA.Services;

/// <summary>
/// SummaryService - A recursive data structure that automatically consolidates data across depth levels based on token counts.
/// Now with JSON persistence that includes all necessary metadata.
/// </summary>
public class SummaryService
{
    private Dictionary<int, List<Utterance>> depthMap;
    private const int MAX_TOKENS_PER_DEPTH = 8000;
    private const int TOKEN_REMOVAL_THRESHOLD = 6000;

    public Guid ConversationId { get; set; } = Guid.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastModified { get; set; } = DateTime.UtcNow;
    private PromptLibrary promptLibrary = new PromptLibrary();
    private string _agentName = "Raina";
    private readonly HttpClient _httpClient;
    public SummaryService(string agentName, string openAIApiKey)
    {
        depthMap = new Dictionary<int, List<Utterance>>();
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {openAIApiKey}");
    }
    public void Initialize(Guid id)
    {
        ConversationId = id;
    }
    public Dictionary<int, List<Utterance>> GetAllSummaries()
    {
        return depthMap;
    }
    public Utterance GetMostRecentUtterance()
    {
        // Get all items at depth 0 (actual utterances, not summaries)
        var depth0Items = depthMap[0];

        if (!depth0Items.Any())
            return null;

        // Return the most recent utterance based on creation time
        return depth0Items[depth0Items.Count - 1];
    }
    public async Task<List<Utterance>> AddItem(string text, string speaker, int depth = 0, Guid? chunkId = null)
    {
        List<Utterance> ReturnSummaries = new List<Utterance>();
        Guid actualChunkId = chunkId ?? Guid.Empty;

        int tokenCount = EstimateTokens(text);

        if (!depthMap.ContainsKey(depth))
        {
            depthMap[depth] = new List<Utterance>();
        }

        depthMap[depth].Add(new Utterance
        {
            Depth = depth,
            ChunkId = actualChunkId,
            Text = text,
            Speaker = speaker,
            TokenCount = tokenCount,
            CreatedAt = DateTime.UtcNow
        });

        LastModified = DateTime.UtcNow;

        int totalTokens = GetTotalTokensAtDepth(depth);
        if (totalTokens >= MAX_TOKENS_PER_DEPTH)
        {
            ReturnSummaries.AddRange(await GenerateNextDepth(depth));
        }
        return ReturnSummaries;
    }

    private async Task<List<Utterance>> GenerateNextDepth(int currentDepth)
    {
        List<Utterance> ReturnSummaries = new List<Utterance>();
        var currentList = depthMap[currentDepth];

        Utterance summaryUtterance = await GenerateSummary(currentList);
        ReturnSummaries.Add(summaryUtterance);
        ReturnSummaries.AddRange(await AddItem(summaryUtterance.Text, "system", summaryUtterance.Depth, summaryUtterance.ChunkId));

        RemoveItemsToThreshold(currentDepth);
        return ReturnSummaries;
    }

    private void RemoveItemsToThreshold(int depth)
    {
        var currentList = depthMap[depth];

        while (GetTotalTokensAtDepth(depth) > TOKEN_REMOVAL_THRESHOLD && currentList.Count > 0)
        {
            currentList.RemoveAt(0);
        }
    }

    private int GetTotalTokensAtDepth(int depth)
    {
        if (!depthMap.ContainsKey(depth))
            return 0;

        return depthMap[depth].Sum(u => u.TokenCount);
    }
    private async Task<OpenAIResponse> CallOpenAIAsync(string systemprompt, string prompt, List<Utterance> utterances, double temperature = 0.7)
    {
        StringContent content;

        List<PromptMessage> messages = new List<PromptMessage>() { new PromptMessage() { role = "system", content = systemprompt } };
        if (utterances != null)
        {
            foreach (var mesg in utterances)
            {
                if (mesg.Speaker == _agentName)
                {
                    messages.Add(new PromptMessage() { role = "assistant", content = mesg.Text });
                }
                else if (mesg.Speaker == "system")
                {
                    messages.Add(new PromptMessage() { role = "assistant", content = mesg.Text });
                }
                else
                {
                    messages.Add(new PromptMessage() { role = "user", content = mesg.Text });
                }
            }
        }
        messages.Add(new PromptMessage() { role = "user", content = prompt });
        var requestBody = new
        {
            model = "gpt-4o",
            messages = messages,
            temperature = temperature,
            max_tokens = 6000
        };

        content = new StringContent(
           JsonSerializer.Serialize(requestBody),
           Encoding.UTF8,
           "application/json");

        var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
        response.EnsureSuccessStatusCode();

        var responseString = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<OpenAIResponse>(responseString);
    }

    private async Task<Utterance> GenerateSummary(List<Utterance> items)
    {
        string summaryText = $"Summary of {items.Count} items ({GetTotalTokensAtDepth(items[0].Depth)} tokens) at depth {items[0].Depth}";

        //nothing to hydrate yet
        var systemPrompt = promptLibrary.HydratePrompt("raina.summarize", new Dictionary<string, object> { });
        var response = await CallOpenAIAsync(
            systemPrompt,
            "",
            items);

        return new Utterance
        {
            Depth = items[0].Depth + 1,
            ChunkId = Guid.NewGuid(),
            Speaker = "system",
            Text = summaryText + "\n" + response.Choices[0].Message.Content,
            TokenCount = EstimateTokens(summaryText),
            CreatedAt = DateTime.UtcNow,
            IsSummary = true
        };
    }

    public static int EstimateTokens(string text)
    {
        if (string.IsNullOrEmpty(text)) return 0;
        return (int)Math.Ceiling(text.Length / 3.7);
    }

    // JSON Persistence Methods
    public void SaveToJson(string filePath)
    {
        var saveData = new SummaryServiceData
        {
            ConversationId = this.ConversationId,
            CreatedAt = this.CreatedAt,
            LastModified = DateTime.UtcNow,
            MaxTokensPerDepth = MAX_TOKENS_PER_DEPTH,
            TokenRemovalThreshold = TOKEN_REMOVAL_THRESHOLD,
            DepthMap = this.depthMap
        };

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        string jsonString = JsonSerializer.Serialize(saveData, options);
        File.WriteAllText(filePath, jsonString);
    }

    public void LoadFromJson(string filePath, string chunkCollectionId)
    {
        if (!File.Exists(filePath))
        {
            // Initialize with new conversation ID if file doesn't exist
            ConversationId = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
            LastModified = DateTime.UtcNow;
            depthMap.Clear();
            return;
        }

        string jsonString = File.ReadAllText(filePath);

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var saveData = JsonSerializer.Deserialize<SummaryServiceData>(jsonString, options);

        if (saveData != null)
        {
            ConversationId = saveData.ConversationId;
            CreatedAt = saveData.CreatedAt;
            LastModified = saveData.LastModified;
            depthMap = saveData.DepthMap ?? new Dictionary<int, List<Utterance>>();
            foreach (var item in depthMap[0])
            {
                if (item.Speaker == null || item.Speaker == "")
                {
                    var chunk = item.ChunkId.ToChunk(chunkCollectionId);
                    if (chunk != null && chunk.Slots.ContainsKey("SpeakerName"))
                    {
                        item.Speaker = item.ChunkId.ToChunk(chunkCollectionId).Slots["SpeakerName"].Value.ToString();
                        Console.WriteLine("Rewireing Speaker: " + item.Speaker);
                    }
                    else
                    {
                        Console.WriteLine("No Speaker Found: " + item.Text);
                        item.Speaker = "Raina";
                    }
                }
            }
        }
        else
        {
            // Fallback if deserialization fails
            ConversationId = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
            LastModified = DateTime.UtcNow;
        }
        Console.WriteLine("Summary Service Loaded: " + filePath);
        //Console.WriteLine(jsonString);
    }

    // Factory method to create and load from JSON in one step
    public static SummaryService FromJson(string filePath, string chunkCollectionId, string agentName, string openAIApiKey)
    {
        var service = new SummaryService(agentName, openAIApiKey);
        service.LoadFromJson(filePath, chunkCollectionId);
        return service;
    }

    // Helper methods (unchanged)
    public List<Utterance> GetItemsAtDepth(int depth)
    {
        return depthMap.ContainsKey(depth) ? depthMap[depth] : new List<Utterance>();
    }

    public Dictionary<int, int> GetDepthTokenCounts()
    {
        return depthMap.ToDictionary(kvp => kvp.Key, kvp => GetTotalTokensAtDepth(kvp.Key));
    }

    public Dictionary<int, int> GetDepthItemCounts()
    {
        return depthMap.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Count);
    }

    public List<Utterance> GetContextForPrompt(int maxTotalTokens = 4000)
    {
        var contextItems = new List<Utterance>();
        int remainingTokens = maxTotalTokens;

        var depthBudgets = new Dictionary<int, int>
        {
            { 0, 2000 },
            { 1, 1000 },
            { 2, 500 },
            { 3, 300 },
            { 4, 200 }
        };

        foreach (var depthBudget in depthBudgets.OrderBy(x => x.Key))
        {
            int depth = depthBudget.Key;
            int budget = Math.Min(depthBudget.Value, remainingTokens);

            if (budget <= 0 || !depthMap.ContainsKey(depth))
                continue;

            var itemsAtDepth = depthMap[depth];
            var selectedItems = new List<Utterance>();
            int usedTokens = 0;

            for (int i = itemsAtDepth.Count - 1; i >= 0; i--)
            {
                var item = itemsAtDepth[i];
                if (usedTokens + item.TokenCount <= budget)
                {
                    selectedItems.Insert(0, item);
                    usedTokens += item.TokenCount;
                }
                else
                {
                    break;
                }
            }

            contextItems.AddRange(selectedItems);
            remainingTokens -= usedTokens;
        }

        return contextItems.OrderBy(x => x.Depth)
                          .ThenBy(x => depthMap[x.Depth].IndexOf(x))
                          .ToList();
    }

    public List<Guid> GetContextChunkIds(int maxTotalTokens = 4000)
    {
        return GetContextForPrompt(maxTotalTokens).Select(u => u.ChunkId).ToList();
    }

    public int GetContextTokenCount(int maxTotalTokens = 4000)
    {
        return GetContextForPrompt(maxTotalTokens).Sum(u => u.TokenCount);
    }
}

// Data transfer object for JSON serialization
public class SummaryServiceData
{
    public Guid ConversationId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastModified { get; set; }
    public int MaxTokensPerDepth { get; set; }
    public int TokenRemovalThreshold { get; set; }
    public Dictionary<int, List<Utterance>> DepthMap { get; set; } = new();
}

// Enhanced Utterance class with additional metadata
public class Utterance
{
    public int Depth { get; set; }
    public Guid ChunkId { get; set; }
    public string Text { get; set; } = string.Empty;
    public int TokenCount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsSummary { get; set; } = false;
    public string Speaker { get; set; } = "";
}