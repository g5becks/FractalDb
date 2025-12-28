---
id: task-29
title: Add QueryOptions Search and Cursor Functions
status: To Do
assignee: []
created_date: '2025-12-28 06:34'
updated_date: '2025-12-28 07:03'
labels:
  - phase-1
  - core
  - options
dependencies:
  - task-28
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add search and cursor pagination functions to QueryOptions module. Reference: FSHARP_PORT_DESIGN.md lines 796-806.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 In QueryOptions module, add 'search' function: takes text, fields, opts - returns opts with Search set
- [ ] #2 Add 'searchCaseSensitive' function with CaseSensitive=true
- [ ] #3 Add 'cursorAfter' function: takes id, opts - returns opts with Cursor After set
- [ ] #4 Add 'cursorBefore' function: takes id, opts - returns opts with Cursor Before set
- [ ] #5 Run 'dotnet build' - build succeeds

- [ ] #6 All public types and functions have XML doc comments with <summary>, and public functions include <param>, <returns>, and <example> (see doc-2 for standards)
<!-- AC:END -->
