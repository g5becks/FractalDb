---
id: task-207
title: Add averageByNullable query operator to QueryBuilder
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
Implement the averageByNullable aggregate operator that handles nullable fields, returning Nullable<float>. SQLite naturally ignores NULL in AVG aggregates.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 Add 'averageByNullable' CustomOperation to QueryBuilder in QueryExpr.fs
- [x] #2 Add translation pattern in translate function
- [x] #3 Return type is Nullable<float>
<!-- AC:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Added averageByNullable CustomOperation to QueryBuilder (QueryExpr.fs lines ~2271-2332).

Implementation:
- Takes Nullable<T> field selector
- Returns Nullable<float> (average always returns float)
- Uses same AggregateOp.Avg translation
- Execution via execAggregateNullable returns option<obj>
<!-- SECTION:NOTES:END -->
