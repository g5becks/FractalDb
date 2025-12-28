---
id: task-22
title: Add Unit Tests for Query Module
status: To Do
assignee: []
created_date: '2025-12-28 06:32'
updated_date: '2025-12-28 16:55'
labels:
  - phase-1
  - testing
  - unit
dependencies:
  - task-21
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Create unit tests for Query construction and composition in tests/QueryTests.fs. Reference: FSHARP_PORT_DESIGN.md Section 10.3.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Add test: Query.empty returns Query.Empty
- [ ] #2 Add test: Query.eq creates Field with CompareOp.Eq
- [ ] #3 Add test: Query.field binds field name correctly
- [ ] #4 Add test: Query.all' combines queries with And
- [ ] #5 Add test: Query.any combines queries with Or
- [ ] #6 Add test: Query.not' wraps query with Not
- [ ] #7 Add test: String operators (like, contains, startsWith, endsWith) create correct FieldOp
- [ ] #8 Run 'dotnet test' - all tests pass
- [ ] #9 Run 'task lint' - no errors or warnings

- [ ] #10 Create file tests/QueryTests.fs
<!-- AC:END -->
