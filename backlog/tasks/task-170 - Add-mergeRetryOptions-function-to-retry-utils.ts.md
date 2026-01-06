---
id: task-170
title: Add mergeRetryOptions function to retry-utils.ts
status: Done
assignee:
  - '@agent'
created_date: '2026-01-05 23:59'
updated_date: '2026-01-06 02:39'
labels:
  - retry
  - utilities
dependencies:
  - task-169
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add the mergeRetryOptions function to retry-utils.ts that merges retry options from database, collection, and operation levels with proper precedence.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 mergeRetryOptions accepts database, collection, and operation options
- [x] #2 Returns undefined when operation-level is false (disables retry)
- [x] #3 Returns undefined when collection-level is false (disables retry)
- [x] #4 Merges options with correct precedence (operation > collection > database)
- [x] #5 Complete JSDoc documentation with @example
- [x] #6 bun run check passes with no errors
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Implement mergeRetryOptions function
2. Handle false values to disable retry
3. Merge options with correct precedence
4. Add JSDoc documentation
5. Run bun run check
<!-- SECTION:PLAN:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Added mergeRetryOptions function to src/retry-utils.ts:
- Accepts database, collection, and operation retry options
- Returns undefined when operation-level is false (disables retry)
- Returns undefined when collection-level is false (disables retry)
- Merges options with correct precedence: operation > collection > database
- Complete JSDoc documentation with example
- All checks pass
<!-- SECTION:NOTES:END -->
