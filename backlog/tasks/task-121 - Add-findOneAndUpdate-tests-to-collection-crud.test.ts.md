---
id: task-121
title: Add findOneAndUpdate tests to collection-crud.test.ts
status: Done
assignee: []
created_date: '2025-11-22 19:52'
updated_date: '2025-11-22 22:15'
labels: []
dependencies:
  - task-120
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add comprehensive tests for findOneAndUpdate covering string ID, query filter, returnDocument options, upsert, sort, and null return in test/integration/collection-crud.test.ts
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 All tests pass
- [ ] #2 Type checking passes
- [ ] #3 Linting passes
- [ ] #4 Tests with string ID pass
- [ ] #5 Tests with query filter pass
- [ ] #6 Tests returnDocument: before pass
- [ ] #7 Tests returnDocument: after pass
- [ ] #8 Tests upsert option pass
- [ ] #9 Tests with sort pass
- [ ] #10 Tests return null when not found
<!-- AC:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Added comprehensive test suite for findOneAndUpdate.

Tests added (7 tests):
- Accept string ID
- Accept query filter
- Return null when not found
- Return document after update by default
- Return document before update when returnDocument is "before"
- Respect sort option with multiple matches
- Handle upsert when document not found

All tests pass with full coverage of options and edge cases.
<!-- SECTION:NOTES:END -->
