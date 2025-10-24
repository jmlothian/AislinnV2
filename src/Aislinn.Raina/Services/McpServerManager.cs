using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using RAINA.Models.Mcp;
using RAINA.Modules;

namespace RAINA.Services
{
    /// <summary>
    /// Manages connections to both external and in-process MCP servers
    /// </summary>
    public class McpServerManager : IAsyncDisposable
    {
        private readonly Dictionary<string, McpClient> _externalClients = new Dictionary<string, McpClient>();
        private readonly Dictionary<string, IIntentModule> _inProcessModules = new Dictionary<string, IIntentModule>();
        private readonly List<IIntentModule> _allModules = new List<IIntentModule>();

        /// <summary>
        /// Register an intent module (either in-process or external)
        /// </summary>
        public void RegisterModule(IIntentModule module)
        {
            if (module == null) throw new ArgumentNullException(nameof(module));

            string serverName = module.GetServerName();
            if (string.IsNullOrEmpty(serverName))
                throw new ArgumentException("ServerName is required");

            _allModules.Add(module);

            if (module.IsInProcess)
            {
                _inProcessModules[serverName] = module;
            }
            else
            {
                var connection = module.GetServerConnection();
                if (connection == null)
                    throw new ArgumentException($"External module {serverName} must provide connection details");
            }
        }

        /// <summary>
        /// Get all registered modules
        /// </summary>
        public IEnumerable<IIntentModule> GetRegisteredModules()
        {
            return _allModules;
        }

        /// <summary>
        /// List available tools for a server
        /// </summary>
        public async Task<IList<McpTool>> ListToolsAsync(string serverName)
        {
            // Check in-process first
            if (_inProcessModules.TryGetValue(serverName, out var inProcessModule))
            {
                return await inProcessModule.ListToolsAsync();
            }

            // Otherwise get from external client
            var client = await GetOrCreateExternalClientAsync(serverName);
            var mcpTools = await client.ListToolsAsync();

            // Convert to our simplified format
            return mcpTools.Select(t => new McpTool
            {
                Name = t.Name,
                Description = t.Description,
                InputSchema = t.JsonSchema // Preserve raw schema
            }).ToList();
        }

        /// <summary>
        /// Execute a tool on an MCP server (in-process or external)
        /// </summary>
        /// <param name="serverName">Name of the MCP server</param>
        /// <param name="toolName">Name of the tool to execute</param>
        /// <param name="baseArguments">Base arguments from intent classification</param>
        /// <param name="context">User context for extracting additional arguments</param>
        /// <param name="intent">The classified intent</param>
        public async Task<McpToolResult> CallToolAsync(
            string serverName,
            string toolName,
            Dictionary<string, string> baseArguments,
            UserContext context,
            Intent intent)
        {
            // Get the module to extract context-based arguments
            var module = _allModules.FirstOrDefault(m => m.GetServerName() == serverName);
            if (module == null)
            {
                throw new InvalidOperationException($"MCP server '{serverName}' is not registered");
            }

            // Extract additional arguments from context
            var contextArguments = module.ExtractArgumentsFromContext(context, intent);

            // Merge arguments (base arguments take precedence over context-extracted ones)
            var mergedArguments = new Dictionary<string, string>(contextArguments);
            foreach (var arg in baseArguments)
            {
                mergedArguments[arg.Key] = arg.Value;
            }

            // Check if in-process or external
            if (_inProcessModules.TryGetValue(serverName, out var inProcessModule))
            {
                return await inProcessModule.CallToolAsync(toolName, mergedArguments);
            }

            // Call external client
            return await CallExternalToolAsync(serverName, toolName, mergedArguments);
        }

        /// <summary>
        /// List available resources for a server
        /// </summary>
        public async Task<IList<McpResource>> ListResourcesAsync(string serverName)
        {
            if (_inProcessModules.TryGetValue(serverName, out var inProcessModule))
            {
                return await inProcessModule.ListResourcesAsync();
            }

            var client = await GetOrCreateExternalClientAsync(serverName);
            var mcpResources = await client.ListResourcesAsync();

            return mcpResources.Select(r => new McpResource
            {
                Uri = r.Uri,
                Name = r.Name,
                Description = r.Description,
                MimeType = r.MimeType
            }).ToList();
        }

