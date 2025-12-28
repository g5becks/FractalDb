---
id: task-28
title: Implement QueryOptions Module Functions
status: To Do
assignee: []
created_date: '2025-12-28 06:34'
updated_date: '2025-12-28 16:55'
labels:
  - phase-1
  - core
  - options
dependencies:
  - task-27
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Create QueryOptions module with helper functions in src/Options.fs. Reference: FSHARP_PORT_DESIGN.md lines 777-806.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Add 'let empty<'T> : QueryOptions<'T>' returning record with Sort=[], all options=None
- [ ] #2 Add 'let limit n opts = { opts with Limit = Some n }'
- [ ] #3 Add 'let skip n opts = { opts with Skip = Some n }'
- [ ] #4 Add 'let sortBy field dir opts = { opts with Sort = (field, dir) :: opts.Sort }'
- [ ] #5 Add 'let sortAsc field opts' and 'let sortDesc field opts' as shortcuts
- [ ] #6 Add 'let select fields opts' and 'let omit fields opts'
- [ ] #7 Run 'dotnet build' - build succeeds
- [ ] #8 All public types and functions have XML doc comments with <summary>, and public functions include <param>, <returns>, and <example> (see doc-2 for standards)

- [ ] #9 Run 'task lint' - no errors or warnings

- [ ] #10 In src/Options.fs, add module QueryOptions
<!-- AC:END -->
