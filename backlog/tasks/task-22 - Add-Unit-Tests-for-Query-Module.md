---
id: task-22
title: Add Unit Tests for Query Module
status: To Do
assignee: []
created_date: '2025-12-28 06:32'
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
Create unit tests for Query construction and composition. Reference: FSHARP_PORT_DESIGN.md Section 10.3.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Create file tests/FractalDb.Tests/Unit/QueryTests.fs
- [ ] #2 Add test: Query.empty returns Query.Empty
- [ ] #3 Add test: Query.eq creates Field with CompareOp.Eq
- [ ] #4 Add test: Query.field binds field name correctly
- [ ] #5 Add test: Query.all' combines queries with And
- [ ] #6 Add test: Query.any combines queries with Or
- [ ] #7 Add test: Query.not' wraps query with Not
- [ ] #8 Add test: String operators (like, contains, startsWith, endsWith) create correct FieldOp
- [ ] #9 Run 'dotnet test' - all tests pass
<!-- AC:END -->
