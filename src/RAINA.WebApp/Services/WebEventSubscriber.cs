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
    public class WebEventSubscriber
    {
        private readonly IHubContext<RainaHub> _hubContext;
        private readonly ILogger<WebEventSubscriber> _logger;

        public WebEventSubscriber(IHubContext<RainaHub> hubContext, ILogger<WebEventSubscriber> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        /// <summary>
        /// Subscribe to all RAINA events
        /// </summary>
        public void Subscribe()
        {
            // IntentProcessor events
            IntentProcessor.IntentClassified += OnIntentClassified;
            IntentProcessor.EntitiesExtracted += OnEntitiesExtracted;

            // ConversationManager events
            ConversationManager.MessageReceived += OnMessageReceived;
            ConversationManager.ResponseGenerated += OnResponseGenerated;
            ConversationManager.ContextUpdated += OnContextUpdated;
            ConversationManager.WorkingMemoryChanged += OnWorkingMemoryChanged;
            ConversationManager.SummaryCreated += OnSummaryCreated;

            _logger.LogInformation("WebEventSubscriber initialized - listening for RAINA events");
        }

        /// <summary>
        /// Unsubscribe from all events
        /// </summary>
        public void Unsubscribe()
        {
            IntentProcessor.IntentClassified -= OnIntentClassified;
            IntentProcessor.EntitiesExtracted -= OnEntitiesExtracted;
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
                var messageData = new MessageReceivedEvent
                {
                    UserInput = e.UserInput,
                    ChunkId = e.UtteranceChunk.ID,
                    ChunkName = e.UtteranceChunk.Name,
                    IntentType = e.Intent?.IntentType,
                    Timestamp = DateTime.UtcNow
                };

                await _hubContext.Clients.All.SendAsync("MessageReceived", messageData);
                _logger.LogDebug("Sent MessageReceived event: {UserInput}", e.UserInput);
            }
            catch (Exception ex)
            {
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

        #endregion
    }
}