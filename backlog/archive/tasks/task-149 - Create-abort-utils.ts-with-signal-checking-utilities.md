---
id: task-149
title: Create abort-utils.ts with signal checking utilities
status: To Do
assignee: []
created_date: '2026-01-05 21:11'
labels:
  - abort
  - utils
  - phase1
dependencies:
  - task-147
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Create utility functions for checking AbortSignal state and creating abort promises. These utilities will be used by all collection methods.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 throwIfAborted(signal?) function throws AbortedError if signal is aborted
- [ ] #2 throwIfAborted does nothing when signal is undefined or not aborted
- [ ] #3 createAbortPromise(signal?) returns { promise, cleanup } object
- [ ] #4 createAbortPromise rejects immediately if signal already aborted
- [ ] #5 createAbortPromise cleanup function removes event listener to prevent memory leaks
- [ ] #6 All functions have complete JSDoc with @param, @returns, @throws, @example
- [ ] #7 Functions exported from src/index.ts
- [ ] #8 bun run check passes with no errors
<!-- AC:END -->
