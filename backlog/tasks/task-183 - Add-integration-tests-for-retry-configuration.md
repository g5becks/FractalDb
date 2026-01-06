---
id: task-183
title: Add integration tests for retry configuration
status: To Do
assignee: []
created_date: '2026-01-06 00:26'
labels:
  - retry
  - testing
  - integration-tests
dependencies:
  - task-181
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Create integration tests in test/integration/retry-configuration.test.ts for retry configuration at database, collection, and operation levels.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Test database-level retry options are passed to collections
- [ ] #2 Test collection-level retry options override database-level
- [ ] #3 Test operation-level retry options override collection-level
- [ ] #4 Test retry: false at collection level disables retries
- [ ] #5 Test retry: false at operation level disables retries
- [ ] #6 Test retries work correctly with AbortSignal
- [ ] #7 Test retries stop when signal is aborted
- [ ] #8 Test onFailedAttempt receives correct context
- [ ] #9 All tests pass with bun test
<!-- AC:END -->
