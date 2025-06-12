# ContextContainer

A sophisticated context management system for cognitive architectures that maintains situational awareness by tracking and organizing contextual information that influences goal selection and execution.

## Overview

The `ContextContainer` provides a structured way to track environmental, social, temporal, and other contextual factors that affect an AI agent's decision-making. It automatically categorizes information, detects significant changes, and provides relevance scoring for goal-oriented behavior.

## Key Features

- **Categorized Context Tracking**: Organizes context into 6 main categories (Environment, Internal, Social, Task, Temporal, Resource)
- **Change Detection**: Automatically detects and reports significant contextual changes
- **Temporal Management**: Expires old context factors based on configurable retention times
- **Chunk Integration**: Links context factors to memory chunks for rich associations
- **Relevance Scoring**: Calculates how relevant current context is to specific goals

## Context Categories

```csharp
public enum ContextCategory
{
    Environment,   // Physical environment (location, objects, conditions)
    Internal,      // Agent's internal state (energy, emotions, physiological)
    Social,        // Social environment (people present, relationships, roles)
    Task,          // Task-related context (current activities, progress)
    Temporal,      // Time-related context (time of day, deadlines)
    Resource       // Available resources (tools, information, capabilities)
}
```

## Basic Usage

### Initialization

```csharp
var contextContainer = new ContextContainer(
    chunkStore: chunkStore,
    chunkCollectionId: "default",
    significantChangeThreshold: 0.3,  // 30% change triggers events
    contextRetentionTime: TimeSpan.FromHours(1)
);

// Subscribe to change notifications
contextContainer.SignificantContextChange += OnContextChanged;
```

### Adding Context Information

```csharp
// Update environmental context
contextContainer.UpdateContextFactor(
    ContextCategory.Environment,
    "CurrentLocation",
    "Conference Room A",
    importance: 0.8,
    confidence: 1.0
);

// Update social context
contextContainer.UpdateContextFactor(
    ContextCategory.Social,
    "PeoplePresent",
    new List<string> { "Alice", "Bob", "Carol" },
    importance: 0.6
);

// Update task context
contextContainer.UpdateContextFactor(
    ContextCategory.Task,
    "CurrentActivity",
    "Planning Meeting",
    importance: 0.9
);
```

### Retrieving Context

```csharp
// Get specific context factor
var location = contextContainer.GetContextValue<string>(
    ContextCategory.Environment,
    "CurrentLocation"
);

// Get all factors in a category
var allSocialFactors = contextContainer.GetCategoryFactors(ContextCategory.Social);

// Check if recent context exists
bool hasRecentLocation = contextContainer.HasRecentContextFactor(
    ContextCategory.Environment,
    "CurrentLocation",
    TimeSpan.FromMinutes(30)
);
```

## Integration with ConversationManager

Here's how to integrate `ContextContainer` with a conversation system:

### 1. Initialize in ConversationManager

```csharp
public class ConversationManager
{
    private readonly ContextContainer _contextContainer;
    private readonly CognitiveMemorySystem _memorySystem;

    public ConversationManager(
        CognitiveMemorySystem memorySystem,
        ContextContainer contextContainer)
    {
        _memorySystem = memorySystem;
        _contextContainer = contextContainer;

        // Subscribe to context changes to adjust conversation behavior
        _contextContainer.SignificantContextChange += OnContextChanged;
    }
}
```

### 2. Update Context During Conversations

```csharp
public async Task<Response> GenerateResponseAsync(string userInput, Intent intent, UserContext context)
{
    // Update conversational context
    _contextContainer.UpdateContextFactor(
        ContextCategory.Social,
        "CurrentSpeaker",
        context.UserName,
        importance: 0.7
    );

    _contextContainer.UpdateContextFactor(
        ContextCategory.Task,
        "ConversationTopic",
        intent?.IntentType ?? "General",
        importance: 0.6
    );

    _contextContainer.UpdateContextFactor(
        ContextCategory.Temporal,
        "LastUserInput",
        DateTime.Now,
        importance: 0.5
    );

    // Record user input as before...
    var userUtterance = await RecordUserInputAsync(userInput, intent, context);

    // Use context to inform response generation
    var contextSnapshot = _contextContainer.CreateContextSnapshot();

    // Generate contextually-aware response...
    string responseText = GenerateContextualResponse(userInput, intent, contextSnapshot);

    // Continue with response creation...
}
```

