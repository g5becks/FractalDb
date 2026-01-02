---
id: task-202
title: Add find query operator to QueryBuilder
status: Done
assignee:
  - '@claude'
created_date: '2026-01-01 23:12'
updated_date: '2026-01-01 23:25'
labels: []
dependencies: []
priority: medium
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Implement the 'find' operator that returns the first element matching a predicate. Throws if not found (unlike headOrDefault). Translates to where + take 1 + throw if empty.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 Add 'find' CustomOperation to QueryBuilder in QueryExpr.fs
- [x] #2 Add translation pattern combining where + take 1
- [x] #3 Execution throws InvalidOperationException if no match
<!-- AC:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Implemented execFind function in Collection.fs (lines ~1205-1285).

Key implementation details:
- Takes Query<T> predicate and Collection<T>
- Translates predicate to SQL WHERE clause with LIMIT 1
- Returns Document<T> on match
- Throws InvalidOperationException if no document matches
- Follows same pattern as findOne but throws instead of returning None

The find CustomOperation was already in QueryExpr.fs (lines 2597-2684).
<!-- SECTION:NOTES:END -->
