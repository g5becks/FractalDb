---
id: task-18
title: Implement SQLite query translator for array operators
status: To Do
assignee: []
created_date: '2025-11-21 02:55'
labels:
  - query
dependencies: []
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add array operator translation (all, size, elemMatch) to SQLite query translator. Array operations use SQLite's json_each and json_array_length functions to query array contents while maintaining type safety and proper parameterization.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 all operator uses jsonb_each subquery to verify all values exist in array
- [ ] #2 size operator translates to json_array_length equality check
- [ ] #3 elemMatch operator recursively translates nested query filter for array elements
- [ ] #4 Array operations correctly use JSONB path syntax for nested arrays
- [ ] #5 Generated subqueries use proper correlation for array element matching
- [ ] #6 TypeScript type checking passes with zero errors
- [ ] #7 No any types used in implementation
- [ ] #8 Complete TypeDoc comments with examples showing SQL generation for array queries
<!-- AC:END -->
