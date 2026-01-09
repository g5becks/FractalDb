---
id: task-200
title: Export event types from src/index.ts
status: Done
assignee:
  - '@agent'
created_date: '2026-01-09 15:38'
updated_date: '2026-01-09 16:29'
labels:
  - feature
  - events
  - exports
dependencies:
  - task-188
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Update src/index.ts to export all event-related types for external use.

## Instructions

1. Open `src/index.ts`
2. Add exports from `./collection-events.js`:
   ```typescript
   export {
     type InsertEvent,
     type InsertManyEvent,
     type UpdateEvent,
     type UpdateManyEvent,
     type ReplaceEvent,
     type DeleteEvent,
     type DeleteManyEvent,
     type FindOneAndDeleteEvent,
     type FindOneAndUpdateEvent,
     type FindOneAndReplaceEvent,
     type DropEvent,
     type ErrorEvent,
     type CollectionEventMap,
     type CollectionEventName,
     CollectionEventEmitter
   } from './collection-events.js'
   ```
3. Group the exports logically with the other type exports
4. Commit changes after completion
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 All 12 event payload types are exported
- [x] #2 CollectionEventMap type is exported
- [x] #3 CollectionEventName type is exported
- [x] #4 CollectionEventEmitter class is exported
- [x] #5 Exports are grouped logically
- [x] #6 bun run typecheck passes with zero errors
- [x] #7 bun run lint:fix produces no new warnings or errors
- [x] #8 Changes are committed with descriptive message
<!-- AC:END -->
