---
id: task-9
title: Add FractalError Member Properties
status: To Do
assignee: []
created_date: '2025-12-28 06:29'
updated_date: '2025-12-28 16:34'
labels:
  - phase-1
  - core
  - errors
dependencies:
  - task-8
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add Message and Category member properties to FractalError. Reference: FSHARP_PORT_DESIGN.md lines 1847-1871.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 In FractalError type, add 'member this.Message' property
- [ ] #2 Message returns descriptive string for each case using pattern matching (see design doc for exact messages)
- [ ] #3 Add 'member this.Category' property returning category string
- [ ] #4 Category returns: 'validation' for Validation, 'database' for UniqueConstraint/Connection, 'query' for Query/NotFound, 'transaction' for Transaction, 'serialization' for Serialization, 'operation' for InvalidOperation
- [ ] #5 Run 'dotnet build' - build succeeds

- [ ] #6 All public types and functions have XML doc comments with <summary>, and public functions include <param>, <returns>, and <example> (see doc-2 for standards)

- [ ] #7 Run 'task lint' - no errors or warnings
<!-- AC:END -->
