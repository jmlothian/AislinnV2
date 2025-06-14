# ADR-002: Memory System Activation and Decay Fixes

## Status

Accepted

## Context

The RAINA/Aislinn memory system was experiencing several critical issues that prevented realistic cognitive behavior and threatened long-term agent operation:

### Issues Identified

1. **Over-Activation Problem**: Specific memories were getting way over-activated (100.0+ when 2.0 should be very high) and sticking in working memory for too long, preventing other relevant memories from entering working memory.

2. **Inconsistent Decay Systems**: Two separate decay systems (global ACT-R decay and working memory decay) were not coordinating, leading to chunks with frozen activation levels and inconsistent memory behavior.

3. **Static Association Network**: Associations never decayed or were forgotten, leading to an increasingly dense and noisy network that would become unusable for long-running agents.

### Root Causes

- Focus re-activation bug causing same chunks to be focused multiple times per conversation
- Activation ceiling not enforced after boosts were applied
- Global decay system never called, leaving non-working-memory chunks with frozen activation
- No mechanism for association forgetting over time

## Decision

We will implement three coordinated fixes to restore realistic memory behavior:

### Fix 1: Enforce Activation Ceiling

**Change**: Apply activation ceiling AFTER activation boosts are added, not before.

**Implementation**:

```csharp
// Calculate base activation + boost
double totalActivation = baseActivation + adjustedBoost;
// Apply ceiling to the total
chunk.ActivationLevel = Math.Min(parameters.ActivationCeiling, totalActivation);
```

**Rationale**: Prevents unlimited activation accumulation regardless of how many times something gets boosted.

### Fix 2: Remove Focus Re-activation Protection

**Changes**:

- Remove duplicate `ManualRefreshCycleAsync()` calls in conversation flow
- Remove special handling for focused items in `RefreshCycleAsync()`
- Let focused chunks decay normally instead of getting artificial 20% decay rate
- Remove focus value reset to 1.0 on each refresh

**Rationale**: If something is truly relevant to current conversation, it will get re-activated through normal conversation processing (entity extraction, spreading activation). Artificial focus protection creates unrealistic activation accumulation.

### Fix 3: Coordinated Decay Schedule

**Implementation**:

- **Working Memory decay**: Every message (maintain current behavior)
- **Global ACT-R decay**: Every 2 messages
- **Association decay**: Every 2 messages (alongside global decay)

**Association Decay Algorithm**:

- Apply time-based decay to association weights: `weight *= (1 - decayRate * timeElapsed)`
- Remove associations when both weights drop below threshold (0.05)
- Use existing `DeleteAssociationAsync()` method for removal

**Rationale**: Creates natural rhythm where immediate cognitive load is managed by working memory decay, while long-term memory health is maintained by periodic global decay and association pruning.

## Consequences

### Positive

- **Realistic Activation Levels**: Prevents over-activation that crowds working memory
- **Natural Forgetting**: Implements realistic forgetting curves for both chunks and associations
- **Network Health**: Association decay prevents network from becoming impossibly dense over time
- **Coordinated Systems**: All decay mechanisms work together instead of conflicting
- **Long-term Viability**: System can operate for months/years without memory degradation
- **Context Sensitivity**: Working memory becomes more responsive to currently relevant information

### Negative

- **Potential Over-Forgetting**: May need tuning to ensure important memories aren't lost too quickly
- **Computational Overhead**: Global decay every 2 messages adds processing cost
- **Association Loss**: Some useful but infrequently used associations may be pruned

### Neutral

- **Behavior Changes**: Agent responses may change as memory system becomes more realistic
- **Tuning Required**: Decay rates and thresholds may need adjustment based on usage patterns

## Implementation Notes

### Code Changes Required

1. **ChunkActivationService.ActivateChunkAsync()**: Add ceiling enforcement after boost application
2. **WorkingMemoryManager.RefreshCycleAsync()**: Remove focused item special handling
3. **ConversationManager.GenerateResponseAsync()**: Remove duplicate refresh calls
4. **ChunkActivationService**: Add `ApplyAssociationDecayAsync()` method
5. **CognitiveMemorySystem**: Add association decay coordination
6. **IChunkAssociationCollection**: Add `GetAllAssociationsAsync()` method

### Configuration Parameters

- Association decay rate: 2% per time unit
- Association removal threshold: 0.05 (5% weight)
- Global decay frequency: Every 2 messages
- Activation ceiling enforcement: Always after boosts

### Monitoring and Tuning

- Track working memory utilization patterns
- Monitor association network size over time
- Verify important memories aren't being lost too quickly
- Adjust decay rates based on agent behavior observations

## Related Decisions

- ADR-001: Memory Isolation and Multi-User Architecture
- Future ADR: Context-Gated Spreading Activation (Issue 4)
- Future ADR: Emotional Memory and Personality Persistence

## Validation Criteria

Success will be measured by:

- Working memory activation levels staying within reasonable ranges (< 5.0 for very high activation)
- Association network size stabilizing rather than growing indefinitely
- Agent maintaining ability to recall important information over weeks
- Improved contextual relevance of working memory contents
- No chunks with frozen activation levels outside working memory
