---
id: task-14
title: Implement StringOp Discriminated Union
status: To Do
assignee: []
created_date: '2025-12-28 06:30'
updated_date: '2025-12-28 16:34'
labels:
  - phase-1
  - core
  - operators
dependencies:
  - task-13
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Create the StringOp DU in Core/Operators.fs for string-specific operations. Reference: FSHARP_PORT_DESIGN.md lines 275-283.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 In src/FractalDb/Core/Operators.fs, add StringOp type below CompareOp
- [ ] #2 Add [<RequireQualifiedAccess>] attribute
- [ ] #3 Define StringOp DU with cases: Like of pattern: string, ILike of pattern: string, Contains of substring: string, StartsWith of prefix: string, EndsWith of suffix: string
- [ ] #4 Run 'dotnet build' - build succeeds

- [ ] #5 All public types and functions have XML doc comments with <summary>, and public functions include <param>, <returns>, and <example> (see doc-2 for standards)

- [ ] #6 Run 'task lint' - no errors or warnings
<!-- AC:END -->
