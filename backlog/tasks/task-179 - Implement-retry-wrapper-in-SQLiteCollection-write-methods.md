---
id: task-179
title: Implement retry wrapper in SQLiteCollection write methods
status: To Do
assignee: []
created_date: '2026-01-06 00:18'
labels:
  - retry
  - implementation
  - collection
dependencies:
  - task-178
  - task-175
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Update SQLiteCollection write method implementations to use withRetry wrapper: insertOne, updateOne, replaceOne, deleteOne, insertMany, updateMany, deleteMany.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 insertOne wrapped with withRetry using merged options
- [ ] #2 updateOne wrapped with withRetry using merged options
- [ ] #3 replaceOne wrapped with withRetry using merged options
- [ ] #4 deleteOne wrapped with withRetry using merged options
- [ ] #5 insertMany wrapped with withRetry using merged options
- [ ] #6 updateMany wrapped with withRetry using merged options
- [ ] #7 deleteMany wrapped with withRetry using merged options
- [ ] #8 Operation-level retry options override collection defaults
- [ ] #9 bun run check passes with no errors
<!-- AC:END -->
