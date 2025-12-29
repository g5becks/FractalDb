---
id: task-107
title: Add sortBy and sortByDescending CustomOperations to QueryBuilder
status: Done
assignee:
  - '@assistant'
created_date: '2025-12-29 06:08'
updated_date: '2025-12-29 17:11'
labels:
  - query-expressions
  - builder
dependencies:
  - task-105
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add sortBy and sortByDescending custom operations to QueryBuilder for ordering results. Both use MaintainsVariableSpace and ProjectionParameter.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 sortBy CustomOperation added to QueryBuilder
- [x] #2 sortByDescending CustomOperation added to QueryBuilder
- [x] #3 thenBy CustomOperation added to QueryBuilder
- [x] #4 thenByDescending CustomOperation added to QueryBuilder
- [x] #5 All operations use MaintainsVariableSpace = true
- [x] #6 All operations use ProjectionParameter on keySelector
- [x] #7 Code builds with no errors or warnings
- [x] #8 All existing tests pass

- [x] #9 XML doc comments on sortBy, sortByDescending, thenBy, thenByDescending operations
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Add sortBy CustomOperation with MaintainsVariableSpace=true
2. Add sortByDescending CustomOperation with MaintainsVariableSpace=true
3. Add thenBy CustomOperation for secondary sorting
4. Add thenByDescending CustomOperation for secondary sorting
5. All operations use [<ProjectionParameter>] on keySelector parameter
6. All return Unchecked.defaultof (quotation-based approach)
7. Add comprehensive XML documentation for all operations
8. Build project and verify compilation
9. Run tests to ensure no regressions
<!-- SECTION:PLAN:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Added 4 sorting CustomOperations: sortBy, sortByDescending, thenBy, thenByDescending. All with MaintainsVariableSpace=true and ProjectionParameter. Added 250+ lines comprehensive XML documentation. Build successful.
<!-- SECTION:NOTES:END -->
