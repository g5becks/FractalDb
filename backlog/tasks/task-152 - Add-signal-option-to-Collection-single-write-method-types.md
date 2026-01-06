---
id: task-152
title: Add signal option to Collection single write method types
status: Done
assignee:
  - '@agent'
created_date: '2026-01-05 23:23'
updated_date: '2026-01-06 01:38'
labels:
  - abort-signal
  - types
  - collection
dependencies:
  - task-151
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Update collection-types.ts to add optional signal?: AbortSignal parameter to single write operation method signatures: insertOne, updateOne, replaceOne, deleteOne.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 insertOne options extended with signal?: AbortSignal
- [x] #2 updateOne options extended with signal?: AbortSignal
- [x] #3 replaceOne options extended with signal?: AbortSignal
- [x] #4 deleteOne options extended with signal?: AbortSignal
- [x] #5 All JSDoc comments updated to document signal parameter
- [x] #6 bun run check passes with no errors
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Read collection-types.ts to find single write method signatures
2. Add signal?: AbortSignal to insertOne, updateOne, replaceOne, deleteOne options
3. Update JSDoc comments with signal examples
4. Run bun run check to verify
<!-- SECTION:PLAN:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Added signal option to all Collection single write method types:

- insertOne: Added options parameter with signal?: AbortSignal
- updateOne: Extended options to include signal?: AbortSignal
- replaceOne: Added options parameter with signal?: AbortSignal
- deleteOne: Added options parameter with signal?: AbortSignal
- Updated all JSDoc comments with signal usage examples
- All checks pass
<!-- SECTION:NOTES:END -->
