---
id: task-163
title: Add integration tests for AbortSignal on read operations
status: To Do
assignee: []
created_date: '2026-01-05 23:46'
labels:
  - abort-signal
  - testing
  - integration-tests
dependencies:
  - task-157
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Create integration tests in test/integration/abort-signal.test.ts for AbortSignal behavior on read operations. Test both pre-aborted signals and signals aborted during operation.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Test find throws AbortedError when signal is pre-aborted
- [ ] #2 Test findOne throws AbortedError when signal is pre-aborted
- [ ] #3 Test findById throws AbortedError when signal is pre-aborted
- [ ] #4 Test count throws AbortedError when signal is pre-aborted
- [ ] #5 Test search throws AbortedError when signal is pre-aborted
- [ ] #6 Test distinct throws AbortedError when signal is pre-aborted
- [ ] #7 Test estimatedDocumentCount throws AbortedError when signal is pre-aborted
- [ ] #8 Test operations complete successfully when signal is not aborted
- [ ] #9 All tests pass with bun test
<!-- AC:END -->
