---
id: task-177
title: Update SQLiteCollection constructor for retry options
status: To Do
assignee: []
created_date: '2026-01-06 00:13'
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
- [ ] #1 Constructor accepts databaseRetryOptions parameter
- [ ] #2 Constructor accepts collectionRetryOptions via CollectionOptions
- [ ] #3 Private retryOptions property stores merged options
- [ ] #4 mergeRetryOptions is called in constructor
- [ ] #5 bun run check passes with no errors
<!-- AC:END -->
