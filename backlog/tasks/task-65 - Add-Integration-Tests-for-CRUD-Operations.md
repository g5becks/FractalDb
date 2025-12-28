---
id: task-65
title: Add Integration Tests for CRUD Operations
status: To Do
assignee: []
created_date: '2025-12-28 06:42'
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
Create integration tests for basic CRUD operations. Reference: FSHARP_PORT_DESIGN.md lines 2122-2297.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Create file tests/FractalDb.Tests/Integration/CrudTests.fs
- [ ] #2 Create test fixture with in-memory database and User schema
- [ ] #3 Add test: insertOne creates document with auto-generated ID
- [ ] #4 Add test: insertOne fails with UniqueConstraint for duplicate unique field
- [ ] #5 Add test: findById returns Some for existing document
- [ ] #6 Add test: findById returns None for non-existent ID
- [ ] #7 Add test: find with query filters correctly
- [ ] #8 Add test: updateById modifies document and updates timestamp
- [ ] #9 Add test: deleteById removes document
- [ ] #10 Run 'dotnet test' - all tests pass
<!-- AC:END -->
