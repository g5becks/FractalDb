---
id: task-169
title: Add withRetry wrapper function to retry-utils.ts
status: To Do
assignee: []
created_date: '2026-01-05 23:57'
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
- [ ] #1 withRetry function accepts operation function and RetryableOptions
- [ ] #2 RetryableOptions extends RetryOptions with optional signal
- [ ] #3 Returns operation result directly when retries is 0
- [ ] #4 Passes all retry options to p-retry correctly
- [ ] #5 Signal is passed to p-retry for abort support
- [ ] #6 Complete JSDoc documentation with @example
- [ ] #7 bun run check passes with no errors
<!-- AC:END -->
