---
id: task-174
title: Write integration tests for cursor pagination - backward and combined
status: Done
assignee: []
created_date: '2025-11-23 07:30'
updated_date: '2025-11-23 15:18'
labels:
  - testing
  - phase-4
  - v0.3.0
dependencies:
  - task-173
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Extend `test/integration/cursor-pagination.test.ts` with tests for backward pagination (using 'before' cursor) and combining cursor with filters.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Test backward pagination with 'before' cursor
- [ ] #2 Test backward pagination with ascending sort
- [ ] #3 Test backward pagination with descending sort
- [ ] #4 Test cursor combined with filter conditions
- [ ] #5 Test cursor with tie-breaking (same sort value, different _id)
- [ ] #6 Test cursor ignores skip option when cursor is provided
- [ ] #7 All tests pass with `bun test test/integration/cursor-pagination.test.ts`
<!-- AC:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Combined with task-173 in the same test file. Tests cover backward pagination and combined scenarios.
<!-- SECTION:NOTES:END -->
