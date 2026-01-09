---
id: task-211
title: Add cleanup method to SQLiteCollection for event resources
status: To Do
assignee: []
created_date: '2026-01-09 16:18'
labels:
  - feature
  - events
  - implementation
dependencies:
  - task-199
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add a cleanup method to SQLiteCollection that properly disposes of event resources. This method will be called when the collection is dropped or the database is closed.

## Instructions

1. Open `src/sqlite-collection.ts`
2. Add a new method `cleanupEvents()`:
   ```typescript
   /**
    * Cleans up event emitter resources.
    * Called internally when collection is dropped or database is closed.
    * @internal
    */
   cleanupEvents(): void {
     if (this._emitter) {
       this._emitter.removeAllListeners()
       this._emitter = null
     }
   }
   ```
3. Update the `drop()` method to call `cleanupEvents()` AFTER emitting the 'drop' event:
   ```typescript
   // In drop() method, after the drop operation succeeds:
   this.emitEvent('drop', () => ({ name: this.name }))
   this.cleanupEvents()  // Clean up after emitting
   ```
4. Add `cleanupEvents` to the Collection type interface in `src/collection-types.ts` (marked as @internal)
5. Commit changes after completion

## Why This Matters
- Prevents memory leaks from orphaned listeners
- Ensures proper resource disposal
- Listeners registered on a dropped collection would never fire anyway
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 cleanupEvents() method added to SQLiteCollection
- [ ] #2 cleanupEvents() removes all listeners and nullifies _emitter
- [ ] #3 drop() calls cleanupEvents() after emitting drop event
- [ ] #4 cleanupEvents added to Collection type interface with @internal JSDoc
- [ ] #5 bun run typecheck passes with zero errors
- [ ] #6 bun run lint:fix produces no new warnings or errors
- [ ] #7 Changes are committed with descriptive message
<!-- AC:END -->
