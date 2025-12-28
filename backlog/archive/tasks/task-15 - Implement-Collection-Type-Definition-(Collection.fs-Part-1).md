---
id: task-15
title: Implement Collection Type Definition (Collection.fs Part 1)
status: To Do
assignee: []
created_date: '2025-12-28 06:07'
labels:
  - storage
  - phase-3
dependencies:
  - task-12
  - task-7
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Create the Collection<'T> type and result types in Storage/Collection.fs.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Collection<'T> record type with Name, Schema, Connection, IdGenerator, Translator, EnableCache fields
- [ ] #2 InsertManyResult<'T> record with Documents and InsertedCount
- [ ] #3 UpdateResult record with MatchedCount and ModifiedCount
- [ ] #4 DeleteResult record with DeletedCount
- [ ] #5 FindOptions record with Sort field
- [ ] #6 FindAndModifyOptions record with Sort, ReturnDocument, Upsert fields
- [ ] #7 ReturnDocument DU with Before and After cases
- [ ] #8 Code compiles successfully with dotnet build
<!-- AC:END -->
