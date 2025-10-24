using System.Collections.Generic;
using System.Threading.Tasks;
using RAINA.Models.Mcp;
using RAINA.Services;

namespace RAINA.Modules
{
    /// <summary>
    /// Interface for intent modules in RAINA. Intent modules represent MCP servers
    /// (either in-process or external) that handle specific intents.
    /// </summary>
    public interface IIntentModule
    {
        /// <summary>
        /// Gets the unique identifier for this MCP server/intent type
        /// </summary>
        string GetServerName();

        /// <summary>
        /// Indicates whether this module runs in-process or as an external MCP server
        /// </summary>
        bool IsInProcess { get; }

        /// <summary>
        /// Get the MCP server connection details (null for in-process modules)
        /// </summary>
        McpServerConnection GetServerConnection();

        /// <summary>
        /// Gets the description of this intent for use in the OpenAI prompt
        /// </summary>
        string GetPromptDescription();

        /// <summary>
        /// Gets examples of this intent type for use in the OpenAI prompt
        /// </summary>
        string[] GetPromptExamples();

        /// <summary>
        /// Gets a formatted string describing the tools for use in the OpenAI intent classification prompt.
        /// This allows each module to control how its tools are presented to the classifier.
        /// </summary>
        string GetToolsPromptSection();

        /// <summary>
        /// Extract arguments from the user context and intent for this module.
        /// This is called before tool execution to enrich arguments with contextual data.
        /// </summary>
        /// <param name="context">The current user context</param>
        /// <param name="intent">The classified intent</param>
        /// <returns>Dictionary of argument name to string value</returns>
        Dictionary<string, string> ExtractArgumentsFromContext(UserContext context, Intent intent);

        // MCP Protocol Methods

        /// <summary>
        /// List available tools (MCP protocol)
        /// For external modules, this queries the remote server.
        /// For in-process modules, this returns the implemented tools.
        /// </summary>
        Task<IList<McpTool>> ListToolsAsync();

        /// <summary>
        /// List available resources (MCP protocol)
        /// </summary>
        Task<IList<McpResource>> ListResourcesAsync();

        /// <summary>
        /// List available prompts (MCP protocol)
        /// </summary>
        Task<IList<McpPrompt>> ListPromptsAsync();

        /// <summary>
        /// Call a tool (MCP protocol)
        /// For in-process modules, this executes the tool directly.
        /// For external modules, this should not be called directly - use McpServerManager instead.
        /// </summary>
        /// <param name="toolName">Name of the tool to call</param>
        /// <param name="arguments">Arguments as string key-value pairs</param>
        Task<McpToolResult> CallToolAsync(string toolName, Dictionary<string, string> arguments);

        /// <summary>
        /// Read a resource (MCP protocol)
        /// </summary>
        /// <param name="uri">URI of the resource to read</param>
        Task<McpResourceContent> ReadResourceAsync(string uri);
    }
}