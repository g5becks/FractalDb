---
id: task-191
title: Add Aggregate field to TranslatedQuery type
status: Done
assignee:
  - '@claude'
created_date: '2026-01-01 22:11'
updated_date: '2026-01-01 22:54'
labels: []
dependencies:
  - task-190
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Extend TranslatedQuery<'T> in QueryExpr.fs to include Aggregate: AggregateOp option field. Update all places that create TranslatedQuery to initialize Aggregate = None.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 Aggregate: AggregateOp option field added
- [x] #2 Default value is None
- [x] #3 All existing TranslatedQuery creations updated
- [x] #4 XML documentation added
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Add Aggregate: AggregateOp option to TranslatedQuery<T>
2. Update compose method to merge Aggregate
3. Update empty initialization in translate function
4. Add XML documentation
<!-- SECTION:PLAN:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Added Aggregate: option<AggregateOp> field to TranslatedQuery<T>:

1. Field added after Distinct with XML documentation (lines 379-395)
2. Default value is None in empty initialization
3. compose method updated to merge Aggregate (later non-None wins)
4. XML documentation explains aggregate operators (minBy, maxBy, sumBy, averageBy, count)

All 648 tests pass.
<!-- SECTION:NOTES:END -->
