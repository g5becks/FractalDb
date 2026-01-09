---
id: task-187
title: Create collection-events.ts with event payload types
status: Done
assignee:
  - '@agent'
created_date: '2026-01-09 15:13'
updated_date: '2026-01-09 16:10'
labels:
  - feature
  - events
  - types
dependencies: []
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Create a new file src/collection-events.ts containing all event payload type definitions for collection events. This is the foundation for the EventEmitter feature.

## Instructions

1. Create file `src/collection-events.ts`
2. Add imports from `node:events`, `./core-types.js`, and `./query-types.js`
3. Define all event payload types with full TSDoc comments:
   - `InsertEvent<T>` - for insertOne operations
   - `InsertManyEvent<T>` - for insertMany operations
   - `UpdateEvent<T>` - for updateOne operations
   - `UpdateManyEvent<T>` - for updateMany operations
   - `ReplaceEvent<T>` - for replaceOne operations
   - `DeleteEvent<T>` - for deleteOne operations
   - `DeleteManyEvent<T>` - for deleteMany operations
   - `FindOneAndDeleteEvent<T>` - for findOneAndDelete operations
   - `FindOneAndUpdateEvent<T>` - for findOneAndUpdate operations
   - `FindOneAndReplaceEvent<T>` - for findOneAndReplace operations
   - `DropEvent` - for drop operations
   - `ErrorEvent` - for error events
4. All payload properties should be `readonly`
5. Reference COLLECTION_EVENTS_DESIGN.md for exact type definitions
6. Commit changes after completion

## Reference
See COLLECTION_EVENTS_DESIGN.md lines 56-193 for exact type definitions.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 File src/collection-events.ts exists
- [x] #2 All 12 event payload types are defined (InsertEvent, InsertManyEvent, UpdateEvent, UpdateManyEvent, ReplaceEvent, DeleteEvent, DeleteManyEvent, FindOneAndDeleteEvent, FindOneAndUpdateEvent, FindOneAndReplaceEvent, DropEvent, ErrorEvent)
- [x] #3 All types have full TSDoc comments
- [x] #4 All payload properties are readonly
- [x] #5 bun run typecheck passes with zero errors
- [x] #6 bun run lint:fix produces no new warnings or errors
- [x] #7 Changes are committed with descriptive message
<!-- AC:END -->
