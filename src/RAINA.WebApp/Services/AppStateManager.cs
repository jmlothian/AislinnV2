using RAINA.Services;
using Aislinn.Core;
using Aislinn.Core.Models;
using Aislinn.Core.Query;
using Aislinn.Core.Services;
using System.Text.Json;

namespace RAINA.Web.Services
{
    public class UserSession
    {
        public string Username { get; set; }
        public DateTime LoginTime { get; set; }
        public DateTime LastActivity { get; set; }
        public UserContext Context { get; set; }
    }

    public class SessionData
    {
        public Dictionary<string, UserSession> Sessions { get; set; } = new();
        public DateTime LastCleanup { get; set; } = DateTime.UtcNow;
    }
    /// <summary>
    /// Manages application state, user contexts, and handles proper startup/shutdown
    /// </summary>
    public class AppStateManager : IHostedService
    {
        private readonly Dictionary<string, UserContext> _activeUsers = new();
        private readonly ConversationManager _conversationManager;
        private readonly EntityRelationshipExtractionService _entityService;
        private readonly AislinnCoreServices _coreServices;
        private readonly ChunkManager _chunkManager;
        private readonly ChunkQueryService _queryService;
        private readonly WorkingMemoryController _workingMemoryController;
        private readonly ILogger<AppStateManager> _logger;
        private readonly object _lockObject = new object();
        private readonly Timer _cleanupTimer;
        private readonly string _sessionsFilePath = "user_sessions.json";
        private readonly TimeSpan _sessionTimeout = TimeSpan.FromHours(6);

        public AppStateManager(
            ConversationManager conversationManager,
            EntityRelationshipExtractionService entityService,
            AislinnCoreServices coreServices,
            ChunkManager chunkManager,
            ChunkQueryService queryService,
            WorkingMemoryController workingMemoryController,
            ILogger<AppStateManager> logger)
        {
            Console.WriteLine("...Starting AppStateMAnager");

            _conversationManager = conversationManager;
            _entityService = entityService;
            _coreServices = coreServices;
            _chunkManager = chunkManager;
            _queryService = queryService;
            _workingMemoryController = workingMemoryController;
            _logger = logger;
            _logger.LogInformation("AppStateManager Constructed");
            _cleanupTimer = new Timer(OnCleanupTimer, null, TimeSpan.FromHours(1), TimeSpan.FromHours(1));

        }

        /// <summary>
        /// Startup is handled by DI, so no initialization needed here
        /// </summary>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("AppStateManager started - user contexts ready");
            Console.WriteLine("AppStateManager started - you may begin.");
            await LoadSessionsFromDiskAsync();
        }

        /// <summary>
        /// Handle user login and create/load their context
        /// </summary>
        public async Task<UserContext> LoginUserAsync(string username)
        {
            lock (_lockObject)
            {
                if (_activeUsers.ContainsKey(username))
                {
                    _logger.LogInformation("User {Username} already has active context", username);
                    return _activeUsers[username];
                }
            }

            try
            {
                _logger.LogInformation("Creating context for user: {Username}", username);

                var userContext = new UserContext
                {
                    UserId = username,
                    UserName = username,
                    CurrentTopic = "general",

                };
                userContext.LoginTime = DateTime.UtcNow;
                userContext.LastActivity = DateTime.UtcNow;
                // Load or create user chunks (similar to console app LoadUserContextAsync)
                await LoadUserContextAsync(userContext);

                // Initialize conversation for this user
                await _conversationManager.InitializeConversationAsync(userContext);

                lock (_lockObject)
                {
                    _activeUsers[username] = userContext;

                }
                await SaveSessionsToDiskAsync();

                _logger.LogInformation("Successfully created context for user: {Username}", username);
                return userContext;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating context for user: {Username}", username);
                throw;
            }
        }

