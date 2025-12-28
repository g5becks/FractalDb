---
id: task-65
title: Add Integration Tests for CRUD Operations
status: To Do
assignee: []
created_date: '2025-12-28 06:42'
updated_date: '2025-12-28 16:58'
labels:
  - phase-3
  - testing
  - integration
dependencies:
  - task-64
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Create integration tests for basic CRUD operations in tests/CrudTests.fs. Reference: FSHARP_PORT_DESIGN.md lines 2122-2297.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Create test fixture with in-memory database and User schema
- [ ] #2 Add test: insertOne creates document with auto-generated ID
- [ ] #3 Add test: insertOne fails with UniqueConstraint for duplicate unique field
- [ ] #4 Add test: findById returns Some for existing document
- [ ] #5 Add test: findById returns None for non-existent ID
- [ ] #6 Add test: find with query filters correctly
- [ ] #7 Add test: updateById modifies document and updates timestamp
- [ ] #8 Add test: deleteById removes document
- [ ] #9 Run 'dotnet test' - all tests pass
- [ ] #10 Run 'task lint' - no errors or warnings

- [ ] #11 Create file tests/CrudTests.fs
<!-- AC:END -->
