---
id: task-38
title: Implement SqlTranslator - Array and Exists Operators
status: To Do
assignee: []
created_date: '2025-12-28 06:36'
updated_date: '2025-12-28 16:56'
labels:
  - phase-2
  - storage
  - translator
dependencies:
  - task-37
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Implement TranslateArray and TranslateExist in src/SqlTranslator.fs. Reference: FSHARP_PORT_DESIGN.md lines 1743-1784.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Add private TranslateArray method
- [ ] #2 ArrayOp.All with empty list returns '1=1'; otherwise generates EXISTS with json_each for each value
- [ ] #3 ArrayOp.Size generates 'json_array_length(field) = @p1'
- [ ] #4 Add private TranslateExist method
- [ ] #5 ExistsOp.Exists true generates 'json_type(field) IS NOT NULL'
- [ ] #6 ExistsOp.Exists false generates 'json_type(field) IS NULL'
- [ ] #7 Run 'dotnet build' - build succeeds

- [ ] #8 All public types and functions have XML doc comments with <summary>, and public functions include <param>, <returns>, and <example> (see doc-2 for standards)

- [ ] #9 Run 'task lint' - no errors or warnings
<!-- AC:END -->
