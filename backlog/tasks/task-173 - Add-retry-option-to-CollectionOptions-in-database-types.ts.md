---
id: task-173
title: Add retry option to CollectionOptions in database-types.ts
status: To Do
assignee: []
created_date: '2026-01-06 00:05'
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
- [ ] #1 retry property added to CollectionOptions type
- [ ] #2 Property accepts RetryOptions or false (retry?: RetryOptions | false)
- [ ] #3 Complete JSDoc documentation explaining collection-level override behavior
- [ ] #4 bun run check passes with no errors
<!-- AC:END -->
