---
id: task-179
title: Add resilience tests
status: Done
assignee:
  - '@claude'
created_date: '2025-12-31 19:10'
updated_date: '2025-12-31 19:36'
labels: []
dependencies:
  - task-178
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add comprehensive tests for retry functionality including unit tests, integration tests, and error mapping tests
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 Add unit tests for RetryableError presets and shouldRetry function
- [x] #2 Add unit tests for ResilienceOptions defaults
- [x] #3 Add integration tests for retry on Busy error
- [x] #4 Add test for max retries exhaustion
- [x] #5 Add test for non-retryable errors (should not retry)
- [x] #6 Add test for CancellationToken support during retry delays
- [x] #7 Add tests for SQLite error code mapping
- [x] #8 All tests pass
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Create ResilienceTests.fs test file
2. Add RetryableError preset tests (defaults, extended, all)
3. Add shouldRetry function tests
4. Add ResilienceOptions defaults tests
5. Add SQLite error code mapping tests
6. Add Retry.executeAsync integration tests
7. Add CancellationToken support tests
8. Register tests in Program.fs
9. Run tests to verify all pass
<!-- SECTION:PLAN:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Added comprehensive resilience tests in tests/ResilienceTests.fs:

## Test Coverage (71 tests):

### Unit Tests:
- RetryableError presets (defaults, extended, all)
- RetryableError.matches function
- RetryableError.shouldRetry function
- ResilienceOptions presets (defaults, none, extended, aggressive)
- FractalError transient error messages and categories

### Integration Tests:
- Retry.executeAsync with various scenarios:
  - Immediate return when opts is None
  - No retry when MaxRetries is 0 or RetryOn is empty
  - Successful retry on Busy/Locked errors
  - Max retries exhaustion
  - Non-retryable errors are not retried
  - IOError retry with extended config

### CancellationToken Tests:
- Respects cancellation during retry delays
- Throws immediately when token is already cancelled
- Normal operation without cancellation

### SQLite Error Code Mapping Tests:
- SQLITE_BUSY (5) → FractalError.Busy
- SQLITE_LOCKED (6) → FractalError.Locked
- SQLITE_IOERR (10) → FractalError.IOError
- SQLITE_FULL (13) → FractalError.DiskFull
- SQLITE_CANTOPEN (14) → FractalError.CantOpen
- SQLITE_CONSTRAINT (19) → FractalError.UniqueConstraint
- Other errors → FractalError.Query
- End-to-end retryability verification for mapped errors

## Changes:
- Created tests/ResilienceTests.fs (776 lines)
- Added InternalsVisibleTo attribute to src/FractalDb.fsproj
- Registered ResilienceTests.fs in tests/FractalDb.Tests.fsproj

All 424 tests pass.
<!-- SECTION:NOTES:END -->
