---
id: task-181
title: Update Strata class to pass retry options to collections
status: To Do
assignee: []
created_date: '2026-01-06 00:22'
labels:
  - retry
  - implementation
  - database
dependencies:
  - task-180
  - task-172
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Update stratadb.ts Strata class to store database-level retry options and pass them to SQLiteCollection constructors.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Strata constructor stores retry options from DatabaseOptions
- [ ] #2 collection method passes database retry options to SQLiteCollection
- [ ] #3 Collection-level options properly override database-level
- [ ] #4 bun run check passes with no errors
<!-- AC:END -->
