using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using RAINA.Models.Mcp;
using RAINA.Services;

namespace RAINA.Modules.Implementations
{
    /// <summary>
    /// Module for handling task creation and tracking via external MCP server
    /// </summary>
    public class TaskManagementIntentModule : IIntentModule
    {
        private readonly TaskManager _taskManager;

        public TaskManagementIntentModule(TaskManager taskManager)
        {
            _taskManager = taskManager ?? throw new ArgumentNullException(nameof(taskManager));
        }

        public string GetServerName() => "TaskManagement";

        public bool IsInProcess => false;

        public McpServerConnection GetServerConnection()
        {
            return new McpServerConnection
            {
                ServerName = "task",
                IsInProcess = false,
                Command = "dotnet",
                Arguments = new[] { "run", "--project", "../RAINA.MCP/RAINA.MCP.Task" }
            };
        }

        public string GetPromptDescription()
        {
            return "TaskManagement: User wants to create or manage a task, to-do item, or project";
        }

        public string[] GetPromptExamples()
        {
            return new[]
            {
                //"Remind me to call John tomorrow at 3pm", //reminders are different
                "Create a task to finish the quarterly report by Friday",
                "Add 'buy groceries' to my to-do list",
                "Track the Johnson project with a deadline of June 15th"
            };
        }

        public string GetToolsPromptSection()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Tools:");
            sb.AppendLine("  - add: Create a new task");
            sb.AppendLine("    Parameters: title (string), description (string), dueDate (string), priority (string)");
            sb.AppendLine("  - delete: Remove an existing task");
            sb.AppendLine("    Parameters: taskId (string)");
            sb.AppendLine("  - set_done: Mark a task as completed");
            sb.AppendLine("    Parameters: taskId (string)");
            sb.AppendLine("  - set_in_progress: Mark a task as currently being worked on");
            sb.AppendLine("    Parameters: taskId (string)");
            sb.AppendLine("  - set_backlog: Move a task to the backlog");
            sb.AppendLine("    Parameters: taskId (string)");
            sb.AppendLine("  - update_title: Change the title of a task");
            sb.AppendLine("    Parameters: taskId (string), title (string)");
            sb.AppendLine("  - update_description: Change the description of a task");
            sb.AppendLine("    Parameters: taskId (string), description (string)");
            sb.AppendLine("  - get_tasks: Retrieve a list of tasks");
            sb.AppendLine("    Parameters: status (string), limit (string)");
            sb.AppendLine("  - get_task_details: Retrieve detailed information about a specific task");
            sb.AppendLine("    Parameters: taskId (string)");
            sb.AppendLine("Expected Entities: task, deadline, person, project");
            sb.AppendLine("Expected Parameters: priority, recurring, category");
            return sb.ToString();
        }

        public Dictionary<string, string> ExtractArgumentsFromContext(UserContext context, Intent intent)
        {
            var args = new Dictionary<string, string>();

            // Add user information for task ownership
            if (!string.IsNullOrEmpty(context.UserId))
            {
                args["userId"] = context.UserId;
            }

            if (!string.IsNullOrEmpty(context.UserName))
            {
                args["userName"] = context.UserName;
            }

            // Add current timestamp for task creation
            args["timestamp"] = DateTime.Now.ToString("o"); // ISO 8601 format

            // Extract task details from entities if available
            var titleEntity = intent.Entities.Find(e => e.Type == "task");
            if (titleEntity != null)
            {
                args["title"] = titleEntity.Name;
            }

            // // Extract due date if available
            // var dateEntity = intent.Entities.Find(e => e.EntityType == "deadline");
            // if (dateEntity != null)
            // {
            //     // Parse date - would need proper implementation
            //     args["dueDate"] = DateTime.Now.AddDays(1).ToString("o"); // Placeholder
            // }

            // // Extract priority if available
            // if (intent.Parameters.TryGetValue("priority", out var priority))
            // {
            //     args["priority"] = priority;
            // }

            return args;
        }

        public async Task<IList<McpTool>> ListToolsAsync()
        {
            // Since this is an external MCP server, the actual tools will be provided by the server
            // This method would be called by McpServerManager which queries the external server
            // For now, return empty list - the external server will provide the real tools
            return new List<McpTool>();
        }

        public async Task<McpToolResult> CallToolAsync(string toolName, Dictionary<string, string> arguments)
        {
            // This should not be called directly for external modules
            // McpServerManager will call the external server instead
            throw new InvalidOperationException(
                "CallToolAsync should not be called directly on external modules. Use McpServerManager instead.");
        }

        public Task<IList<McpResource>> ListResourcesAsync()
        {
            // Resources will be provided by the external MCP server
            return Task.FromResult<IList<McpResource>>(new List<McpResource>());
        }

        public Task<IList<McpPrompt>> ListPromptsAsync()
        {
            // Prompts will be provided by the external MCP server
            return Task.FromResult<IList<McpPrompt>>(new List<McpPrompt>());
        }

        public Task<McpResourceContent> ReadResourceAsync(string uri)
        {
            throw new NotImplementedException("External MCP server handles resource reading");
        }

        // Keep these helper methods for future use if needed
        private RainaTask ExtractTaskFromIntent(Intent intent)
        {
            // Extract task details
            var task = new RainaTask();

            // Extract title from entities
            // we will need to add domains to our entity ontology list, this will likely change
            var titleEntity = intent.Entities.Find(e => e.Type == "task");
            if (titleEntity != null)
            {
                task.Title = titleEntity.Name;
            }

            // // Extract due date if available
            // var dateEntity = intent.Entities.Find(e => e.EntityType == "deadline");
            // if (dateEntity != null)
            // {
            //     // Parse date - would need proper implementation
            //     task.DueDate = DateTime.Now.AddDays(1); // Placeholder
            // }

            // // Extract priority if available
            // if (intent.Parameters.TryGetValue("priority", out var priority))
            // {
            //     task.Priority = priority;
            // }

            //get task type - addTask, deleteTask, setTaskToDone, setTaskToInProgress, setTaskToBacklog, updateTaskTitle, updateTaskDescription

            //get task details - title, description, dueDate, trelloLabels, priority

            return task;
        }
    }
}