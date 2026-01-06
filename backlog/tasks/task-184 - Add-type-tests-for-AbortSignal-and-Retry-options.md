---
id: task-184
title: Add type tests for AbortSignal and Retry options
status: In Progress
assignee:
  - '@agent'
created_date: '2026-01-06 00:28'
updated_date: '2026-01-06 03:47'
labels:
  - abort-signal
  - retry
  - testing
  - type-tests
dependencies:
  - task-183
  - task-165
priority: medium
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Create type tests in test/type/abort-retry.test-d.ts to verify TypeScript types are correct for signal and retry options on all collection methods.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Test signal option is accepted on all collection methods
- [ ] #2 Test retry option is accepted on all collection methods
- [ ] #3 Test retry: false is accepted as valid value
- [ ] #4 Test RetryOptions type structure is correct
- [ ] #5 Test AbortedError extends StrataDBError
- [ ] #6 All type tests pass with bun run test:types
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Check existing type test structure
2. Create test/type/abort-retry.test-d.ts
3. Add type tests for signal and retry options
4. Run bun run test:types to verify
<!-- SECTION:PLAN:END -->
