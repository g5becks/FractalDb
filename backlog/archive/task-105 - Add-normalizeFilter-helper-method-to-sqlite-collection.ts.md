---
id: task-105
title: Add normalizeFilter helper method to sqlite-collection.ts
status: Done
assignee: []
created_date: '2025-11-22 19:19'
updated_date: '2025-11-22 20:58'
labels: []
dependencies:
  - task-104
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add private normalizeFilter helper method that converts string ID to QueryFilter in src/sqlite-collection.ts around line 200
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 Type compiles without errors
- [x] #2 Linting passes
- [x] #3 Method is private and properly typed
- [x] #4 JSDoc explains internal use
- [x] #5 Converts string to { _id: string } filter
<!-- AC:END -->
