---
id: task-15
title: Implement ArrayOp and ExistsOp Discriminated Unions
status: To Do
assignee: []
created_date: '2025-12-28 06:30'
updated_date: '2025-12-28 16:34'
labels:
  - phase-1
  - core
  - operators
dependencies:
  - task-14
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Create ArrayOp<'T> and ExistsOp DUs in Core/Operators.fs. Reference: FSHARP_PORT_DESIGN.md lines 288-304.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 In src/FractalDb/Core/Operators.fs, add ArrayOp<'T> type
- [ ] #2 Add [<RequireQualifiedAccess>] attribute to ArrayOp
- [ ] #3 Add ExistsOp type with [<RequireQualifiedAccess>]
- [ ] #4 Define ExistsOp with case: Exists of bool
- [ ] #5 Run 'dotnet build' - build succeeds
- [ ] #6 All public types and functions have XML doc comments with <summary>, and public functions include <param>, <returns>, and <example> (see doc-2 for standards)

- [ ] #7 Define ArrayOp<'T> with cases: All of 'T list, Size of int, ElemMatch of Query<'T>, Index of index: int * Query<'T>

- [ ] #8 Run 'task lint' - no errors or warnings
<!-- AC:END -->
