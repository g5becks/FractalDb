---
id: task-204
title: Add minByNullable query operator to QueryBuilder
status: Done
assignee:
  - '@claude'
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
Implement the minByNullable aggregate operator that handles nullable fields, returning Nullable<'T>. SQLite naturally ignores NULL in MIN aggregates.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 Add 'minByNullable' CustomOperation to QueryBuilder in QueryExpr.fs
- [x] #2 Add translation pattern in translate function
- [x] #3 Return type is Nullable<'T>
<!-- AC:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Added minByNullable CustomOperation to QueryBuilder (QueryExpr.fs lines ~2085-2146).

Implementation:
- Takes Nullable<T> field selector
- Returns Nullable<T>
- Uses same AggregateOp.Min translation (SQLite handles NULL naturally)
- Execution via execAggregateNullable returns option<obj>
<!-- SECTION:NOTES:END -->
