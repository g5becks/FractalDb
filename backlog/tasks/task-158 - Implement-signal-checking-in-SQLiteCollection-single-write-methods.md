---
id: task-158
title: Implement signal checking in SQLiteCollection single write methods
status: Done
assignee:
  - '@agent'
created_date: '2026-01-05 23:36'
updated_date: '2026-01-06 01:53'
labels:
  - abort-signal
  - implementation
  - collection
dependencies:
  - task-157
  - task-152
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Update sqlite-collection.ts to integrate throwIfAborted checks in single write operation implementations: insertOne, updateOne, replaceOne, deleteOne.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 insertOne checks signal at start
- [x] #2 updateOne checks signal at start and after find
- [x] #3 replaceOne checks signal at start and after find
- [x] #4 deleteOne checks signal at start
- [x] #5 All method signatures updated to accept signal parameter
- [x] #6 bun run check passes with no errors
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Find insertOne, updateOne, replaceOne, deleteOne in sqlite-collection.ts
2. Add signal checks at appropriate points
3. Update method signatures
4. Run checks
<!-- SECTION:PLAN:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Implemented signal checking in single write methods:

- insertOne: Check signal at start
- updateOne: Check signal at start and after find, pass signal to nested calls
- replaceOne: Check signal at start and after find
- deleteOne: Check signal at start
- Updated all method signatures
- All checks pass
<!-- SECTION:NOTES:END -->
