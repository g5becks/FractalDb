---
id: task-10
title: Implement SQL Translator - Comparison Operators (SqlTranslator.fs Part 3)
status: To Do
assignee: []
created_date: '2025-12-28 06:06'
labels:
  - storage
  - phase-2
dependencies:
  - task-9
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Implement translation of comparison operators (Eq, Ne, Gt, Gte, Lt, Lte, In, NotIn) in SqlTranslator.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 TranslateCompare handles Eq with = operator
- [ ] #2 TranslateCompare handles Ne with != operator
- [ ] #3 TranslateCompare handles Gt, Gte, Lt, Lte with >, >=, <, <= operators
- [ ] #4 TranslateCompare handles In with IN (placeholders) - empty list returns 0=1
- [ ] #5 TranslateCompare handles NotIn with NOT IN - empty list returns 1=1
- [ ] #6 Parameters are properly extracted and named sequentially (@p1, @p2, etc.)
- [ ] #7 Code compiles successfully with dotnet build
<!-- AC:END -->
