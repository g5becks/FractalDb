---
id: task-11
title: >-
  Implement SQL Translator - String and Array Operators (SqlTranslator.fs Part
  4)
status: To Do
assignee: []
created_date: '2025-12-28 06:06'
labels:
  - storage
  - phase-2
dependencies:
  - task-10
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Implement translation of string operators (Like, Contains, etc.), array operators (All, Size), and existence operators in SqlTranslator.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 TranslateString handles Like with SQL LIKE
- [ ] #2 TranslateString handles ILike with LIKE COLLATE NOCASE
- [ ] #3 TranslateString handles Contains with LIKE %substring%
- [ ] #4 TranslateString handles StartsWith with LIKE prefix%
- [ ] #5 TranslateString handles EndsWith with LIKE %suffix
- [ ] #6 TranslateArray handles All with EXISTS/json_each
- [ ] #7 TranslateArray handles Size with json_array_length
- [ ] #8 TranslateExist handles Exists true/false with json_type IS [NOT] NULL
- [ ] #9 Code compiles successfully with dotnet build
<!-- AC:END -->
