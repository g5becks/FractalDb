---
id: task-39
title: Implement SqlTranslator - Options Translation
status: To Do
assignee: []
created_date: '2025-12-28 06:36'
updated_date: '2025-12-28 07:03'
labels:
  - phase-2
  - storage
  - translator
dependencies:
  - task-38
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add TranslateOptions method for query options. Reference: FSHARP_PORT_DESIGN.md lines 1787-1822.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Add public TranslateOptions(options: QueryOptions<'T>) : string * (string * obj) list
- [ ] #2 Generate ORDER BY clause from Sort list with ASC/DESC
- [ ] #3 Generate LIMIT @opt1 clause from Limit option
- [ ] #4 Generate OFFSET @opt2 clause from Skip option
- [ ] #5 Return combined SQL clauses and parameters
- [ ] #6 Run 'dotnet build' - build succeeds

- [ ] #7 All public types and functions have XML doc comments with <summary>, and public functions include <param>, <returns>, and <example> (see doc-2 for standards)
<!-- AC:END -->
