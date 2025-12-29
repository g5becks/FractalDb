---
id: task-113
title: Implement QueryTranslator.translatePredicate for logical operators
status: Done
assignee:
  - '@assistant'
created_date: '2025-12-29 06:11'
updated_date: '2025-12-29 17:22'
labels:
  - query-expressions
  - translator
dependencies:
  - task-112
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Extend translatePredicate to handle logical operators: && (AND), || (OR), not. These recursively call translatePredicate for nested expressions.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 Handles && operator via SpecificCall, returns Query.And
- [x] #2 Handles || operator via SpecificCall, returns Query.Or
- [x] #3 Handles not operator via SpecificCall, returns Query.Not
- [x] #4 Recursively translates nested predicates
- [x] #5 Code builds with no errors or warnings
- [x] #6 All existing tests pass

- [x] #7 Internal implementation - brief doc comment explaining purpose
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Study F# logical operator representations in quotations
2. Add Call pattern cases for logical operators to translatePredicate:
   - op_BooleanAnd (&&) → Query.And with recursive translation
   - op_BooleanOr (||) → Query.Or with recursive translation  
   - op_LogicalNot (not) → Query.Not with recursive translation
3. Ensure recursive calls preserve type parameter <'T>
4. Add brief doc comments explaining logical operator handling
5. Build and verify no errors
6. Run tests to ensure no regressions
<!-- SECTION:PLAN:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Extended translatePredicate with 3 logical operators: && (Query.And), || (Query.Or), not (Query.Not). All operators recursively call translatePredicate for nested expressions. Enables complex boolean logic in where clauses. Build successful 0 errors/warnings.
<!-- SECTION:NOTES:END -->
