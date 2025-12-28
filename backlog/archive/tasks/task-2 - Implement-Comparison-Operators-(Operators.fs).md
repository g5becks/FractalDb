---
id: task-2
title: Implement Comparison Operators (Operators.fs)
status: To Do
assignee: []
created_date: '2025-12-28 06:04'
labels:
  - core
  - phase-1
dependencies:
  - task-1
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Create the comparison, string, array, and existence operators as discriminated unions in Core/Operators.fs. Depends on Types.fs for basic types.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 CompareOp<'T> DU with Eq, Ne, Gt, Gte, Lt, Lte, In, NotIn cases
- [ ] #2 StringOp DU with Like, ILike, Contains, StartsWith, EndsWith cases
- [ ] #3 ArrayOp<'T> DU with All, Size cases
- [ ] #4 ExistsOp DU with Exists case
- [ ] #5 FieldOp DU with Compare, String, Array, Exist cases (type-erased)
- [ ] #6 Code compiles successfully with dotnet build
<!-- AC:END -->
