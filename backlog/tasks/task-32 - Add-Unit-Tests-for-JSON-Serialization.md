---
id: task-32
title: Add Unit Tests for JSON Serialization
status: To Do
assignee: []
created_date: '2025-12-28 06:34'
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
Create unit tests for JSON serialization.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Create file tests/FractalDb.Tests/Unit/SerializationTests.fs
- [ ] #2 Define simple test record type: type TestUser = { Name: string; Age: int }
- [ ] #3 Add test: serialize then deserialize roundtrips TestUser correctly
- [ ] #4 Add test: camelCase property naming is applied (Name becomes 'name' in JSON)
- [ ] #5 Add test: F# option types serialize correctly (Some as value, None as null)
- [ ] #6 Run 'dotnet test' - all tests pass
<!-- AC:END -->
