---
id: task-194
title: Emit delete and deleteMany events in SQLiteCollection
status: Done
assignee:
  - '@agent'
created_date: '2026-01-09 15:26'
updated_date: '2026-01-09 16:24'
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
Add event emission to deleteOne and deleteMany methods in SQLiteCollection.

## Instructions

1. Open `src/sqlite-collection.ts`
2. In `deleteOne` method, after successful delete, add:
   ```typescript
   this.emitEvent('delete', () => ({
     filter: filterOrId,
     deleted: result  // boolean indicating if document was deleted
   }))
   ```
3. In `deleteMany` method, after successful batch delete, add:
   ```typescript
   this.emitEvent('deleteMany', () => ({
     filter: filter,
     deletedCount: result.deletedCount
   }))
   ```
4. Ensure the filter and result variables are captured correctly for the payload
5. Events must be emitted AFTER the operation succeeds but BEFORE returning
6. Commit changes after completion
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 deleteOne emits 'delete' event with { filter, deleted } payload
- [x] #2 deleteMany emits 'deleteMany' event with { filter, deletedCount } payload
- [x] #3 Events emitted after successful operation, before return
- [x] #4 Uses emitEvent helper with factory function
- [x] #5 bun run typecheck passes with zero errors
- [x] #6 bun run lint:fix produces no new warnings or errors
- [x] #7 Changes are committed with descriptive message
<!-- AC:END -->
