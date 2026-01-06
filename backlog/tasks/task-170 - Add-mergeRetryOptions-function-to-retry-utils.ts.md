---
id: task-170
title: Add mergeRetryOptions function to retry-utils.ts
status: To Do
assignee: []
created_date: '2026-01-05 23:59'
labels:
  - retry
  - utilities
dependencies:
  - task-169
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add the mergeRetryOptions function to retry-utils.ts that merges retry options from database, collection, and operation levels with proper precedence.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 mergeRetryOptions accepts database, collection, and operation options
- [ ] #2 Returns undefined when operation-level is false (disables retry)
- [ ] #3 Returns undefined when collection-level is false (disables retry)
- [ ] #4 Merges options with correct precedence (operation > collection > database)
- [ ] #5 Complete JSDoc documentation with @example
- [ ] #6 bun run check passes with no errors
<!-- AC:END -->