### 3. Context-Aware Response Generation

```csharp
private string GenerateContextualResponse(string userInput, Intent intent,
    Dictionary<ContextCategory, Dictionary<string, object>> contextSnapshot)
{
    var socialContext = contextSnapshot[ContextCategory.Social];
    var taskContext = contextSnapshot[ContextCategory.Task];
    var temporalContext = contextSnapshot[ContextCategory.Temporal];

    // Adjust response based on context
    if (socialContext.ContainsKey("PeoplePresent") &&
        ((List<string>)socialContext["PeoplePresent"]).Count > 2)
    {
        // More formal response in group settings
        return GenerateFormalResponse(userInput, intent);
    }

    if (taskContext.ContainsKey("ConversationTopic") &&
        taskContext["ConversationTopic"].ToString() == "Emergency")
    {
        // Prioritize urgent responses
        return GenerateUrgentResponse(userInput, intent);
    }

    // Default response generation
    return GenerateStandardResponse(userInput, intent);
}
```

### 4. Update Context from Working Memory

```csharp
public async Task RefreshContextFromMemoryAsync()
{
    // Get currently active chunks from working memory
    var workingMemoryChunks = await _memorySystem.GetWorkingMemoryContentsAsync();

    // Let context container extract relevant context factors
    await _contextContainer.UpdateContextFromWorkingMemoryAsync(workingMemoryChunks);
}
```

### 5. Handle Context Changes

```csharp
private void OnContextChanged(object sender, ContextContainer.ContextChangeEventArgs e)
{
    // Log significant context changes
    Console.WriteLine($"Context changed: {e.Category}.{e.FactorName} " +
                     $"changed from {e.OldValue} to {e.NewValue} " +
                     $"(significance: {e.ChangeSignificance:P1})");

    // Trigger goal re-evaluation if context change is significant enough
    if (e.ChangeSignificance > 0.5)
    {
        // Notify other systems that context has changed significantly
        TriggerGoalReEvaluation(e);
    }
}
```

## Integration with Goal System

```csharp
// Goals can specify context requirements
var goal = new Chunk
{
    ChunkType = "Goal",
    Name = "ScheduleMeeting",
    Slots = new Dictionary<string, ModelSlot>
    {
        {
            "ContextRequirements",
            new ModelSlot
            {
                Value = new Dictionary<string, object>
                {
                    { "Social.PeoplePresent", 2 }, // Need at least 2 people
                    { "Environment.CurrentLocation", "Conference Room" }, // Need meeting room
                    { "Temporal.TimeOfDay", "Business Hours" } // During work hours
                }
            }
        }
    }
};

// Check goal relevance against current context
double relevance = _contextContainer.CalculateContextRelevance(goal);
if (relevance > 0.7)
{
    // Goal is highly relevant to current context
    await ExecuteGoal(goal);
}
```

## Best Practices

1. **Regular Updates**: Update context factors as new information becomes available
2. **Appropriate Importance**: Set importance values based on how much the factor should influence decisions
3. **Cleanup**: Regularly call `CleanupExpiredFactors()` to remove outdated context
4. **Event Handling**: Subscribe to `SignificantContextChange` events to react to important changes
5. **Categorization**: Use appropriate categories to organize context factors logically
6. **Memory Integration**: Link context factors to relevant chunks using `SourceChunkId` metadata

## Configuration Options

- **significantChangeThreshold**: Percentage change needed to trigger events (default: 0.3)
- **contextRetentionTime**: How long to keep context factors (default: 1 hour)
- **chunkCollectionId**: Which chunk collection to use for integration

This system provides a foundation for context-aware AI behavior, enabling more natural and appropriate responses based on the current situation.
