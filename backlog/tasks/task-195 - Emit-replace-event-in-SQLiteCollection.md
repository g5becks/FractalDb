---
id: task-195
title: Emit replace event in SQLiteCollection
status: Done
assignee:
  - '@agent'
created_date: '2026-01-09 15:28'
updated_date: '2026-01-09 16:25'
labels:
  - feature
  - events
  - implementation
dependencies:
  - task-191
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add event emission to replaceOne method in SQLiteCollection.

## Instructions

1. Open `src/sqlite-collection.ts`
2. In `replaceOne` method, after successful replace, add:
   ```typescript
   this.emitEvent('replace', () => ({
     filter: filterOrId,
     document: result  // the replaced document or null
   }))
   ```
3. Ensure the filter and result variables are captured correctly for the payload
4. Events must be emitted AFTER the operation succeeds but BEFORE returning
5. Commit changes after completion
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 replaceOne emits 'replace' event with { filter, document } payload
- [x] #2 Events emitted after successful operation, before return
- [x] #3 Uses emitEvent helper with factory function
- [x] #4 bun run typecheck passes with zero errors
- [x] #5 bun run lint:fix produces no new warnings or errors
- [x] #6 Changes are committed with descriptive message
<!-- AC:END -->