        /// <summary>
        /// Handle user logout and cleanup their context
        /// </summary>
        public async Task LogoutUserAsync(string username)
        {
            lock (_lockObject)
            {
                if (!_activeUsers.ContainsKey(username))
                {
                    _logger.LogWarning("User {Username} does not have active context", username);
                    return;
                }

                _activeUsers.Remove(username);

            }
            await SaveSessionsToDiskAsync();

            try
            {
                _logger.LogInformation("Cleaning up context for user: {Username}", username);

                // TODO: Save any user-specific state here if needed
                // For now, just log the logout

                _logger.LogInformation("Successfully cleaned up context for user: {Username}", username);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up context for user: {Username}", username);
            }
        }

        /// <summary>
        /// Get the context for a user (must be logged in)
        /// </summary>
        public UserContext GetUserContext(string username)
        {
            lock (_lockObject)
            {
                if (_activeUsers.TryGetValue(username, out var context))
                {
                    return context;
                }
            }

            throw new InvalidOperationException($"User {username} is not logged in or context not found");
        }

        /// <summary>
        /// Check if a user has an active context
        /// </summary>
        public bool IsUserLoggedIn(string username)
        {
            lock (_lockObject)
            {
                return _activeUsers.ContainsKey(username);
            }
        }

        /// <summary>
        /// Get all active users
        /// </summary>
        public string[] GetActiveUsers()
        {
            lock (_lockObject)
            {
                return _activeUsers.Keys.ToArray();
            }
        }

        /// <summary>
        /// Proper shutdown handling like console app
        /// </summary>
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("AppStateManager shutting down...");

