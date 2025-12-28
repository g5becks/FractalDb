---
id: task-72
title: Add Integration Tests for Transactions
status: To Do
assignee: []
created_date: '2025-12-28 06:44'
updated_date: '2025-12-28 16:37'
labels:
  - phase-4
  - testing
  - integration
dependencies:
  - task-71
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Create integration tests for transaction behavior. Reference: FSHARP_PORT_DESIGN.md lines 2300-2357.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Create file tests/FractalDb.Tests/Integration/TransactionTests.fs
- [ ] #2 Add test: transaction commits on success - both documents persisted
- [ ] #3 Add test: transaction rolls back on error - first insert not persisted
- [ ] #4 Add test: TransactionBuilder CE works with let! binding
- [ ] #5 Add test: nested operations in transaction all commit together
- [ ] #6 Run 'dotnet test' - all tests pass

- [ ] #7 Run 'task lint' - no errors or warnings
<!-- AC:END -->
