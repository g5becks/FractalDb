---
id: task-75
title: Implement filter-based deleteOne
status: To Do
assignee: []
created_date: '2025-11-22 06:27'
labels:
  - mongodb-compat
  - breaking-change
dependencies:
  - task-74
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Change deleteOne to accept a QueryFilter instead of just an ID string, and return DeleteResult instead of boolean.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Update deleteOne signature in src/collection-types.ts: `deleteOne(filter: QueryFilter<T>): Promise<DeleteResult>`
- [ ] #2 Update deleteOne JSDoc with filter-based examples
- [ ] #3 Implement filter-based deleteOne in src/sqlite-collection.ts
- [ ] #4 Find first matching document using translated filter with LIMIT 1
- [ ] #5 Delete by _id after finding match
- [ ] #6 Return { deletedCount: 0 | 1, acknowledged: true }
- [ ] #7 Run `bun run typecheck` - should pass
- [ ] #8 Run `bun run lint` - no linter errors
<!-- AC:END -->
