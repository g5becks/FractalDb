---
id: task-17
title: Implement Query Discriminated Union
status: To Do
assignee: []
created_date: '2025-12-28 06:31'
updated_date: '2025-12-28 16:55'
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
- [ ] #2 Define Query<'T> DU with cases: Empty, Field of fieldName: string * FieldOp, And of Query<'T> list, Or of Query<'T> list, Nor of Query<'T> list, Not of Query<'T>
- [ ] #3 Run 'dotnet build' - build succeeds
- [ ] #4 All public types and functions have XML doc comments with <summary>, and public functions include <param>, <returns>, and <example> (see doc-2 for standards)
- [ ] #5 Run 'task lint' - no errors or warnings

- [ ] #6 Create file src/Query.fs

- [ ] #7 Add module declaration: module FractalDb.Query
<!-- AC:END -->
