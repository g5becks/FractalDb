---
id: task-151
title: Add signal option to Collection read method types
status: Done
assignee:
  - '@agent'
created_date: '2026-01-05 23:21'
updated_date: '2026-01-06 00:46'
labels:
  - abort-signal
  - types
  - collection
dependencies:
  - task-147
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Update collection-types.ts to add optional signal?: AbortSignal parameter to all read operation method signatures: findById, find, findOne, count, search, distinct, estimatedDocumentCount.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 findById accepts options parameter with signal?: AbortSignal
- [x] #2 find options extended with signal?: AbortSignal
- [x] #3 findOne options extended with signal?: AbortSignal
- [x] #4 count accepts options parameter with signal?: AbortSignal
- [x] #5 search options extended with signal?: AbortSignal
- [x] #6 distinct options extended with signal?: AbortSignal
- [x] #7 estimatedDocumentCount accepts options parameter with signal?: AbortSignal
- [x] #8 All JSDoc comments updated to document signal parameter
- [x] #9 bun run check passes with no errors
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Read collection-types.ts to understand current method signatures
2. Add signal?: AbortSignal to read method options types
3. Update JSDoc comments to document the signal parameter
4. Run bun run check to verify types are correct
<!-- SECTION:PLAN:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Added signal option to all Collection read method types:

- Added signal?: AbortSignal to QueryOptionsBase (automatically applies to find, findOne, search)
- Added signal option to findById with new options parameter
- Added signal option to count with new options parameter
- Added signal option to distinct with new options parameter
- Added signal option to estimatedDocumentCount with new options parameter
- Updated all JSDoc comments with examples showing signal usage
- All checks pass (typecheck + lint)
<!-- SECTION:NOTES:END -->
