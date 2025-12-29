---
id: task-119
title: Create QueryExprTests.fs with basic predicate tests
status: In Progress
assignee:
  - '@assistant'
created_date: '2025-12-29 06:12'
updated_date: '2025-12-29 18:15'
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
- [ ] #1 Test file created at tests/QueryExprTests.fs
- [ ] #2 Test for where with equality
- [ ] #3 Test for where with inequality
- [ ] #4 Test for where with greater than
- [ ] #5 Test for where with greater than or equal
- [ ] #6 Test for where with less than
- [ ] #7 Test for where with less than or equal
- [ ] #8 Test file added to fsproj
- [ ] #9 All tests pass
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
**Testing Challenge Discovered:**

QueryExpr translation requires real Collection<T> objects from FractalDb.Collection module. Mock collections fail because:

1. SpecificCall pattern matching in QueryTranslator.translate looks for specific QueryBuilder method calls
2. The For clause extraction uses reflection to get collection.Name property
3. Mock types (simple records) don't match the expected Collection<T> structure

**Impact:**
- Unit tests for query expression translation cannot work with mock collections
- Tests require actual Collection<T> instances (integration tests)
- This means tests need database setup/teardown

**Options:**
1. Convert to integration tests (use real DB + Collection objects)
2. Defer QueryExpr testing until Collection integration is implemented
3. Test only the QueryTranslator internals that don't require collections

**Decision Needed:** How to proceed with QueryExpr testing given this architectural constraint?
<!-- SECTION:NOTES:END -->
