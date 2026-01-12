using System.ComponentModel;
using ModelContextProtocol.Server;

namespace RAINA.MCP.Memory;

[McpServerToolType]
public static class MemoryTools
{
    [McpServerTool, Description("Store important personal information, facts, or experiences in memory for future reference")]
    public static string Memory(
        [Description("Content to store in memory")] string content,
        [Description("Type or category of memory")] string type,
        [Description("Array of entities as key-value pairs")] string[]? entities = null)
    {
        // TODO: Implement memory storage logic
        return $"Memory stored: {content} (type: {type})";
    }
}