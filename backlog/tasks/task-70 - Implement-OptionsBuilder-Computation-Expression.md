---
id: task-70
title: Implement OptionsBuilder Computation Expression
status: To Do
assignee: []
created_date: '2025-12-28 06:44'
updated_date: '2025-12-28 07:03'
labels:
  - phase-4
  - builders
dependencies:
  - task-69
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Create OptionsBuilder CE for query options. Reference: FSHARP_PORT_DESIGN.md lines 808-844.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Create file src/FractalDb/Builders/OptionsBuilder.fs
- [ ] #2 Add namespace FractalDb.Builders
- [ ] #3 Define OptionsBuilder<'T> class with Yield/Zero returning QueryOptions.empty
- [ ] #4 Add [<CustomOperation("sortBy")>], [<CustomOperation("sortAsc")>], [<CustomOperation("sortDesc")>]
- [ ] #5 Add [<CustomOperation("limit")>] and [<CustomOperation("skip")>]
- [ ] #6 Add [<CustomOperation("select")>] and [<CustomOperation("omit")>]
- [ ] #7 Add [<CustomOperation("search")>]
- [ ] #8 Add [<CustomOperation("cursorAfter")>] and [<CustomOperation("cursorBefore")>]
- [ ] #9 Add [<AutoOpen>] module with 'let options<'T> = OptionsBuilder<'T>()'
- [ ] #10 Run 'dotnet build' - build succeeds

- [ ] #11 All public types and functions have XML doc comments with <summary>, and public functions include <param>, <returns>, and <example> (see doc-2 for standards)
<!-- AC:END -->
