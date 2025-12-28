---
id: task-26
title: Implement SortDirection and CursorSpec Types
status: To Do
assignee: []
created_date: '2025-12-28 06:33'
updated_date: '2025-12-28 07:03'
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
Create SortDirection DU and CursorSpec record in Core/Options.fs. Reference: FSHARP_PORT_DESIGN.md lines 751-759.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Create file src/FractalDb/Core/Options.fs
- [ ] #2 Add namespace FractalDb.Core
- [ ] #3 Add [<RequireQualifiedAccess>] attribute to SortDirection
- [ ] #4 Define SortDirection DU with cases: Ascending, Descending
- [ ] #5 Add CursorSpec record with: After: string option, Before: string option
- [ ] #6 Run 'dotnet build' - build succeeds

- [ ] #7 All public types and functions have XML doc comments with <summary>, and public functions include <param>, <returns>, and <example> (see doc-2 for standards)
<!-- AC:END -->
