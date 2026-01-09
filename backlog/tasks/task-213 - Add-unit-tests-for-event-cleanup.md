---
id: task-213
title: Add unit tests for event cleanup
status: Done
assignee: []
created_date: '2026-01-09 16:22'
labels:
  - feature
  - events
  - testing
dependencies:
  - task-211
  - task-212
  - task-206
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add unit tests to verify event resources are properly cleaned up on drop and database close.

## Instructions

1. Open `test/collection-events.test.ts`
2. Add test suite for cleanup:
   - After `drop()`, `listenerCount()` returns 0 for all events
   - After `drop()`, registering new listeners still works (emitter recreated on next `on()`)
   - After database `close()`, collection listeners are cleaned up
   - Verify no errors thrown when dropping collection with no listeners
   - Verify no errors thrown when closing database with no event listeners
3. Test the cleanup order: drop event fires BEFORE cleanup
   ```typescript
   let dropEventFired = false
   collection.on('drop', () => { dropEventFired = true })
   await collection.drop()
   expect(dropEventFired).toBe(true)
   expect(collection.listenerCount('drop')).toBe(0)
   ```
4. Commit changes after completion
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Test: drop() clears all listeners
- [ ] #2 Test: drop event fires before cleanup
- [ ] #3 Test: new listeners can be added after drop (if collection recreated)
- [ ] #4 Test: database close cleans up collection listeners
- [ ] #5 Test: no errors with no listeners on drop/close
- [ ] #6 All tests pass with bun test
- [ ] #7 bun run typecheck passes with zero errors
- [ ] #8 bun run lint:fix produces no new warnings or errors
- [ ] #9 Changes are committed with descriptive message
<!-- AC:END -->
