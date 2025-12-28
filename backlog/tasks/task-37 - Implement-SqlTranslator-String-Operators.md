---
id: task-37
title: Implement SqlTranslator - String Operators
status: To Do
assignee: []
created_date: '2025-12-28 06:36'
updated_date: '2025-12-28 07:03'
labels:
  - phase-2
  - storage
  - translator
dependencies:
  - task-36
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Implement TranslateString for string operators. Reference: FSHARP_PORT_DESIGN.md lines 1724-1741.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Add private TranslateString method
- [ ] #2 StringOp.Like generates 'field LIKE @p1' with pattern as parameter
- [ ] #3 StringOp.ILike generates 'field LIKE @p1 COLLATE NOCASE'
- [ ] #4 StringOp.Contains generates LIKE with '%substring%' pattern
- [ ] #5 StringOp.StartsWith generates LIKE with 'prefix%' pattern
- [ ] #6 StringOp.EndsWith generates LIKE with '%suffix' pattern
- [ ] #7 Run 'dotnet build' - build succeeds

- [ ] #8 All public types and functions have XML doc comments with <summary>, and public functions include <param>, <returns>, and <example> (see doc-2 for standards)
<!-- AC:END -->
