---
id: task-151
title: Add signal option to Collection read method types
status: To Do
assignee: []
created_date: '2026-01-05 23:21'
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
- [ ] #1 findById accepts options parameter with signal?: AbortSignal
- [ ] #2 find options extended with signal?: AbortSignal
- [ ] #3 findOne options extended with signal?: AbortSignal
- [ ] #4 count accepts options parameter with signal?: AbortSignal
- [ ] #5 search options extended with signal?: AbortSignal
- [ ] #6 distinct options extended with signal?: AbortSignal
- [ ] #7 estimatedDocumentCount accepts options parameter with signal?: AbortSignal
- [ ] #8 All JSDoc comments updated to document signal parameter
- [ ] #9 bun run check passes with no errors
<!-- AC:END -->
