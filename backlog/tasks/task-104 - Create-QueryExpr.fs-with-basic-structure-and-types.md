---
id: task-104
title: Create QueryExpr.fs with basic structure and types
status: Done
assignee:
  - '@assistant'
created_date: '2025-12-29 06:08'
updated_date: '2025-12-29 17:01'
labels:
  - query-expressions
  - foundation
dependencies: []
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Create the QueryExpr.fs file with the basic structure: TranslatedQuery<T> record, Projection DU, SortDirection type. This sets up the foundation for the quotation-based query builder.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 QueryExpr.fs file created in src/ directory
- [x] #2 TranslatedQuery<T> record defined with Source, Where, OrderBy, Skip, Take, Projection fields
- [x] #3 Projection DU defined with SelectAll, SelectFields, SelectSingle cases
- [x] #4 File added to fsproj in correct order
- [x] #5 Code builds with no errors or warnings
- [x] #6 All existing tests pass

- [x] #7 XML doc comments on TranslatedQuery<T> record and Projection DU with summary and remarks
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Create QueryExpr.fs file in src/ directory
2. Define TranslatedQuery<T> record with fields: Source, Where, OrderBy, Skip, Take, Projection
3. Define Projection discriminated union with cases: SelectAll, SelectFields of string list, SelectSingle of string
4. Add XML documentation to both types following FractalDb documentation standards
5. Add QueryExpr.fs to FractalDb.fsproj after Query.fs and before Schema.fs
6. Build project to verify no errors
7. Run existing test suite to ensure no regressions
<!-- SECTION:PLAN:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Created QueryExpr.fs with foundational types: SortDirection (Asc/Desc), Projection (SelectAll/SelectFields/SelectSingle), and TranslatedQuery<'T> record. Added comprehensive XML documentation. Build successful (0 errors, 0 warnings).
<!-- SECTION:NOTES:END -->
