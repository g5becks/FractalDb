---
id: task-16
title: Implement Collection Read Operations (Collection.fs Part 2)
status: To Do
assignee: []
created_date: '2025-12-28 06:07'
labels:
  - storage
  - phase-3
dependencies:
  - task-15
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Implement the read operations in the Collection module: findById, findOne, find, count.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Collection.findById function takes id string and returns Task<Document<'T> option>
- [ ] #2 Collection.findOne function takes Query<'T> and returns Task<Document<'T> option>
- [ ] #3 Collection.findOneWith function adds QueryOptions support
- [ ] #4 Collection.find function takes Query<'T> and returns Task<Document<'T> list>
- [ ] #5 Collection.findWith function adds QueryOptions support
- [ ] #6 Collection.count function takes Query<'T> and returns Task<int>
- [ ] #7 All functions use Donald for database operations
- [ ] #8 Code compiles successfully with dotnet build
<!-- AC:END -->
