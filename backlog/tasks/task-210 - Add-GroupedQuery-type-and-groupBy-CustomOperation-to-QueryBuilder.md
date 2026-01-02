---
id: task-210
title: Add GroupedQuery type and groupBy CustomOperation to QueryBuilder
status: Done
assignee:
  - '@claude'
created_date: '2026-01-01 23:36'
updated_date: '2026-01-01 23:59'
labels:
  - query
  - grouping
dependencies: []
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Implement the groupBy operator that groups documents by a field. This is a HIGH complexity task requiring new types and SQL GROUP BY generation. The groupBy operator enables GROUP BY queries with aggregate functions on groups.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 Add GroupedQuery<'Key, 'T> type to represent grouped results
- [x] #2 Add 'groupBy' CustomOperation to QueryBuilder in QueryExpr.fs
- [x] #3 CustomOperation captures grouping field for SQL generation
<!-- AC:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Implemented groupBy operator:
- Added GroupBy field to TranslatedQuery type (QueryExpr.fs ~400-423)
- Added groupBy CustomOperation to QueryBuilder (QueryExpr.fs ~3360-3436)
- Added execGroupBy function to Collection.fs
- Translation captures field name via extractPropertyName
- Tests verify GROUP BY SQL generation and counting
<!-- SECTION:NOTES:END -->
