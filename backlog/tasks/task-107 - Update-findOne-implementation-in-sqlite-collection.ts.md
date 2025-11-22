---
id: task-107
title: Update findOne implementation in sqlite-collection.ts
status: Done
assignee: []
created_date: '2025-11-22 19:23'
updated_date: '2025-11-22 21:01'
labels: []
dependencies:
  - task-105
  - task-106
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add normalizeFilter call to findOne implementation in src/sqlite-collection.ts around line 340
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 Type compiles without errors
- [x] #2 Linting passes
- [x] #3 Uses normalizeFilter helper
- [x] #4 Handles both string and filter correctly
<!-- AC:END -->
