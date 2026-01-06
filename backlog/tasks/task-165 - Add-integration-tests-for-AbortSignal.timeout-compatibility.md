---
id: task-165
title: Add integration tests for AbortSignal.timeout compatibility
status: To Do
assignee: []
created_date: '2026-01-05 23:50'
labels:
  - abort-signal
  - testing
  - integration-tests
dependencies:
  - task-164
  - task-162
priority: medium
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add integration tests verifying AbortSignal.timeout() works correctly with collection operations.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Test AbortSignal.timeout() works correctly with find operation
- [ ] #2 Test AbortSignal.timeout() works correctly with insertOne operation
- [ ] #3 Test db.execute throws AbortedError when signal is pre-aborted
- [ ] #4 All tests pass with bun test
<!-- AC:END -->
