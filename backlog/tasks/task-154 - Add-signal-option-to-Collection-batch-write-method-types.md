---
id: task-154
title: Add signal option to Collection batch write method types
status: Done
assignee:
  - '@agent'
created_date: '2026-01-05 23:27'
updated_date: '2026-01-06 01:41'
labels:
  - abort-signal
  - types
  - collection
dependencies:
  - task-153
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Update collection-types.ts to add optional signal?: AbortSignal parameter to batch write operation method signatures: insertMany, updateMany, deleteMany.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 insertMany options extended with signal?: AbortSignal
- [x] #2 updateMany options extended with signal?: AbortSignal
- [x] #3 deleteMany options extended with signal?: AbortSignal
- [x] #4 All JSDoc comments updated to document signal parameter
- [x] #5 bun run check passes with no errors
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Find batch write methods in collection-types.ts
2. Add signal to insertMany, updateMany, deleteMany
3. Update JSDoc
4. Run checks
<!-- SECTION:PLAN:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Added signal option to batch write methods:

- insertMany: Extended options with signal (preserved ordered option)
- updateMany: Added options with signal
- deleteMany: Added options with signal
- Updated JSDoc with examples
- All checks pass
<!-- SECTION:NOTES:END -->
