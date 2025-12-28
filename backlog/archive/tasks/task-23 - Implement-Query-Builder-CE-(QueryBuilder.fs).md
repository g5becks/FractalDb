---
id: task-23
title: Implement Query Builder CE (QueryBuilder.fs)
status: To Do
assignee: []
created_date: '2025-12-28 06:10'
labels:
  - builders
  - phase-4
dependencies:
  - task-3
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Create the QueryBuilder computation expression in Builders/QueryBuilder.fs for type-safe query construction.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 QueryBuilder class with Yield and Zero returning Query.Empty
- [ ] #2 where CustomOperation adds field condition with implicit AND
- [ ] #3 field CustomOperation as alias for where
- [ ] #4 andAlso CustomOperation for additional AND conditions
- [ ] #5 orElse CustomOperation for OR branches
- [ ] #6 norElse CustomOperation for NOR branches
- [ ] #7 not' CustomOperation for negation
- [ ] #8 Combine member merges queries with AND
- [ ] #9 Global query instance via AutoOpen module
- [ ] #10 Code compiles successfully with dotnet build
<!-- AC:END -->
