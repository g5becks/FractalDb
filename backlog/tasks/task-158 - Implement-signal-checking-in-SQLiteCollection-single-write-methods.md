---
id: task-158
title: Implement signal checking in SQLiteCollection single write methods
status: To Do
assignee: []
created_date: '2026-01-05 23:36'
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
- [ ] #1 insertOne checks signal at start
- [ ] #2 updateOne checks signal at start and after find
- [ ] #3 replaceOne checks signal at start and after find
- [ ] #4 deleteOne checks signal at start
- [ ] #5 All method signatures updated to accept signal parameter
- [ ] #6 bun run check passes with no errors
<!-- AC:END -->
