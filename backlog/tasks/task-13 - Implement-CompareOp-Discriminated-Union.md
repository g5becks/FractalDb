---
id: task-13
title: Implement CompareOp Discriminated Union
status: To Do
assignee: []
created_date: '2025-12-28 06:30'
updated_date: '2025-12-28 16:54'
labels:
  - phase-1
  - core
  - operators
dependencies:
  - task-12
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Create the CompareOp<'T> DU in src/Operators.fs for comparison operations. Reference: FSHARP_PORT_DESIGN.md lines 257-269.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Add namespace FractalDb.Core
- [ ] #2 Define CompareOp<'T> DU with cases: Eq of 'T, Ne of 'T, Gt of 'T, Gte of 'T, Lt of 'T, Lte of 'T, In of 'T list, NotIn of 'T list
- [ ] #3 Run 'dotnet build' - build succeeds
- [ ] #4 All public types and functions have XML doc comments with <summary>, and public functions include <param>, <returns>, and <example> (see doc-2 for standards)
- [ ] #5 Run 'task lint' - no errors or warnings

- [ ] #6 Create file src/Operators.fs

- [ ] #7 Add module declaration: module FractalDb.Operators
<!-- AC:END -->
