---
id: task-51
title: Write integration tests for error handling
status: To Do
assignee: []
created_date: '2025-11-21 01:53'
updated_date: '2025-11-21 02:07'
labels:
  - testing
  - errors
  - integration
dependencies:
  - task-6
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Test comprehensive error scenarios and verify appropriate error types are thrown with helpful messages.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Create test/errors/error-handling.test.ts file
- [ ] #2 Test SchemaValidationError thrown with field and value context
- [ ] #3 Test QueryError thrown for invalid query operators
- [ ] #4 Test TypeMismatchError thrown when operator doesn't match field type
- [ ] #5 Test UniqueConstraintError thrown with field, value, and helpful message
- [ ] #6 Test DatabaseError thrown for SQLite errors
- [ ] #7 Test TransactionError thrown for transaction failures
- [ ] #8 Test error messages are descriptive and actionable
- [ ] #9 All tests pass with bun test
- [ ] #10 Tests compile with strict mode
<!-- AC:END -->
