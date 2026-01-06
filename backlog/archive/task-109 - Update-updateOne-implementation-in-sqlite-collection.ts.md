---
id: task-109
title: Update updateOne implementation in sqlite-collection.ts
status: Done
assignee: []
created_date: '2025-11-22 19:27'
updated_date: '2025-11-22 21:36'
labels: []
dependencies:
  - task-105
  - task-108
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Update updateOne to use normalizeFilter and findOne instead of findById in src/sqlite-collection.ts around line 590
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 Type compiles without errors
- [x] #2 Linting passes
- [x] #3 Uses normalizeFilter helper
- [x] #4 Uses findOne instead of findById
- [x] #5 Upsert handles merged filter fields correctly
<!-- AC:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Simplified updateOne implementation by replacing complex upsert logic with direct insertOne call.

Changes made:
- Removed generateId import (no longer needed)
- Simplified upsert logic: now merges filter fields with update and calls insertOne
- Added biome-ignore comment for necessary type assertion
- Proper formatting for multi-line ternary

All acceptance criteria met:
- Type checking passes
- Linting passes (with proper ignore comment)
- Uses normalizeFilter helper
- Uses findOne instead of findById
- Upsert correctly handles merged filter fields for both string IDs and QueryFilter objects
<!-- SECTION:NOTES:END -->
