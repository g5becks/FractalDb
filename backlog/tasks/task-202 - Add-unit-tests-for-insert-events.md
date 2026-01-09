---
id: task-202
title: Add unit tests for insert events
status: Done
assignee:
  - '@agent'
created_date: '2026-01-09 15:43'
updated_date: '2026-01-09 16:55'
labels:
  - feature
  - events
  - testing
dependencies:
  - task-201
  - task-192
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add unit tests for insert and insertMany event emission.

## Instructions

1. Open `test/collection-events.test.ts`
2. Add test suite for insert events:
   - `insertOne` emits 'insert' event with correct payload
   - Insert event payload contains full document with _id, createdAt, updatedAt
   - Insert event fires after document is persisted (can query it)
   - `insertMany` emits 'insertMany' event with correct payload
   - InsertMany payload contains documents array and insertedCount
   - No events emitted when no listeners registered (verify via spy/mock)
3. Use async/await and promises to verify event timing
4. Commit changes after completion
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 Test: insertOne emits 'insert' event
- [x] #2 Test: insert event payload has document with _id, createdAt, updatedAt
- [x] #3 Test: insert event fires after document persisted
- [x] #4 Test: insertMany emits 'insertMany' event
- [x] #5 Test: insertMany payload has documents array and insertedCount
- [x] #6 Test: no events when no listeners (performance check)
- [x] #7 All tests pass with bun test
- [x] #8 bun run typecheck passes with zero errors
- [x] #9 bun run lint:fix produces no new warnings or errors
- [x] #10 Changes are committed with descriptive message
<!-- AC:END -->
