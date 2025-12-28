---
id: task-5
title: Implement Query Options Types (Options.fs)
status: To Do
assignee: []
created_date: '2025-12-28 06:05'
labels:
  - core
  - phase-1
dependencies:
  - task-1
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Create query options, sort, cursor, and text search types in Core/Options.fs. Depends only on Types.fs.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 SortDirection DU with Ascending, Descending cases
- [ ] #2 CursorSpec record with After, Before fields (string option)
- [ ] #3 TextSearchSpec record with Text, Fields, CaseSensitive fields
- [ ] #4 QueryOptions<'T> record with Sort, Limit, Skip, Select, Omit, Search, Cursor fields
- [ ] #5 QueryOptions module with empty, limit, skip, sortBy, sortAsc, sortDesc, select, omit functions
- [ ] #6 QueryOptions module with search, searchCaseSensitive, cursorAfter, cursorBefore functions
- [ ] #7 Code compiles successfully with dotnet build
<!-- AC:END -->
