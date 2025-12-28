---
id: task-20
title: Implement Query Module - Array and Existence Helpers
status: To Do
assignee: []
created_date: '2025-12-28 06:31'
updated_date: '2025-12-28 16:34'
labels:
  - phase-1
  - core
  - query
dependencies:
  - task-19
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add array and existence helper functions to Query module. Reference: FSHARP_PORT_DESIGN.md lines 387-403.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 In Query module, add 'let all<'T> values = Query.Field("", FieldOp.Array(box (ArrayOp.All<'T> values)))'
- [ ] #2 Add 'let size n = Query.Field("", FieldOp.Array(box (ArrayOp.Size n)))'
- [ ] #3 Add 'let exists = Query.Field("", FieldOp.Exist(ExistsOp.Exists true))'
- [ ] #4 Add 'let notExists = Query.Field("", FieldOp.Exist(ExistsOp.Exists false))'
- [ ] #5 Run 'dotnet build' - build succeeds

- [ ] #6 All public types and functions have XML doc comments with <summary>, and public functions include <param>, <returns>, and <example> (see doc-2 for standards)

- [ ] #7 Run 'task lint' - no errors or warnings
<!-- AC:END -->
