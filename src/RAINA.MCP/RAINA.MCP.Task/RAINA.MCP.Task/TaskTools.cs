using System.ComponentModel;
using ModelContextProtocol.Server;

namespace RAINA.MCP.Task;

[McpServerToolType]
public static class TaskTools
{
    [McpServerTool, Description("Create a new task")]
    public static string Add(
        [Description("Task title")] string title,
        [Description("Task description")] string? description = null,
        [Description("Array of entities as key-value pairs")] string[]? entities = null)
    {
        // TODO: Implement task creation logic
        return $"Task created: {title}";
    }

    [McpServerTool, Description("Remove an existing task")]
    public static string Delete(
        [Description("Task ID to delete")] string taskId,
        [Description("Array of entities as key-value pairs")] string[]? entities = null)
    {
        // TODO: Implement task deletion logic
        return $"Task deleted: {taskId}";
    }

    [McpServerTool, Description("Mark a task as completed")]
    public static string SetDone(
        [Description("Task ID to mark as done")] string taskId,
        [Description("Array of entities as key-value pairs")] string[]? entities = null)
    {
        // TODO: Implement task status update logic
        return $"Task marked as done: {taskId}";
    }

    [McpServerTool(Name = "set_in_progress"), Description("Mark a task as currently being worked on")]
    public static string SetInProgress(
        [Description("Task ID to mark as in progress")] string taskId,
        [Description("Array of entities as key-value pairs")] string[]? entities = null)
    {
        // TODO: Implement task status update logic
        return $"Task marked as in progress: {taskId}";
    }

    [McpServerTool, Description("Move a task to the backlog")]
    public static string SetBacklog(
        [Description("Task ID to move to backlog")] string taskId,
        [Description("Array of entities as key-value pairs")] string[]? entities = null)
    {
        // TODO: Implement task status update logic
        return $"Task moved to backlog: {taskId}";
    }

    [McpServerTool, Description("Change the title of a task")]
    public static string UpdateTitle(
        [Description("Task ID to update")] string taskId,
        [Description("New title for the task")] string newTitle,
        [Description("Array of entities as key-value pairs")] string[]? entities = null)
    {
        // TODO: Implement task title update logic
        return $"Task title updated: {taskId} -> {newTitle}";
    }

    [McpServerTool, Description("Change the description of a task")]
    public static string UpdateDescription(
        [Description("Task ID to update")] string taskId,
        [Description("New description for the task")] string newDescription,
        [Description("Array of entities as key-value pairs")] string[]? entities = null)
    {
        // TODO: Implement task description update logic
        return $"Task description updated: {taskId}";
    }

    [McpServerTool, Description("Retrieve a list of tasks")]
    public static string GetTasks(
        [Description("Optional filter criteria")] string? filter = null,
        [Description("Array of entities as key-value pairs")] string[]? entities = null)
    {
        // TODO: Implement task listing logic
        return "Tasks: [placeholder list]";
    }

    [McpServerTool, Description("Retrieve detailed information about a specific task")]
    public static string GetTaskDetails(
        [Description("Task ID to retrieve details for")] string taskId,
        [Description("Array of entities as key-value pairs")] string[]? entities = null)
    {
        // TODO: Implement task detail retrieval logic
        return $"Task details for: {taskId}";
    }
}