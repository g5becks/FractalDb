---
id: task-173
title: Add retry option to CollectionOptions in database-types.ts
status: Done
assignee:
  - '@agent'
created_date: '2026-01-06 00:05'
updated_date: '2026-01-06 02:46'
labels:
  - retry
  - types
  - collection
dependencies:
  - task-172
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Update database-types.ts to add optional retry property to CollectionOptions type for collection-level retry configuration. Supports false to disable retries.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 retry property added to CollectionOptions type
- [x] #2 Property accepts RetryOptions or false (retry?: RetryOptions | false)
- [x] #3 Complete JSDoc documentation explaining collection-level override behavior
- [x] #4 bun run check passes with no errors
<!-- AC:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Added retry property to CollectionOptions type:
- Property type: retry?: RetryOptions | false
- Supports false to explicitly disable retries
- Comprehensive JSDoc with examples for enabling and disabling
- Follows same pattern as enableCache property
- TypeScript validation passing
<!-- SECTION:NOTES:END -->
