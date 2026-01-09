---
id: task-177
title: Update SQLiteCollection constructor for retry options
status: Done
assignee:
  - '@agent'
created_date: '2026-01-06 00:13'
updated_date: '2026-01-06 02:59'
labels:
  - retry
  - implementation
  - collection
dependencies:
  - task-170
  - task-173
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Update SQLiteCollection constructor to accept database-level retry options and collection-level retry options. Store merged options as instance property.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 Constructor accepts databaseRetryOptions parameter
- [x] #2 Constructor accepts collectionRetryOptions via CollectionOptions
- [x] #3 Private retryOptions property stores merged options
- [x] #4 mergeRetryOptions is called in constructor
- [x] #5 bun run check passes with no errors
<!-- AC:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Updated SQLiteCollection constructor and Strata class for retry options:
- Constructor accepts databaseRetryOptions and collectionRetryOptions parameters
- Added private retryOptions property to store merged options
- mergeRetryOptions called in constructor to merge database and collection options
- Updated Strata class to store database-level retry options and pass to collections
- Updated collection-builder to pass undefined retry options (uses database defaults)
- TypeScript validation passing (unused variable warning expected until next tasks)
<!-- SECTION:NOTES:END -->
