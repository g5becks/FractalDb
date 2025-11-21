---
id: task-7
title: Define query comparison operators
status: To Do
assignee: []
created_date: '2025-11-21 02:30'
labels:
  - query
  - types
dependencies: []
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Implement type-safe comparison operator types that enforce type constraints at compile time. For example, greater-than operators only work with numbers and dates, preventing nonsensical queries like checking if a string is greater than another string.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 ComparisonOperator<T> type defined with eq, ne, gt, gte, lt, lte, in, nin operators
- [ ] #2 Ordering operators (gt, gte, lt, lte) constrained to number and Date types only using conditional types
- [ ] #3 in and nin operators use ReadonlyArray<T> for type safety
- [ ] #4 All operator properties use readonly modifier
- [ ] #5 Type correctly prevents usage of ordering operators on string, boolean, and object types
- [ ] #6 TypeScript type checking passes with zero errors
- [ ] #7 No any types used in implementation
- [ ] #8 Complete TypeDoc comments with examples showing valid and invalid operator usage
<!-- AC:END -->
