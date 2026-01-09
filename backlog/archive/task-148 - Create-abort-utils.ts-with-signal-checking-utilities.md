---
id: task-148
title: Create abort-utils.ts with signal checking utilities
status: Done
assignee:
  - '@agent'
created_date: '2026-01-05 23:15'
updated_date: '2026-01-06 00:39'
labels:
  - abort-signal
  - utilities
dependencies:
  - task-147
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Create a new abort-utils.ts file with helper functions for checking and handling AbortSignals. These utilities will be used throughout the codebase to implement abort behavior.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 throwIfAborted function throws AbortedError when signal is already aborted
- [x] #2 throwIfAborted does nothing when signal is undefined or not aborted
- [x] #3 createAbortPromise function returns promise that rejects on abort and cleanup function
- [x] #4 createAbortPromise handles already-aborted signals correctly
- [x] #5 Cleanup function properly removes event listener to prevent memory leaks
- [x] #6 All functions have complete JSDoc documentation with @example
- [x] #7 bun run check passes with no errors
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Create src/abort-utils.ts file
2. Implement throwIfAborted function with proper error handling
3. Implement createAbortPromise with cleanup mechanism
4. Add comprehensive JSDoc with examples for both functions
5. Run bun run check to verify implementation
<!-- SECTION:PLAN:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Created abort-utils.ts with signal checking utilities:

- throwIfAborted: Checks signal and throws AbortedError if already aborted
- createAbortPromise: Returns promise that rejects on abort with cleanup function
- Both functions handle undefined signals gracefully
- Cleanup function properly removes event listeners to prevent memory leaks
- Comprehensive JSDoc with examples for both functions
- All checks pass (typecheck + lint)
<!-- SECTION:NOTES:END -->
