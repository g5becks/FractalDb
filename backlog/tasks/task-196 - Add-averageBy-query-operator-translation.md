---
id: task-196
title: Add averageBy query operator translation
status: Done
assignee: []
created_date: '2026-01-01 22:21'
updated_date: '2026-01-01 22:59'
labels: []
dependencies:
  - task-195
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add pattern matching in translateQuery to handle averageBy query operator. Uses AggregateOp.Avg.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 Pattern match for averageBy in translateQuery
- [x] #2 Sets Aggregate = Some(AggregateOp.Avg field)
<!-- AC:END -->
