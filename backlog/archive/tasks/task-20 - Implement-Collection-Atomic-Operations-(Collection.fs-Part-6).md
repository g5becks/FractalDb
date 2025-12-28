---
id: task-20
title: Implement Collection Atomic Operations (Collection.fs Part 6)
status: To Do
assignee: []
created_date: '2025-12-28 06:09'
labels:
  - storage
  - phase-3
dependencies:
  - task-19
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Implement atomic find-and-modify operations: findOneAndDelete, findOneAndUpdate, findOneAndReplace.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Collection.findOneAndDelete takes Query<'T>, returns Task<Document<'T> option>
- [ ] #2 Collection.findOneAndDeleteWith adds FindOptions for sorting
- [ ] #3 Collection.findOneAndUpdate takes Query<'T>, update function, FindAndModifyOptions
- [ ] #4 findOneAndUpdate supports ReturnDocument.Before and After
- [ ] #5 Collection.findOneAndReplace takes Query<'T>, replacement document, options
- [ ] #6 All atomic operations execute in single SQL transaction
- [ ] #7 Code compiles successfully with dotnet build
<!-- AC:END -->
