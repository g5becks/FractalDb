---
id: task-171
title: Add unit tests for retry-utils.ts
status: To Do
assignee: []
created_date: '2026-01-06 00:01'
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
- [ ] #1 Test withRetry executes operation once when retries is 0
- [ ] #2 Test withRetry retries on retryable errors
- [ ] #3 Test withRetry stops on non-retryable errors
- [ ] #4 Test withRetry respects maxRetryTime
- [ ] #5 Test withRetry calls onFailedAttempt on each failure
- [ ] #6 Test withRetry respects shouldRetry predicate
- [ ] #7 Test withRetry respects shouldConsumeRetry predicate
- [ ] #8 Test mergeRetryOptions returns undefined when operation-level is false
- [ ] #9 Test mergeRetryOptions returns undefined when collection-level is false
- [ ] #10 Test mergeRetryOptions merges options with correct precedence
- [ ] #11 Test defaultShouldRetry returns correct values for each error type
- [ ] #12 All tests pass with bun test
<!-- AC:END -->
