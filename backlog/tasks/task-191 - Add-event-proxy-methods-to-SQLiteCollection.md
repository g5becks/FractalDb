---
id: task-191
title: Add event proxy methods to SQLiteCollection
status: Done
assignee: []
created_date: '2026-01-09 15:19'
updated_date: '2026-01-09 16:18'
labels:
  - feature
  - events
  - implementation
dependencies:
  - task-190
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add the public event API methods to SQLiteCollection that proxy to the internal EventEmitter. These methods implement the Collection interface event methods.

## Instructions

1. Open `src/sqlite-collection.ts`
2. Add the following public methods that proxy to the emitter:

   ```typescript
   on<E extends CollectionEventName>(
     event: E,
     listener: (...args: CollectionEventMap<T>[E]) => void
   ): this {
     this.emitter.on(event, listener)
     return this
   }

   once<E extends CollectionEventName>(
     event: E,
     listener: (...args: CollectionEventMap<T>[E]) => void
   ): this {
     this.emitter.once(event, listener)
     return this
   }

   off<E extends CollectionEventName>(
     event: E,
     listener: (...args: CollectionEventMap<T>[E]) => void
   ): this {
     this.emitter.off(event, listener)
     return this
   }

   removeAllListeners(event?: CollectionEventName): this {
     this.emitter.removeAllListeners(event)
     return this
   }

   listenerCount(event: CollectionEventName): number {
     return this._emitter?.listenerCount(event) ?? 0
   }

   listeners<E extends CollectionEventName>(
     event: E
   ): ((...args: CollectionEventMap<T>[E]) => void)[] {
     return (this._emitter?.listeners(event) ?? []) as ((...args: CollectionEventMap<T>[E]) => void)[]
   }
   ```
3. Note: on/once/off use this.emitter (triggers lazy init), listenerCount/listeners use this._emitter (no init needed)
4. Add TSDoc comments to each method
5. Commit changes after completion
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 on() method implemented, proxies to emitter, returns this
- [x] #2 once() method implemented, proxies to emitter, returns this
- [x] #3 off() method implemented, proxies to emitter, returns this
- [x] #4 removeAllListeners() method implemented, returns this
- [x] #5 listenerCount() returns 0 when no emitter exists
- [x] #6 listeners() returns empty array when no emitter exists
- [x] #7 All methods have TSDoc comments
- [x] #8 bun run typecheck passes with zero errors
- [x] #9 bun run lint:fix produces no new warnings or errors
- [x] #10 Changes are committed with descriptive message
<!-- AC:END -->
