---
id: task-204
title: Add unit tests for delete events
status: To Do
assignee: []
created_date: '2026-01-09 15:47'
labels:
  - feature
  - events
  - testing
dependencies:
  - task-201
  - task-194
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add unit tests for delete and deleteMany event emission.

## Instructions

1. Open `test/collection-events.test.ts`
2. Add test suite for delete events:
   - `deleteOne` emits 'delete' event with correct payload
   - Delete event payload contains filter, deleted boolean
   - Delete event with successful delete emits deleted: true
   - Delete event with no match emits deleted: false
   - `deleteMany` emits 'deleteMany' event with correct payload
   - DeleteMany payload contains filter, deletedCount
   - DeleteMany with no matches emits deletedCount: 0
3. Test both string ID and QueryFilter for filter field
4. Commit changes after completion
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Test: deleteOne emits 'delete' event
- [ ] #2 Test: delete payload has filter, deleted
- [ ] #3 Test: deleteOne with match emits deleted: true
- [ ] #4 Test: deleteOne with no match emits deleted: false
- [ ] #5 Test: deleteMany emits 'deleteMany' event
- [ ] #6 Test: deleteMany payload has deletedCount
- [ ] #7 All tests pass with bun test
- [ ] #8 bun run typecheck passes with zero errors
- [ ] #9 bun run lint:fix produces no new warnings or errors
- [ ] #10 Changes are committed with descriptive message
<!-- AC:END -->
