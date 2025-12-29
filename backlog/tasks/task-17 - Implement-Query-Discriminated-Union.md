---
id: task-17
title: Implement Query Discriminated Union
status: Done
assignee:
  - '@assistant'
created_date: '2025-12-28 06:31'
updated_date: '2025-12-28 17:22'
labels:
  - phase-1
  - core
  - query
dependencies:
  - task-16
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Create the Query<'T> DU in src/Query.fs - the main query structure. Reference: FSHARP_PORT_DESIGN.md lines 322-329.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Add namespace FractalDb.Core
- [x] #2 Define Query<'T> DU with cases: Empty, Field of fieldName: string * FieldOp, And of Query<'T> list, Or of Query<'T> list, Nor of Query<'T> list, Not of Query<'T>
- [x] #3 Run 'dotnet build' - build succeeds
- [x] #4 All public types and functions have XML doc comments with <summary>, and public functions include <param>, <returns>, and <example> (see doc-2 for standards)
- [x] #5 Run 'task lint' - no errors or warnings

- [ ] #6 Create file src/Query.fs

- [ ] #7 Add module declaration: module FractalDb.Query
<!-- AC:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Implemented Query<'T> discriminated union in src/Operators.fs (not src/Query.fs) with six cases:
- Empty (match all)
- Field of fieldName: string * FieldOp
- And of Query<'T> list
- Or of Query<'T> list  
- Nor of Query<'T> list
- Not of Query<'T>

Architectural note: Due to F# mutual recursion requirements (ArrayOp → Query → FieldOp → ArrayOp), implemented in Operators.fs using `and` keyword rather than separate Query.fs file. This keeps all mutually recursive types in one file. ACs #1, #6, #7 (namespace/module/file) are not applicable as type is in Operators.fs module. All types include comprehensive XML documentation. Build passes with 0 errors/warnings, lint passes with 0 warnings.
<!-- SECTION:NOTES:END -->
