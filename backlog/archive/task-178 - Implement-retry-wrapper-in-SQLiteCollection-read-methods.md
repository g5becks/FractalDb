---
id: task-178
title: Implement retry wrapper in SQLiteCollection read methods
status: Done
assignee:
  - '@agent'
created_date: '2026-01-06 00:16'
updated_date: '2026-01-06 03:06'
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
- [x] #1 findById wrapped with withRetry using merged options
- [x] #2 find wrapped with withRetry using merged options
- [x] #3 findOne wrapped with withRetry (delegates to find)
- [x] #4 count wrapped with withRetry using merged options
- [x] #5 search wrapped with withRetry (delegates to find)
- [x] #6 distinct wrapped with withRetry using merged options
- [x] #7 estimatedDocumentCount wrapped with withRetry using merged options
- [x] #8 Operation-level retry options override collection defaults
- [x] #9 bun run check passes with no errors
<!-- AC:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Implemented retry wrapper for all read methods in SQLiteCollection:
- findById: Wrapped with withRetry using merged options
- find: Wrapped with withRetry using merged options
- findOne: Delegates to find (automatically gets retry support)
- count: Wrapped with withRetry using merged options
- search: Delegates to find (automatically gets retry support)
- distinct: Wrapped with withRetry using merged options
- estimatedDocumentCount: Wrapped with withRetry using merged options
- Created buildRetryOptions helper to properly merge collection and operation-level options
- Operation-level retry options override collection defaults via mergeRetryOptions
- TypeScript validation passing (linter warnings expected for test code and existing complexity)
<!-- SECTION:NOTES:END -->
