---
id: task-198
title: Add integration tests for aggregate query operators
status: Done
assignee:
  - '@claude'
created_date: '2026-01-01 22:26'
updated_date: '2026-01-01 23:04'
labels: []
dependencies:
  - task-197
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add integration tests that execute actual aggregate queries against a real SQLite database to verify correct results.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 Test: minBy returns minimum value
- [x] #2 Test: maxBy returns maximum value
- [x] #3 Test: sumBy returns sum of values
- [x] #4 Test: averageBy returns average of values
- [x] #5 Test: aggregates work with where clause filter
- [x] #6 Test: aggregates on empty collection return appropriate default
<!-- AC:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Added 7 integration tests in tests/QueryExprTests.fs (lines 2167-2253):
- execAggregate with Min returns minimum value
- execAggregate with Max returns maximum value
- execAggregate with Sum returns sum of values
- execAggregate with Avg returns average of values
- execAggregate with Count returns count
- execAggregate with filter (Min on active users)
- execAggregate with Sum and filter

All tests pass. Test count increased from 648 to 655.

Additional tests added:
- execAggregate on empty result set returns DBNull for Min/Max/Sum/Avg
- execAggregate Count on empty result set returns 0

Final test count: 657 (up from 648)
<!-- SECTION:NOTES:END -->
