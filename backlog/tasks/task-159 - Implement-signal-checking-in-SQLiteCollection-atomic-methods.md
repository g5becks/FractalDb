---
id: task-159
title: Implement signal checking in SQLiteCollection atomic methods
status: Done
assignee:
  - '@agent'
created_date: '2026-01-05 23:38'
updated_date: '2026-01-06 02:05'
labels:
  - abort-signal
  - implementation
  - collection
dependencies:
  - task-158
  - task-153
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Update sqlite-collection.ts to integrate throwIfAborted checks in atomic find-and-modify operation implementations: findOneAndUpdate, findOneAndReplace, findOneAndDelete.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 findOneAndUpdate checks signal at start and after find
- [x] #2 findOneAndReplace checks signal at start and after find
- [x] #3 findOneAndDelete checks signal at start and after find
- [x] #4 All method signatures updated to accept signal parameter
- [x] #5 bun run check passes with no errors
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Find atomic methods in sqlite-collection.ts
2. Add signal checks at start and after find operations
3. Update signatures
4. Run checks
<!-- SECTION:PLAN:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Implemented signal checking in atomic methods:

- findOneAndDelete: Check signal at start and after find
- findOneAndUpdate: Check signal at start and after find
- findOneAndReplace: Check signal at start and after find
- Updated all signatures
- All checks pass
<!-- SECTION:NOTES:END -->
