---
id: task-12
title: Implement SQL Translator - Query Options (SqlTranslator.fs Part 5)
status: To Do
assignee: []
created_date: '2025-12-28 06:07'
labels:
  - storage
  - phase-2
dependencies:
  - task-11
  - task-5
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Implement translation of query options (ORDER BY, LIMIT, OFFSET) in SqlTranslator.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 TranslateOptions method takes QueryOptions<'T> and returns SQL clauses
- [ ] #2 ORDER BY clause generated from Sort list with correct direction
- [ ] #3 LIMIT clause generated from Limit option with parameter
- [ ] #4 OFFSET clause generated from Skip option with parameter
- [ ] #5 Multiple sort fields are comma-separated
- [ ] #6 Code compiles successfully with dotnet build
<!-- AC:END -->
