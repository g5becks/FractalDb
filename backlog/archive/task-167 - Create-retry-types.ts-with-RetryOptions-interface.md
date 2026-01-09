---
id: task-167
title: Create retry-types.ts with RetryOptions interface
status: Done
assignee:
  - '@agent'
created_date: '2026-01-05 23:52'
updated_date: '2026-01-06 02:25'
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
- [x] #1 RetryOptions interface with retries, factor, minTimeout, maxTimeout, randomize, maxRetryTime properties
- [x] #2 shouldRetry callback property with RetryContext parameter
- [x] #3 onFailedAttempt callback property with RetryContext parameter
- [x] #4 shouldConsumeRetry callback property with RetryContext parameter
- [x] #5 RetryContext type is re-exported from p-retry
- [x] #6 All properties have complete JSDoc documentation
- [x] #7 Default values documented in JSDoc (retries: 0, factor: 2, minTimeout: 1000, maxTimeout: 30000, randomize: true)
- [x] #8 bun run check passes with no errors
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Check p-retry exports for RetryContext type
2. Create src/retry-types.ts with RetryOptions interface
3. Add JSDoc documentation with defaults
4. Run bun run check
<!-- SECTION:PLAN:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Created src/retry-types.ts with:
- RetryOptions type alias with all required properties
- Re-exported RetryContext from p-retry
- Complete JSDoc documentation for all properties
- Documented default values: retries: 0, factor: 2, minTimeout: 1000, maxTimeout: 30000, randomize: true, maxRetryTime: Infinity
- All checks pass
<!-- SECTION:NOTES:END -->
