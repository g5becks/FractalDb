---
id: task-177
title: Implement internal Retry module
status: Done
assignee:
  - '@claude'
created_date: '2025-12-31 19:06'
updated_date: '2025-12-31 19:19'
labels: []
dependencies:
  - task-175
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Create internal Retry module with exponential backoff and jitter support for retrying operations
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 Add internal Retry module in Errors.fs or new Resilience.fs
- [x] #2 Implement calculateDelay with exponential backoff support
- [x] #3 Implement jitter calculation
- [x] #4 Implement executeAsync that wraps operations with retry logic
- [x] #5 Implement executeCancellableAsync for CancellationToken support
- [x] #6 Handle retry exhaustion correctly (re-raise original error)
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Add internal Retry module at end of Errors.fs (after ResilienceOptions module)
2. Add private random instance for jitter
3. Implement calculateDelay with exponential backoff and jitter
4. Implement executeAsync for Task operations
5. Implement executeCancellableAsync for CancellableTask operations with CancellationToken
<!-- SECTION:PLAN:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Added internal Retry module at end of Errors.fs:

- Private random instance for jitter calculation
- calculateDelay function with exponential backoff (base * 2^attempt) and jitter (up to 20%)
- executeAsync for Task<FractalResult> operations
- executeCancellableAsync with CancellationToken support

Both execute functions:
- Return immediately if opts is None or retry disabled
- Loop up to MaxRetries on retryable errors
- Use shouldRetry to check if error matches configured RetryOn set
- Cap delay at MaxDelay
- Pass CT to Task.Delay for responsive cancellation
<!-- SECTION:NOTES:END -->
