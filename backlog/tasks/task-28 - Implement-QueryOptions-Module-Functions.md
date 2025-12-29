---
id: task-28
title: Implement QueryOptions Module Functions
status: Done
assignee:
  - '@assistant'
created_date: '2025-12-28 06:34'
updated_date: '2025-12-28 18:08'
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
- [x] #1 Add 'let empty<'T> : QueryOptions<'T>' returning record with Sort=[], all options=None
- [x] #2 Add 'let limit n opts = { opts with Limit = Some n }'
- [x] #3 Add 'let skip n opts = { opts with Skip = Some n }'
- [x] #4 Add 'let sortBy field dir opts = { opts with Sort = (field, dir) :: opts.Sort }'
- [x] #5 Add 'let sortAsc field opts' and 'let sortDesc field opts' as shortcuts
- [x] #6 Add 'let select fields opts' and 'let omit fields opts'
- [x] #7 Run 'dotnet build' - build succeeds
- [x] #8 All public types and functions have XML doc comments with <summary>, and public functions include <param>, <returns>, and <example> (see doc-2 for standards)

- [x] #9 Run 'task lint' - no errors or warnings

- [x] #10 In src/Options.fs, add module QueryOptions
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Read design spec lines 777-806 for QueryOptions module functions
2. Add module QueryOptions to src/Options.fs
3. Implement empty<'T> function
4. Implement limit, skip, sortBy, sortAsc, sortDesc functions
5. Implement select and omit functions
6. Add comprehensive XML documentation for all functions
7. Run dotnet build to verify
8. Run task lint to verify
9. Mark all ACs complete
<!-- SECTION:PLAN:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Added QueryOptions module with helper functions to src/Options.fs.

Implementation details:
- Added module QueryOptions with 8 functions:
  * empty<'T> - creates default QueryOptions with all fields set to None/[]
  * limit n - sets maximum number of results
  * skip n - sets offset for pagination
  * sortBy field dir - adds sort field with direction
  * sortAsc field - shortcut for ascending sort
  * sortDesc field - shortcut for descending sort
  * select fields - whitelist projection
  * omit fields - blacklist projection
- All functions are immutable, returning new QueryOptions instances
- Comprehensive XML documentation for module and all functions
- Each function has <summary>, <param>, <returns>, <remarks>, and <example> sections
- Documentation explains fluent API usage pattern
- Documented sort order precedence and performance considerations
- Explained mutually exclusive options (select/omit)

Verification:
- dotnet build: Success (0 warnings, 0 errors)
- task lint: Success (0 warnings)
- File size: 976 lines (under 1000 line limit)

Ready for next task: Task 29 - QueryOptions Search and Cursor Functions
<!-- SECTION:NOTES:END -->
