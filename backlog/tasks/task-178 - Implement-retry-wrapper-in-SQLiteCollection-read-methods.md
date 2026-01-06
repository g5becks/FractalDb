---
id: task-178
title: Implement retry wrapper in SQLiteCollection read methods
status: To Do
assignee: []
created_date: '2026-01-06 00:16'
labels:
  - retry
  - implementation
  - collection
dependencies:
  - task-177
  - task-174
  - task-161
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Update SQLiteCollection read method implementations to use withRetry wrapper: findById, find, findOne, count, search, distinct, estimatedDocumentCount.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 findById wrapped with withRetry using merged options
- [ ] #2 find wrapped with withRetry using merged options
- [ ] #3 findOne wrapped with withRetry (delegates to find)
- [ ] #4 count wrapped with withRetry using merged options
- [ ] #5 search wrapped with withRetry (delegates to find)
- [ ] #6 distinct wrapped with withRetry using merged options
- [ ] #7 estimatedDocumentCount wrapped with withRetry using merged options
- [ ] #8 Operation-level retry options override collection defaults
- [ ] #9 bun run check passes with no errors
<!-- AC:END -->
