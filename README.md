# Aislinn Cognitive Architecture

> **IMPORTANT DISCLAIMER**: This project is currently under active development and is not yet complete. Much of the code is being ported from V1 and reorganized into a new structure. Most components have not been tested. Expect frequent changes and potential instability as development continues.

**AISLINN**: **A**rtificial **I**ntelligence **S**ystems **L**everaging **I**ntuition and **N**eurocognitive **N**uance

Aislinn is a modular, extensible cognitive architecture designed for creating intelligent agents with human-like cognitive capabilities. Unlike many academic cognitive architectures focused on modeling specific aspects of cognition for research, Aislinn is built for practical applications, providing a comprehensive framework for general-purpose AI agents that can reason, learn, plan, and interact naturally with humans and environments.

## Key Features

- **Chunk-Based Memory System**: Inspired by cognitive science but engineered for practical applications
- **Spreading Activation Networks**: Dynamic activation flows through knowledge structures based on relevance
- **Emotional Modeling**: Implements the PAD (Pleasure-Arousal-Dominance) emotional model for realistic affective states
- **Goal Management System**: Hierarchical goal structures with sophisticated priority management
- **Procedural Knowledge**: Flexible representation of actions and procedures for accomplishing goals
- **Context-Aware Processing**: Maintains situational awareness that influences cognitive processes
- **Modular Design**: Extensible components that can be enhanced or replaced independently

## Core Components

### Memory Systems

- **Working Memory**: Human-like capacity limitations with interference and decay
- **Long-term Memory**: Persistent storage with activation-based retrieval
- **Associative Memory**: Relationship-based connections between knowledge chunks
- **Procedural Memory**: Action sequences with preconditions and effects

### Goal Management

- **Goal Templates**: Reusable patterns for goal instantiation
- **Goal Hierarchy**: Complex goals decompose into subgoals
- **Goal Selection**: Context-sensitive priority calculation
- **Goal Monitoring**: Tracks progress and completion criteria

### Procedural Execution

- **Procedure Matching**: Identifies procedures that can achieve specific goals
- **Execution Engine**: Manages the running of procedures with monitoring
- **Error Handling**: Sophisticated retry and recovery strategies
- **Resource Management**: Tracks and allocates resources needed for procedures

### Cognitive Context

- **Environmental Context**: Awareness of physical surroundings
- **Internal Context**: Mental and emotional states
- **Social Context**: Understanding of social dynamics and relationships
- **Task Context**: Current activities and their progress
- **Emotional Context**: PAD-based emotion modeling affecting decision-making and behavior

## Architecture Design

Aislinn is built as a modular system where components communicate through well-defined interfaces. This allows:

- **Extensibility**: Add new cognitive capabilities without disrupting existing ones
- **Customization**: Tailor the architecture for specific application domains
- **Experimentation**: Test different approaches to specific cognitive functions
- **Scalability**: Deploy configurations ranging from lightweight to comprehensive

## Getting Started

### Prerequisites

- .NET 9
- Any IDE supporting C# (Visual Studio, VS Code with C# extensions, etc.)

### Installation

1. Clone the repository:

   ```
   git clone https://github.com/jmlothian/AislinnV2.git
   ```

2. Open the solution in your preferred IDE

3. Build the solution:
   ```
   dotnet build
   ```

### Basic Usage

```csharp
// Initialize core services
var chunkStore = new ChunkStore();
var associationStore = new AssociationStore();
var timeManager = new CognitiveTimeManager();
var activationModel = new ActRActivationModel(timeManager);

// Create memory system
var memorySystem = new CognitiveMemorySystem(
    chunkStore,
    associationStore,
    activationModel,
    timeManager);

// Initialize goal management
var goalManager = new GoalManagementService(
    chunkStore,
    associationStore,
    new ChunkActivationService(chunkStore, associationStore, activationModel));

// Create a new goal
var goal = await goalManager.CreateGoalTemplateAsync(
    "LearnTopic",
    0.8,
    new List<string> { "topic" });

// Instantiate the goal
var learningGoal = await goalManager.InstantiateGoalAsync(
    goal.ID,
    new Dictionary<string, object> { { "topic", "AI Architecture" } });

// Start cognitive cycle
await memorySystem.StartWorkingMemoryRefresh();
```

## Applications

Aislinn is designed for creating intelligent agents across various domains:

- **Virtual Assistants**: Sophisticated agents with common-sense reasoning
- **Game Characters**: NPCs with believable behavior and memory
- **Simulation Agents**: Entities that behave realistically in virtual environments
- **Robotic Control**: Cognitive layer for robot decision-making
- **Interactive Storytelling**: Characters that adapt to narrative context

## Development Status

This project is a port and reorganization of Aislinn V1. The current codebase represents the architecture and structure of the system, but many components are still being integrated and tested. Contributors should note:

- Many classes have been ported but not yet tested in the new organization
- Interfaces may change as integration continues

## Acknowledgments

While Aislinn draws inspiration from cognitive architectures like ACT-R, Soar, and CLARION, it prioritizes practical application over strict cognitive modeling, providing a flexible foundation for building next-generation intelligent agents.
