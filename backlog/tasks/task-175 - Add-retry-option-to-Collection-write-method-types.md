---
id: task-175
title: Add retry option to Collection write method types
status: To Do
assignee: []
created_date: '2026-01-06 00:09'
labels:
  - retry
  - types
  - collection
dependencies:
  - task-174
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Update collection-types.ts to add optional retry?: RetryOptions | false parameter to all write operation method options: insertOne, updateOne, replaceOne, deleteOne, insertMany, updateMany, deleteMany.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 insertOne options extended with retry?: RetryOptions | false
- [ ] #2 updateOne options extended with retry?: RetryOptions | false
- [ ] #3 replaceOne options extended with retry?: RetryOptions | false
- [ ] #4 deleteOne options extended with retry?: RetryOptions | false
- [ ] #5 insertMany options extended with retry?: RetryOptions | false
- [ ] #6 updateMany options extended with retry?: RetryOptions | false
- [ ] #7 deleteMany options extended with retry?: RetryOptions | false
- [ ] #8 bun run check passes with no errors
<!-- AC:END -->
