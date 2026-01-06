---
id: task-155
title: Add signal option to Collection utility method types
status: To Do
assignee: []
created_date: '2026-01-05 23:29'
labels:
  - abort-signal
  - types
  - collection
dependencies:
  - task-154
priority: medium
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Update collection-types.ts to add optional signal?: AbortSignal parameter to utility operation method signatures: drop, validate.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 drop options extended with signal?: AbortSignal
- [ ] #2 validate options extended with signal?: AbortSignal
- [ ] #3 All JSDoc comments updated to document signal parameter
- [ ] #4 bun run check passes with no errors
<!-- AC:END -->
