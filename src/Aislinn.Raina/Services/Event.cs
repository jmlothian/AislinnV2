// ===== EVENT DEFINITIONS (create new file: RainaEvents.cs) =====

using Aislinn.Core.Context;
using Aislinn.Core.Models;
using RAINA.Events;
using RAINA.Services;
using static Aislinn.Core.Context.ContextContainer;

namespace RAINA.Events
{
    // Event argument classes
    public class IntentClassifiedEventArgs : EventArgs
    {
        public string UserInput { get; set; }
        public Intent Intent { get; set; }
        public UserContext Context { get; set; }
    }

    public class EntitiesExtractedEventArgs : EventArgs
    {
        public List<Entity> IntentEntities { get; set; }
        public List<Entity> ExtractedEntities { get; set; }
        public string UserInput { get; set; }
    }

    public class MessageReceivedEventArgs : EventArgs
    {
        public string UserInput { get; set; }
        public Intent Intent { get; set; }
        public UserContext Context { get; set; }
        public Chunk UtteranceChunk { get; set; }
    }

    public class ResponseGeneratedEventArgs : EventArgs
    {
        public string ResponseText { get; set; }
        public Chunk ResponseChunk { get; set; }
        public UserContext Context { get; set; }
    }

    public class ContextUpdatedEventArgs : EventArgs
    {
        public Dictionary<ContextCategory, Dictionary<string, object>> ContextSnapshot { get; set; }
        public string ContextSummary { get; set; }
    }

    public class WorkingMemoryChangedEventArgs : EventArgs
    {
        public List<Chunk> WorkingMemoryContents { get; set; }
        public List<Chunk> PrimedChunks { get; set; }
    }

    public class SummaryCreatedEventArgs : EventArgs
    {
        public List<Utterance> NewSummaries { get; set; }
        public string UserInput { get; set; }
    }

    // Add GraphUpdated event and args in the same convention
    public class GraphUpdatedEventArgs : EventArgs
    {
        public string GraphJson { get; set; }
    }
}