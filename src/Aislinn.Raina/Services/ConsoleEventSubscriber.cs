using RAINA.Events;
using RAINA.Services;

namespace RAINA.ConsoleEvents
{
    /// <summary>
    /// Subscribes to RAINA events and displays them in the console
    /// </summary>
    public class ConsoleEventSubscriber
    {
        private readonly bool _showTimestamps;
        private readonly bool _useColors;

        public ConsoleEventSubscriber(bool showTimestamps = true, bool useColors = true)
        {
            _showTimestamps = showTimestamps;
            _useColors = useColors;
        }

        /// <summary>
        /// Subscribe to all RAINA events
        /// </summary>
        public void Subscribe()
        {
            // IntentProcessor events
            IntentProcessor.IntentClassified += OnIntentClassified;
            ConversationManager.EntitiesExtracted += OnEntitiesExtracted;

            // ConversationManager events
            ConversationManager.MessageReceived += OnMessageReceived;
            ConversationManager.ResponseGenerated += OnResponseGenerated;
            ConversationManager.ContextUpdated += OnContextUpdated;
            ConversationManager.WorkingMemoryChanged += OnWorkingMemoryChanged;
            ConversationManager.SummaryCreated += OnSummaryCreated;

            WriteEventLog("EVENT SYSTEM", "Console subscriber initialized", ConsoleColor.Green);
        }

        /// <summary>
        /// Unsubscribe from all events
        /// </summary>
        public void Unsubscribe()
        {
            IntentProcessor.IntentClassified -= OnIntentClassified;
            ConversationManager.EntitiesExtracted -= OnEntitiesExtracted;
            ConversationManager.MessageReceived -= OnMessageReceived;
            ConversationManager.ResponseGenerated -= OnResponseGenerated;
            ConversationManager.ContextUpdated -= OnContextUpdated;
            ConversationManager.WorkingMemoryChanged -= OnWorkingMemoryChanged;
            ConversationManager.SummaryCreated -= OnSummaryCreated;

            WriteEventLog("EVENT SYSTEM", "Console subscriber stopped", ConsoleColor.Yellow);
        }

        #region Event Handlers

        private void OnIntentClassified(object sender, IntentClassifiedEventArgs e)
        {
            WriteEventLog("INTENT", $"Classified as '{e.Intent.IntentType}' ({e.Intent.Confidence:P1})", ConsoleColor.Cyan);
            if (e.Intent.Entities?.Any() == true)
            {
                var entityList = string.Join(", ", e.Intent.Entities.Select(ent => $"{ent.Name}({ent.Type})"));
                WriteEventLog("INTENT", $"Entities: {entityList}", ConsoleColor.DarkCyan);
            }
        }

        private void OnEntitiesExtracted(object sender, EntitiesExtractedEventArgs e)
        {
            var totalEntities = (e.IntentEntities?.Count ?? 0) + (e.ExtractedEntities?.Count ?? 0);
            if (totalEntities > 0)
            {
                WriteEventLog("ENTITIES", $"Extracted {totalEntities} entities", ConsoleColor.Magenta);

                if (e.IntentEntities?.Any() == true)
                {
                    var intentList = string.Join(", ", e.IntentEntities.Select(e => e.Name));
                    WriteEventLog("ENTITIES", $"  Intent: {intentList}", ConsoleColor.DarkMagenta);
                }

                if (e.ExtractedEntities?.Any() == true)
                {
                    var extractedList = string.Join(", ", e.ExtractedEntities.Select(e => e.Name));
                    WriteEventLog("ENTITIES", $"  Extracted: {extractedList}", ConsoleColor.DarkMagenta);
                }
            }
        }

        private void OnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            WriteEventLog("MESSAGE", $"User: {e.UserInput}", ConsoleColor.Blue);
            WriteEventLog("MESSAGE", $"Chunk: {e.UtteranceChunk.Name} (ID: {e.UtteranceChunk.ID.ToString()[..8]}...)", ConsoleColor.DarkBlue);
        }

        private void OnResponseGenerated(object sender, ResponseGeneratedEventArgs e)
        {
            WriteEventLog("RESPONSE", $"Assistant: {e.ResponseText}", ConsoleColor.Green);
            WriteEventLog("RESPONSE", $"Chunk: {e.ResponseChunk.Name} (ID: {e.ResponseChunk.ID.ToString()[..8]}...)", ConsoleColor.DarkGreen);
        }

        private void OnContextUpdated(object sender, ContextUpdatedEventArgs e)
        {
            var categoryCount = e.ContextSnapshot?.Count ?? 0;
            WriteEventLog("CONTEXT", $"Updated {categoryCount} context categories", ConsoleColor.Yellow);

            if (!string.IsNullOrEmpty(e.ContextSummary))
            {
                WriteEventLog("CONTEXT", $"Summary: {e.ContextSummary}", ConsoleColor.DarkYellow);
            }
        }

        private void OnWorkingMemoryChanged(object sender, WorkingMemoryChangedEventArgs e)
        {
            var wmCount = e.WorkingMemoryContents?.Count ?? 0;
            var primedCount = e.PrimedChunks?.Count ?? 0;

            WriteEventLog("MEMORY", $"Working Memory: {wmCount} chunks, Primed: {primedCount} chunks", ConsoleColor.Red);

            if (e.WorkingMemoryContents?.Any() == true)
            {
                var topChunk = e.WorkingMemoryContents.OrderByDescending(c => c.ActivationLevel).First();
                WriteEventLog("MEMORY", $"  Top: {topChunk.Name} ({topChunk.ActivationLevel:F2})", ConsoleColor.DarkRed);
            }
        }

        private void OnSummaryCreated(object sender, SummaryCreatedEventArgs e)
        {
            if (e.NewSummaries?.Any() == true)
            {
                WriteEventLog("SUMMARY", $"Created {e.NewSummaries.Count} new summaries", ConsoleColor.White);
                foreach (var summary in e.NewSummaries)
                {
                    WriteEventLog("SUMMARY", $"  Depth {summary.Depth}: {summary.Text}", ConsoleColor.Gray);
                }
            }
        }

        #endregion

        #region Helper Methods

        private void WriteEventLog(string category, string message, ConsoleColor color = ConsoleColor.White)
        {
            var timestamp = _showTimestamps ? $"[{DateTime.Now:HH:mm:ss.fff}] " : "";
            var categoryFormatted = $"[{category,-8}]";

            if (_useColors)
            {
                System.Console.ForegroundColor = ConsoleColor.DarkGray;
                System.Console.Write(timestamp);

                System.Console.ForegroundColor = color;
                System.Console.Write(categoryFormatted);

                System.Console.ForegroundColor = ConsoleColor.White;
                System.Console.WriteLine($" {message}");

                System.Console.ResetColor();
            }
            else
            {
                System.Console.WriteLine($"{timestamp}{categoryFormatted} {message}");
            }
        }

        // private string TruncateText(string text, int maxLength)
        // {
        //     if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
        //         return text;

        //     return text.Substring(0, maxLength - 3) + "...";
        // }

        #endregion
    }
}

// ===== HOW TO USE IN YOUR MAIN PROGRAM =====

/*
// In your Main() method or wherever you initialize RAINA:

var eventSubscriber = new ConsoleEventSubscriber(showTimestamps: true, useColors: true);
eventSubscriber.Subscribe();

// ... your existing RAINA initialization and loop ...

// When shutting down:
eventSubscriber.Unsubscribe();
*/