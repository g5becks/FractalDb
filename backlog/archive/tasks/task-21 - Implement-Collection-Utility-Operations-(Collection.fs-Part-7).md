---
id: task-21
title: Implement Collection Utility Operations (Collection.fs Part 7)
status: To Do
assignee: []
created_date: '2025-12-28 06:09'
labels:
  - storage
  - phase-3
dependencies:
  - task-20
priority: medium
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Implement utility operations: search, distinct, estimatedCount, drop, validate.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Collection.search takes text and fields list, returns Task<Document<'T> list>
- [ ] #2 Collection.searchWith adds QueryOptions support
- [ ] #3 Collection.distinct takes field name and optional filter, returns Task<obj list>
- [ ] #4 Collection.estimatedCount returns Task<int> using SQLite stats
- [ ] #5 Collection.drop deletes the collection table, returns Task<unit>
- [ ] #6 Collection.validate runs schema validation on document, returns FractalResult<'T>
- [ ] #7 Code compiles successfully with dotnet build
<!-- AC:END -->
