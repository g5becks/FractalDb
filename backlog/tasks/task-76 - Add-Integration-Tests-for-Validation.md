---
id: task-76
title: Add Integration Tests for Validation
status: To Do
assignee: []
created_date: '2025-12-28 06:45'
updated_date: '2025-12-28 16:37'
labels:
  - phase-5
  - testing
  - integration
dependencies:
  - task-75
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Create integration tests for schema validation.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Create file tests/FractalDb.Tests/Integration/ValidationTests.fs
- [ ] #2 Add test: insertOne with valid data succeeds
- [ ] #3 Add test: insertOne with invalid data returns Validation error
- [ ] #4 Add test: updateOne with invalid result returns Validation error
- [ ] #5 Add test: Collection.validate returns error for invalid data
- [ ] #6 Run 'dotnet test' - all tests pass

- [ ] #7 Run 'task lint' - no errors or warnings
<!-- AC:END -->
