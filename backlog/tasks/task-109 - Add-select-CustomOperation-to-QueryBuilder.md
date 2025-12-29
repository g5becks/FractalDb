---
id: task-109
title: Add select CustomOperation to QueryBuilder
status: Done
assignee:
  - '@assistant'
created_date: '2025-12-29 06:09'
updated_date: '2025-12-29 17:14'
labels:
  - query-expressions
  - builder
dependencies:
  - task-105
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add select custom operation to QueryBuilder for projections. Uses AllowIntoPattern = true to enable projection into different types.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 select CustomOperation added to QueryBuilder
- [x] #2 AllowIntoPattern = true attribute set
- [x] #3 ProjectionParameter attribute on projection parameter
- [x] #4 Returns TranslatedQuery<R> (different type parameter)
- [x] #5 Code builds with no errors or warnings
- [x] #6 All existing tests pass

- [x] #7 XML doc comments on select operation explaining projection types
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Add select CustomOperation with AllowIntoPattern=true
2. Use ProjectionParameter attribute on projection parameter
3. Return type changes from seq<T> to TranslatedQuery<R> (different type)
4. Return Unchecked.defaultof (quotation-based approach)
5. Add comprehensive XML documentation explaining projection patterns
6. Build project and verify compilation
7. Run tests to ensure no regressions
<!-- SECTION:PLAN:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Added select CustomOperation with AllowIntoPattern=true enabling type transformation from T to R. Supports 6 projection patterns: identity, single field, tuple, anonymous record, nested fields, computed values. Added 150+ lines comprehensive XML documentation. Build successful 0 errors/warnings.
<!-- SECTION:NOTES:END -->
