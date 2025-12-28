---
id: task-11
title: Add FractalResult Traverse and Combine Functions
status: To Do
assignee: []
created_date: '2025-12-28 06:29'
updated_date: '2025-12-28 07:03'
labels:
  - phase-1
  - core
  - errors
dependencies:
  - task-10
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add traverse, sequence, and combine functions to FractalResult module. Reference: FSHARP_PORT_DESIGN.md lines 1908-1927.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Add 'traverse' function: takes (f: 'T -> FractalResult<'U>) and list, returns FractalResult<'U list> - stops at first error
- [ ] #2 Add 'sequence' function: takes FractalResult<'T> list, returns FractalResult<'T list> - implemented as traverse id
- [ ] #3 Add 'combine' function: takes two results, returns Ok tuple if both Ok, first Error otherwise
- [ ] #4 Run 'dotnet build' - build succeeds

- [ ] #5 All public types and functions have XML doc comments with <summary>, and public functions include <param>, <returns>, and <example> (see doc-2 for standards)
<!-- AC:END -->
