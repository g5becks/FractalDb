---
id: task-76
title: Implement filter-based updateOne with $set support
status: To Do
assignee: []
created_date: '2025-11-22 06:27'
labels:
  - mongodb-compat
  - breaking-change
dependencies:
  - task-75
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Change updateOne to accept a QueryFilter and UpdateFilter, returning UpdateResult instead of the document.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Update updateOne signature in src/collection-types.ts to accept filter and UpdateFilter
- [ ] #2 Update updateOne JSDoc with filter-based and $set examples
- [ ] #3 Rename existing updateOne logic to private updateById method in src/sqlite-collection.ts
- [ ] #4 Implement new updateOne that finds first match then calls updateById
- [ ] #5 Unwrap $set if present: `const data = '$set' in update ? update.$set : update`
- [ ] #6 Return UpdateResult { matchedCount, modifiedCount, upsertedCount?, acknowledged }
- [ ] #7 Support upsert option
- [ ] #8 Run `bun run typecheck` - should pass
- [ ] #9 Run `bun run lint` - no linter errors
<!-- AC:END -->
