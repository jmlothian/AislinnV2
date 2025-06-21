using Microsoft.AspNetCore.SignalR;
using RAINA.Events;
using RAINA.Services;
using RAINA.Web.Hubs;
using RAINA.Web.Models;

namespace RAINA.Web.Services
{
    /// <summary>
    /// Subscribes to RAINA events and forwards them to SignalR clients
    /// </summary>
    public class WebEventSubscriber : IHostedService
    {
        private readonly IHubContext<RainaHub> _hubContext;
        private readonly ILogger<WebEventSubscriber> _logger;

        public WebEventSubscriber(IHubContext<RainaHub> hubContext, ILogger<WebEventSubscriber> logger)
        {
            Console.WriteLine("... Creating WebEventSubscriber");

            _hubContext = hubContext;
            _logger = logger;
            Console.WriteLine("... WebEventSubscriber Created");
        }

        /// <summary>
        /// Subscribe to all RAINA events
        /// </summary>
        public void Subscribe()
        {
            Console.WriteLine("WebEventSubscriber.Subscribe() - Starting");
            try
            {
                Console.WriteLine("Subscribing to IntentProcessor.IntentClassified...");
                IntentProcessor.IntentClassified += OnIntentClassified;
                Console.WriteLine("✓ IntentProcessor.IntentClassified subscribed");

                Console.WriteLine("Subscribing to IntentProcessor.EntitiesExtracted...");
                ConversationManager.EntitiesExtracted += OnEntitiesExtracted;
                Console.WriteLine("✓ IntentProcessor.EntitiesExtracted subscribed");

                Console.WriteLine("Subscribing to ConversationManager.MessageReceived...");
                ConversationManager.MessageReceived += OnMessageReceived;
                Console.WriteLine("✓ ConversationManager.MessageReceived subscribed");

                Console.WriteLine("Subscribing to ConversationManager.ResponseGenerated...");
                ConversationManager.ResponseGenerated += OnResponseGenerated;
                Console.WriteLine("✓ ConversationManager.ResponseGenerated subscribed");

                Console.WriteLine("Subscribing to ConversationManager.ContextUpdated...");
                ConversationManager.ContextUpdated += OnContextUpdated;
                Console.WriteLine("✓ ConversationManager.ContextUpdated subscribed");

                Console.WriteLine("Subscribing to ConversationManager.WorkingMemoryChanged...");
                ConversationManager.WorkingMemoryChanged += OnWorkingMemoryChanged;
                Console.WriteLine("✓ ConversationManager.WorkingMemoryChanged subscribed");

                Console.WriteLine("Subscribing to ConversationManager.SummaryCreated...");
                ConversationManager.SummaryCreated += OnSummaryCreated;
                Console.WriteLine("✓ ConversationManager.SummaryCreated subscribed");

                ConversationManager.GraphUpdated += OnGraphUpdated;

                Console.WriteLine("WebEventSubscriber initialized - listening for RAINA events");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in WebEventSubscriber.Subscribe(): {ex}");
                throw;
            }
        }

        private async void OnGraphUpdated(object? sender, GraphUpdatedEventArgs e)
        {

            if (e?.GraphJson != null)
            {
                await _hubContext.Clients.All.SendAsync("GraphUpdated", e.GraphJson);
            }
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

            _logger.LogInformation("WebEventSubscriber unsubscribed from RAINA events");
        }

        #region Event Handlers

        private async void OnIntentClassified(object sender, IntentClassifiedEventArgs e)
        {
            try
            {
                var intentData = new IntentClassifiedEvent
                {
                    IntentType = e.Intent.IntentType,
                    Confidence = e.Intent.Confidence,
                    UserInput = e.UserInput,
                    Entities = e.Intent.Entities?.Select(ent => new EntityInfo
                    {
                        Name = ent.Name,
                        Type = ent.Type
                    }).ToList() ?? new List<EntityInfo>(),
                    Timestamp = DateTime.UtcNow
                };

                await _hubContext.Clients.All.SendAsync("IntentClassified", intentData);
                _logger.LogDebug("Sent IntentClassified event: {IntentType} ({Confidence:P1})",
                    e.Intent.IntentType, e.Intent.Confidence);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending IntentClassified event");
            }
        }

