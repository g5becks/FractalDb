---
id: task-26
title: Implement SortDirection and CursorSpec Types
status: To Do
assignee: []
created_date: '2025-12-28 06:33'
updated_date: '2025-12-28 16:55'
labels:
  - phase-1
  - core
  - options
dependencies:
  - task-25
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Create SortDirection DU and CursorSpec record in src/Options.fs. Reference: FSHARP_PORT_DESIGN.md lines 751-759.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Add namespace FractalDb.Core
- [ ] #2 Define SortDirection DU with cases: Ascending, Descending
- [ ] #3 Add CursorSpec record with: After: string option, Before: string option
- [ ] #4 Run 'dotnet build' - build succeeds
- [ ] #5 All public types and functions have XML doc comments with <summary>, and public functions include <param>, <returns>, and <example> (see doc-2 for standards)
- [ ] #6 Run 'task lint' - no errors or warnings

- [ ] #7 Create file src/Options.fs

- [ ] #8 Add module declaration: module FractalDb.Options
<!-- AC:END -->
