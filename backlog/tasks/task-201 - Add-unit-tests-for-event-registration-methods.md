---
id: task-201
title: Add unit tests for event registration methods
status: Done
assignee:
  - '@agent'
created_date: '2026-01-09 15:41'
updated_date: '2026-01-09 16:32'
labels:
  - feature
  - events
  - testing
dependencies:
  - task-191
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Create unit tests for event registration/management methods (on, once, off, removeAllListeners, listenerCount, listeners).

## Instructions

1. Create file `test/collection-events.test.ts`
2. Set up test infrastructure with Bun test framework
3. Create a test collection for each test
4. Write tests for:
   - `on()` registers listener and receives events
   - `on()` returns collection instance for chaining
   - `once()` fires listener only once then auto-removes
   - `once()` returns collection instance for chaining
   - `off()` removes specific listener
   - `off()` returns collection instance for chaining
   - `removeAllListeners()` with event name removes only those listeners
   - `removeAllListeners()` without args removes all listeners
   - `listenerCount()` returns correct count
   - `listenerCount()` returns 0 when no listeners registered
   - `listeners()` returns array of registered listeners
   - `listeners()` returns empty array when no listeners
   - Multiple listeners receive same event
5. Clean up test database after each test
6. Commit changes after completion
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 Test file test/collection-events.test.ts exists
- [x] #2 Tests for on() method (registration, chaining)
- [x] #3 Tests for once() method (fires once, chaining)
- [x] #4 Tests for off() method (removal, chaining)
- [x] #5 Tests for removeAllListeners() (with/without event name)
- [x] #6 Tests for listenerCount() (with/without listeners)
- [x] #7 Tests for listeners() (returns correct array)
- [x] #8 Test for multiple listeners receiving same event
- [x] #9 All tests pass with bun test
- [x] #10 bun run typecheck passes with zero errors
- [x] #11 bun run lint:fix produces no new warnings or errors
- [x] #12 Changes are committed with descriptive message
<!-- AC:END -->
