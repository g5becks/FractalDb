---
id: task-168
title: Create retry-utils.ts with defaultShouldRetry function
status: Done
assignee:
  - '@agent'
created_date: '2026-01-05 23:55'
updated_date: '2026-01-06 02:28'
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
- [x] #1 defaultShouldRetry function accepts RetryContext parameter
- [x] #2 Returns false for ValidationError
- [x] #3 Returns false for SchemaValidationError
- [x] #4 Returns false for UniqueConstraintError
- [x] #5 Returns false for ConstraintError
- [x] #6 Returns true for ConnectionError
- [x] #7 Returns true for SQLITE_BUSY (code 5)
- [x] #8 Returns true for SQLITE_LOCKED (code 6)
- [x] #9 Returns true for TransactionError
- [x] #10 Returns false for unknown errors
- [x] #11 RETRYABLE_SQLITE_CODES constant includes codes 5, 6, 7, 10
- [x] #12 Complete JSDoc documentation with @example
- [x] #13 bun run check passes with no errors
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Check error types in errors.ts
2. Create src/retry-utils.ts with RETRYABLE_SQLITE_CODES constant
3. Implement defaultShouldRetry function
4. Add JSDoc documentation
5. Run bun run check
<!-- SECTION:PLAN:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Created src/retry-utils.ts with:
- RETRYABLE_SQLITE_CODES constant with codes [5, 6, 7, 10]
- defaultShouldRetry function that:
  - Returns false for ValidationError, SchemaValidationError, UniqueConstraintError, ConstraintError
  - Returns true for ConnectionError, TransactionError
  - Returns true for SQLITE_BUSY (5), SQLITE_LOCKED (6), SQLITE_NOMEM (7), SQLITE_IOERR (10)
  - Returns false for unknown errors
- Complete JSDoc documentation with example
- All checks pass
<!-- SECTION:NOTES:END -->
