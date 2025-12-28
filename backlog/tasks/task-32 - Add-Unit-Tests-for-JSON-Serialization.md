---
id: task-32
title: Add Unit Tests for JSON Serialization
status: To Do
assignee: []
created_date: '2025-12-28 06:34'
updated_date: '2025-12-28 16:56'
labels:
  - phase-2
  - testing
  - unit
dependencies:
  - task-31
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Create unit tests for JSON serialization in tests/SerializationTests.fs.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Define simple test record type: type TestUser = { Name: string; Age: int }
- [ ] #2 Add test: serialize then deserialize roundtrips TestUser correctly
- [ ] #3 Add test: camelCase property naming is applied (Name becomes 'name' in JSON)
- [ ] #4 Add test: F# option types serialize correctly (Some as value, None as null)
- [ ] #5 Run 'dotnet test' - all tests pass
- [ ] #6 Run 'task lint' - no errors or warnings

- [ ] #7 Create file tests/SerializationTests.fs
<!-- AC:END -->
