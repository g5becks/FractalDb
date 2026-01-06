---
id: task-167
title: Create retry-types.ts with RetryOptions interface
status: To Do
assignee: []
created_date: '2026-01-05 23:52'
labels:
  - retry
  - types
dependencies:
  - task-166
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Create a new retry-types.ts file with the RetryOptions interface and re-export RetryContext from p-retry. This provides type definitions for retry configuration.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 RetryOptions interface with retries, factor, minTimeout, maxTimeout, randomize, maxRetryTime properties
- [ ] #2 shouldRetry callback property with RetryContext parameter
- [ ] #3 onFailedAttempt callback property with RetryContext parameter
- [ ] #4 shouldConsumeRetry callback property with RetryContext parameter
- [ ] #5 RetryContext type is re-exported from p-retry
- [ ] #6 All properties have complete JSDoc documentation
- [ ] #7 Default values documented in JSDoc (retries: 0, factor: 2, minTimeout: 1000, maxTimeout: 30000, randomize: true)
- [ ] #8 bun run check passes with no errors
<!-- AC:END -->
