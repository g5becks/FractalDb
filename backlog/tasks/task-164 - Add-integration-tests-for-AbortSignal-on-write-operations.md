---
id: task-164
title: Add integration tests for AbortSignal on write operations
status: To Do
assignee: []
created_date: '2026-01-05 23:48'
labels:
  - abort-signal
  - testing
  - integration-tests
dependencies:
  - task-163
  - task-158
  - task-159
  - task-160
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Create integration tests in test/integration/abort-signal.test.ts for AbortSignal behavior on write operations (single, batch, and atomic).
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Test insertOne throws AbortedError when signal is pre-aborted
- [ ] #2 Test insertMany throws AbortedError when signal is pre-aborted
- [ ] #3 Test updateOne throws AbortedError when signal is pre-aborted
- [ ] #4 Test updateMany throws AbortedError when signal is pre-aborted
- [ ] #5 Test deleteOne throws AbortedError when signal is pre-aborted
- [ ] #6 Test deleteMany throws AbortedError when signal is pre-aborted
- [ ] #7 Test findOneAndUpdate throws AbortedError when signal is pre-aborted
- [ ] #8 Test findOneAndReplace throws AbortedError when signal is pre-aborted
- [ ] #9 Test findOneAndDelete throws AbortedError when signal is pre-aborted
- [ ] #10 All tests pass with bun test
<!-- AC:END -->
