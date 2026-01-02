---
id: task-212
title: Add integration tests for groupBy operator
status: Done
assignee: []
created_date: '2026-01-01 23:37'
updated_date: '2026-01-01 23:43'
labels:
  - tests
  - grouping
dependencies: []
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add comprehensive tests for the groupBy query operator including GROUP BY SQL generation and aggregate functions on groups.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 Test: groupBy generates correct GROUP BY SQL
- [x] #2 Test: groupBy with Count() returns correct counts per group
- [x] #3 Test: groupBy with where clause filters before grouping
- [x] #4 Test: groupBy with multiple groups works correctly
<!-- AC:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
4 integration tests added to QueryExprTests.fs covering groupBy SQL generation, counts, where clause filtering, and multiple groups.
<!-- SECTION:NOTES:END -->
