using Microsoft.AspNetCore.Mvc;
using RAINA.Services;
using RAINA.Web.Models;
using Aislinn.Core;
using RAINA.Events;
using RAINA.Web.Services;

namespace RAINA.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RainaController : ControllerBase
    {
        private readonly IntentProcessor _intentProcessor;
        private readonly AislinnCoreServices _coreServices;
        private readonly RainaServices _rainaServices;
        private readonly AppStateManager _appStateManager;
        private readonly ILogger<RainaController> _logger;
        private readonly ConversationManager convo;

        public RainaController(
            IntentProcessor intentProcessor,
            AislinnCoreServices coreServices,
            RainaServices rainaServices,
            AppStateManager appStateManager,
            ILogger<RainaController> logger,
            ConversationManager convo)
        {
            _intentProcessor = intentProcessor;
            _coreServices = coreServices;
            _rainaServices = rainaServices;
            this._appStateManager = appStateManager;
            _logger = logger;
            this.convo = convo;
        }

        /// <summary>
        /// Send a message to RAINA
        /// </summary>
        [HttpPost("chat")]
        public async Task<ActionResult<ChatResponse>> SendMessage([FromBody] ChatRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Message))
                {
                    return BadRequest(new ChatResponse
                    {
                        Message = "Message cannot be empty",
                        Success = false,
                        Timestamp = DateTime.UtcNow
                    });
                }

                var userContext = _appStateManager.GetUserContext(request.SessionId);
                var response = await _intentProcessor.ProcessInputAsync(request.Message, userContext);

                return Ok(new ChatResponse
                {
                    Message = response.Message,
                    Success = true,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing chat message: {Message}", request.Message);
                return StatusCode(500, new ChatResponse
                {
                    Message = "Internal server error",
                    Success = false,
                    Error = ex.Message,
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Get current working memory status
        /// </summary>
        [HttpGet("memory/working")]
        public async Task<ActionResult<MemoryStatus>> GetWorkingMemory()
        {
            try
            {
                var workingMemory = await _coreServices.MemorySystem.GetWorkingMemoryContentsAsync();
                var primedChunks = await _coreServices.MemorySystem.GetPrimedChunksAsync();

                var memoryStatus = new MemoryStatus
                {
                    WorkingMemory = workingMemory.Select(chunk => new WorkingMemoryItem
                    {
                        Id = chunk.ID,
                        Name = chunk.Name,
                        ChunkType = chunk.ChunkType,
                        ActivationLevel = chunk.ActivationLevel,
                        Subsystem = chunk.Slots.GetValueOrDefault("Subsystem")?.Value?.ToString() ?? "Unknown"
                    }).ToList(),
                    PrimedChunks = primedChunks.Select(chunk => new WorkingMemoryItem
                    {
                        Id = chunk.ID,
                        Name = chunk.Name,
                        ChunkType = chunk.ChunkType,
                        ActivationLevel = chunk.ActivationLevel,
                        Subsystem = "Primed"
                    }).ToList(),
                    CognitiveSteps = _coreServices.TimeManager.GetCognitiveSteps(),
                    AgentTime = _coreServices.TimeManager.GetAgentTime(),
                    Timestamp = DateTime.UtcNow
                };

                return Ok(memoryStatus);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting working memory");
                return StatusCode(500, "Failed to retrieve working memory");
            }
        }

        /// <summary>
        /// Get current context snapshot
        /// </summary>
        [HttpGet("context")]
        public async Task<ActionResult> GetContext()
        {
            try
            {
                var contextSnapshot = _coreServices.ContextContainer.CreateContextSnapshot();
                return Ok(new
                {
                    ContextSnapshot = contextSnapshot,
                    CategoryCount = contextSnapshot.Count,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting context");
                return StatusCode(500, "Failed to retrieve context");
            }
        }

        /// <summary>
        /// Manually activate a chunk
        /// </summary>
        [HttpPost("chunks/activate")]
        public async Task<ActionResult> ActivateChunk([FromBody] ActivateChunkRequest request)
        {
            try
            {
                if (!Guid.TryParse(request.ChunkId, out var chunkId))
                {
                    return BadRequest("Invalid chunk ID format");
                }

                var chunk = await _coreServices.MemorySystem.ActivateChunkAsync(
                    chunkId,
                    request.Emotion,
                    request.ActivationBoost);

                if (chunk == null)
                {
                    return NotFound("Chunk not found");
                }

                return Ok(new
                {
                    ChunkId = chunk.ID,
                    Name = chunk.Name,
                    ActivationLevel = chunk.ActivationLevel,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating chunk: {ChunkId}", request.ChunkId);
                return StatusCode(500, "Failed to activate chunk");
            }
        }

        /// <summary>
        /// Query chunks in memory
        /// </summary>
        [HttpPost("chunks/query")]
        public async Task<ActionResult> QueryChunks([FromBody] QueryChunksRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Query))
                {
                    return BadRequest("Query cannot be empty");
                }

                // Simple text-based search for now (like console app)
                var allChunks = await _coreServices.MemorySystem.GetTopActivatedChunksAsync(200, excludeWorkingMemory: false);
                var matchingChunks = allChunks.Where(c =>
                    (c.Name?.ToLower().Contains(request.Query.ToLower()) == true) ||
                    (c.ChunkType?.ToLower().Contains(request.Query.ToLower()) == true) ||
                    (c.SemanticType?.ToLower().Contains(request.Query.ToLower()) == true))
                    .Take(request.MaxResults)
                    .ToList();

                var results = matchingChunks.Select(chunk => new ChunkInfo
                {
                    Id = chunk.ID,
                    Name = chunk.Name,
                    ChunkType = chunk.ChunkType,
                    SemanticType = chunk.SemanticType,
                    ActivationLevel = chunk.ActivationLevel,
                    Slots = chunk.Slots.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Value
                    )
                }).ToList();

                return Ok(new
                {
                    Query = request.Query,
                    ResultCount = results.Count,
                    Results = results,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error querying chunks: {Query}", request.Query);
                return StatusCode(500, "Failed to query chunks");
            }
        }

        /// <summary>
        /// Get system status
        /// </summary>
        [HttpGet("status")]
        public async Task<ActionResult<SystemStatus>> GetStatus()
        {
            try
            {
                var workingMemory = await _coreServices.MemorySystem.GetWorkingMemoryContentsAsync();
                var primedChunks = await _coreServices.MemorySystem.GetPrimedChunksAsync();

                var status = new SystemStatus
                {
                    Status = "Running",
                    CognitiveSteps = _coreServices.TimeManager.GetCognitiveSteps(),
                    AgentTime = _coreServices.TimeManager.GetAgentTime(),
                    WorkingMemoryCount = workingMemory.Count,
                    PrimedCount = primedChunks.Count,
                    Timestamp = DateTime.UtcNow
                };

                return Ok(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting system status");
                return StatusCode(500, "Failed to retrieve system status");
            }
        }

        /// <summary>
        /// Get detailed chunk information
        /// </summary>
        [HttpGet("chunks/{chunkId}")]
        public async Task<ActionResult<ChunkInfo>> GetChunk(string chunkId)
        {
            try
            {
                if (!Guid.TryParse(chunkId, out var id))
                {
                    return BadRequest("Invalid chunk ID format");
                }

                var chunk = await _coreServices.MemorySystem.GetChunkAsync(id);
                if (chunk == null)
                {
                    return NotFound("Chunk not found");
                }

                var chunkInfo = new ChunkInfo
                {
                    Id = chunk.ID,
                    Name = chunk.Name,
                    ChunkType = chunk.ChunkType,
                    SemanticType = chunk.SemanticType,
                    ActivationLevel = chunk.ActivationLevel,
                    Slots = chunk.Slots.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Value
                    )
                };

                return Ok(chunkInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting chunk: {ChunkId}", chunkId);
                return StatusCode(500, "Failed to retrieve chunk");
            }
        }

        /// <summary>
        /// Force a working memory refresh cycle
        /// </summary>
        [HttpPost("memory/refresh")]
        public async Task<ActionResult> RefreshMemory([FromBody] List<string> focusedChunkIds = null)
        {
            try
            {
                var focusedIds = new List<Guid>();
                if (focusedChunkIds != null)
                {
                    foreach (var idStr in focusedChunkIds)
                    {
                        if (Guid.TryParse(idStr, out var id))
                        {
                            focusedIds.Add(id);
                        }
                    }
                }

                await _coreServices.MemorySystem.ManualRefreshCycleAsync(focusedIds.Any() ? focusedIds : null);

                return Ok(new
                {
                    Message = "Memory refresh completed",
                    FocusedChunks = focusedIds.Count,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing memory");
                return StatusCode(500, "Failed to refresh memory");
            }
        }
    }
}