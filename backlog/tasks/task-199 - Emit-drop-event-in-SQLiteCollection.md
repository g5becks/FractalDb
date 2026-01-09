---
id: task-199
title: Emit drop event in SQLiteCollection
status: Done
assignee:
  - '@agent'
created_date: '2026-01-09 15:36'
updated_date: '2026-01-09 16:29'
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
Add event emission to drop method in SQLiteCollection.

## Instructions

1. Open `src/sqlite-collection.ts`
2. In `drop` method, after successful drop operation, add:
   ```typescript
   this.emitEvent('drop', () => ({
     name: this.name
   }))
   ```
3. Note: this.name refers to the collection name
4. Events must be emitted AFTER the operation succeeds but BEFORE returning
5. Commit changes after completion
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 drop emits 'drop' event with { name } payload containing collection name
- [x] #2 Events emitted after successful operation, before return
- [x] #3 Uses emitEvent helper with factory function
- [x] #4 bun run typecheck passes with zero errors
- [x] #5 bun run lint:fix produces no new warnings or errors
- [x] #6 Changes are committed with descriptive message
<!-- AC:END -->
