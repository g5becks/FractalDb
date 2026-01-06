---
id: task-180
title: Implement retry wrapper in SQLiteCollection atomic and utility methods
status: In Progress
assignee:
  - '@agent'
created_date: '2026-01-06 00:20'
updated_date: '2026-01-06 03:15'
labels:
  - retry
  - implementation
  - collection
dependencies:
  - task-179
  - task-176
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Update SQLiteCollection atomic and utility method implementations to use withRetry wrapper: findOneAndUpdate, findOneAndReplace, findOneAndDelete, drop, validate.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 findOneAndUpdate wrapped with withRetry using merged options
- [ ] #2 findOneAndReplace wrapped with withRetry using merged options
- [ ] #3 findOneAndDelete wrapped with withRetry using merged options
- [ ] #4 drop wrapped with withRetry using merged options
- [ ] #5 validate wrapped with withRetry using merged options
- [ ] #6 Operation-level retry options override collection defaults
- [ ] #7 bun run check passes with no errors
<!-- AC:END -->
