---
id: task-190
title: Add lazy emitter and emit helper to SQLiteCollection
status: Done
assignee:
  - '@agent'
created_date: '2026-01-09 15:17'
updated_date: '2026-01-09 16:18'
labels:
  - feature
  - events
  - implementation
dependencies:
  - task-189
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add the lazy-initialized EventEmitter and optimized emit helper method to SQLiteCollection. This implements the performance-optimized event infrastructure.

## Instructions

1. Open `src/sqlite-collection.ts`
2. Add imports from `./collection-events.js`:
   - CollectionEventEmitter
   - CollectionEventName
   - CollectionEventMap
3. Add private property: `private _emitter: CollectionEventEmitter<T> | null = null`
4. Add private getter for lazy initialization:
   ```typescript
   private get emitter(): CollectionEventEmitter<T> {
     if (!this._emitter) {
       this._emitter = new CollectionEventEmitter<T>()
     }
     return this._emitter
   }
   ```
5. Add optimized emit helper method:
   ```typescript
   private emitEvent<E extends CollectionEventName>(
     event: E,
     createPayload: () => CollectionEventMap<T>[E][0]
   ): void {
     if (!this._emitter) return
     if (this._emitter.listenerCount(event) === 0) return
     this._emitter.emit(event, createPayload())
   }
   ```
6. Commit changes after completion

## Reference
See COLLECTION_EVENTS_DESIGN.md lines 448-491 for implementation details.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 Import statements for event types added to sqlite-collection.ts
- [x] #2 _emitter private property declared as nullable
- [x] #3 emitter getter implements lazy initialization
- [x] #4 emitEvent helper checks _emitter existence before emitting
- [x] #5 emitEvent helper checks listenerCount before creating payload
- [x] #6 emitEvent uses factory function pattern for deferred payload creation
- [x] #7 bun run typecheck passes with zero errors
- [x] #8 bun run lint:fix produces no new warnings or errors
- [x] #9 Changes are committed with descriptive message
<!-- AC:END -->
