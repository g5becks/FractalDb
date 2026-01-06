---
id: task-153
title: Add signal option to Collection atomic find-and-modify method types
status: To Do
assignee: []
created_date: '2026-01-05 23:25'
labels:
  - abort-signal
  - types
  - collection
dependencies:
  - task-152
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Update collection-types.ts to add optional signal?: AbortSignal parameter to atomic find-and-modify operation method signatures: findOneAndUpdate, findOneAndReplace, findOneAndDelete.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 findOneAndUpdate options extended with signal?: AbortSignal
- [ ] #2 findOneAndReplace options extended with signal?: AbortSignal
- [ ] #3 findOneAndDelete options extended with signal?: AbortSignal
- [ ] #4 All JSDoc comments updated to document signal parameter
- [ ] #5 bun run check passes with no errors
<!-- AC:END -->
