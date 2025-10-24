using System.Collections.Generic;

namespace RAINA.Models.Mcp
{
    /// <summary>
    /// Result from an MCP tool execution
    /// </summary>
    public class McpToolResult
    {
        public string ToolName { get; set; }
        public string Content { get; set; }
        public bool IsError { get; set; }
    }

    /// <summary>
    /// Simplified tool definition for prompting and in-process modules
    /// </summary>
    public class McpTool
    {
        public string Name { get; set; }
        public string Description { get; set; }

        /// <summary>
        /// Raw input schema from MCP server (for external servers) or manually defined (for in-process)
        /// This is typically a JSON Schema object
        /// </summary>
        public object InputSchema { get; set; }
    }

    /// <summary>
    /// MCP resource definition
    /// </summary>
    public class McpResource
    {
        public string Uri { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string MimeType { get; set; }
    }

    /// <summary>
    /// Content of an MCP resource
    /// </summary>
    public class McpResourceContent
    {
        public string Uri { get; set; }
        public string MimeType { get; set; }
        public string Text { get; set; }
        public byte[] Blob { get; set; }
    }

    /// <summary>
    /// MCP prompt definition
    /// </summary>
    public class McpPrompt
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<McpPromptArgument> Arguments { get; set; } = new List<McpPromptArgument>();
    }

    /// <summary>
    /// Argument for an MCP prompt
    /// </summary>
    public class McpPromptArgument
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Required { get; set; }
    }

    /// <summary>
    /// Connection details for an MCP server
    /// </summary>
    public class McpServerConnection
    {
        public string ServerName { get; set; }
        public bool IsInProcess { get; set; }
        public string Command { get; set; }
        public string[] Arguments { get; set; }
        public Dictionary<string, string> Environment { get; set; } = new Dictionary<string, string>();
    }
}