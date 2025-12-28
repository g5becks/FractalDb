---
id: task-15
title: Implement ArrayOp and ExistsOp Discriminated Unions
status: To Do
assignee: []
created_date: '2025-12-28 06:30'
updated_date: '2025-12-28 07:03'
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
- [ ] #3 Define ArrayOp<'T> with cases: All of 'T list, Size of int (Note: ElemMatch and Index are deferred - add comments for future)
- [ ] #4 Add ExistsOp type with [<RequireQualifiedAccess>]
- [ ] #5 Define ExistsOp with case: Exists of bool
- [ ] #6 Run 'dotnet build' - build succeeds

- [ ] #7 All public types and functions have XML doc comments with <summary>, and public functions include <param>, <returns>, and <example> (see doc-2 for standards)
<!-- AC:END -->
