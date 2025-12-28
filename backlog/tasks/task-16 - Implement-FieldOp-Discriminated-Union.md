---
id: task-16
title: Implement FieldOp Discriminated Union
status: To Do
assignee: []
created_date: '2025-12-28 06:31'
updated_date: '2025-12-28 07:03'
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
Create the FieldOp DU in Core/Operators.fs - type-erased wrapper for all field operations. Reference: FSHARP_PORT_DESIGN.md lines 309-316.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 In src/FractalDb/Core/Operators.fs, add FieldOp type
- [ ] #2 Add [<RequireQualifiedAccess>] attribute
- [ ] #3 Define FieldOp DU with cases: Compare of obj (boxed CompareOp), String of StringOp, Array of obj (boxed ArrayOp), Exist of ExistsOp
- [ ] #4 Run 'dotnet build' - build succeeds

- [ ] #5 All public types and functions have XML doc comments with <summary>, and public functions include <param>, <returns>, and <example> (see doc-2 for standards)
<!-- AC:END -->
