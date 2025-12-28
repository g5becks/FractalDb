---
id: task-26
title: Implement Transaction Builder CE (TransactionBuilder.fs)
status: To Do
assignee: []
created_date: '2025-12-28 06:10'
labels:
  - builders
  - phase-4
dependencies:
  - task-22
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Create the TransactionBuilder computation expression in Builders/TransactionBuilder.fs for Result-aware transaction workflows.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 TransactionBuilder class takes FractalDb instance
- [ ] #2 Bind member for Task<FractalResult<'T>> to next step
- [ ] #3 Bind member for FractalResult<'T> to next step
- [ ] #4 Return member wraps value in Task.FromResult(Ok value)
- [ ] #5 ReturnFrom member passes through Task<FractalResult<'T>>
- [ ] #6 Zero member returns Task.FromResult(Ok ())
- [ ] #7 Run member executes function within db.ExecuteTransaction
- [ ] #8 TryWith member for exception handling
- [ ] #9 TryFinally member for cleanup
- [ ] #10 FractalDb.Transact extension property returns TransactionBuilder
- [ ] #11 Code compiles successfully with dotnet build
<!-- AC:END -->
