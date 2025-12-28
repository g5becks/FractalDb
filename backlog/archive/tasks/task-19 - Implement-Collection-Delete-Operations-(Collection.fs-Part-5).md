---
id: task-19
title: Implement Collection Delete Operations (Collection.fs Part 5)
status: To Do
assignee: []
created_date: '2025-12-28 06:08'
labels:
  - storage
  - phase-3
dependencies:
  - task-18
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Implement the delete operations in the Collection module: deleteById, deleteOne, deleteMany.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Collection.deleteById takes id string, returns Task<bool>
- [ ] #2 Collection.deleteOne takes Query<'T>, returns Task<bool>
- [ ] #3 Collection.deleteMany takes Query<'T>, returns Task<DeleteResult>
- [ ] #4 Delete operations use Donald for database execution
- [ ] #5 Code compiles successfully with dotnet build
<!-- AC:END -->
