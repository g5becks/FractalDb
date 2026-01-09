---
id: task-192
title: Emit insert and insertMany events in SQLiteCollection
status: Done
assignee:
  - '@agent'
created_date: '2026-01-09 15:22'
updated_date: '2026-01-09 16:19'
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
Add event emission to insertOne and insertMany methods in SQLiteCollection.

## Instructions

1. Open `src/sqlite-collection.ts`
2. In `insertOne` method, after successful insert (after the document is created), add:
   ```typescript
   this.emitEvent('insert', () => ({ document: result }))
   ```
3. In `insertMany` method, after successful batch insert, add:
   ```typescript
   this.emitEvent('insertMany', () => ({
     documents: results,
     insertedCount: results.length
   }))
   ```
4. Events must be emitted AFTER the operation succeeds but BEFORE returning
5. Events are emitted inside the retry wrapper (after success, before return)
6. Commit changes after completion

## Reference
See COLLECTION_EVENTS_DESIGN.md lines 493-519 for the emission pattern.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 insertOne emits 'insert' event with { document } payload
- [x] #2 insertMany emits 'insertMany' event with { documents, insertedCount } payload
- [x] #3 Events emitted after successful operation, before return
- [x] #4 Uses emitEvent helper with factory function
- [x] #5 bun run typecheck passes with zero errors
- [x] #6 bun run lint:fix produces no new warnings or errors
- [x] #7 Changes are committed with descriptive message
<!-- AC:END -->
