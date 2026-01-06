---
id: task-152
title: Add signal option to Collection single write method types
status: To Do
assignee: []
created_date: '2026-01-05 23:23'
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
- [ ] #1 insertOne options extended with signal?: AbortSignal
- [ ] #2 updateOne options extended with signal?: AbortSignal
- [ ] #3 replaceOne options extended with signal?: AbortSignal
- [ ] #4 deleteOne options extended with signal?: AbortSignal
- [ ] #5 All JSDoc comments updated to document signal parameter
- [ ] #6 bun run check passes with no errors
<!-- AC:END -->
