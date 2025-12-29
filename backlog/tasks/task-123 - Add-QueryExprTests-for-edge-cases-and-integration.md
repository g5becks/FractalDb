---
id: task-123
title: Add QueryExprTests for edge cases and integration
status: To Do
assignee: []
created_date: '2025-12-29 06:13'
updated_date: '2025-12-29 06:15'
labels:
  - tests
  - query-expressions
dependencies:
  - task-120
  - task-121
  - task-122
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add edge case tests and integration tests to verify query expressions work correctly with the full system.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Test for empty collection returns empty results
- [ ] #2 Test for query expression produces same results as Query.field
- [ ] #3 Test for query expression works with transactions
- [ ] #4 Test for unsupported predicate throws clear error
- [ ] #5 Test for nested property access works
- [ ] #6 Test for deeply nested boolean expressions
- [ ] #7 All tests pass
<!-- AC:END -->
