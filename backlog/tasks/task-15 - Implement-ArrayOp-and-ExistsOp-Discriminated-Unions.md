---
id: task-15
title: Implement ArrayOp and ExistsOp Discriminated Unions
status: To Do
assignee: []
created_date: '2025-12-28 06:30'
updated_date: '2025-12-28 16:54'
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
Create ArrayOp<'T> and ExistsOp DUs in src/Operators.fs. Reference: FSHARP_PORT_DESIGN.md lines 288-304.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Add [<RequireQualifiedAccess>] attribute to ArrayOp
- [ ] #2 Add ExistsOp type with [<RequireQualifiedAccess>]
- [ ] #3 Define ExistsOp with case: Exists of bool
- [ ] #4 Run 'dotnet build' - build succeeds
- [ ] #5 All public types and functions have XML doc comments with <summary>, and public functions include <param>, <returns>, and <example> (see doc-2 for standards)
- [ ] #6 Define ArrayOp<'T> with cases: All of 'T list, Size of int, ElemMatch of Query<'T>, Index of index: int * Query<'T>

- [ ] #7 Run 'task lint' - no errors or warnings

- [ ] #8 In src/Operators.fs, add ArrayOp<'T> type
<!-- AC:END -->
