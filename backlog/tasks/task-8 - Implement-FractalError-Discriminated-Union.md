---
id: task-8
title: Implement FractalError Discriminated Union
status: To Do
assignee: []
created_date: '2025-12-28 06:29'
updated_date: '2025-12-28 07:03'
labels:
  - phase-1
  - core
  - errors
dependencies:
  - task-1
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Create the FractalError DU in Core/Errors.fs with all error cases. Reference: FSHARP_PORT_DESIGN.md lines 1835-1858.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Create file src/FractalDb/Core/Errors.fs
- [ ] #2 Add namespace FractalDb.Core
- [ ] #3 Add [<RequireQualifiedAccess>] attribute to FractalError type
- [ ] #4 Define FractalError DU with cases: Validation of field: string option * message: string
- [ ] #5 Add case: UniqueConstraint of field: string * value: obj
- [ ] #6 Add case: Query of message: string * sql: string option
- [ ] #7 Add case: Connection of message: string
- [ ] #8 Add case: Transaction of message: string
- [ ] #9 Add case: NotFound of id: string
- [ ] #10 Add case: Serialization of message: string
- [ ] #11 Add case: InvalidOperation of message: string
- [ ] #12 Run 'dotnet build' - build succeeds

- [ ] #13 All public types and functions have XML doc comments with <summary>, and public functions include <param>, <returns>, and <example> (see doc-2 for standards)
<!-- AC:END -->
