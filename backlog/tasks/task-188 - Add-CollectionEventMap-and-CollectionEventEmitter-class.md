---
id: task-188
title: Add CollectionEventMap and CollectionEventEmitter class
status: Done
assignee:
  - '@agent'
created_date: '2026-01-09 15:13'
updated_date: '2026-01-09 16:12'
labels:
  - feature
  - events
  - types
dependencies:
  - task-187
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add the CollectionEventMap type and CollectionEventEmitter class to src/collection-events.ts. These provide the type-safe event system foundation.

## Instructions

1. Open `src/collection-events.ts` (created in previous task)
2. Add `CollectionEventMap<T>` type that maps event names to payload tuple types:
   - insert: [InsertEvent<T>]
   - insertMany: [InsertManyEvent<T>]
   - update: [UpdateEvent<T>]
   - updateMany: [UpdateManyEvent<T>]
   - replace: [ReplaceEvent<T>]
   - delete: [DeleteEvent<T>]
   - deleteMany: [DeleteManyEvent<T>]
   - findOneAndDelete: [FindOneAndDeleteEvent<T>]
   - findOneAndUpdate: [FindOneAndUpdateEvent<T>]
   - findOneAndReplace: [FindOneAndReplaceEvent<T>]
   - drop: [DropEvent]
   - error: [ErrorEvent]
3. Add `CollectionEventName` type as `keyof CollectionEventMap<Document>`
4. Add `CollectionEventEmitter<T>` class extending `EventEmitter<CollectionEventMap<T>>`
5. Add TSDoc comments for all new types/classes
6. Commit changes after completion

## Reference
See COLLECTION_EVENTS_DESIGN.md lines 195-226 for exact definitions.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 CollectionEventMap<T> type is defined with all 12 event mappings
- [x] #2 CollectionEventName type is defined
- [x] #3 CollectionEventEmitter<T> class extends EventEmitter with proper generic
- [x] #4 All new types have TSDoc comments
- [x] #5 bun run typecheck passes with zero errors
- [x] #6 bun run lint:fix produces no new warnings or errors
- [x] #7 Changes are committed with descriptive message
<!-- AC:END -->
