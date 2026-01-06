---
id: task-154
title: Add signal option to Collection batch write method types
status: To Do
assignee: []
created_date: '2026-01-05 23:27'
labels:
  - abort-signal
  - types
  - collection
dependencies:
  - task-153
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Update collection-types.ts to add optional signal?: AbortSignal parameter to batch write operation method signatures: insertMany, updateMany, deleteMany.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 insertMany options extended with signal?: AbortSignal
- [ ] #2 updateMany options extended with signal?: AbortSignal
- [ ] #3 deleteMany options extended with signal?: AbortSignal
- [ ] #4 All JSDoc comments updated to document signal parameter
- [ ] #5 bun run check passes with no errors
<!-- AC:END -->
