---
id: task-157
title: Implement signal checking in SQLiteCollection read methods
status: To Do
assignee: []
created_date: '2026-01-05 23:33'
labels:
  - abort-signal
  - implementation
  - collection
dependencies:
  - task-148
  - task-151
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Update sqlite-collection.ts to integrate throwIfAborted checks in all read operation implementations: findById, find, findOne, count, search, distinct, estimatedDocumentCount.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 findById checks signal at start and throws AbortedError if aborted
- [ ] #2 find checks signal at start and before result processing
- [ ] #3 findOne checks signal via find implementation
- [ ] #4 count checks signal at start
- [ ] #5 search checks signal via find implementation
- [ ] #6 distinct checks signal at start
- [ ] #7 estimatedDocumentCount checks signal at start
- [ ] #8 All method signatures updated to accept signal parameter
- [ ] #9 bun run check passes with no errors
<!-- AC:END -->
