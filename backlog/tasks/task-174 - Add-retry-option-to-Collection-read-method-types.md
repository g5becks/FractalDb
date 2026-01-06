---
id: task-174
title: Add retry option to Collection read method types
status: To Do
assignee: []
created_date: '2026-01-06 00:07'
labels:
  - retry
  - types
  - collection
dependencies:
  - task-173
  - task-155
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Update collection-types.ts to add optional retry?: RetryOptions | false parameter to all read operation method options: findById, find, findOne, count, search, distinct, estimatedDocumentCount.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 findById options extended with retry?: RetryOptions | false
- [ ] #2 find options extended with retry?: RetryOptions | false
- [ ] #3 findOne options extended with retry?: RetryOptions | false
- [ ] #4 count options extended with retry?: RetryOptions | false
- [ ] #5 search options extended with retry?: RetryOptions | false
- [ ] #6 distinct options extended with retry?: RetryOptions | false
- [ ] #7 estimatedDocumentCount options extended with retry?: RetryOptions | false
- [ ] #8 bun run check passes with no errors
<!-- AC:END -->
