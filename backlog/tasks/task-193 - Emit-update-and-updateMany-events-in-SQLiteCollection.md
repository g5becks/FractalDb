---
id: task-193
title: Emit update and updateMany events in SQLiteCollection
status: Done
assignee:
  - '@agent'
created_date: '2026-01-09 15:24'
updated_date: '2026-01-09 16:23'
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
Add event emission to updateOne and updateMany methods in SQLiteCollection.

## Instructions

1. Open `src/sqlite-collection.ts`
2. In `updateOne` method, after successful update, add:
   ```typescript
   this.emitEvent('update', () => ({
     filter: filterOrId,
     update: update,
     document: result.document,
     upserted: result.upserted
   }))
   ```
3. In `updateMany` method, after successful batch update, add:
   ```typescript
   this.emitEvent('updateMany', () => ({
     filter: filter,
     update: update,
     matchedCount: result.matchedCount,
     modifiedCount: result.modifiedCount
   }))
   ```
4. Ensure the filter, update, and result variables are captured correctly for the payload
5. Events must be emitted AFTER the operation succeeds but BEFORE returning
6. Commit changes after completion
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 updateOne emits 'update' event with { filter, update, document, upserted } payload
- [x] #2 updateMany emits 'updateMany' event with { filter, update, matchedCount, modifiedCount } payload
- [x] #3 Events emitted after successful operation, before return
- [x] #4 Uses emitEvent helper with factory function
- [x] #5 bun run typecheck passes with zero errors
- [x] #6 bun run lint:fix produces no new warnings or errors
- [x] #7 Changes are committed with descriptive message
<!-- AC:END -->
