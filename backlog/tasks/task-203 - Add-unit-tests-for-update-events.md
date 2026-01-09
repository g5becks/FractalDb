---
id: task-203
title: Add unit tests for update events
status: To Do
assignee: []
created_date: '2026-01-09 15:45'
labels:
  - feature
  - events
  - testing
dependencies:
  - task-201
  - task-193
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add unit tests for update and updateMany event emission.

## Instructions

1. Open `test/collection-events.test.ts`
2. Add test suite for update events:
   - `updateOne` emits 'update' event with correct payload
   - Update event payload contains filter, update, document, upserted
   - Update event with upsert=true emits upserted: true
   - Update event with no match emits document: null, upserted: false
   - `updateMany` emits 'updateMany' event with correct payload
   - UpdateMany payload contains filter, update, matchedCount, modifiedCount
   - UpdateMany with no matches emits matchedCount: 0, modifiedCount: 0
3. Test both string ID and QueryFilter for filter field
4. Commit changes after completion
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Test: updateOne emits 'update' event
- [ ] #2 Test: update payload has filter, update, document, upserted
- [ ] #3 Test: updateOne with upsert emits upserted: true
- [ ] #4 Test: updateOne with no match emits document: null
- [ ] #5 Test: updateMany emits 'updateMany' event
- [ ] #6 Test: updateMany payload has matchedCount, modifiedCount
- [ ] #7 All tests pass with bun test
- [ ] #8 bun run typecheck passes with zero errors
- [ ] #9 bun run lint:fix produces no new warnings or errors
- [ ] #10 Changes are committed with descriptive message
<!-- AC:END -->
