---
id: task-77
title: Create Test Assertions Module
status: To Do
assignee: []
created_date: '2025-12-28 06:45'
labels:
  - phase-1
  - testing
dependencies:
  - task-1
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Create custom FsUnit.Light assertions for FractalDb. Reference: FSHARP_PORT_DESIGN.md lines 1962-2023.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Create file tests/FractalDb.Tests/Assertions.fs - should be early in test project compile order
- [ ] #2 Add 'let shouldBeOk (result: FractalResult<'T>)' assertion
- [ ] #3 Add 'let shouldBeOkWith (f: 'T -> unit) result' for asserting Ok with value check
- [ ] #4 Add 'let shouldBeError result' assertion
- [ ] #5 Add 'let shouldBeSome opt' and 'let shouldBeNone opt' assertions
- [ ] #6 Add 'let shouldNotBeEmpty (s: string)' assertion
- [ ] #7 Run 'dotnet build' on test project - build succeeds
<!-- AC:END -->