            try
            {
                // Shutdown conversation manager
                _conversationManager.Shutdown();
                _logger.LogInformation("Conversation manager shutdown completed");

                // Save entity relationship cache
                await _entityService.SaveCacheAsync("relationship_cache.json");
                _logger.LogInformation("Entity relationship cache saved");

                await SaveSessionsToDiskAsync();
                // Clear active users
                lock (_lockObject)
                {
                    var userCount = _activeUsers.Count;


                    // Dispose cleanup timer
                    _cleanupTimer?.Dispose();
                    _activeUsers.Clear();
                    _logger.LogInformation("Cleared {UserCount} active user contexts", userCount);
                }

                _logger.LogInformation("AppStateManager shutdown completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during AppStateManager shutdown");
            }
        }
        private async Task LoadSessionsFromDiskAsync()
        {
            try
            {
                if (!File.Exists(_sessionsFilePath))
                {
                    _logger.LogInformation("No existing sessions file found");
                    return;
                }

                var json = await File.ReadAllTextAsync(_sessionsFilePath);
                var sessionData = JsonSerializer.Deserialize<SessionData>(json);

                if (sessionData?.Sessions != null)
                {
                    var validSessions = sessionData.Sessions.Values
                        .Where(s => DateTime.UtcNow - s.LastActivity < _sessionTimeout)
                        .ToList();

                    lock (_lockObject)
                    {
                        _activeUsers.Clear();
                        foreach (var session in validSessions)
                        {
                            _activeUsers[session.Username] = session.Context;
                            _logger.LogInformation("Restored session for user: {Username}", session.Username);
                        }
                    }

                    _logger.LogInformation("Loaded {ValidCount} valid sessions, expired {ExpiredCount}",
                        validSessions.Count, sessionData.Sessions.Count - validSessions.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading sessions from disk");
            }
        }

        private async Task SaveSessionsToDiskAsync()
        {
            try
            {
                var sessions = new Dictionary<string, UserSession>();

                lock (_lockObject)
                {
                    foreach (var kvp in _activeUsers)
                    {
                        sessions[kvp.Key] = new UserSession
                        {
                            Username = kvp.Key,
                            Context = kvp.Value,
                            LoginTime = kvp.Value.LoginTime ?? DateTime.UtcNow,
                            LastActivity = DateTime.UtcNow
                        };
                    }
                }

                var sessionData = new SessionData
                {
                    Sessions = sessions,
                    LastCleanup = DateTime.UtcNow
                };

                var json = JsonSerializer.Serialize(sessionData, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(_sessionsFilePath, json);

                _logger.LogDebug("Saved {SessionCount} sessions to disk", sessions.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving sessions to disk");
            }
        }

        private async Task CleanupExpiredSessionsAsync()
        {
            try
            {
                var expiredUsers = new List<string>();

                lock (_lockObject)
                {
                    foreach (var kvp in _activeUsers.ToList())
                    {
                        var lastActivity = kvp.Value.LastActivity ?? kvp.Value.LoginTime ?? DateTime.UtcNow;
                        if (DateTime.UtcNow - lastActivity > _sessionTimeout)
                        {
                            expiredUsers.Add(kvp.Key);
                            _activeUsers.Remove(kvp.Key);
                        }
                    }
                }

                foreach (var username in expiredUsers)
                {
                    _logger.LogInformation("Expired session for user: {Username}", username);
                }

                if (expiredUsers.Any())
                {
                    await SaveSessionsToDiskAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during session cleanup");
            }
        }

        private void OnCleanupTimer(object state)
        {
            _ = Task.Run(CleanupExpiredSessionsAsync);
        }
        /// <summary>
        /// Load user context (similar to console app logic)
        /// </summary>
        private async Task LoadUserContextAsync(UserContext userContext)
        {
            try
            {
                // Look up the user chunk
                var userQuery = new ChunkQuery
                {
                    ChunkType = "Declarative",
                    SemanticType = "entity.person.instance",
                    Name = userContext.UserName,
                    NameHandling = NameMatchType.ExactMatch,
                    ExtraSlotsHandling = ExtraSlotsHandling.Ignore,
                    MinimumThreshold = 0.9
                };

                var userResults = await _queryService.ExecuteQueryAsync(userQuery);
                if (userResults.Count == 0)
                {
                    var personChunk = await _chunkManager.CreateChunkAsync(
                        "Declarative",
                        "entity.person.instance",
                        userContext.UserName,
                        new Dictionary<string, object>
                        {
                            { "Name", userContext.UserName },
                            { "Role", "User" }
                        });
                    userContext.UserChunk = personChunk;
                    _logger.LogInformation("Created new user chunk for {UserName}", userContext.UserName);
                }
                else
                {
                    userContext.UserChunk = userResults.FirstOrDefault().Chunk;
                    _logger.LogInformation("Loaded existing user chunk for {UserName}", userContext.UserName);
                }

                // Look up RAINA chunk
                var rainaQuery = new ChunkQuery
                {
                    ChunkType = "Declarative",
                    SemanticType = "entity.person.instance",
                    Name = "Raina",
                    NameHandling = NameMatchType.ExactMatch,
                    ExtraSlotsHandling = ExtraSlotsHandling.Ignore,
                    MinimumThreshold = 0.9
                };

                var rainaResults = await _queryService.ExecuteQueryAsync(rainaQuery);
                if (rainaResults.Count == 0)
                {
                    var rainaChunk = await _chunkManager.CreateChunkAsync(
                        "Declarative",
                        "entity.person.instance",
                        "Raina",
                        new Dictionary<string, object>
                        {
                            { "Name", "Raina" },
                            { "Role", "AI Assistant" }
                        });
                    userContext.RainaChunk = rainaChunk;
                    _logger.LogInformation("Created new Raina chunk");
                }
                else
                {
                    userContext.RainaChunk = rainaResults.FirstOrDefault().Chunk;
                    _logger.LogInformation("Loaded existing Raina chunk");
                }

                // Load active chunks into user context
                var activeChunks = await _workingMemoryController.GetActiveChunksAsync();
                userContext.ActiveMemoryChunks = activeChunks;

                _logger.LogInformation("Loaded {ActiveChunkCount} active memory chunks into context for {UserName}",
                    activeChunks.Count, userContext.UserName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user context for {UserName}", userContext.UserName);
                throw;
            }
        }
    }
}