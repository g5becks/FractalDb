---
id: task-19
title: Implement Query Module - String Helpers
status: To Do
assignee: []
created_date: '2025-12-28 06:31'
updated_date: '2025-12-28 16:34'
labels:
  - phase-1
  - core
  - query
dependencies:
  - task-18
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add string operation helper functions to Query module. Reference: FSHARP_PORT_DESIGN.md lines 369-384.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 In Query module, add 'let like pattern = Query.Field("", FieldOp.String(StringOp.Like pattern))'
- [ ] #2 Add 'ilike' for case-insensitive LIKE
- [ ] #3 Add 'contains' for substring matching
- [ ] #4 Add 'startsWith' for prefix matching
- [ ] #5 Add 'endsWith' for suffix matching
- [ ] #6 Run 'dotnet build' - build succeeds

- [ ] #7 All public types and functions have XML doc comments with <summary>, and public functions include <param>, <returns>, and <example> (see doc-2 for standards)

- [ ] #8 Run 'task lint' - no errors or warnings
<!-- AC:END -->
