---
id: task-217
title: Add sortByNullable query operator to QueryBuilder
status: Done
assignee:
  - '@claude'
created_date: '2026-01-01 23:59'
updated_date: '2026-01-02 00:08'
labels:
  - query
  - sorting
dependencies: []
priority: medium
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Implement sortByNullable operator that handles NULL values explicitly in sorting (NULLs last by default). Uses: ORDER BY field IS NULL, field ASC
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 Add sortByNullable CustomOperation to QueryBuilder
- [x] #2 Add sortByNullableDescending CustomOperation
- [x] #3 Generates SQL: ORDER BY field IS NULL, field ASC/DESC
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Add AscNullsLast and DescNullsLast to SortDirection type in QueryExpr.fs
2. Add sortByNullable CustomOperation to QueryBuilder
3. Add sortByNullableDescending CustomOperation to QueryBuilder
4. Add translation patterns for SortByNullable and SortByNullableDescending
5. Update SQL generation in Collection.fs to handle nulls-last sorting
6. Add integration tests
<!-- SECTION:PLAN:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Implemented sortByNullable and sortByNullableDescending operators:

- Added AscNullsLast and DescNullsLast variants to SortDirection type
- Added sortByNullable and sortByNullableDescending CustomOperations in QueryBuilder
- Added translation patterns for both operators
- Updated exec, execLast, execLastOrDefault, and execNth functions to handle nullable sort directions
- Generates SQL: ORDER BY field IS NULL, field ASC/DESC (puts NULLs last)
- Added 9 integration tests covering ascending/descending, filtering, take, execLast, execLastOrDefault
- All 705 tests pass
<!-- SECTION:NOTES:END -->
