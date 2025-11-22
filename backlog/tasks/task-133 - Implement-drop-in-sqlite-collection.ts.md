---
id: task-133
title: Implement drop in sqlite-collection.ts
status: Done
assignee: []
created_date: '2025-11-22 20:16'
updated_date: '2025-11-22 22:17'
labels: []
dependencies:
  - task-132
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Implement drop method using DROP TABLE IF EXISTS in src/sqlite-collection.ts around line 910
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Type compiles without errors
- [ ] #2 Linting passes
- [ ] #3 Uses DROP TABLE IF EXISTS SQL
- [ ] #4 Returns void Promise
- [ ] #5 Full JSDoc with examples included
<!-- AC:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Implemented drop method.

Implementation:
- Uses DROP TABLE IF EXISTS
- Safe deletion
- Returns void promise
<!-- SECTION:NOTES:END -->
