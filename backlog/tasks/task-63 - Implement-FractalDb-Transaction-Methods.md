---
id: task-63
title: Implement FractalDb Transaction Methods
status: To Do
assignee: []
created_date: '2025-12-28 06:42'
updated_date: '2025-12-28 07:03'
labels:
  - phase-3
  - storage
  - database
dependencies:
  - task-62
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add transaction support to FractalDb. Reference: FSHARP_PORT_DESIGN.md lines 1458-1489.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Add member this.Transaction() : Transaction - creates new transaction from connection
- [ ] #2 Add member this.Execute<'T>(fn: Transaction -> Task<'T>) : Task<'T>
- [ ] #3 Execute creates transaction, runs fn, commits on success, rollbacks on exception
- [ ] #4 Add member this.ExecuteTransaction<'T>(fn: Transaction -> Task<FractalResult<'T>>) : Task<FractalResult<'T>>
- [ ] #5 ExecuteTransaction commits on Ok, rollbacks on Error
- [ ] #6 Run 'dotnet build' - build succeeds

- [ ] #7 All public types and functions have XML doc comments with <summary>, and public functions include <param>, <returns>, and <example> (see doc-2 for standards)
<!-- AC:END -->