        private async void OnEntitiesExtracted(object sender, EntitiesExtractedEventArgs e)
        {
            try
            {
                var entityData = new EntitiesExtractedEvent
                {
                    IntentEntities = e.IntentEntities?.Select(ent => new EntityInfo
                    {
                        Name = ent.Name,
                        Type = ent.Type
                    }).ToList() ?? new List<EntityInfo>(),
                    ExtractedEntities = e.ExtractedEntities?.Select(ent => new EntityInfo
                    {
                        Name = ent.Name,
                        Type = ent.Type
                    }).ToList() ?? new List<EntityInfo>(),
                    UserInput = e.UserInput,
                    Timestamp = DateTime.UtcNow
                };

                await _hubContext.Clients.All.SendAsync("EntitiesExtracted", entityData);
                _logger.LogDebug("Sent EntitiesExtracted event: {TotalCount} entities",
                    entityData.IntentEntities.Count + entityData.ExtractedEntities.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending EntitiesExtracted event");
            }
        }

        private async void OnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            try
            {
                Console.WriteLine($"WebEventSubscriber: Received message event: {e.UserInput}");
                var messageData = new MessageReceivedEvent
                {
                    UserInput = e.UserInput,
                    ChunkId = e.UtteranceChunk.ID,
                    ChunkName = e.UtteranceChunk.Name,
                    IntentType = e.Intent?.IntentType,
                    Timestamp = DateTime.UtcNow
                };

                Console.WriteLine($"WebEventSubscriber: Sending to SignalR clients...");
                await _hubContext.Clients.Group("All").SendAsync("MessageReceived", messageData);
                Console.WriteLine($"WebEventSubscriber: SignalR message sent successfully");

                _logger.LogDebug("Sent MessageReceived event: {UserInput}", e.UserInput);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"WebEventSubscriber ERROR: {ex}");
                _logger.LogError(ex, "Error sending MessageReceived event");
            }
        }

        private async void OnResponseGenerated(object sender, ResponseGeneratedEventArgs e)
        {
            try
            {
                var responseData = new ResponseGeneratedEvent
                {
                    ResponseText = e.ResponseText,
                    ChunkId = e.ResponseChunk.ID,
                    ChunkName = e.ResponseChunk.Name,
                    Timestamp = DateTime.UtcNow
                };

                await _hubContext.Clients.All.SendAsync("ResponseGenerated", responseData);
                _logger.LogDebug("Sent ResponseGenerated event: {ResponseLength} chars", e.ResponseText.Length);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending ResponseGenerated event");
            }
        }

        private async void OnContextUpdated(object sender, ContextUpdatedEventArgs e)
        {
            try
            {
                var contextData = new ContextUpdatedEvent
                {
                    ContextSnapshot = e.ContextSnapshot,
                    ContextSummary = e.ContextSummary,
                    CategoryCount = e.ContextSnapshot?.Count ?? 0,
                    Timestamp = DateTime.UtcNow
                };

                await _hubContext.Clients.All.SendAsync("ContextUpdated", contextData);
                _logger.LogDebug("Sent ContextUpdated event: {CategoryCount} categories", contextData.CategoryCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending ContextUpdated event");
            }
        }

        private async void OnWorkingMemoryChanged(object sender, WorkingMemoryChangedEventArgs e)
        {
            try
            {
                var memoryData = new WorkingMemoryChangedEvent
                {
                    WorkingMemoryItems = e.WorkingMemoryContents?.Select(chunk => new WorkingMemoryItem
                    {
                        Id = chunk.ID,
                        Name = chunk.Name,
                        ChunkType = chunk.ChunkType,
                        ActivationLevel = chunk.ActivationLevel,
                        Subsystem = chunk.Slots.GetValueOrDefault("Subsystem")?.Value?.ToString() ?? "Unknown"
                    }).ToList() ?? new List<WorkingMemoryItem>(),
                    PrimedChunks = e.PrimedChunks?.Select(chunk => new WorkingMemoryItem
                    {
                        Id = chunk.ID,
                        Name = chunk.Name,
                        ChunkType = chunk.ChunkType,
                        ActivationLevel = chunk.ActivationLevel,
                        Subsystem = "Primed"
                    }).ToList() ?? new List<WorkingMemoryItem>(),
                    Timestamp = DateTime.UtcNow
                };

                await _hubContext.Clients.All.SendAsync("WorkingMemoryChanged", memoryData);
                _logger.LogDebug("Sent WorkingMemoryChanged event: {WorkingCount} working, {PrimedCount} primed",
                    memoryData.WorkingMemoryItems.Count, memoryData.PrimedChunks.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending WorkingMemoryChanged event");
            }
        }

        private async void OnSummaryCreated(object sender, SummaryCreatedEventArgs e)
        {
            try
            {
                var summaryData = new SummaryCreatedEvent
                {
                    NewSummaries = e.NewSummaries?.Select(summary => new SummaryInfo
                    {
                        Id = summary.ChunkId,
                        Text = summary.Text,
                        Depth = summary.Depth,
                        TokenCount = summary.TokenCount,
                        IsSummary = summary.IsSummary,
                        CreatedAt = summary.CreatedAt
                    }).ToList() ?? new List<SummaryInfo>(),
                    UserInput = e.UserInput,
                    Timestamp = DateTime.UtcNow
                };

                await _hubContext.Clients.All.SendAsync("SummaryCreated", summaryData);
                _logger.LogDebug("Sent SummaryCreated event: {SummaryCount} new summaries",
                    summaryData.NewSummaries.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending SummaryCreated event");
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Subscribe();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Unsubscribe();
            return Task.CompletedTask;
        }

        #endregion
    }
}