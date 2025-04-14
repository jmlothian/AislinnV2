using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace RAINA.Services
{
    //quick and dirty way to manage prompt templates for now.
    public class PromptLibrary
    {
        // Dictionary to store all prompts
        private readonly Dictionary<string, string> _prompts;

        public PromptLibrary()
        {
            _prompts = new Dictionary<string, string>
            {
                // Add your prompt templates with named parameters
                ["task.createdata"] = @"
You are a task creation service for RAINA (Realtime Adaptive Intelligence Neural Assistant).  
Analyze the following user input and create the task creation data for a Trello card. The title should be descriptive when possible, and can be a paraphrased sentence.

It should inclue these details, only if they are known:
    trelloLabels: work, home, projects, or family
    dueDate: any assigned date
    title: title with enough information to know what it is at a glance, be verbose and use up to 10 words
    description: long description of task
    priority: none, low, medium, high, or critical

Use the following JSON:
{
  ""title"": ""Create charts for Nishino Project"",
  ""description"": ""Create charts for Nishino Project, consult with Sara for details"",
  ""dueDate"": null,
  ""trelloLabels"": [
    ""work""
  ],
  ""priority"": ""none"",
}

User input: ""{userInput}""
",
                ["task.tasktype"] = @"
You are a Task Management service for RAINA (Realtime Adaptive Intelligence Neural Assistant).
Analyze the user input to determine the task management type.  

Available types are: addTask, deleteTask, setTaskToDone, setTaskToInProgress, setTaskToBacklog, updateTaskTitle, updateTaskDescription 

Please return JSON in the following format:
{
    ""taskType"": ""setTaskToDone""
}
User input: ""{userInput}""
",
                ["CodeExplainer"] = @"
Explain the following code in simple terms:
{code}

Use language appropriate for a {level} level programmer.
"
            };
        }

        // Get a prompt with named parameters filled in
        public string HydratePrompt(string promptName, Dictionary<string, object> parameters)
        {
            if (!_prompts.TryGetValue(promptName, out var template))
            {
                throw new KeyNotFoundException($"Prompt '{promptName}' not found");
            }

            string result = template;

            // Replace each named parameter in the template
            foreach (var param in parameters)
            {
                result = result.Replace("{" + param.Key + "}", param.Value?.ToString() ?? string.Empty);
            }

            return result;
        }

        // Convenience method to create parameters dictionary
        public static Dictionary<string, object> Params(params object[] keyValuePairs)
        {
            if (keyValuePairs.Length % 2 != 0)
            {
                throw new ArgumentException("Parameters must be provided as key/value pairs");
            }

            var dict = new Dictionary<string, object>();

            for (int i = 0; i < keyValuePairs.Length; i += 2)
            {
                dict[keyValuePairs[i].ToString()] = keyValuePairs[i + 1];
            }

            return dict;
        }

        // Add a new prompt
        public void AddPrompt(string name, string template)
        {
            _prompts[name] = template;
        }
    }
}