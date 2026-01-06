---
id: task-171
title: Add unit tests for retry-utils.ts
status: Done
assignee:
  - '@agent'
created_date: '2026-01-06 00:01'
updated_date: '2026-01-06 02:43'
labels:
  - retry
  - testing
  - unit-tests
dependencies:
  - task-170
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Create comprehensive unit tests for retry utility functions in test/unit/retry-utils.test.ts. Tests should cover all edge cases mentioned in the ENHANCEMENTS.md document.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 Test withRetry executes operation once when retries is 0
- [x] #2 Test withRetry retries on retryable errors
- [x] #3 Test withRetry stops on non-retryable errors
- [x] #4 Test withRetry respects maxRetryTime
- [x] #5 Test withRetry calls onFailedAttempt on each failure
- [x] #6 Test withRetry respects shouldRetry predicate
- [x] #7 Test withRetry respects shouldConsumeRetry predicate
- [x] #8 Test mergeRetryOptions returns undefined when operation-level is false
- [x] #9 Test mergeRetryOptions returns undefined when collection-level is false
- [x] #10 Test mergeRetryOptions merges options with correct precedence
- [x] #11 Test defaultShouldRetry returns correct values for each error type
- [x] #12 All tests pass with bun test
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Create test/unit/retry-utils.test.ts
2. Add tests for withRetry function
3. Add tests for mergeRetryOptions function
4. Add tests for defaultShouldRetry function
5. Run bun test
<!-- SECTION:PLAN:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Created comprehensive unit tests for retry utilities:
- 8 tests for withRetry function covering all edge cases
- 5 tests for mergeRetryOptions function
- 12 tests for defaultShouldRetry function
- All 25 tests pass
- Tests use += instead of ++ for linter compliance
- Minor linter warnings about async functions without await (acceptable for test code)
<!-- SECTION:NOTES:END -->
