---
id: task-18
title: Implement Collection Update Operations (Collection.fs Part 4)
status: To Do
assignee: []
created_date: '2025-12-28 06:08'
labels:
  - storage
  - phase-3
dependencies:
  - task-17
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Implement the update operations in the Collection module: updateById, updateOne, replaceOne.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Collection.updateById takes id, update function, returns Task<FractalResult<Document<'T> option>>
- [ ] #2 updateById updates the updatedAt timestamp
- [ ] #3 Collection.updateOne takes Query<'T> and update function
- [ ] #4 Collection.updateOneWith adds upsert option support
- [ ] #5 Collection.replaceOne replaces entire document body (preserves ID and createdAt)
- [ ] #6 Update functions serialize modified document and UPDATE via Donald
- [ ] #7 Code compiles successfully with dotnet build
<!-- AC:END -->
