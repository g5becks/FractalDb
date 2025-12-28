---
id: task-25
title: Implement Options Builder CE (OptionsBuilder.fs)
status: To Do
assignee: []
created_date: '2025-12-28 06:10'
labels:
  - builders
  - phase-4
dependencies:
  - task-5
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Create the OptionsBuilder computation expression in Builders/OptionsBuilder.fs for query options.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 OptionsBuilder<'T> class with Yield returning QueryOptions.empty
- [ ] #2 sortBy CustomOperation adds sort with direction
- [ ] #3 sortAsc CustomOperation adds ascending sort
- [ ] #4 sortDesc CustomOperation adds descending sort
- [ ] #5 limit CustomOperation sets result limit
- [ ] #6 skip CustomOperation sets result offset
- [ ] #7 select CustomOperation sets field inclusion
- [ ] #8 omit CustomOperation sets field exclusion
- [ ] #9 search CustomOperation sets text search
- [ ] #10 cursorAfter and cursorBefore CustomOperations for pagination
- [ ] #11 Global options<'T> instance via AutoOpen module
- [ ] #12 Code compiles successfully with dotnet build
<!-- AC:END -->
