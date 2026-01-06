---
id: task-155
title: Add signal option to Collection utility method types
status: Done
assignee:
  - '@agent'
created_date: '2026-01-05 23:29'
updated_date: '2026-01-06 01:45'
labels:
  - abort-signal
  - types
  - collection
dependencies:
  - task-154
priority: medium
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Update collection-types.ts to add optional signal?: AbortSignal parameter to utility operation method signatures: drop, validate.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 drop options extended with signal?: AbortSignal
- [x] #2 validate options extended with signal?: AbortSignal
- [x] #3 All JSDoc comments updated to document signal parameter
- [x] #4 bun run check passes with no errors
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Find drop and validate methods
2. Add signal options
3. Update JSDoc
4. Run checks
<!-- SECTION:PLAN:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Added signal to utility methods:

- drop: Added options with signal
- validate: Added options with signal
- Updated JSDoc with examples
- All checks pass
<!-- SECTION:NOTES:END -->
