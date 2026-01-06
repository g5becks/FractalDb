---
id: task-176
title: Add retry option to Collection atomic and utility method types
status: Done
assignee:
  - '@agent'
created_date: '2026-01-06 00:11'
updated_date: '2026-01-06 02:57'
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
- [x] #1 findOneAndUpdate options extended with retry?: RetryOptions | false
- [x] #2 findOneAndReplace options extended with retry?: RetryOptions | false
- [x] #3 findOneAndDelete options extended with retry?: RetryOptions | false
- [x] #4 drop options extended with retry?: RetryOptions | false
- [x] #5 validate options extended with retry?: RetryOptions | false
- [x] #6 bun run check passes with no errors
<!-- AC:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Added retry property to all atomic and utility method types:
- findOneAndUpdate: Added retry?: RetryOptions | false to options
- findOneAndReplace: Added retry?: RetryOptions | false to options
- findOneAndDelete: Added retry?: RetryOptions | false to options
- drop: Added retry?: RetryOptions | false to options
- validate: Added retry?: RetryOptions | false to options
- TypeScript validation passing
<!-- SECTION:NOTES:END -->
