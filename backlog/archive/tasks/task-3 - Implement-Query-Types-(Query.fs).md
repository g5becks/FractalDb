---
id: task-3
title: Implement Query Types (Query.fs)
status: To Do
assignee: []
created_date: '2025-12-28 06:04'
labels:
  - core
  - phase-1
dependencies:
  - task-2
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Create the Query<'T> discriminated union and Query helper module in Core/Query.fs. Depends on Operators.fs for field operations.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Query<'T> DU with Empty, Field, And, Or, Nor, Not cases
- [ ] #2 Query module with empty constructor function
- [ ] #3 Query module with comparison helpers: eq, ne, gt, gte, lt, lte, isIn, notIn
- [ ] #4 Query module with string helpers: like, ilike, contains, startsWith, endsWith
- [ ] #5 Query module with array helpers: all, size
- [ ] #6 Query module with existence helpers: exists, notExists
- [ ] #7 Query module with field binding function
- [ ] #8 Query module with logical combinators: all', any, none, not'
- [ ] #9 Code compiles successfully with dotnet build
<!-- AC:END -->
