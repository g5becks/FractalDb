---
id: task-68
title: Add QueryBuilder Logical Operations
status: To Do
assignee: []
created_date: '2025-12-28 06:43'
updated_date: '2025-12-28 07:03'
labels:
  - phase-4
  - builders
dependencies:
  - task-67
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add orElse, norElse, not' operations to QueryBuilder. Reference: FSHARP_PORT_DESIGN.md lines 492-512.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 In QueryBuilder, add [<CustomOperation("andAlso")>] member for additional AND
- [ ] #2 Add [<CustomOperation("orElse")>] member taking Query list, combines with OR
- [ ] #3 Add [<CustomOperation("norElse")>] member taking Query list, combines with NOR
- [ ] #4 Add [<CustomOperation("not'")>] member for negation
- [ ] #5 Run 'dotnet build' - build succeeds

- [ ] #6 All public types and functions have XML doc comments with <summary>, and public functions include <param>, <returns>, and <example> (see doc-2 for standards)
<!-- AC:END -->
