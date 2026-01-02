---
id: task-209
title: Add unit and integration tests for nullable aggregate operators
status: Done
assignee: []
created_date: '2026-01-01 23:27'
updated_date: '2026-01-01 23:31'
labels:
  - tests
  - aggregate
dependencies: []
priority: medium
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add comprehensive tests for minByNullable, maxByNullable, sumByNullable, averageByNullable operators.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 Test: nullable aggregates return value when data exists
- [x] #2 Test: nullable aggregates return null when no data matches
- [x] #3 Test: nullable aggregates ignore NULL values in calculation
- [x] #4 Integration tests with real SQLite database
<!-- AC:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Added 6 integration tests for execAggregateNullable in QueryExprTests.fs:

1. Min returns Some when data exists
2. Max returns Some when data exists
3. Sum returns Some when data exists
4. Avg returns Some when data exists
5. Returns None when no documents match filter
6. With filter returns correct value

All 672 tests pass.
<!-- SECTION:NOTES:END -->
