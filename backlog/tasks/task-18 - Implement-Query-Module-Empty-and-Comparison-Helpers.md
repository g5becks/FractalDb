---
id: task-18
title: Implement Query Module - Empty and Comparison Helpers
status: To Do
assignee: []
created_date: '2025-12-28 06:31'
updated_date: '2025-12-28 16:55'
labels:
  - phase-1
  - core
  - query
dependencies:
  - task-17
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add Query module with empty constructor and comparison helper functions in src/Query.fs. Reference: FSHARP_PORT_DESIGN.md lines 335-366.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Add 'let empty<'T> : Query<'T> = Query.Empty'
- [ ] #2 Add 'let inline eq value = Query.Field("", FieldOp.Compare(box (CompareOp.Eq value)))'
- [ ] #3 Add 'ne', 'gt', 'gte', 'lt', 'lte' following same pattern as eq
- [ ] #4 Add 'let inline isIn values = Query.Field("", FieldOp.Compare(box (CompareOp.In values)))'
- [ ] #5 Add 'notIn' following same pattern
- [ ] #6 Run 'dotnet build' - build succeeds
- [ ] #7 All public types and functions have XML doc comments with <summary>, and public functions include <param>, <returns>, and <example> (see doc-2 for standards)

- [ ] #8 Run 'task lint' - no errors or warnings

- [ ] #9 In src/Query.fs, add [<RequireQualifiedAccess>] module Query
<!-- AC:END -->
