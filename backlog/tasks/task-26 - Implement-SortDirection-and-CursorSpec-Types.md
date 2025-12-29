---
id: task-26
title: Implement SortDirection and CursorSpec Types
status: Done
assignee:
  - '@assistant'
created_date: '2025-12-28 06:33'
updated_date: '2025-12-28 18:04'
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
- [x] #1 Add namespace FractalDb.Core
- [x] #2 Define SortDirection DU with cases: Ascending, Descending
- [x] #3 Add CursorSpec record with: After: string option, Before: string option
- [x] #4 Run 'dotnet build' - build succeeds
- [x] #5 All public types and functions have XML doc comments with <summary>, and public functions include <param>, <returns>, and <example> (see doc-2 for standards)
- [x] #6 Run 'task lint' - no errors or warnings

- [x] #7 Create file src/Options.fs

- [x] #8 Add module declaration: module FractalDb.Options
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Create new file src/Options.fs with module declaration
2. Add SortDirection DU with RequireQualifiedAccess attribute
3. Add CursorSpec record type
4. Add comprehensive XML documentation for all types
5. Update src/FractalDb.fsproj to include Options.fs (after Schema.fs)
6. Run dotnet build to verify
7. Run task lint to verify
8. Mark all ACs complete
<!-- SECTION:PLAN:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Created new file src/Options.fs with SortDirection DU and CursorSpec record.

Implementation details:
- Created src/Options.fs with module declaration: module FractalDb.Options
- Added SortDirection DU with [<RequireQualifiedAccess>] attribute and two cases: Ascending, Descending
- Added CursorSpec record with two option<string> fields: After and Before
- Used prefix syntax for generics: option<string> (not string option)
- Comprehensive XML documentation for all types and cases with <summary>, <remarks>, and <example> sections
- Documentation explains cursor-based pagination advantages over offset-based pagination
- Included multiple usage examples for sort directions and cursor pagination flows
- Updated src/FractalDb.fsproj to include Options.fs after Schema.fs in compilation order

Verification:
- dotnet build: Success (0 warnings, 0 errors)
- task lint: Success (0 warnings)
- File size: 212 lines (well under 1000 line limit)

Ready for next task: Task 27 - TextSearchSpec and QueryOptions Types
<!-- SECTION:NOTES:END -->
