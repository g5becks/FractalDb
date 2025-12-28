---
id: task-35
title: Implement SqlTranslator - Query Translation Core
status: To Do
assignee: []
created_date: '2025-12-28 06:35'
updated_date: '2025-12-28 16:35'
labels:
  - phase-2
  - storage
  - translator
dependencies:
  - task-34
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Implement TranslateQuery method for logical operators. Reference: FSHARP_PORT_DESIGN.md lines 1635-1665.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Add private mutable paramCounter for generating @p1, @p2, etc.
- [ ] #2 Add private nextParam() function incrementing counter and returning param name
- [ ] #3 Add private TranslateQuery method taking Query<'T> and returning TranslatorResult
- [ ] #4 Handle Query.Empty - return TranslatorResult.empty (SQL: '1=1')
- [ ] #5 Handle Query.Field - call TranslateFieldOp (stub for now)
- [ ] #6 Handle Query.And - combine results with ' AND ', wrap in parentheses
- [ ] #7 Handle Query.Or - combine results with ' OR ', wrap in parentheses
- [ ] #8 Handle Query.Nor - combine with OR, wrap with 'NOT (...)'
- [ ] #9 Handle Query.Not - wrap single result with 'NOT (...)'
- [ ] #10 Run 'dotnet build' - build succeeds

- [ ] #11 All public types and functions have XML doc comments with <summary>, and public functions include <param>, <returns>, and <example> (see doc-2 for standards)

- [ ] #12 Run 'task lint' - no errors or warnings
<!-- AC:END -->
