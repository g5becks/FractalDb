---
id: task-18
title: Implement SQLite query translator for array operators
status: In Progress
assignee:
  - '@claude'
created_date: '2025-11-21 02:55'
updated_date: '2025-11-21 06:35'
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

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Add array operator cases to translateSingleOperator
2. Implement $all operator using json_each to verify all values exist
3. Implement $size operator using json_array_length
4. Implement $elemMatch with recursive query filter translation
5. Implement $index operator for array element access
6. Add comprehensive TypeDoc with SQL examples
7. Verify TypeScript compilation and linting
8. Update task 18 and mark complete
<!-- SECTION:PLAN:END -->
