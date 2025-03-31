using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Aislinn.ChunkStorage;
using Aislinn.ChunkStorage.Interfaces;
using Aislinn.ChunkStorage.Storage;
using Aislinn.Core.Activation;
using Aislinn.Core.Agent;
using Aislinn.Core.Cognitive;
using Aislinn.Core.Context;
using Aislinn.Core.Goals;
using Aislinn.Core.Goals.Execution;
using Aislinn.Core.Goals.Selection;
using Aislinn.Core.Procedural;
using Aislinn.Core.Relationships;
using Aislinn.Core.Services;
using Aislinn.VectorStorage.Interfaces;
using Aislinn.VectorStorage.Storage;
using Aislinn.Storage.AssociationStore;

namespace Aislinn
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            // Run the cognitive agent
            using (var scope = host.Services.CreateScope())
            {
                var agent = scope.ServiceProvider.GetRequiredService<CognitiveAgent>();
                await agent.InitializeAsync();

                // Simple demo loop
                Console.WriteLine("Aislinn Cognitive Agent initialized. Type 'exit' to quit.");

                while (true)
                {
                    Console.Write("> ");
                    string input = Console.ReadLine();

                    if (string.Equals(input, "exit", StringComparison.OrdinalIgnoreCase))
                        break;

                    await agent.ProcessInputAsync(input);
                    string response = await agent.GenerateResponseAsync();
                    Console.WriteLine(response);
                }
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    // Register storage services
                    services.AddSingleton<IChunkStore, ChunkStore>();
                    services.AddSingleton<IAssociationStore, AssociationStore>();
                    services.AddSingleton<IVectorizer, SimpleVectorizer>();
                    services.AddSingleton<VectorStore>();

                    // Register core services
                    services.AddSingleton<CognitiveTimeManager>();
                    services.AddSingleton<ActivationParametersRegistry>();

                    // Register activation model and service
                    services.AddSingleton<IActivationModel, ActRActivationModel>();
                    services.AddSingleton<ChunkActivationService>();

                    // Register memory system
                    services.AddSingleton<CognitiveMemorySystem>();

                    // Register relationship service
                    services.AddSingleton<RelationshipTraversalService>();

                    // Register context system
                    services.AddSingleton<ContextContainer>();

                    // Register goal systems
                    services.AddSingleton<GoalManagementService>();
                    services.AddSingleton<GoalSelectionService>();

                    // Register procedural system
                    services.AddSingleton<ProcedureMatcher>();
                    services.AddSingleton<GoalExecutionService>();

                    // Register the cognitive agent
                    services.AddSingleton<CognitiveAgent>();
                });
    }
}