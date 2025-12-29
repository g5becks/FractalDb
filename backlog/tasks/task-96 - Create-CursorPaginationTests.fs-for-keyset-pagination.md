---
id: task-96
title: Create CursorPaginationTests.fs for keyset pagination
status: To Do
assignee:
  - '@assistant'
created_date: '2025-12-29 06:04'
updated_date: '2025-12-29 07:41'
labels:
  - tests
  - pagination
  - cursor
dependencies: []
priority: medium
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Create tests for cursor-based (keyset) pagination. Verify cursorAfter and cursorBefore work correctly with various scenarios.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Test file created at tests/CursorPaginationTests.fs
- [ ] #2 cursorAfter returns documents after specified ID
- [ ] #3 cursorBefore returns documents before specified ID
- [ ] #4 Cursor pagination with sorting works
- [ ] #5 Cursor with non-existent ID returns empty
- [ ] #6 First page has no cursor requirement
- [ ] #7 Last page cursorAfter returns empty
- [ ] #8 Test file added to fsproj
- [ ] #9 All tests pass
<!-- AC:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Cannot proceed with this task. Cursor pagination (cursorAfter/cursorBefore) is not yet implemented in the codebase:\n\n1. The API exists (QueryOptions.cursorAfter/cursorBefore in Options.fs)\n2. The QueryBuilder has CustomOperations for cursorAfter/cursorBefore\n3. BUT SqlTranslator.TranslateOptions does NOT implement cursor handling (see SqlTranslator.fs:887, 901-902 - comments explicitly say 'Cursor will be implemented in future tasks')\n\nThis task is blocked until cursor pagination is actually implemented in the SQL translation layer. Need to complete implementation tasks 104-118 first (QueryExpr computation expression features).
<!-- SECTION:NOTES:END -->
