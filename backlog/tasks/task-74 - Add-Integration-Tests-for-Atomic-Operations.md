---
id: task-74
title: Add Integration Tests for Atomic Operations
status: To Do
assignee: []
created_date: '2025-12-28 06:45'
updated_date: '2025-12-28 16:37'
labels:
  - phase-4
  - testing
  - integration
dependencies:
  - task-73
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Create integration tests for findOneAnd* operations.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Create file tests/FractalDb.Tests/Integration/AtomicTests.fs
- [ ] #2 Add test: findOneAndDelete returns deleted document
- [ ] #3 Add test: findOneAndDelete returns None if not found
- [ ] #4 Add test: findOneAndUpdate with ReturnDocument.Before returns original
- [ ] #5 Add test: findOneAndUpdate with ReturnDocument.After returns modified
- [ ] #6 Add test: findOneAndReplace replaces document body
- [ ] #7 Run 'dotnet test' - all tests pass

- [ ] #8 Run 'task lint' - no errors or warnings
<!-- AC:END -->
