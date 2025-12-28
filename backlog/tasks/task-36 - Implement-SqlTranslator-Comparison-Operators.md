---
id: task-36
title: Implement SqlTranslator - Comparison Operators
status: To Do
assignee: []
created_date: '2025-12-28 06:35'
updated_date: '2025-12-28 07:03'
labels:
  - phase-2
  - storage
  - translator
dependencies:
  - task-35
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Implement TranslateCompare for comparison operators. Reference: FSHARP_PORT_DESIGN.md lines 1679-1722.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Add private TranslateFieldOp method dispatching to TranslateCompare, TranslateString, TranslateArray, TranslateExist
- [ ] #2 Add private TranslateCompare method unboxing CompareOp and generating SQL
- [ ] #3 Eq generates 'field = @p1' with parameter
- [ ] #4 Ne generates 'field != @p1'
- [ ] #5 Gt/Gte/Lt/Lte generate '>/>=/</<='
- [ ] #6 In with empty list returns '0=1' (matches nothing); otherwise 'field IN (@p1, @p2, ...)'
- [ ] #7 NotIn with empty list returns '1=1' (matches all); otherwise 'field NOT IN (...)'
- [ ] #8 Run 'dotnet build' - build succeeds

- [ ] #9 All public types and functions have XML doc comments with <summary>, and public functions include <param>, <returns>, and <example> (see doc-2 for standards)
<!-- AC:END -->
