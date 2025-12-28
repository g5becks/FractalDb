---
id: task-6
title: Implement Error Types (Errors.fs)
status: To Do
assignee: []
created_date: '2025-12-28 06:05'
labels:
  - core
  - phase-1
dependencies: []
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Create the FractalError discriminated union and FractalResult type alias in Core/Errors.fs. No dependencies on other FractalDb modules.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 FractalError DU with Validation, UniqueConstraint, Query, Connection, Transaction, NotFound, Serialization, InvalidOperation cases
- [ ] #2 FractalError.Message member property for human-readable messages
- [ ] #3 FractalError.Category member property for error grouping
- [ ] #4 FractalResult<'T> type alias for Result<'T, FractalError>
- [ ] #5 FractalResult module with map, bind, mapError functions
- [ ] #6 FractalResult module with ofOption, toOption, getOrRaise functions
- [ ] #7 FractalResult module with traverse, sequence, combine functions
- [ ] #8 Code compiles successfully with dotnet build
<!-- AC:END -->
