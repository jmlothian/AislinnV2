using System;
using System.Threading.Tasks;
using RAINA.Modules;

namespace RAINA.Modules.Implementations
{
    /// <summary>
    /// module for handling task creation and tracking
    /// </summary>
    public class TaskCreationIntentModule : IIntentModule
    {
        private readonly TaskManager _taskManager;
        private readonly ConversationManager _conversationManager;

        public TaskCreationIntentModule(TaskManager taskManager, ConversationManager conversationManager)
        {
            _taskManager = taskManager ?? throw new ArgumentNullException(nameof(taskManager));
            _conversationManager = conversationManager ?? throw new ArgumentNullException(nameof(conversationManager));
        }

        public string GetIntentType()
        {
            return "TaskCreation";
        }

        public string GetPromptDescription()
        {
            return "TaskCreation: User wants to create or track a task, to-do item, or project";
        }

        public string[] GetPromptExamples()
        {
            return new[]
            {
                "Remind me to call John tomorrow at 3pm",
                "Create a task to finish the quarterly report by Friday",
                "Add 'buy groceries' to my to-do list",
                "Track the Johnson project with a deadline of June 15th"
            };
        }

        public string[] GetExpectedEntities()
        {
            return new[]
            {
                "task",
                "deadline",
                "person",
                "project"
            };
        }

        public string[] GetExpectedParameters()
        {
            return new[]
            {
                "priority",
                "recurring",
                "category"
            };
        }

        public async Task<Response> HandleAsync(string userInput, Intent intent, UserContext context)
        {
            // Extract task details from intent entities and parameters
            var task = ExtractTaskFromIntent(intent);

            // Add task to the system
            var result = await _taskManager.CreateTaskAsync(task, context);

            // Generate confirmation response
            return await _conversationManager.GenerateTaskConfirmationAsync(result, context);
        }

        private RainaTask ExtractTaskFromIntent(Intent intent)
        {
            // Extract task details
            var task = new RainaTask();

            // Extract title from entities
            var titleEntity = intent.Entities.Find(e => e.Type == "task");
            if (titleEntity != null)
            {
                task.Title = titleEntity.Value;
            }

            // Extract due date if available
            var dateEntity = intent.Entities.Find(e => e.Type == "deadline");
            if (dateEntity != null)
            {
                // Parse date - would need proper implementation
                task.DueDate = DateTime.Now.AddDays(1); // Placeholder
            }

            // Extract priority if available
            if (intent.Parameters.TryGetValue("priority", out var priority))
            {
                task.Priority = priority;
            }

            return task;
        }
    }
}