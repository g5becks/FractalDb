---
id: task-116
title: Implement QueryTranslator.translateProjection helper
status: Done
assignee:
  - '@assistant'
created_date: '2025-12-29 06:12'
updated_date: '2025-12-29 17:26'
labels:
  - query-expressions
  - translator
dependencies:
  - task-111
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Implement translateProjection helper that analyzes projection expressions to determine if it's SelectAll, SelectSingle (single field), or SelectFields (multiple fields via anonymous record).
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 translateProjection function defined
- [x] #2 Lambda with PropertyGet returns SelectSingle
- [x] #3 Lambda with NewRecord returns SelectFields
- [x] #4 Other patterns return SelectAll
- [x] #5 Code builds with no errors or warnings
- [x] #6 All existing tests pass

- [x] #7 XML doc comments on translateProjection explaining SelectAll/SelectFields/SelectSingle detection
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Study Projection type cases (SelectAll, SelectFields, SelectSingle)
2. Research F# quotation patterns for projections:
   - Lambda + Var: identity (select x) → SelectAll
   - Lambda + PropertyGet: single field (select x.Name) → SelectSingle
   - Lambda + NewTuple: tuple projection (select (x.A, x.B)) → SelectFields
   - Lambda + NewRecord: anonymous record (select {| A=x.A |}) → SelectFields
3. Implement translateProjection function with pattern matching
4. Extract field names from projections using extractPropertyName
5. Integrate into translate function (replace SelectAll placeholder)
6. Add XML documentation
7. Build and verify
8. Run tests
<!-- SECTION:PLAN:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Implemented translateProjection helper analyzing projection expressions. Handles 5 patterns: Lambda+Var (SelectAll), Lambda+PropertyGet (SelectSingle), Lambda+NewTuple (SelectFields), Lambda+NewRecord (SelectFields), fallback (SelectAll). Integrated into translate function Select case. Added 150+ lines comprehensive XML documentation. Build successful 0 errors/warnings.
<!-- SECTION:NOTES:END -->
