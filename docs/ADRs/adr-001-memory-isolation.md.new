# ADR-001: Memory Isolation and Multi-User Architecture

## Status

Accepted

## Context

RAINA currently operates with a single user context ("User") and shared memory system. As we implement proper user authentication, we need to decide how to handle memory isolation and whether RAINA should be a shared learning entity or provide isolated experiences per user.

### Current State

- Single hardcoded user context
- Shared memory system with speaker/listener attribution in chunk slots
- Single working memory space
- No user authentication

### Requirements

- Support multiple authenticated users
- Maintain conversational context appropriately
- Allow RAINA to learn and grow from interactions
- Respect user privacy expectations
- Support different deployment scenarios

## Decision

We will implement a **dual-mode architecture** that supports both shared and isolated memory models, selectable by users at login time.

### Architecture Components

#### 1. Memory Modes

**Shared Mode (Default):**

- Single declarative memory system containing all chunks from all users
- RAINA learns from all interactions and maintains collective knowledge
- Privacy handled through contextual evaluation in response generation
- Speaker/listener attribution maintained in chunk slots
- Cross-user knowledge sharing allowed with appropriate attribution

**Isolated Mode:**

- Each user receives their own complete memory system instance
- No cross-user knowledge sharing
- Higher resource usage but complete privacy
- Each user develops independent relationship with RAINA

#### 2. Working Memory Isolation

- **Always isolated per active session** regardless of mode
- Each user session maintains separate working memory
- SignalR groups segregated by user session
- Context updates only visible to the respective user

#### 3. Configuration Options

- User choice at login (default behavior)
- Environment variable override to force specific mode for deployment
- Per-deployment flexibility (shared vs isolated instances)

### Privacy Handling in Shared Mode

RAINA will make contextual decisions about information sharing based on:

- Content sensitivity assessment
- Relationship to current conversation
- General vs personal knowledge distinction
- Prompt-level instructions for privacy behavior

Example behaviors:

- "I remember discussing this topic before" (no user attribution)
- "Another user mentioned something similar" (explicit attribution for non-sensitive topics)
- Silent incorporation of knowledge without attribution (for sensitive information)

## Consequences

### Positive

- Maximum deployment flexibility
- Supports both privacy-focused and collaborative learning scenarios
- Maintains current architecture while adding new capabilities
- Allows experimentation with different interaction models
- Scalable approach that can adapt to different use cases

### Negative

- Increased complexity in session management
- Additional configuration and mode-switching logic
- Potential resource overhead for isolated mode
- Need for careful privacy evaluation implementation

### Neutral

- Working memory always isolated (consistent with user expectations)
- Authentication required for all interactions

## Implementation Notes

- UserContextManager will be enhanced to support mode selection
- SignalR hub contexts will use user-specific groups
- Memory system instantiation will depend on selected mode
- Privacy evaluation will be implemented through response prompt engineering

## Related Decisions

- ADR-002: User Authentication Implementation (planned)
- Future: Privacy evaluation algorithm details
- Future: Resource management for isolated mode scaling
