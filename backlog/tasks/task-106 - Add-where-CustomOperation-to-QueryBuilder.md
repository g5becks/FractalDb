---
id: task-106
title: Add where CustomOperation to QueryBuilder
status: Done
assignee:
  - '@assistant'
created_date: '2025-12-29 06:08'
updated_date: '2025-12-29 17:10'
labels:
  - query-expressions
  - builder
dependencies:
  - task-105
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add the where custom operation to QueryBuilder with MaintainsVariableSpace and ProjectionParameter attributes. This enables 'where (predicate)' syntax in query expressions.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 where CustomOperation added to QueryBuilder
- [x] #2 MaintainsVariableSpace = true attribute set
- [x] #3 ProjectionParameter attribute on predicate parameter
- [x] #4 Code builds with no errors or warnings
- [x] #5 All existing tests pass

- [x] #6 XML doc comments on Where operation explaining predicate translation
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Research F# CustomOperation attribute requirements
2. Add [<CustomOperation("where")>] attribute with MaintainsVariableSpace=true
3. Add [<ProjectionParameter>] attribute on predicate parameter
4. Implement Where member that takes source and predicate
5. Return Unchecked.defaultof since method is never executed
6. Add comprehensive XML documentation
7. Build project and verify compilation
8. Run tests to ensure no regressions
<!-- SECTION:PLAN:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Added where CustomOperation with MaintainsVariableSpace=true and ProjectionParameter attributes. Enables filtering syntax with support for comparison, logical, and string operators. Added 150+ lines XML documentation with examples. Build successful.
<!-- SECTION:NOTES:END -->
