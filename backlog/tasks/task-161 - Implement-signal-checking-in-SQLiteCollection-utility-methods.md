---
id: task-161
title: Implement signal checking in SQLiteCollection utility methods
status: To Do
assignee: []
created_date: '2026-01-05 23:42'
labels:
  - abort-signal
  - implementation
  - collection
dependencies:
  - task-160
  - task-155
priority: medium
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Update sqlite-collection.ts to integrate throwIfAborted checks in utility operation implementations: drop, validate.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 drop checks signal at start
- [ ] #2 validate checks signal at start
- [ ] #3 All method signatures updated to accept signal parameter
- [ ] #4 bun run check passes with no errors
<!-- AC:END -->
