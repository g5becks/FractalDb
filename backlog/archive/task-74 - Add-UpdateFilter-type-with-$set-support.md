---
id: task-74
title: Add UpdateFilter type with $set support
status: Done
assignee: []
created_date: '2025-11-22 06:27'
labels:
  - mongodb-compat
  - feature
dependencies:
  - task-73
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add a new UpdateFilter type that supports both direct partial updates and MongoDB-style $set operator syntax.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Create UpdateFilter<T> type in src/collection-types.ts that accepts either { $set: Partial<T> } or Partial<T>
- [ ] #2 Add comprehensive JSDoc with examples showing both syntaxes
- [ ] #3 Export UpdateFilter from src/index.ts
- [ ] #4 Run `bun run typecheck` - should pass
- [ ] #5 Run `bun run lint` - no linter errors
<!-- AC:END -->
