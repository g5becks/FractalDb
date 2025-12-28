---
id: task-40
title: Add Unit Tests for SqlTranslator
status: To Do
assignee: []
created_date: '2025-12-28 06:37'
updated_date: '2025-12-28 16:56'
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
- [ ] #1 Create test schema with indexed and non-indexed fields
- [ ] #2 Add test: Empty query translates to '1=1'
- [ ] #3 Add test: Indexed field uses generated column name (_fieldName)
- [ ] #4 Add test: Non-indexed field uses jsonb_extract
- [ ] #5 Add test: Eq generates '= @p1' with correct parameter
- [ ] #6 Add test: In generates 'IN (@p1, @p2, ...)'
- [ ] #7 Add test: Empty In generates '0=1'
- [ ] #8 Add test: Contains generates 'LIKE %substring%'
- [ ] #9 Add test: And combines with 'AND'
- [ ] #10 Add test: Or combines with 'OR'
- [ ] #11 Add test: Not wraps with 'NOT (...)'
- [ ] #12 Run 'dotnet test' - all tests pass
- [ ] #13 Run 'task lint' - no errors or warnings

- [ ] #14 Create file tests/SqlTranslatorTests.fs
<!-- AC:END -->
