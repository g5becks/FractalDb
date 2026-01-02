---
id: task-218
title: Add thenByNullable query operators
status: Done
assignee:
  - '@claude'
created_date: '2026-01-01 23:59'
updated_date: '2026-01-02 00:10'
labels:
  - query
  - sorting
dependencies: []
priority: medium
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Implement thenByNullable and thenByNullableDescending operators for secondary nullable sorting.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 Add thenByNullable CustomOperation
- [x] #2 Add thenByNullableDescending CustomOperation
- [x] #3 Works with existing nullable sort for multi-field sorting
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Add thenByNullable CustomOperation to QueryBuilder (similar to sortByNullable but appends to OrderBy)
2. Add thenByNullableDescending CustomOperation to QueryBuilder
3. Add translation patterns for ThenByNullable and ThenByNullableDescending
4. Add integration tests for multi-field nullable sorting
5. Verify all tests pass
<!-- SECTION:PLAN:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Implemented thenByNullable and thenByNullableDescending operators:

- Added thenByNullable CustomOperation to QueryBuilder
- Added thenByNullableDescending CustomOperation to QueryBuilder
- Added translation patterns for both operators (appends to OrderBy with AscNullsLast/DescNullsLast)
- Works with existing sortBy/sortByDescending/sortByNullable/sortByNullableDescending
- Added 5 integration tests covering:
  - thenByNullable with regular sortBy
  - thenByNullableDescending with regular sortBy
  - sortByNullable with regular thenBy
  - thenByNullable with take
  - sortByNullable with thenByNullable
- All 710 tests pass
<!-- SECTION:NOTES:END -->
