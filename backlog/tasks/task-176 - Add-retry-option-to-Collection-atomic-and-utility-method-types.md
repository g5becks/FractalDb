---
id: task-176
title: Add retry option to Collection atomic and utility method types
status: To Do
assignee: []
created_date: '2026-01-06 00:11'
labels:
  - retry
  - types
  - collection
dependencies:
  - task-175
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Update collection-types.ts to add optional retry?: RetryOptions | false parameter to atomic and utility method options: findOneAndUpdate, findOneAndReplace, findOneAndDelete, drop, validate.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 findOneAndUpdate options extended with retry?: RetryOptions | false
- [ ] #2 findOneAndReplace options extended with retry?: RetryOptions | false
- [ ] #3 findOneAndDelete options extended with retry?: RetryOptions | false
- [ ] #4 drop options extended with retry?: RetryOptions | false
- [ ] #5 validate options extended with retry?: RetryOptions | false
- [ ] #6 bun run check passes with no errors
<!-- AC:END -->
