---
id: task-169
title: Add withRetry wrapper function to retry-utils.ts
status: Done
assignee:
  - '@agent'
created_date: '2026-01-05 23:57'
updated_date: '2026-01-06 02:37'
labels:
  - retry
  - utilities
dependencies:
  - task-168
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add the withRetry async function to retry-utils.ts that wraps operations with p-retry for automatic retry with exponential backoff.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 withRetry function accepts operation function and RetryableOptions
- [x] #2 RetryableOptions extends RetryOptions with optional signal
- [x] #3 Returns operation result directly when retries is 0
- [x] #4 Passes all retry options to p-retry correctly
- [x] #5 Signal is passed to p-retry for abort support
- [x] #6 Complete JSDoc documentation with @example
- [x] #7 bun run check passes with no errors
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Check p-retry API for usage
2. Add RetryableOptions type extending RetryOptions
3. Implement withRetry function
4. Add JSDoc documentation
5. Run bun run check
<!-- SECTION:PLAN:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Added to src/retry-utils.ts:
- RetryableOptions type extending RetryOptions with optional signal
- withRetry function that wraps operations with p-retry
- Returns operation result directly when retries is 0 or undefined
- Conditionally builds p-retry options object to handle exactOptionalPropertyTypes
- Signal is passed to p-retry for abort support
- Complete JSDoc documentation with example
- All checks pass
<!-- SECTION:NOTES:END -->
