---
id: task-71
title: Implement TransactionBuilder Computation Expression
status: To Do
assignee: []
created_date: '2025-12-28 06:44'
updated_date: '2025-12-28 16:37'
labels:
  - phase-4
  - builders
dependencies:
  - task-70
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Create TransactionBuilder CE for Result-aware transactions. Reference: FSHARP_PORT_DESIGN.md lines 882-944.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Create file src/FractalDb/Builders/TransactionBuilder.fs
- [ ] #2 Add namespace FractalDb.Builders
- [ ] #3 Define TransactionBuilder class taking FractalDb instance
- [ ] #4 Add member _.Bind for Task<FractalResult<'T>> chaining
- [ ] #5 Add member _.Bind for FractalResult<'T> chaining
- [ ] #6 Add member _.Return wrapping value in Task.FromResult(Ok value)
- [ ] #7 Add member _.ReturnFrom passing through Task<FractalResult>
- [ ] #8 Add member _.Zero() returning Task.FromResult(Ok ())
- [ ] #9 Add member _.Delay(f) = f and member _.Run(f) calling db.ExecuteTransaction
- [ ] #10 Add type extension: FractalDb.Transact property returning TransactionBuilder
- [ ] #11 Run 'dotnet build' - build succeeds

- [ ] #12 All public types and functions have XML doc comments with <summary>, and public functions include <param>, <returns>, and <example> (see doc-2 for standards)

- [ ] #13 Run 'task lint' - no errors or warnings
<!-- AC:END -->
