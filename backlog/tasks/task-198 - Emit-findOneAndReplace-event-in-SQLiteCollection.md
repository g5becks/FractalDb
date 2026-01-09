---
id: task-198
title: Emit findOneAndReplace event in SQLiteCollection
status: Done
assignee:
  - '@agent'
created_date: '2026-01-09 15:34'
updated_date: '2026-01-09 16:28'
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
Add event emission to findOneAndReplace method in SQLiteCollection.

## Instructions

1. Open `src/sqlite-collection.ts`
2. In `findOneAndReplace` method, after successful operation, add:
   ```typescript
   this.emitEvent('findOneAndReplace', () => ({
     filter: filterOrId,
     document: result.document,
     upserted: result.upserted ?? false
   }))
   ```
3. Ensure the filter and result variables are captured correctly for the payload
4. The document in the payload is either before or after replace based on options.returnDocument
5. Events must be emitted AFTER the operation succeeds but BEFORE returning
6. Commit changes after completion
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 findOneAndReplace emits 'findOneAndReplace' event with { filter, document, upserted } payload
- [x] #2 Events emitted after successful operation, before return
- [x] #3 Uses emitEvent helper with factory function
- [x] #4 bun run typecheck passes with zero errors
- [x] #5 bun run lint:fix produces no new warnings or errors
- [x] #6 Changes are committed with descriptive message
<!-- AC:END -->