        /// <summary>
        /// List available prompts for a server
        /// </summary>
        public async Task<IList<McpPrompt>> ListPromptsAsync(string serverName)
        {
            if (_inProcessModules.TryGetValue(serverName, out var inProcessModule))
            {
                return await inProcessModule.ListPromptsAsync();
            }

            var client = await GetOrCreateExternalClientAsync(serverName);
            var mcpPrompts = await client.ListPromptsAsync();

            return mcpPrompts.Select(p => new McpPrompt
            {
                Name = p.Name,
                Description = p.Description,
                Arguments = p.Arguments?.Select(a => new McpPromptArgument
                {
                    Name = a.Name,
                    Description = a.Description,
                    Required = a.Required
                }).ToList() ?? new List<McpPromptArgument>()
            }).ToList();
        }

        /// <summary>
        /// Read a resource from a server
        /// </summary>
        public async Task<McpResourceContent> ReadResourceAsync(string serverName, string uri)
        {
            if (_inProcessModules.TryGetValue(serverName, out var inProcessModule))
            {
                return await inProcessModule.ReadResourceAsync(uri);
            }

            var client = await GetOrCreateExternalClientAsync(serverName);
            var result = await client.ReadResourceAsync(uri);

            // Convert to our format
            return new McpResourceContent
            {
                Uri = uri,
                MimeType = result.Contents[0].MimeType,
                Text = (result.Contents[0] as TextResourceContents)?.Text,
                Blob = (result.Contents[0] as BlobResourceContents)?.Blob != null
                    ? Convert.FromBase64String((result.Contents[0] as BlobResourceContents).Blob)
                    : null
            };
        }

        /// <summary>
        /// Call a tool on an external MCP server
        /// </summary>
        private async Task<McpToolResult> CallExternalToolAsync(
            string serverName,
            string toolName,
            Dictionary<string, string> arguments)
        {
            var client = await GetOrCreateExternalClientAsync(serverName);

            try
            {
                // Convert string dictionary to object dictionary for MCP client
                var objectArguments = arguments.ToDictionary(
                    kvp => kvp.Key,
                    kvp => (object)kvp.Value
                );

                var result = await client.CallToolAsync(
                    toolName: toolName,
                    arguments: objectArguments
                );

                return new McpToolResult
                {
                    ToolName = toolName,
                    Content = ExtractTextFromContent(result.Content),
                    IsError = result.IsError ?? false
                };
            }
            catch (Exception ex)
            {
                return new McpToolResult
                {
                    ToolName = toolName,
                    Content = $"Error executing tool: {ex.Message}",
                    IsError = true
                };
            }
        }

        /// <summary>
        /// Get or create an external MCP client
        /// </summary>
        private async Task<McpClient> GetOrCreateExternalClientAsync(string serverName)
        {
            if (_externalClients.TryGetValue(serverName, out var existingClient))
            {
                return existingClient;
            }

            var module = _allModules.FirstOrDefault(m => m.GetServerName() == serverName && !m.IsInProcess);
            if (module == null)
                throw new InvalidOperationException($"External MCP server '{serverName}' is not registered");

            var connection = module.GetServerConnection();

            var transport = new StdioClientTransport(new StdioClientTransportOptions
            {
                Command = connection.Command,
                Arguments = connection.Arguments,
            });

            var client = await McpClient.CreateAsync(transport);
            _externalClients[serverName] = client;

            return client;
        }

        /// <summary>
        /// Extract text content from MCP content blocks
        /// </summary>
        private string ExtractTextFromContent(IList<ContentBlock> content)
        {
            if (content == null || content.Count == 0)
                return string.Empty;

            // TODO: Handle multiple content blocks and different content types
            // For now, just get the first text block
            var textBlock = content[0] as TextContentBlock;
            return textBlock?.Text ?? string.Empty;
        }

        public async ValueTask DisposeAsync()
        {
            foreach (var client in _externalClients.Values)
            {
                if (client != null)
                {
                    await client.DisposeAsync();
                }
            }
            _externalClients.Clear();
        }
    }
}