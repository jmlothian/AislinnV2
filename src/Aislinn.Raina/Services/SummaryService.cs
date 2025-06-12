using System.Text.Json;

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

    public SummaryService()
    {
        depthMap = new Dictionary<int, List<Utterance>>();
    }
    public void Initialize(Guid id)
    {
        ConversationId = id;
    }
    public List<Utterance> AddItem(string text, int depth = 0, Guid? chunkId = null)
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
            TokenCount = tokenCount,
            CreatedAt = DateTime.UtcNow
        });

        LastModified = DateTime.UtcNow;

        int totalTokens = GetTotalTokensAtDepth(depth);
        if (totalTokens >= MAX_TOKENS_PER_DEPTH)
        {
            ReturnSummaries.AddRange(GenerateNextDepth(depth));
        }
        return ReturnSummaries;
    }

    private List<Utterance> GenerateNextDepth(int currentDepth)
    {
        List<Utterance> ReturnSummaries = new List<Utterance>();
        var currentList = depthMap[currentDepth];

        Utterance summaryUtterance = GenerateSummary(currentList);
        ReturnSummaries.Add(summaryUtterance);
        ReturnSummaries.AddRange(AddItem(summaryUtterance.Text, summaryUtterance.Depth, summaryUtterance.ChunkId));

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

    private Utterance GenerateSummary(List<Utterance> items)
    {
        string summaryText = $"Summary of {items.Count} items ({GetTotalTokensAtDepth(items[0].Depth)} tokens) at depth {items[0].Depth}";

        return new Utterance
        {
            Depth = items[0].Depth + 1,
            ChunkId = Guid.NewGuid(),
            Text = summaryText,
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

    public void LoadFromJson(string filePath)
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
        }
        else
        {
            // Fallback if deserialization fails
            ConversationId = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
            LastModified = DateTime.UtcNow;
            depthMap.Clear();
        }
    }

    // Factory method to create and load from JSON in one step
    public static SummaryService FromJson(string filePath)
    {
        var service = new SummaryService();
        service.LoadFromJson(filePath);
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
}