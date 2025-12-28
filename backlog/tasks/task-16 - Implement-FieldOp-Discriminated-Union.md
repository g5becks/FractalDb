---
id: task-16
title: Implement FieldOp Discriminated Union
status: To Do
assignee: []
created_date: '2025-12-28 06:31'
updated_date: '2025-12-28 16:54'
labels:
  - phase-1
  - core
  - operators
dependencies:
  - task-15
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Create the FieldOp DU in src/Operators.fs - type-erased wrapper for all field operations. Reference: FSHARP_PORT_DESIGN.md lines 309-316.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Add [<RequireQualifiedAccess>] attribute
- [ ] #2 Define FieldOp DU with cases: Compare of obj (boxed CompareOp), String of StringOp, Array of obj (boxed ArrayOp), Exist of ExistsOp
- [ ] #3 Run 'dotnet build' - build succeeds
- [ ] #4 All public types and functions have XML doc comments with <summary>, and public functions include <param>, <returns>, and <example> (see doc-2 for standards)

- [ ] #5 Run 'task lint' - no errors or warnings

- [ ] #6 In src/Operators.fs, add FieldOp type
<!-- AC:END -->
