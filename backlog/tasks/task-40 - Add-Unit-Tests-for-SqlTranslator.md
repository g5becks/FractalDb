---
id: task-40
title: Add Unit Tests for SqlTranslator
status: In Progress
assignee:
  - '@assistant'
created_date: '2025-12-28 06:37'
updated_date: '2025-12-28 18:43'
labels:
  - phase-2
  - testing
  - unit
dependencies:
  - task-39
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Create comprehensive unit tests for SQL translation in tests/SqlTranslatorTests.fs. Reference: FSHARP_PORT_DESIGN.md lines 2028-2119.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 Create test schema with indexed and non-indexed fields
- [x] #2 Add test: Empty query translates to '1=1'
- [x] #3 Add test: Indexed field uses generated column name (_fieldName)
- [x] #4 Add test: Non-indexed field uses jsonb_extract
- [x] #5 Add test: Eq generates '= @p1' with correct parameter
- [x] #6 Add test: In generates 'IN (@p1, @p2, ...)'
- [x] #7 Add test: Empty In generates '0=1'
- [x] #8 Add test: Contains generates 'LIKE %substring%'
- [x] #9 Add test: And combines with 'AND'
- [x] #10 Add test: Or combines with 'OR'
- [x] #11 Add test: Not wraps with 'NOT (...)'
- [x] #12 Run 'dotnet test' - all tests pass
- [x] #13 Run 'task lint' - no errors or warnings

- [x] #14 Create file tests/SqlTranslatorTests.fs
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Create tests/SqlTranslatorTests.fs file
2. Add test module setup with sample schema (indexed and non-indexed fields)
3. Write test: Empty query → "1=1"
4. Write test: Field resolution for indexed fields → "_fieldName"
5. Write test: Field resolution for non-indexed → jsonb_extract
6. Write test: Eq operator → "= @p0"
7. Write test: In operator with values → "IN (@p0, @p1)"
8. Write test: In operator empty list → "0=1"
9. Write test: String Contains → "LIKE @p0" with %%pattern%%
10. Write test: And combinator → "(...) AND (...)"
11. Write test: Or combinator → "(...) OR (...)"
12. Write test: Not combinator → "NOT (...)"
13. Write test: TranslateOptions with Sort, Limit, Skip
14. Add file to test project and run tests
<!-- SECTION:PLAN:END -->
