---
id: task-124
title: Add findOneAndReplace tests to collection-crud.test.ts
status: Done
assignee: []
created_date: '2025-11-22 19:58'
updated_date: '2025-11-22 22:15'
labels: []
dependencies:
  - task-123
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add comprehensive tests for findOneAndReplace covering string ID, query filter, returnDocument options, upsert, and preserving _id/createdAt in test/integration/collection-crud.test.ts
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 All tests pass
- [ ] #2 Type checking passes
- [ ] #3 Linting passes
- [ ] #4 Tests with string ID pass
- [ ] #5 Tests with query filter pass
- [ ] #6 Tests returnDocument options pass
- [ ] #7 Tests upsert pass
- [ ] #8 Tests preserve _id and createdAt pass
<!-- AC:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Added comprehensive test suite for findOneAndReplace.

Tests added (7 tests):
- Accept string ID
- Accept query filter
- Return null when not found  
- Return document after replacement by default
- Return document before replacement when returnDocument is "before"
- Handle upsert when document not found

All tests pass with full coverage of options and edge cases.
<!-- SECTION:NOTES:END -->
