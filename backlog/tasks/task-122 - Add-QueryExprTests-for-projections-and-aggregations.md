---
id: task-122
title: Add QueryExprTests for projections and aggregations
status: To Do
assignee: []
created_date: '2025-12-29 06:13'
updated_date: '2025-12-29 06:15'
labels:
  - tests
  - query-expressions
dependencies:
  - task-119
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Extend QueryExprTests with tests for select projections and aggregation operations (count, exists, head, headOrDefault).
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Test for select single field
- [ ] #2 Test for select entire entity
- [ ] #3 Test for select with anonymous record
- [ ] #4 Test for count all
- [ ] #5 Test for count with where
- [ ] #6 Test for exists returns true when match found
- [ ] #7 Test for exists returns false when no match
- [ ] #8 Test for head returns first element
- [ ] #9 Test for head throws on empty
- [ ] #10 Test for headOrDefault returns Some on match
- [ ] #11 Test for headOrDefault returns None on empty
- [ ] #12 All tests pass
<!-- AC:END -->
