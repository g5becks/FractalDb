---
id: task-206
title: Add sumByNullable query operator to QueryBuilder
status: Done
assignee: []
created_date: '2026-01-01 23:27'
updated_date: '2026-01-01 23:31'
labels:
  - query
  - aggregate
dependencies: []
priority: medium
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Implement the sumByNullable aggregate operator that handles nullable fields, returning Nullable<'T>. SQLite naturally ignores NULL in SUM aggregates.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 Add 'sumByNullable' CustomOperation to QueryBuilder in QueryExpr.fs
- [x] #2 Add translation pattern in translate function
- [x] #3 Return type is Nullable<'T>
<!-- AC:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Added sumByNullable CustomOperation to QueryBuilder (QueryExpr.fs lines ~2209-2270).

Implementation:
- Takes Nullable<T> field selector
- Returns Nullable<T>
- Uses same AggregateOp.Sum translation
- Execution via execAggregateNullable returns option<obj>
<!-- SECTION:NOTES:END -->
