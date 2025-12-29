---
id: task-29
title: Add QueryOptions Search and Cursor Functions
status: Done
assignee:
  - '@assistant'
created_date: '2025-12-28 06:34'
updated_date: '2025-12-28 18:12'
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
Add search and cursor pagination functions to QueryOptions module in src/Options.fs. Reference: FSHARP_PORT_DESIGN.md lines 796-806.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 In QueryOptions module, add 'search' function: takes text, fields, opts - returns opts with Search set
- [x] #2 Add 'searchCaseSensitive' function with CaseSensitive=true
- [x] #3 Add 'cursorAfter' function: takes id, opts - returns opts with Cursor After set
- [x] #4 Add 'cursorBefore' function: takes id, opts - returns opts with Cursor Before set
- [x] #5 Run 'dotnet build' - build succeeds

- [x] #6 All public types and functions have XML doc comments with <summary>, and public functions include <param>, <returns>, and <example> (see doc-2 for standards)

- [x] #7 Run 'task lint' - no errors or warnings
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Read design spec lines 796-806 for search and cursor functions
2. Add search function to QueryOptions module
3. Add searchCaseSensitive function
4. Add cursorAfter function
5. Add cursorBefore function
6. Add comprehensive XML documentation for all functions
7. Run dotnet build to verify
8. Run task lint to verify
9. Mark all ACs complete
<!-- SECTION:PLAN:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Added search and cursor pagination functions to QueryOptions module in src/Options.fs.

Implementation details:
- Added 4 functions to QueryOptions module:
  * search text fields - case-insensitive text search
  * searchCaseSensitive text fields - case-sensitive search
  * cursorAfter id - forward pagination
  * cursorBefore id - backward pagination
- All functions return new QueryOptions with appropriate fields set
- Comprehensive XML documentation with <summary>, <param>, <returns>, <remarks>, <example>
- search sets CaseSensitive = false, searchCaseSensitive sets true
- Cursor functions set After/Before appropriately (mutually exclusive)

Also condensed documentation throughout file to stay under 1000 line limit:
- Reduced TextSearchSpec docs from 70+ lines to 13 lines
- Reduced QueryOptions<'T> docs from 130+ lines to 40 lines
- Kept all essential information while removing verbose examples
- Final file size: 720 lines (well under limit)

Verification:
- dotnet build: Success (0 warnings, 0 errors)
- task lint: Success (0 warnings)

Ready for next task: Task 30 - Configure FSharp.SystemTextJson Serialization
<!-- SECTION:NOTES:END -->
