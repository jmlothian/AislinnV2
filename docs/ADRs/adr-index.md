# Architecture Decision Records (ADR) Index

This document maintains an index of all Architecture Decision Records for the RAINA project.

## Active ADRs

| ADR                                      | Title                                        | Status   | Date       | Summary                                                                                                                |
| ---------------------------------------- | -------------------------------------------- | -------- | ---------- | ---------------------------------------------------------------------------------------------------------------------- |
| [ADR-001](./adr-001-memory-isolation.md) | Memory Isolation and Multi-User Architecture | Accepted | 2025-06-13 | Dual-mode architecture supporting both shared and isolated memory models with user-selectable modes                    |
| [ADR-002](./adr-002-memory-fixes.md)     | Memory System Activation and Decay Fixes     | Accepted | 2025-06-14 | Fixed over-activation, coordinated decay systems, and implemented association forgetting for realistic memory behavior |

## Planned ADRs

| ADR     | Title                              | Expected | Summary                                                  |
| ------- | ---------------------------------- | -------- | -------------------------------------------------------- |
| ADR-002 | User Authentication Implementation | TBD      | Basic login with username/password, session management   |
| ADR-003 | Privacy Evaluation Algorithm       | TBD      | How RAINA decides what information to share across users |
| ADR-004 | SignalR Session Management         | TBD      | User-specific groups and real-time update isolation      |

## ADR Template

For consistency, new ADRs should follow this structure:

```markdown
# ADR-XXX: [Title]

## Status

[Proposed | Accepted | Deprecated | Superseded]

## Context

[Description of the problem and context]

## Decision

[The decision made and reasoning]

## Consequences

[Positive and negative consequences]

## Implementation Notes

[Technical details and considerations]

## Related Decisions

[Links to other ADRs]
```

## ADR Process

1. **Propose**: Create ADR with "Proposed" status
2. **Review**: Team discussion and feedback
3. **Accept**: Mark as "Accepted" and implement
4. **Update**: Modify if needed, mark as "Superseded" if replaced

## Guidelines

- ADRs are immutable once accepted
- Use clear, concise language
- Focus on architectural significance
- Include both positive and negative consequences
- Reference related decisions and dependencies
