---
id: task-108
title: Add take and skip CustomOperations to QueryBuilder
status: Done
assignee:
  - '@assistant'
created_date: '2025-12-29 06:09'
updated_date: '2025-12-29 17:12'
labels:
  - query-expressions
  - builder
dependencies:
  - task-105
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add take and skip custom operations to QueryBuilder for pagination. These take int count directly, not a projection parameter.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 take CustomOperation added to QueryBuilder
- [x] #2 skip CustomOperation added to QueryBuilder
- [x] #3 Both operations use MaintainsVariableSpace = true
- [x] #4 Code builds with no errors or warnings
- [x] #5 All existing tests pass

- [x] #6 XML doc comments on take and skip operations with examples
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Add take CustomOperation with MaintainsVariableSpace=true
2. Add skip CustomOperation with MaintainsVariableSpace=true
3. Both operations take int count parameter (not ProjectionParameter)
4. Return Unchecked.defaultof (quotation-based approach)
5. Add comprehensive XML documentation for both operations
6. Build project and verify compilation
7. Run tests to ensure no regressions
<!-- SECTION:PLAN:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Added take and skip CustomOperations with MaintainsVariableSpace=true. Enables pagination syntax in query expressions. Added 150+ lines with comprehensive XML documentation. Build successful with 0 errors, 0 warnings.
<!-- SECTION:NOTES:END -->
