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
using Microsoft.AspNetCore.SignalR;
using Azure.Monitor.OpenTelemetry.AspNetCore;

namespace RAINA.Web
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddOpenTelemetry().UseAzureMonitor(options =>
            {
                options.ConnectionString = "InstrumentationKey=327ff7f2-ea28-45c3-bd14-7b855dcd643f;IngestionEndpoint=https://eastus-8.in.applicationinsights.azure.com/;LiveEndpoint=https://eastus.livediagnostics.monitor.azure.com/;ApplicationId=d904bdef-7076-4f9b-b55c-b14dcb74e08e";
            });
            // Configure logging
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole(options =>
            {
                options.IncludeScopes = true;
            });
            builder.Logging.SetMinimumLevel(LogLevel.Debug);
            using ILoggerFactory loggerFactory =
                LoggerFactory.Create(builder =>
                    builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
            ILogger<Program> logger = loggerFactory.CreateLogger<Program>();
            using (logger.BeginScope("[scope is enabled]"))
            {
                logger.LogInformation("Logger Initialized");
            }
            // builder.Services.AddCors(options =>
            // {
            //     options.AddDefaultPolicy(policy =>
            //     {
            //         policy
            //             .AllowAnyOrigin() // ⚠️ Unsafe in production
            //             .AllowAnyHeader()
            //             .AllowAnyMethod();
            //     });
            // });
            // Add services to the container
            await ConfigureServicesAsync(builder.Services);

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

            // Add controllers
            services.AddControllers();
            // Add SignalR
            services.AddSignalR(options =>
            {
                options.EnableDetailedErrors = true;
                options.KeepAliveInterval = TimeSpan.FromSeconds(10);
                options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
            });
            //services.AddSingleton<RainaHub>();
            // After AddSignalR(), register the hub context
            //services.AddSingleton<IHubContext<RainaHub>>();

            // Add Swagger/OpenAPI
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new() { Title = "RAINA API", Version = "v1" });
            });

            services.AddSingleton<UserContextManager>();

            // Configure RAINA services using the bootstrapper
            //var rainaServiceCollection = new ServiceCollection();
            services.AddHostedService<WebEventSubscriber>();
            //remember, HostedServices aren't available for injection on their own
            services.AddSingleton<AppStateManager>();
            services.AddHostedService<AppStateManager>(provider => provider.GetRequiredService<AppStateManager>());
            services.AddSingleton<ChunkManager>();
            //rainaServiceCollection.AddSingleton<RainaHub>();
            // services.AddSingleton(provider => rainaServiceProvider.GetRequiredService<AislinnCoreServices>());
            // services.AddSingleton(provider => rainaServiceProvider.GetRequiredService<RainaServices>());
            // services.AddSingleton(provider => rainaServiceProvider.GetRequiredService<IntentProcessor>());

            // Add web-specific services
            services.AddSingleton<UserContextManager>();
            var bootstrapper = new RainaBootstrapper(services);

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
            Console.WriteLine("... RAINA Core Services Loaded");
            // Initialize conversation manager
            rainaServices.ConversationManager.Init();
            Console.WriteLine("... Conversation Manager Initialized");
            // Subscribe to events
            //var webEventSubscriber = rainaServiceProvider.GetRequiredService<WebEventSubscriber>();
            //webEventSubscriber.Subscribe();

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
            //app.UseCors();

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
        private readonly AislinnCoreServices _coreServices;

        public UserContextManager(RainaServices rainaServices, AislinnCoreServices coreServices)
        {
            _rainaServices = rainaServices;
            _coreServices = coreServices;
        }



        public async Task<UserContext> GetOrCreateContext(string sessionId)
        {
            var userContext = new UserContext
            {
                UserId = "user1",
                UserName = sessionId,
                CurrentTopic = "general"
            };
            try
            {
                // Try to find existing user chunk
                var userChunk = await _coreServices.MemorySystem.FindChunkBySemanticTypeAndName("entity.person.instance", userContext.UserName);
                if (userChunk != null)
                {
                    userContext.UserChunk = userChunk;
                    Console.WriteLine($"Loaded existing user context for: {userContext.UserName}");
                }
                else
                {
                    // Create new user chunk
                    var newUserChunk = new Chunk
                    {
                        ChunkType = "Declarative",
                        CognitiveCategory = "Instance",
                        SemanticType = "entity.person.instance",
                        Name = userContext.UserName,
                        Slots = new Dictionary<string, ModelSlot>
                        {
                            { "EntityName", new ModelSlot { Name = "EntityName", Value = userContext.UserName } },
                            { "Role", new ModelSlot { Name = "Role", Value = "User" } },
                            { "CreatedTimestamp", new ModelSlot { Name = "CreatedTimestamp", Value = DateTime.Now } }
                        }
                    };

                    userContext.UserChunk = await _coreServices.MemorySystem.AddChunkAsync(newUserChunk);
                    Console.WriteLine($"Created new user context for: {userContext.UserName}");
                }

                // Try to find RAINA chunk
                var rainaChunk = await _coreServices.MemorySystem.FindChunkBySemanticTypeAndName("entity.person.instance", "Raina");
                if (rainaChunk == null)
                {
                    var newRainaChunk = new Chunk
                    {
                        ChunkType = "Declarative",
                        CognitiveCategory = "Instance",
                        SemanticType = "entity.person.instance",
                        Name = "Raina",
                        Slots = new Dictionary<string, ModelSlot>
                        {
                            { "EntityName", new ModelSlot { Name = "EntityName", Value = "Raina" } },
                            { "Role", new ModelSlot { Name = "Role", Value = "AI Assistant" } },
                            { "CreatedTimestamp", new ModelSlot { Name = "CreatedTimestamp", Value = DateTime.Now } }
                        }
                    };

                    userContext.RainaChunk = await _coreServices.MemorySystem.AddChunkAsync(newRainaChunk);
                }
                else
                {
                    userContext.RainaChunk = rainaChunk;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading user context: {ex.Message}");
            }
            return userContext;

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