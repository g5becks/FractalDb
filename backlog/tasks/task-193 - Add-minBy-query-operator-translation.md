---
id: task-193
title: Add minBy query operator translation
status: Done
assignee:
  - '@claude'
created_date: '2026-01-01 22:15'
updated_date: '2026-01-01 22:58'
labels: []
dependencies:
  - task-192
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add pattern matching in translateQuery to handle minBy query operator. Extract the field from the selector expression and set Aggregate = Some(AggregateOp.Min field).
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 Pattern match for minBy in translateQuery
- [x] #2 Extracts field name from selector
- [x] #3 Sets Aggregate = Some(AggregateOp.Min field)
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Add MinBy custom operation to QueryBuilder
2. Add pattern match for MinBy in translate function
3. Extract field name and set Aggregate = Some(AggregateOp.Min field)
<!-- SECTION:PLAN:END -->
