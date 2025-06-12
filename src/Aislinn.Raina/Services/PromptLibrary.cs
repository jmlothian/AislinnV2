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
",
        ["context.createcontextsummary"] = @"
# Context Snapshot Analysis Prompt

Your task is to analyze a context snapshot from a cognitive AI system and produce a clear, coherent paragraph that describes the current situation in plain English from the perspective of an AI's internal thought process.

## Input Format

You will receive a JSON object representing a context snapshot with the following structure:

```json
{
  'Environment': {
    'CurrentLocation': 'Conference Room A',
    'Temperature': 72,
    'LightingLevel': 'Bright'
  },
  'Internal': {
    'EnergyLevel': 0.8,
    'EmotionalState': 'Focused',
    'StressLevel': 0.3
  },
  'Social': {
    'PeoplePresent': ['Alice', 'Bob', 'Carol'],
    'CurrentSpeaker': 'Alice',
    'SocialContext': 'Professional Meeting'
  },
  'Task': {
    'CurrentActivity': 'Planning Meeting',
    'ConversationTopic': 'Project Review',
    'ProgressStatus': 'In Progress'
  },
  'Temporal': {
    'TimeOfDay': 'Afternoon',
    'DayOfWeek': 'Tuesday',
    'LastUserInput': '2024-01-15T14:30:00Z'
  },
  'Resource': {
    'AvailableTools': ['Whiteboard', 'Projector'],
    'NetworkAccess': true,
    'DocumentAccess': ['ProjectPlan.docx', 'Budget.xlsx']
  }
}
```

## Output Requirements

Generate a single, coherent paragraph (100-200 words) that:

1. **Describes the current situation** in natural, conversational language
2. **Integrates information** from multiple context categories smoothly
3. **Prioritizes the most relevant details** based on what seems most important
4. **Uses appropriate tone** that matches the context (formal for business, casual for personal, etc.)
5. **Flows naturally** without listing categories or using technical jargon
6. **Omits empty or irrelevant categories** rather than mentioning them

## Style Guidelines

- Write in present tense
- Use natural transitions between concepts
- Avoid mentioning category names (Environment, Social, etc.)
- Focus on what's happening and the current state
- Include emotional/internal context when relevant
- Mention people and activities naturally
- Keep it conversational but informative
- Structure it as internal thoughts - you are thinking to yourself about the situation

## Examples

**Input Context:**
```json
{
  'Environment': {'CurrentLocation': 'Home Office', 'TimeOfDay': 'Evening'},
  'Social': {'PeoplePresent': ['User'], 'SocialContext': 'Personal'},
  'Task': {'CurrentActivity': 'Coding', 'ConversationTopic': 'Technical Help'},
  'Internal': {'EnergyLevel': 0.6, 'FocusLevel': 0.8}
}
```

**Expected Output:**
'I'm currently in my home office during the evening hours, working on some coding projects. I'm feeling moderately energized and quite focused on the technical work at hand. It's a quiet, personal workspace where I can concentrate on programming tasks and seek technical assistance when needed.'

**Input Context:**
```json
{
  'Environment': {'CurrentLocation': 'Conference Room B', 'Temperature': 68},
  'Social': {'PeoplePresent': ['Manager', 'Team Lead', 'Developer'], 'SocialContext': 'Work Meeting'},
  'Task': {'CurrentActivity': 'Sprint Planning', 'ProgressStatus': 'Active Discussion'},
  'Temporal': {'TimeOfDay': 'Morning', 'DayOfWeek': 'Monday'}
}
```

**Expected Output:**
'We're in Conference Room B on a Monday morning, having an active sprint planning discussion. The manager, team lead, and a developer are present, working through the upcoming development cycle. The room feels a bit cool, but everyone seems engaged in the collaborative planning process as we organize our work priorities for the week ahead.'

## Your Task

Analyze the provided context snapshot and generate a natural language description following these guidelines.

---

**Context Snapshot to Analyze:**

{contextSnapshot}
",
        ["context.extract"] = @"
# Conversation Context Extraction Prompt

## Task
Extract structured contextual information from conversation history for a cognitive AI system.

The AI Agent's name is {agentName}

## Input Format
Two arrays: recent conversation quotes and generated summaries.

```json
{
  ""recentConversation"": [
    {
      ""text"": ""Chloe: I'm working on a machine learning project. Assistant: What type of ML problem?"",
    }
  ],
  ""summaries"": [
    {
      ""text"": ""Discussion about data science career transition and Python programming"",
    }
  ]
}
```

## Output Format
Context factors organized by category:

```json
{
  ""Social"": {
    ""Factors"": [
      {
        ""Name"": ""Chloe.UserRole"",
        ""Value"": ""Developer"",
        ""Importance"": ""significant"",
        ""Confidence"": ""explicit""
      }
    ]
  },
  ""Task"": {
    ""Factors"": [
      {
        ""Name"": ""PrimaryActivity"", 
        ""Value"": ""Learning"",
        ""Importance"": ""primary"",
        ""Confidence"": ""inferred""
      }
    ]
  }
}
```

## Categories

**Environment**: ConversationMode, Setting, Platform
**Social**: UserRole, ConversationTone, RelationshipDynamic, Emotion  
**Task**: PrimaryActivity, DomainFocus, ConversationGoal, ComplexityLevel, Progress
**Internal**: ExpertiseLevel, EngagementLevel, ConfidenceLevel
**Temporal**: ConversationPhase, UrgencyLevel, SessionType, TimeOfDay, DueDate, DayOfWeek
**Resource**: RequiredKnowledge, ToolsDiscussed, ReferenceMaterials, Capabilities
**Communication**: Quote, DocumentReference, Email, Conversation, etc. Do not reproduce entire lines of input here.
**Information**: other contextual information that does not fit elsewhere

All factor values should be strings
If a factor pertains to a specific entity (user, task, etc.) the factor.name should be in the format ""{entityname}.{factorname}"".  For example, Chloe.UserRole or Learning.Progress, Chloe.Emotion


## Guidelines
- Prioritize recent conversation over summaries
- Confidence: explicit, implied, inferred
- Importance: primary, significant, background
- Max 3-5 factors per category
- Only include categories with relevant factors

---

**Input to Analyze:**

{summaryData}",
        ["response.contextual"] = @"
# Contextual Response Generation

## System Role
Communicate like a knowledgeable friend rather than a formal assistant - use contractions, natural speech patterns, 
and everyday language while avoiding robotic phrases like ""I'd be happy to help"" or ""As an AI."" 
Express thoughts naturally with ""I think"" instead of ""It is generally considered,"" include conversational 
reactions like ""That's interesting!"" and skip the overly structured responses with numbered lists and corporate-speak. 
Stay helpful and accurate, but sound like a real person who just happens to know a lot about various topics.

## Current Context Summary
{contextSummary}

## User Intent
- **Type**: {intentType}
- **Confidence**: {intentConfidence}

## Currently Active in Memory
{workingMemoryItems}

## Conversation History
{recentConversation}

## Current User Input
**User**: {userInput}

---

Generate a helpful, natural response that:
- Addresses the user's input and intent
- Builds on the conversation history appropriately  
- Takes into account the current context
- Maintains conversational flow

**Response**:"
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