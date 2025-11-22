---
id: task-77
title: Implement filter-based replaceOne
status: To Do
assignee: []
created_date: '2025-11-22 06:28'
labels:
  - mongodb-compat
  - breaking-change
dependencies:
  - task-76
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Change replaceOne to accept a QueryFilter instead of just an ID, returning UpdateResult instead of the document.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Update replaceOne signature in src/collection-types.ts to accept filter
- [ ] #2 Update replaceOne JSDoc with filter-based examples
- [ ] #3 Rename existing replaceOne logic to private replaceById method in src/sqlite-collection.ts
- [ ] #4 Implement new replaceOne that finds first match then calls replaceById
- [ ] #5 Return UpdateResult { matchedCount, modifiedCount, upsertedCount?, acknowledged }
- [ ] #6 Support upsert option
- [ ] #7 Run `bun run typecheck` - should pass
- [ ] #8 Run `bun run lint` - no linter errors
<!-- AC:END -->
