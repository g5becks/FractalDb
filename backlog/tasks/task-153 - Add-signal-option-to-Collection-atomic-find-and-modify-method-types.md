---
id: task-153
title: Add signal option to Collection atomic find-and-modify method types
status: Done
assignee:
  - '@agent'
created_date: '2026-01-05 23:25'
updated_date: '2026-01-06 01:40'
labels:
  - abort-signal
  - types
  - collection
dependencies:
  - task-152
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Update collection-types.ts to add optional signal?: AbortSignal parameter to atomic find-and-modify operation method signatures: findOneAndUpdate, findOneAndReplace, findOneAndDelete.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 findOneAndUpdate options extended with signal?: AbortSignal
- [x] #2 findOneAndReplace options extended with signal?: AbortSignal
- [x] #3 findOneAndDelete options extended with signal?: AbortSignal
- [x] #4 All JSDoc comments updated to document signal parameter
- [x] #5 bun run check passes with no errors
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Find atomic find-and-modify method signatures in collection-types.ts
2. Add signal?: AbortSignal to options for findOneAndUpdate, findOneAndReplace, findOneAndDelete
3. Update JSDoc comments
4. Run bun run check
<!-- SECTION:PLAN:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Added signal option to all Collection atomic find-and-modify method types:

- findOneAndDelete: Extended options to include signal?: AbortSignal
- findOneAndUpdate: Extended options to include signal?: AbortSignal
- findOneAndReplace: Extended options to include signal?: AbortSignal
- Updated all JSDoc comments with signal usage examples
- All checks pass
<!-- SECTION:NOTES:END -->
