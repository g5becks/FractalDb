---
id: task-46
title: Write integration tests for Collection CRUD operations
status: To Do
assignee: []
created_date: '2025-11-21 02:59'
labels:
  - testing
dependencies: []
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Create integration tests that verify Collection CRUD operations work correctly with actual SQLite database. These tests ensure end-to-end functionality including serialization, querying, and data integrity.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Tests verify insertOne creates document with generated ID and returns typed document
- [ ] #2 Tests verify findById retrieves document by ID and returns null when not found
- [ ] #3 Tests verify find returns matching documents with correct filtering and pagination
- [ ] #4 Tests verify updateOne merges partial updates and prevents ID modification
- [ ] #5 Tests verify deleteOne removes document and returns boolean indicating success
- [ ] #6 Tests verify validation errors thrown for invalid documents
- [ ] #7 Tests verify unique constraint violations throw UniqueConstraintError
- [ ] #8 All tests pass when running test suite
- [ ] #9 Complete test descriptions documenting expected behavior and edge cases
<!-- AC:END -->
