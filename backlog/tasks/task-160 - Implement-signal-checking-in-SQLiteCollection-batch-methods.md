---
id: task-160
title: Implement signal checking in SQLiteCollection batch methods
status: To Do
assignee: []
created_date: '2026-01-05 23:40'
labels:
  - abort-signal
  - implementation
  - collection
dependencies:
  - task-159
  - task-154
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Update sqlite-collection.ts to integrate throwIfAborted checks in batch write operation implementations: insertMany, updateMany, deleteMany. Check signal between batch item processing.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 insertMany checks signal at start and between document insertions
- [ ] #2 updateMany checks signal at start and between document updates
- [ ] #3 deleteMany checks signal at start
- [ ] #4 All method signatures updated to accept signal parameter
- [ ] #5 bun run check passes with no errors
<!-- AC:END -->
