---
id: task-55
title: Write edge case tests for deeply nested queries
status: To Do
assignee: []
created_date: '2025-11-21 01:53'
updated_date: '2025-11-21 02:07'
labels:
  - testing
  - edge-cases
  - queries
dependencies:
  - task-15
priority: medium
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Test complex nested logical operator combinations to ensure correct SQL precedence and parenthesization.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Create test/edge-cases/nested-queries.test.ts file
- [ ] #2 Test deeply nested / combinations
- [ ] #3 Test  operator with multiple conditions
- [ ] #4 Test  operator negating complex conditions
- [ ] #5 Test query precedence with parentheses in generated SQL
- [ ] #6 Test nested queries match expected documents
- [ ] #7 Verify generated SQL is syntactically correct
- [ ] #8 All tests pass with bun test
- [ ] #9 Tests compile with strict mode
<!-- AC:END -->
