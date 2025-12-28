---
id: task-155
title: Write integration tests for $ilike and $contains operators
status: Done
assignee:
  - '@claude'
created_date: '2025-11-23 07:27'
updated_date: '2025-11-23 07:50'
labels:
  - testing
  - phase-1
  - v0.3.0
dependencies:
  - task-154
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Create `test/integration/string-operators.test.ts` with integration tests that verify the operators work correctly with actual SQLite database queries through the Collection interface.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 Create new file `test/integration/string-operators.test.ts`
- [x] #2 Test $ilike finds case-insensitive matches (Alice, ALICE, alice)
- [x] #3 Test $ilike works with startsWith pattern (value%)
- [x] #4 Test $ilike works with endsWith pattern (%value)
- [x] #5 Test $contains finds substring matches
- [x] #6 Test $contains works with special characters like @
- [x] #7 Test combining string operators with filters
- [x] #8 Seed test data in beforeEach, cleanup in afterEach
- [x] #9 All tests pass with `bun test test/integration/string-operators.test.ts`
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Read existing integration test file to understand patterns
2. Create test/integration/string-operators.test.ts
3. Set up test database and collection with seed data
4. Write tests for $ilike case-insensitivity
5. Write tests for $contains substring matching
6. Write tests for combining operators
7. Clean up resources in afterEach
8. Run tests and lint to verify
<!-- SECTION:PLAN:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Created `test/integration/string-operators.test.ts` with comprehensive integration tests:

- 7 tests for $ilike operator (case variations, startsWith/endsWith patterns, email domain, non-indexed field, empty results, underscore wildcard)
- 6 tests for $contains operator (substring matching, special characters, hyphen, case-insensitivity note, non-indexed field, empty results)
- 7 tests for combined operators ($ilike+status, $contains+$in, $ilike+$or, $contains+$and, $not with $ilike, $not with $contains, multiple string operators)
- 4 edge case tests (empty string, exact match, sort order, limit/skip)

Note: During testing, discovered that SQLite's LIKE is case-insensitive by default for ASCII characters. This means both $like and $contains are case-insensitive. The $ilike operator adds explicit COLLATE NOCASE for clarity and documentation purposes.

All 24 tests pass. Lint passes.
<!-- SECTION:NOTES:END -->
