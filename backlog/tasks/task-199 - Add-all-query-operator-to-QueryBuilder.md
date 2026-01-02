---
id: task-199
title: Add all query operator to QueryBuilder
status: Done
assignee:
  - '@claude'
created_date: '2026-01-01 23:12'
updated_date: '2026-01-01 23:14'
labels: []
dependencies: []
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Implement the 'all' operator that checks if ALL documents match a predicate. Returns true if every document satisfies the condition, false otherwise. Implementation uses NOT EXISTS (SELECT 1 WHERE NOT predicate) pattern.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 Add 'all' CustomOperation to QueryBuilder in QueryExpr.fs
- [ ] #2 Add AllOp or flag to TranslatedQuery type
- [ ] #3 Add translation pattern in translate function
<!-- AC:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Added "all" CustomOperation to QueryBuilder in QueryExpr.fs (lines ~2279-2358).
The operator takes a predicate and returns bool.
Full XML documentation added.

Note: ACs 2 and 3 are not needed for this architecture. Like aggregate operators (minBy, maxBy, etc.), "all" returns a scalar type (bool), not TranslatedQuery. The execution happens via Collection.execAll directly with a Query<T> filter. No TranslatedQuery.AllOp field or translate pattern is required.
<!-- SECTION:NOTES:END -->
