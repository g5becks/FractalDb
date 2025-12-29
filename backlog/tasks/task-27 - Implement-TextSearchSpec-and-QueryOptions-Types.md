---
id: task-27
title: Implement TextSearchSpec and QueryOptions Types
status: Done
assignee:
  - '@assistant'
created_date: '2025-12-28 06:33'
updated_date: '2025-12-28 18:05'
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
- [x] #1 TextSearchSpec has: Text: string, Fields: string list, CaseSensitive: bool
- [x] #2 Add QueryOptions<'T> record
- [x] #3 QueryOptions has: Sort: (string * SortDirection) list, Limit: int option, Skip: int option, Select: string list option, Omit: string list option, Search: TextSearchSpec option, Cursor: CursorSpec option
- [x] #4 Run 'dotnet build' - build succeeds
- [x] #5 All public types and functions have XML doc comments with <summary>, and public functions include <param>, <returns>, and <example> (see doc-2 for standards)

- [x] #6 Run 'task lint' - no errors or warnings

- [x] #7 In src/Options.fs, add TextSearchSpec record
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Read design spec lines 761-775 for TextSearchSpec and QueryOptions
2. Add TextSearchSpec record to src/Options.fs
3. Add QueryOptions<'T> generic record to src/Options.fs
4. Add comprehensive XML documentation for both types and all fields
5. Run dotnet build to verify
6. Run task lint to verify
7. Mark all ACs complete
<!-- SECTION:PLAN:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Added TextSearchSpec and QueryOptions<'T> records to src/Options.fs.

Implementation details:
- Added TextSearchSpec record with fields: Text (string), Fields (list<string>), CaseSensitive (bool)
- Added QueryOptions<'T> generic record with 7 fields:
  * Sort: list<(string * SortDirection)>
  * Limit: option<int>
  * Skip: option<int>
  * Select: option<list<string>>
  * Omit: option<list<string>>
  * Search: option<TextSearchSpec>
  * Cursor: option<CursorSpec>
- Used prefix syntax for all generic types
- Comprehensive XML documentation for both types with detailed <summary>, <remarks>, and <example> sections
- Documentation explains execution order (filter → sort → skip/cursor → limit → projection)
- Documented mutually exclusive options: Select/Omit and Skip/Cursor
- Included 8 detailed examples covering various query scenarios
- Explained performance considerations for Skip vs Cursor pagination

Verification:
- dotnet build: Success (0 warnings, 0 errors)
- task lint: Success (0 warnings)
- File size: 641 lines (well under 1000 line limit)

Ready for next task: Task 28 - QueryOptions Module Functions
<!-- SECTION:NOTES:END -->
