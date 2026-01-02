---
id: task-208
title: Add execAggregateNullable function to Collection.fs
status: Done
assignee: []
created_date: '2026-01-01 23:27'
updated_date: '2026-01-01 23:31'
labels:
  - collection
  - aggregate
dependencies: []
priority: medium
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add execution function for nullable aggregate operators. Similar to execAggregate but returns Nullable<obj> to handle NULL results from empty collections or all-NULL fields.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 Add execAggregateNullable function to Collection.fs
- [x] #2 Handle NULL results from SQLite
- [x] #3 Return Nullable<obj> that can be cast to appropriate type
<!-- AC:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Added execAggregateNullable function to Collection.fs (lines ~1108-1195).

Implementation:
- Same SQL as execAggregate (SQLite ignores NULL in aggregates)
- Returns option<obj> instead of obj
- Handles DBNull.Value by returning None
- Returns None for empty result sets or all-NULL fields
- Caller casts inner value to appropriate type
<!-- SECTION:NOTES:END -->
