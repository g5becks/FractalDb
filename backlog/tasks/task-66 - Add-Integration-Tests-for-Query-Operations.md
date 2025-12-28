---
id: task-66
title: Add Integration Tests for Query Operations
status: To Do
assignee: []
created_date: '2025-12-28 06:43'
updated_date: '2025-12-28 16:37'
labels:
  - phase-3
  - testing
  - integration
dependencies:
  - task-65
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Create integration tests for query functionality.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Create file tests/FractalDb.Tests/Integration/QueryTests.fs
- [ ] #2 Add test: Query.eq filters correctly
- [ ] #3 Add test: Query.gte and Query.lt range query works
- [ ] #4 Add test: Query.contains string operator works
- [ ] #5 Add test: Query.isIn filters by list
- [ ] #6 Add test: Query.all' combines conditions with AND
- [ ] #7 Add test: Query.any combines conditions with OR
- [ ] #8 Add test: QueryOptions sort orders correctly
- [ ] #9 Add test: QueryOptions limit restricts results
- [ ] #10 Run 'dotnet test' - all tests pass

- [ ] #11 Run 'task lint' - no errors or warnings
<!-- AC:END -->
