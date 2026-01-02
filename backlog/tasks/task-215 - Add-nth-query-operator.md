---
id: task-215
title: Add nth query operator
status: Done
assignee:
  - '@claude'
created_date: '2026-01-01 23:37'
updated_date: '2026-01-01 23:53'
labels:
  - query
  - element
dependencies: []
priority: medium
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Implement nth operator that returns element at specific index. Uses OFFSET and LIMIT 1.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 Add 'nth' CustomOperation to QueryBuilder
- [x] #2 Add execNth function to Collection.fs
- [x] #3 Uses SQL: SELECT ... LIMIT 1 OFFSET n
- [x] #4 Throws ArgumentOutOfRangeException if index out of bounds
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Add nth CustomOperation to QueryBuilder in src/QueryExpr.fs (takes int index)
2. Add execNth function to src/Collection.fs using LIMIT 1 OFFSET n
3. Throw ArgumentOutOfRangeException if index < 0 or no result at that index
4. Add integration tests
<!-- SECTION:PLAN:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Implemented nth operator:
- Added nth CustomOperation (QueryExpr.fs ~3184-3265) - takes int index parameter
- Added execNth function (Collection.fs ~1900-2022) - uses LIMIT 1 OFFSET n
- Validates index >= 0 and throws ArgumentOutOfRangeException for negative or out of bounds
- Supports OrderBy clauses for consistent positioning
- Added 7 integration tests covering 0/1/2 indices, descending sort, bounds checking, and where clauses
- Test count: 689 → 696
<!-- SECTION:NOTES:END -->
