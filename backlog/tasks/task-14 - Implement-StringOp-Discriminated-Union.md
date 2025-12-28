---
id: task-14
title: Implement StringOp Discriminated Union
status: To Do
assignee: []
created_date: '2025-12-28 06:30'
updated_date: '2025-12-28 16:54'
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
Create the StringOp DU in src/Operators.fs for string-specific operations. Reference: FSHARP_PORT_DESIGN.md lines 275-283.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Add [<RequireQualifiedAccess>] attribute
- [ ] #2 Define StringOp DU with cases: Like of pattern: string, ILike of pattern: string, Contains of substring: string, StartsWith of prefix: string, EndsWith of suffix: string
- [ ] #3 Run 'dotnet build' - build succeeds
- [ ] #4 All public types and functions have XML doc comments with <summary>, and public functions include <param>, <returns>, and <example> (see doc-2 for standards)

- [ ] #5 Run 'task lint' - no errors or warnings

- [ ] #6 In src/Operators.fs, add StringOp type below CompareOp
<!-- AC:END -->
