---
id: task-17
title: Implement Collection Insert Operations (Collection.fs Part 3)
status: To Do
assignee: []
created_date: '2025-12-28 06:08'
labels:
  - storage
  - phase-3
dependencies:
  - task-16
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Implement the insert operations in the Collection module: insertOne, insertMany.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Collection.insertOne takes document, auto-generates ID if empty, returns Task<FractalResult<Document<'T>>>
- [ ] #2 insertOne sets createdAt and updatedAt timestamps
- [ ] #3 insertOne serializes document to JSONB and inserts via Donald
- [ ] #4 Collection.insertMany takes list of documents, returns Task<FractalResult<InsertManyResult<'T>>>
- [ ] #5 insertMany wraps all inserts in a transaction
- [ ] #6 Code compiles successfully with dotnet build
<!-- AC:END -->
