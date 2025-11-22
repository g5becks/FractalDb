---
id: task-120
title: Implement findOneAndUpdate in sqlite-collection.ts
status: Done
assignee: []
created_date: '2025-11-22 19:50'
updated_date: '2025-11-22 22:13'
labels: []
dependencies:
  - task-119
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Implement findOneAndUpdate method with normalizeFilter, upsert handling, and returnDocument option (before/after) in src/sqlite-collection.ts around line 810
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Type compiles without errors
- [ ] #2 Linting passes
- [ ] #3 Uses normalizeFilter helper
- [ ] #4 Handles upsert correctly
- [ ] #5 Handles returnDocument: before
- [ ] #6 Handles returnDocument: after (default)
- [ ] #7 Full JSDoc with examples included
<!-- AC:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Implemented findOneAndUpdate method.

Implementation:
- Uses normalizeFilter helper
- Finds document with findOne (respects sort)
- Handles upsert option
- Returns before or after state based on returnDocument
- Default returnDocument is "after"
- Clean code, no type casting
<!-- SECTION:NOTES:END -->
