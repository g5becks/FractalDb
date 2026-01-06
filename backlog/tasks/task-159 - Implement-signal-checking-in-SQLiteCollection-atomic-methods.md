---
id: task-159
title: Implement signal checking in SQLiteCollection atomic methods
status: To Do
assignee: []
created_date: '2026-01-05 23:38'
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
- [ ] #1 findOneAndUpdate checks signal at start and after find
- [ ] #2 findOneAndReplace checks signal at start and after find
- [ ] #3 findOneAndDelete checks signal at start and after find
- [ ] #4 All method signatures updated to accept signal parameter
- [ ] #5 bun run check passes with no errors
<!-- AC:END -->
