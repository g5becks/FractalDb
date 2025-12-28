---
id: task-17
title: Implement Query Discriminated Union
status: To Do
assignee: []
created_date: '2025-12-28 06:31'
updated_date: '2025-12-28 07:03'
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
Create the Query<'T> DU in Core/Query.fs - the main query structure. Reference: FSHARP_PORT_DESIGN.md lines 322-329.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Create file src/FractalDb/Core/Query.fs
- [ ] #2 Add namespace FractalDb.Core
- [ ] #3 Add [<RequireQualifiedAccess>] attribute to Query type
- [ ] #4 Define Query<'T> DU with cases: Empty, Field of fieldName: string * FieldOp, And of Query<'T> list, Or of Query<'T> list, Nor of Query<'T> list, Not of Query<'T>
- [ ] #5 Run 'dotnet build' - build succeeds

- [ ] #6 All public types and functions have XML doc comments with <summary>, and public functions include <param>, <returns>, and <example> (see doc-2 for standards)
<!-- AC:END -->
