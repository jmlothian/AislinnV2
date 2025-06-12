using Aislinn.Core.Context;

namespace RAINA.Web.Models
{
    // API Request/Response Models
    public class ChatRequest
    {
        public string Message { get; set; }
        public string SessionId { get; set; } = "default";
    }

    public class ChatResponse
    {
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
        public bool Success { get; set; }
        public string Error { get; set; }
    }

    public class ActivateChunkRequest
    {
        public string ChunkId { get; set; }
        public string Emotion { get; set; } = "manual_web";
        public double ActivationBoost { get; set; } = 1.0;
    }

    public class QueryChunksRequest
    {
        public string Query { get; set; }
        public int MaxResults { get; set; } = 20;
    }

    // Memory Models
    public class WorkingMemoryItem
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string ChunkType { get; set; }
        public double ActivationLevel { get; set; }
        public string Subsystem { get; set; }
    }

    public class ChunkInfo
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string ChunkType { get; set; }
        public string SemanticType { get; set; }
        public double ActivationLevel { get; set; }
        public Dictionary<string, object> Slots { get; set; } = new();
    }

    // Entity Models
    public class EntityInfo
    {
        public string Name { get; set; }
        public string Type { get; set; }
    }

    // Summary Models
    public class SummaryInfo
    {
        public Guid Id { get; set; }
        public string Text { get; set; }
        public int Depth { get; set; }
        public int TokenCount { get; set; }
        public bool IsSummary { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // Event Models (for SignalR)
    public class IntentClassifiedEvent
    {
        public string IntentType { get; set; }
        public double Confidence { get; set; }
        public string UserInput { get; set; }
        public List<EntityInfo> Entities { get; set; } = new();
        public DateTime Timestamp { get; set; }
    }

    public class EntitiesExtractedEvent
    {
        public List<EntityInfo> IntentEntities { get; set; } = new();
        public List<EntityInfo> ExtractedEntities { get; set; } = new();
        public string UserInput { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class MessageReceivedEvent
    {
        public string UserInput { get; set; }
        public Guid ChunkId { get; set; }
        public string ChunkName { get; set; }
        public string IntentType { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class ResponseGeneratedEvent
    {
        public string ResponseText { get; set; }
        public Guid ChunkId { get; set; }
        public string ChunkName { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class ContextUpdatedEvent
    {
        public Dictionary<ContextCategory, Dictionary<string, object>> ContextSnapshot { get; set; }
        public string ContextSummary { get; set; }
        public int CategoryCount { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class WorkingMemoryChangedEvent
    {
        public List<WorkingMemoryItem> WorkingMemoryItems { get; set; } = new();
        public List<WorkingMemoryItem> PrimedChunks { get; set; } = new();
        public DateTime Timestamp { get; set; }
    }

    public class SummaryCreatedEvent
    {
        public List<SummaryInfo> NewSummaries { get; set; } = new();
        public string UserInput { get; set; }
        public DateTime Timestamp { get; set; }
    }

    // Status Models
    public class SystemStatus
    {
        public string Status { get; set; }
        public long CognitiveSteps { get; set; }
        public double AgentTime { get; set; }
        public int WorkingMemoryCount { get; set; }
        public int PrimedCount { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class MemoryStatus
    {
        public List<WorkingMemoryItem> WorkingMemory { get; set; } = new();
        public List<WorkingMemoryItem> PrimedChunks { get; set; } = new();
        public long CognitiveSteps { get; set; }
        public double AgentTime { get; set; }
        public DateTime Timestamp { get; set; }
    }
}