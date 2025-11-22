---
id: task-78
title: Update updateMany to support $set syntax
status: To Do
assignee: []
created_date: '2025-11-22 06:28'
labels:
  - mongodb-compat
dependencies:
  - task-77
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Update updateMany to accept UpdateFilter type and unwrap $set if present.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Update updateMany signature in src/collection-types.ts to use UpdateFilter
- [ ] #2 Update updateMany JSDoc with $set examples
- [ ] #3 Update updateMany implementation to unwrap $set if present
- [ ] #4 Run `bun run typecheck` - should pass
- [ ] #5 Run `bun run lint` - no linter errors
<!-- AC:END -->
