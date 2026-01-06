---
id: task-165
title: Add integration tests for AbortSignal.timeout compatibility
status: In Progress
assignee:
  - '@agent'
created_date: '2026-01-05 23:50'
updated_date: '2026-01-06 03:50'
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

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Check existing abort-signal integration tests
2. Add tests for AbortSignal.timeout() with find and insertOne
3. Add test for db.execute with pre-aborted signal
4. Run bun test to verify
<!-- SECTION:PLAN:END -->
