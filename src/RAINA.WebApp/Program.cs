using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RAINA.Web.Hubs;
using RAINA.Web.Services;
using RAINA.Services;
using Aislinn.Core;
using Aislinn.Configuration;
using Aislinn.Core.Query;
using Aislinn.Core.Models;
using Aislinn.Core.Cognitive;
using System.Runtime.CompilerServices;

namespace RAINA.Web
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configure logging
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();
            builder.Logging.AddDebug();

            // Add services to the container
            ConfigureServicesAsync(builder.Services);

            var app = builder.Build();

            // Configure the HTTP request pipeline
            ConfigurePipeline(app);



            await app.RunAsync();
        }

        private static async Task ConfigureServicesAsync(IServiceCollection services)
        {
            // Add CORS for frontend
            services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    policy.WithOrigins("http://localhost:3000", "http://localhost:5173") // React dev servers
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials();
                });
            });

            // Add SignalR
            services.AddSignalR(options =>
            {
                options.EnableDetailedErrors = true;
                options.KeepAliveInterval = TimeSpan.FromSeconds(10);
                options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
            });

            // Add controllers
            services.AddControllers();

            // Add Swagger/OpenAPI
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new() { Title = "RAINA API", Version = "v1" });
            });

            // Configure RAINA services using the bootstrapper
            var rainaServiceCollection = new ServiceCollection();
            var bootstrapper = new RainaBootstrapper(rainaServiceCollection);

            // Load configuration
            string openAIApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
            string voyageAPIKey = Environment.GetEnvironmentVariable("VOYAGE_API_KEY");

            if (string.IsNullOrEmpty(openAIApiKey) || string.IsNullOrEmpty(voyageAPIKey))
            {
                throw new InvalidOperationException("Please set OPENAI_API_KEY and VOYAGE_API_KEY environment variables");
            }
            var config = new RainaConfiguration();
            config.ChunkCollectionId = "raina_main";
            config.AssociationCollectionId = "raina_associations";
            config.OpenAIApiKey = openAIApiKey;
            var voyageConfig = new VoyageConfiguration();
            voyageConfig.VoyageApiKey = voyageAPIKey;
            // Build RAINA services
            var rainaServiceProvider = bootstrapper
                .ConfigureWithSettings(config, voyageConfig)
                .ConfigureChunkMemorySystem()
                .ConfigureCore()
                .RegisterStandardModules()
                .ConfigureIntegrations()
                .Build();

            Console.WriteLine("Initializing RAINA services...");

            // Get services
            var rainaServices = rainaServiceProvider.GetRequiredService<RainaServices>();
            var webEventSubscriber = rainaServiceProvider.GetRequiredService<WebEventSubscriber>();
            var userContextManager = rainaServiceProvider.GetRequiredService<UserContextManager>();
            // Initialize conversation manager
            rainaServices.ConversationManager.Init();

            // Subscribe to events
            webEventSubscriber.Subscribe();

            Console.WriteLine("RAINA Web API initialized successfully");

            var defaultContext = new UserContext
            {
                UserName = "Web User",
                UserId = "web_default",
                CurrentTopic = "general"
            };
            var workingMemoryController = rainaServiceProvider.GetRequiredService<WorkingMemoryController>();
            var chunkManager = rainaServiceProvider.GetRequiredService<ChunkManager>();
            var chunkQueryService = rainaServiceProvider.GetRequiredService<ChunkQueryService>();
            rainaServices.EntityExtractionService.LoadCacheAsync("relationship_cache.json").Wait();
            // Load user context like console app does
            await LoadUserContextAsync(defaultContext, workingMemoryController, chunkManager, chunkQueryService);
            await rainaServices.ConversationManager.InitializeConversationAsync(defaultContext);
            // Register RAINA services in the main DI container
            services.AddSingleton(provider => rainaServiceProvider.GetRequiredService<AislinnCoreServices>());
            services.AddSingleton(provider => rainaServiceProvider.GetRequiredService<RainaServices>());
            services.AddSingleton(provider => rainaServiceProvider.GetRequiredService<IntentProcessor>());

            // Add web-specific services
            services.AddSingleton<WebEventSubscriber>();
            services.AddSingleton<UserContextManager>();
        }



        private static void ConfigurePipeline(WebApplication app)
        {
            // Configure the HTTP request pipeline
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseCors("AllowFrontend");
            app.UseRouting();
            app.UseAuthorization();

            // Map controllers and hubs
            app.MapControllers();
            app.MapHub<RainaHub>("/rainahub");

            // Health check endpoint
            app.MapGet("/health", () => new { Status = "Healthy", Timestamp = DateTime.UtcNow });
        }

        private async static Task LoadUserContextAsync(UserContext userContext, WorkingMemoryController workingMemoryController, ChunkManager chunkManager, ChunkQueryService queryService)
        {
            // lookup the speaker
            var query = new ChunkQuery
            {
                ChunkType = "Declarative",
                SemanticType = "entity.person.instance",
                Name = userContext.UserName,  // Variable containing the username
                NameHandling = NameMatchType.ExactMatch,  // Require exact name match
                ExtraSlotsHandling = ExtraSlotsHandling.Ignore,  // Ignore additional slots
                MinimumThreshold = 0.9  // Set high threshold since we're doing exact matching
            };

            var results = await queryService.ExecuteQueryAsync(query);
            if (results.Count == 0)
            {
                var personChunk = await chunkManager.CreateChunkAsync("Declarative", "entity.person.instance", userContext.UserName, new Dictionary<string, object> { { "Name", userContext.UserName }, { "Role", "User" } });
                userContext.UserChunk = personChunk;
                Console.WriteLine($"Created new user chunk for {userContext.UserName}");
            }
            else
            {
                userContext.UserChunk = results.FirstOrDefault().Chunk;
                Console.WriteLine($"Loaded existing user chunk for {userContext.UserName}");
            }

            //add Raina
            query.Name = "Raina";
            query.SemanticType = "entity.person.instance"; //we should set this to something like entity.ai.cognitive.instance, but this makes the queries easier for now, we'll add a role for further filtering
            results = await queryService.ExecuteQueryAsync(query);
            if (results.Count == 0)
            {
                var personChunk = await chunkManager.CreateChunkAsync("Declarative", "entity.person.instance", userContext.UserName, new Dictionary<string, object> { { "Name", userContext.UserName }, { "Role", "AI Assistant" } });
                userContext.RainaChunk = personChunk;
                Console.WriteLine($"Created new user chunk for {userContext.UserName}");
            }
            else
            {
                userContext.RainaChunk = results.FirstOrDefault().Chunk;
                Console.WriteLine($"Loaded existing user chunk for {userContext.UserName}");
            }


            // Load active chunks into user context
            var activeChunks = await workingMemoryController.GetActiveChunksAsync();
            userContext.ActiveMemoryChunks = activeChunks;

            Console.WriteLine($"Loaded {activeChunks.Count} active memory chunks into context");
        }
    }

    /// <summary>
    /// Manages user contexts for web sessions
    /// </summary>
    public class UserContextManager
    {
        private readonly Dictionary<string, UserContext> _userContexts = new();
        private readonly RainaServices _rainaServices;

        public UserContextManager(RainaServices rainaServices)
        {
            _rainaServices = rainaServices;
        }



        public UserContext GetOrCreateContext(string sessionId)
        {
            if (!_userContexts.TryGetValue(sessionId, out var context))
            {
                context = new UserContext
                {
                    UserName = $"User_{sessionId[..8]}",
                    // Initialize other properties
                };
                _userContexts[sessionId] = context;
            }
            return context;
        }

        public UserContext GetDefaultContext()
        {
            return _userContexts.GetValueOrDefault("default");
        }
    }
    public class ChunkManager
    {
        private readonly CognitiveMemorySystem _memorySystem;

        public ChunkManager(CognitiveMemorySystem memorySystem)
        {
            _memorySystem = memorySystem;
        }

        public async Task<Chunk> CreateChunkAsync(string chunkType, string semanticType, string name, Dictionary<string, object> slots = null)
        {
            var chunk = new Chunk
            {
                ChunkType = chunkType,
                SemanticType = semanticType,
                Name = name,
                Slots = new Dictionary<string, ModelSlot>()
            };

            if (slots != null)
            {
                foreach (var slot in slots)
                {
                    chunk.Slots[slot.Key] = new ModelSlot
                    {
                        Name = slot.Key,
                        Value = slot.Value
                    };
                }
            }

            return await _memorySystem.AddChunkAsync(chunk);
        }

        public async Task<Chunk> GetChunkAsync(Guid chunkId)
        {
            return await _memorySystem.GetChunkAsync(chunkId);
        }
    }
}