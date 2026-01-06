---
id: task-174
title: Add retry option to Collection read method types
status: Done
assignee:
  - '@agent'
created_date: '2026-01-06 00:07'
updated_date: '2026-01-06 02:50'
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
- [x] #1 findById options extended with retry?: RetryOptions | false
- [x] #2 find options extended with retry?: RetryOptions | false
- [x] #3 findOne options extended with retry?: RetryOptions | false
- [x] #4 count options extended with retry?: RetryOptions | false
- [x] #5 search options extended with retry?: RetryOptions | false
- [x] #6 distinct options extended with retry?: RetryOptions | false
- [x] #7 estimatedDocumentCount options extended with retry?: RetryOptions | false
- [x] #8 bun run check passes with no errors
<!-- AC:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Added retry property to all read operation method types:
- findById: Added retry?: RetryOptions | false to options
- find: Added retry to QueryOptionsBase (inherited by all overloads)
- findOne: Inherits from QueryOptions (already has retry)
- count: Added retry?: RetryOptions | false to options
- search: Inherits from QueryOptions (already has retry)
- distinct: Added retry?: RetryOptions | false to options
- estimatedDocumentCount: Added retry?: RetryOptions | false to options
- Imported RetryOptions in collection-types.ts and query-options-types.ts
- TypeScript validation passing
<!-- SECTION:NOTES:END -->
