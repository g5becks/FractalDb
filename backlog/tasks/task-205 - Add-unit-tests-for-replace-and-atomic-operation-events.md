---
id: task-205
title: Add unit tests for replace and atomic operation events
status: Done
assignee: []
created_date: '2026-01-09 15:49'
labels:
  - feature
  - events
  - testing
dependencies:
  - task-201
  - task-195
  - task-196
  - task-197
  - task-198
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add unit tests for replace, findOneAndDelete, findOneAndUpdate, findOneAndReplace events.

## Instructions

1. Open `test/collection-events.test.ts`
2. Add test suite for replace events:
   - `replaceOne` emits 'replace' event with { filter, document }
   - Replace with no match emits document: null
3. Add test suite for findOneAndDelete:
   - `findOneAndDelete` emits 'findOneAndDelete' event with { filter, document }
   - Returns the deleted document in payload
   - No match emits document: null
4. Add test suite for findOneAndUpdate:
   - `findOneAndUpdate` emits 'findOneAndUpdate' event with { filter, update, document, upserted }
   - With upsert emits upserted: true
5. Add test suite for findOneAndReplace:
   - `findOneAndReplace` emits 'findOneAndReplace' event with { filter, document, upserted }
   - With upsert emits upserted: true
6. Commit changes after completion
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Test: replaceOne emits 'replace' event with correct payload
- [ ] #2 Test: findOneAndDelete emits event with deleted document
- [ ] #3 Test: findOneAndUpdate emits event with all fields
- [ ] #4 Test: findOneAndUpdate with upsert emits upserted: true
- [ ] #5 Test: findOneAndReplace emits event with correct payload
- [ ] #6 Test: findOneAndReplace with upsert emits upserted: true
- [ ] #7 All tests pass with bun test
- [ ] #8 bun run typecheck passes with zero errors
- [ ] #9 bun run lint:fix produces no new warnings or errors
- [ ] #10 Changes are committed with descriptive message
<!-- AC:END -->
