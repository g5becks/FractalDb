---
id: task-119
title: Create QueryExprTests.fs with basic predicate tests
status: Done
assignee:
  - '@assistant'
created_date: '2025-12-29 06:12'
updated_date: '2025-12-29 18:32'
labels:
  - tests
  - query-expressions
dependencies:
  - task-118
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Create the query expression test file with tests for basic predicates: equality, inequality, comparison operators. These verify the translation works correctly.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 Test file created at tests/QueryExprTests.fs
- [x] #2 Test for where with equality
- [x] #3 Test for where with inequality
- [x] #4 Test for where with greater than
- [x] #5 Test for where with greater than or equal
- [x] #6 Test for where with less than
- [x] #7 Test for where with less than or equal
- [x] #8 Test file added to fsproj
- [x] #9 All tests pass
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Create QueryExprTests.fs test file with basic structure
2. Add test types and setup
3. Implement tests for comparison operators (=, <>, >, >=, <, <=)
4. Add test file to .fsproj
5. Attempt to run tests
6. Analyze results and determine if integration tests are needed instead
<!-- SECTION:PLAN:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Implemented QueryExprTests with real Collection<T> integration tests.

Fixes made:
1. Replaced broken SpecificCall patterns with Call pattern matching in QueryTranslator
2. Added isQueryBuilderMethod helper for cleaner pattern matching
3. Improved collection Name property reflection with better error handling
4. Tests use CompareOp<obj> since evaluateExpr returns obj

All 11 tests pass: Eq, Ne, Gt, Gte, Lt, Lte operators with int64, string, bool types.
Total test suite: 232 passed, 6 failed (pre-existing ArrayOp bugs).

**Implementation Summary:**

Created comprehensive integration tests for QueryExpr using real Collection<T> objects.

**Critical Bug Fixes (in QueryExpr.fs):**
1. **SpecificCall Pattern Failure**: Original patterns `<@ Unchecked.defaultof<QueryBuilder>.For @>` failed because they weren't valid method call quotations
   - Solution: Replaced with `Call(Some _, mi, args) when isQueryBuilderMethod mi "For" ->` pattern
   - This properly matches method calls on QueryBuilder instances

2. **Collection Name Reflection**: Added robust property reflection with better error handling
   - Handles both public and non-public properties
   - Clear error messages showing available properties if Name not found

3. **Type Boxing**: Fixed test assertions to use `CompareOp<obj>` since `evaluateExpr` returns `obj`

**Test Coverage:**
- Equality operators (int64, string, bool)
- Inequality operators
- Comparison operators (>, >=, <, <=)
- Source collection name extraction
- Edge cases (queries without where clauses)

**Results:**
- 11 new tests added (all passing)
- Total: 232 passed, 6 failed (pre-existing ArrayOp bugs)
- Tests use IClassFixture pattern with real in-memory database
<!-- SECTION:NOTES:END -->
