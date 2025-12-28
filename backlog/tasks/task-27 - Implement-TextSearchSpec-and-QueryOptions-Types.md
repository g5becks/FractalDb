---
id: task-27
title: Implement TextSearchSpec and QueryOptions Types
status: To Do
assignee: []
created_date: '2025-12-28 06:33'
updated_date: '2025-12-28 16:55'
labels:
  - phase-1
  - core
  - options
dependencies:
  - task-26
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Create TextSearchSpec and QueryOptions records in src/Options.fs. Reference: FSHARP_PORT_DESIGN.md lines 761-775.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 TextSearchSpec has: Text: string, Fields: string list, CaseSensitive: bool
- [ ] #2 Add QueryOptions<'T> record
- [ ] #3 QueryOptions has: Sort: (string * SortDirection) list, Limit: int option, Skip: int option, Select: string list option, Omit: string list option, Search: TextSearchSpec option, Cursor: CursorSpec option
- [ ] #4 Run 'dotnet build' - build succeeds
- [ ] #5 All public types and functions have XML doc comments with <summary>, and public functions include <param>, <returns>, and <example> (see doc-2 for standards)

- [ ] #6 Run 'task lint' - no errors or warnings

- [ ] #7 In src/Options.fs, add TextSearchSpec record
<!-- AC:END -->
