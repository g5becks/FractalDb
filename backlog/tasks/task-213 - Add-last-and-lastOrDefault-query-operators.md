---
id: task-213
title: Add last and lastOrDefault query operators
status: Done
assignee:
  - '@claude'
created_date: '2026-01-01 23:37'
updated_date: '2026-01-01 23:48'
labels:
  - query
  - element
dependencies: []
priority: medium
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Implement last/lastOrDefault operators that return the last element of a sorted query. Translates to reversing sort order and taking first element.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 Add 'last' CustomOperation to QueryBuilder
- [x] #2 Add 'lastOrDefault' CustomOperation to QueryBuilder
- [x] #3 Add execLast function to Collection.fs that reverses sort and takes first
- [x] #4 last throws InvalidOperationException if empty, lastOrDefault returns None
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Add last CustomOperation to QueryBuilder in src/QueryExpr.fs
2. Add lastOrDefault CustomOperation to QueryBuilder
3. Add execLast function to src/Collection.fs (reverses sort, takes 1, throws if empty)
4. Add execLastOrDefault function to src/Collection.fs (reverses sort, takes 1, returns option)
<!-- SECTION:PLAN:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Implemented last/lastOrDefault operators:
- Added last CustomOperation to QueryBuilder (QueryExpr.fs ~2862-2935)
- Added lastOrDefault CustomOperation to QueryBuilder (QueryExpr.fs ~2937-3013)
- Added execLast function to Collection.fs (~1474-1591) - reverses sort order and takes first
- Added execLastOrDefault function to Collection.fs (~1593-1704) - returns option
- Added 7 integration tests covering sort reversal, where clause filtering, exception throwing, and Option handling
- Test count: 675 → 682
<!-- SECTION:NOTES:END -->
