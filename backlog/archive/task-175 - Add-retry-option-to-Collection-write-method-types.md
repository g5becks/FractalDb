---
id: task-175
title: Add retry option to Collection write method types
status: Done
assignee:
  - '@agent'
created_date: '2026-01-06 00:09'
updated_date: '2026-01-06 02:52'
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
- [x] #1 insertOne options extended with retry?: RetryOptions | false
- [x] #2 updateOne options extended with retry?: RetryOptions | false
- [x] #3 replaceOne options extended with retry?: RetryOptions | false
- [x] #4 deleteOne options extended with retry?: RetryOptions | false
- [x] #5 insertMany options extended with retry?: RetryOptions | false
- [x] #6 updateMany options extended with retry?: RetryOptions | false
- [x] #7 deleteMany options extended with retry?: RetryOptions | false
- [x] #8 bun run check passes with no errors
<!-- AC:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Added retry property to all write operation method types:
- insertOne: Added retry?: RetryOptions | false to options
- updateOne: Added retry?: RetryOptions | false to options (with upsert)
- replaceOne: Added retry?: RetryOptions | false to options
- deleteOne: Added retry?: RetryOptions | false to options
- insertMany: Added retry?: RetryOptions | false to options (with ordered)
- updateMany: Added retry?: RetryOptions | false to options
- deleteMany: Added retry?: RetryOptions | false to options
- TypeScript validation passing
<!-- SECTION:NOTES:END -->
