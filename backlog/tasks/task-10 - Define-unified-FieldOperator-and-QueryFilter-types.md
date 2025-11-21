---
id: task-10
title: Define unified FieldOperator and QueryFilter types
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
Combine all operator types into a unified type-safe query filter system. FieldOperator unions all applicable operators for a field based on its type, while QueryFilter provides the top-level query interface supporting both direct property access and nested path queries.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 FieldOperator<T> type defined as union of ComparisonOperator, conditional StringOperator, conditional ArrayOperator, and ExistenceOperator
- [ ] #2 QueryFilter<T> type defined supporting LogicalOperator, direct property access, and nested path access
- [ ] #3 QueryFilter uses Simplify from type-fest for clean IDE hover display
- [ ] #4 Nested path queries use DocumentPath<T> and PathValue<T, P> for type safety
- [ ] #5 Type correctly prevents invalid operator usage based on field types
- [ ] #6 TypeScript type checking passes with zero errors
- [ ] #7 No any types used in implementation
- [ ] #8 Complete TypeDoc comments with examples showing complex nested queries with operators
<!-- AC:END -->
