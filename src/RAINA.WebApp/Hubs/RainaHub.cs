using Microsoft.AspNetCore.SignalR;
using RAINA.Services;
using RAINA.Web.Models;
using Aislinn.Core;

namespace RAINA.Web.Hubs
{
    /// <summary>
    /// SignalR hub for real-time communication with RAINA
    /// </summary>
    public class RainaHub : Hub
    {
        private readonly IntentProcessor _intentProcessor;
        private readonly AislinnCoreServices _coreServices;
        private readonly RainaServices _rainaServices;
        private readonly UserContextManager _userContextManager;
        private readonly ILogger<RainaHub> _logger;

        public RainaHub(
            IntentProcessor intentProcessor,
            AislinnCoreServices coreServices,
            RainaServices rainaServices,
            UserContextManager userContextManager,
            ILogger<RainaHub> logger)
        {
            _intentProcessor = intentProcessor;
            _coreServices = coreServices;
            _rainaServices = rainaServices;
            _userContextManager = userContextManager;
            _logger = logger;
        }

        /// <summary>
        /// Client sends a message to RAINA
        /// </summary>
        public async Task SendMessage(string message, string sessionId = "default")
        {
            try
            {
                _logger.LogInformation("Received message from client: {Message}", message);

                var userContext = _userContextManager.GetOrCreateContext(sessionId);
                var response = await _intentProcessor.ProcessInputAsync(message, userContext);

                // Send response back to the client
                await Clients.Caller.SendAsync("MessageResponse", new ChatResponse
                {
                    Message = response.Message,
                    Timestamp = DateTime.UtcNow,
                    Success = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message: {Message}", message);

                await Clients.Caller.SendAsync("MessageResponse", new ChatResponse
                {
                    Message = "Sorry, I encountered an error processing your message.",
                    Timestamp = DateTime.UtcNow,
                    Success = false,
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Client requests current working memory
        /// </summary>
        public async Task GetWorkingMemory()
        {
            try
            {
                var workingMemory = await _coreServices.MemorySystem.GetWorkingMemoryContentsAsync();
                var memoryData = workingMemory.Select(chunk => new WorkingMemoryItem
                {
                    Id = chunk.ID,
                    Name = chunk.Name,
                    ChunkType = chunk.ChunkType,
                    ActivationLevel = chunk.ActivationLevel,
                    Subsystem = chunk.Slots.GetValueOrDefault("Subsystem")?.Value?.ToString() ?? "Unknown"
                }).ToList();

                await Clients.Caller.SendAsync("WorkingMemoryUpdate", memoryData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting working memory");
                await Clients.Caller.SendAsync("Error", "Failed to get working memory");
            }
        }

        /// <summary>
        /// Client requests current context
        /// </summary>
        public async Task GetContext()
        {
            try
            {
                var contextSnapshot = _coreServices.ContextContainer.CreateContextSnapshot();
                await Clients.Caller.SendAsync("ContextUpdate", contextSnapshot);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting context");
                await Clients.Caller.SendAsync("Error", "Failed to get context");
            }
        }

        /// <summary>
        /// Client manually activates a chunk
        /// </summary>
        public async Task ActivateChunk(string chunkId)
        {
            try
            {
                if (!Guid.TryParse(chunkId, out var id))
                {
                    await Clients.Caller.SendAsync("Error", "Invalid chunk ID format");
                    return;
                }

                var chunk = await _coreServices.MemorySystem.ActivateChunkAsync(id, "manual_web", 1.0);
                if (chunk != null)
                {
                    await Clients.Caller.SendAsync("ChunkActivated", new
                    {
                        ChunkId = chunk.ID,
                        Name = chunk.Name,
                        ActivationLevel = chunk.ActivationLevel
                    });
                }
                else
                {
                    await Clients.Caller.SendAsync("Error", "Chunk not found");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating chunk: {ChunkId}", chunkId);
                await Clients.Caller.SendAsync("Error", "Failed to activate chunk");
            }
        }

        /// <summary>
        /// Client joins a specific session/room
        /// </summary>
        public async Task JoinSession(string sessionId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"session_{sessionId}");
            _logger.LogInformation("Client {ConnectionId} joined session {SessionId}", Context.ConnectionId, sessionId);
        }

        /// <summary>
        /// Client leaves a session/room
        /// </summary>
        public async Task LeaveSession(string sessionId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"session_{sessionId}");
            _logger.LogInformation("Client {ConnectionId} left session {SessionId}", Context.ConnectionId, sessionId);
        }

        /// <summary>
        /// Connection established
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);

            // Send initial system status
            await Clients.Caller.SendAsync("SystemStatus", new
            {
                Status = "Connected",
                CognitiveSteps = _coreServices.TimeManager.GetCognitiveSteps(),
                AgentTime = _coreServices.TimeManager.GetAgentTime(),
                Timestamp = DateTime.UtcNow
            });

            await base.OnConnectedAsync();
        }

        /// <summary>
        /// Connection terminated
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }
    }
}