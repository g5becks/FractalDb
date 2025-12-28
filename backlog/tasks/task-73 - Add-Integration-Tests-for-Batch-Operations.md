---
id: task-73
title: Add Integration Tests for Batch Operations
status: To Do
assignee: []
created_date: '2025-12-28 06:45'
updated_date: '2025-12-28 16:37'
labels:
  - phase-4
  - testing
  - integration
dependencies:
  - task-72
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Create integration tests for insertMany, updateMany, deleteMany.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Create file tests/FractalDb.Tests/Integration/BatchTests.fs
- [ ] #2 Add test: insertMany inserts all documents
- [ ] #3 Add test: insertMany returns correct InsertedCount
- [ ] #4 Add test: insertMany rolls back on error when ordered=true
- [ ] #5 Add test: updateMany updates all matching documents
- [ ] #6 Add test: deleteMany removes all matching documents
- [ ] #7 Run 'dotnet test' - all tests pass

- [ ] #8 Run 'task lint' - no errors or warnings
<!-- AC:END -->
