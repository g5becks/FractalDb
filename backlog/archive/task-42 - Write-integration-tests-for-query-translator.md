---
id: task-42
title: Write integration tests for query translator
status: To Do
assignee: []
created_date: '2025-11-21 01:51'
updated_date: '2025-11-21 02:07'
labels:
  - testing
  - query
  - integration
dependencies:
  - task-12
  - task-13
  - task-14
  - task-15
  - task-16
  - task-17
  - task-18
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Test the QueryTranslator generates correct SQL for all query operators with proper parameterization.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Create test/query/translator.test.ts file
- [ ] #2 Test comparison operators generate correct SQL (=, !=, >, >=, <, <=, IN, NOT IN)
- [ ] #3 Test string operators generate correct SQL (REGEXP, LIKE patterns)
- [ ] #4 Test array operators generate correct subqueries
- [ ] #5 Test logical operators generate correct nested conditions with parentheses
- [ ] #6 Test existence operator generates correct json_type() checks
- [ ] #7 Test sort, limit, skip generate correct ORDER BY, LIMIT, OFFSET clauses
- [ ] #8 Verify all queries use parameterized statements (no SQL injection)
- [ ] #9 Test that generated SQL is valid SQLite syntax
- [ ] #10 All tests pass with bun test
- [ ] #11 Tests compile with strict mode
- [ ] #12 No use of any type in tests
<!-- AC:END -->
