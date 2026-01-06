---
id: task-168
title: Create retry-utils.ts with defaultShouldRetry function
status: To Do
assignee: []
created_date: '2026-01-05 23:55'
labels:
  - retry
  - utilities
dependencies:
  - task-167
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Create the retry-utils.ts file with the defaultShouldRetry function that determines which errors should be retried based on error type and SQLite error codes.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 defaultShouldRetry function accepts RetryContext parameter
- [ ] #2 Returns false for ValidationError
- [ ] #3 Returns false for SchemaValidationError
- [ ] #4 Returns false for UniqueConstraintError
- [ ] #5 Returns false for ConstraintError
- [ ] #6 Returns true for ConnectionError
- [ ] #7 Returns true for SQLITE_BUSY (code 5)
- [ ] #8 Returns true for SQLITE_LOCKED (code 6)
- [ ] #9 Returns true for TransactionError
- [ ] #10 Returns false for unknown errors
- [ ] #11 RETRYABLE_SQLITE_CODES constant includes codes 5, 6, 7, 10
- [ ] #12 Complete JSDoc documentation with @example
- [ ] #13 bun run check passes with no errors
<!-- AC:END -->
