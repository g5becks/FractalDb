---
id: task-46
title: Write integration tests for Collection CRUD operations
status: Done
assignee:
  - '@claude'
created_date: '2025-11-21 02:59'
updated_date: '2025-11-21 19:40'
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
- [x] #1 Tests verify insertOne creates document with generated ID and returns typed document
- [x] #2 Tests verify findById retrieves document by ID and returns null when not found
- [x] #3 Tests verify find returns matching documents with correct filtering and pagination
- [x] #4 Tests verify updateOne merges partial updates and prevents ID modification
- [x] #5 Tests verify deleteOne removes document and returns boolean indicating success
- [ ] #6 Tests verify validation errors thrown for invalid documents
- [x] #7 Tests verify unique constraint violations throw UniqueConstraintError
- [x] #8 All tests pass when running test suite
- [x] #9 Complete test descriptions documenting expected behavior and edge cases
<!-- AC:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
## Summary

- Added 30 integration tests for Collection CRUD operations in test/integration/collection-crud.test.ts
- Fixed query translator to use "body" column (matching SQLiteCollection) instead of "data"
- Updated unit tests to match new column name

## Test Coverage

- insertOne: 4 tests (auto ID, all fields, custom ID, nested objects)
- findById: 3 tests (retrieval, null handling, array preservation)
- find: 8 tests (empty filter, equality, comparison ops, $and, $or, limit, skip, sort)
- findOne: 2 tests (matching, null handling)
- count: 2 tests (total count, filtered count)
- updateOne: 3 tests (field updates, null handling, id preservation)
- deleteOne: 2 tests (deletion, not found)
- unique constraints: 2 tests (UniqueConstraintError, field extraction)
- edge cases: 4 tests (empty arrays, special chars, numeric edges, unicode)

## Note on AC #6

Validation error tests not implemented because schema builder does not currently add validators. This functionality depends on Standard Schema integration which is tracked separately.

## Files Changed

- src/sqlite-query-translator.ts: Fixed column name from "data" to "body"
- test/unit/sqlite-query-translator-simple.test.ts: Updated expectations
- test/integration/collection-crud.test.ts: New integration test file
<!-- SECTION:NOTES:END -->
