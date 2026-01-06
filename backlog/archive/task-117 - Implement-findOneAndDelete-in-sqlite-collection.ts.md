---
id: task-117
title: Implement findOneAndDelete in sqlite-collection.ts
status: Done
assignee:
  - '@droid'
created_date: '2025-11-22 19:44'
updated_date: '2025-11-22 22:08'
labels: []
dependencies:
  - task-116
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Implement findOneAndDelete method using normalizeFilter, find document then delete by ID, return deleted document in src/sqlite-collection.ts around line 780
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 Type compiles without errors
- [x] #2 Linting passes
- [x] #3 Uses normalizeFilter helper
- [x] #4 Finds document then deletes by _id
- [x] #5 Returns deleted document or null
- [x] #6 Full JSDoc with examples included
<!-- AC:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Implemented findOneAndDelete method in sqlite-collection.ts.

Changes made:
- Added async method with filter: string | QueryFilter<T>
- Uses normalizeFilter helper to standardize input
- Finds document using findOne (respects sort option)
- Deletes by _id for atomicity
- Returns deleted document or null
- Comprehensive JSDoc with examples
- Imported SortSpec type

Implementation approach:
- Atomic: findOne -> delete by _id
- No type casting needed
- Clean, simple code (~13 lines)

All acceptance criteria met:
- Type checking passes
- Linting passes
- Uses normalizeFilter helper
- Finds document then deletes by _id
- Returns deleted document or null
- Full JSDoc with examples
<!-- SECTION:NOTES:END -->
