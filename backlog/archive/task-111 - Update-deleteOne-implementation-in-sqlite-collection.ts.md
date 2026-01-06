---
id: task-111
title: Update deleteOne implementation in sqlite-collection.ts
status: Done
assignee:
  - '@droid'
created_date: '2025-11-22 19:31'
updated_date: '2025-11-22 21:41'
labels: []
dependencies:
  - task-105
  - task-110
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Update deleteOne to use normalizeFilter with optimization for _id-only filters in src/sqlite-collection.ts around line 730
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 Type compiles without errors
- [x] #2 Linting passes
- [x] #3 Uses normalizeFilter helper
- [x] #4 Optimizes _id-only filters with direct DELETE
- [x] #5 Handles complex filters via find-then-delete
<!-- AC:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Updated deleteOne implementation in sqlite-collection.ts to support uniform filter pattern.

Changes made:
- Updated method signature to accept filter: string | QueryFilter<T>
- Added normalizeFilter call to standardize input
- Added optimization for _id-only filters (direct DELETE)
- Implemented find-then-delete for complex filters
- Updated JSDoc with examples for both patterns

All acceptance criteria met:
- Type checking passes
- Linting passes
- Uses normalizeFilter helper
- Optimizes _id-only filters with direct DELETE query
- Handles complex filters via find-then-delete pattern
<!-- SECTION:NOTES:END -->
