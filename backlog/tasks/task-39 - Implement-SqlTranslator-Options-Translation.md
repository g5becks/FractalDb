---
id: task-39
title: Implement SqlTranslator - Options Translation
status: Done
assignee:
  - '@assistant'
created_date: '2025-12-28 06:36'
updated_date: '2025-12-28 18:36'
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
Add TranslateOptions method for query options in src/SqlTranslator.fs. Reference: FSHARP_PORT_DESIGN.md lines 1787-1822.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 Add public TranslateOptions(options: QueryOptions<'T>) : string * (string * obj) list
- [x] #2 Generate ORDER BY clause from Sort list with ASC/DESC
- [x] #3 Generate LIMIT @opt1 clause from Limit option
- [x] #4 Generate OFFSET @opt2 clause from Skip option
- [x] #5 Return combined SQL clauses and parameters
- [x] #6 Run 'dotnet build' - build succeeds

- [x] #7 All public types and functions have XML doc comments with <summary>, and public functions include <param>, <returns>, and <example> (see doc-2 for standards)

- [x] #8 Run 'task lint' - no errors or warnings
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Review QueryOptions and SortDirection definitions in Options.fs
2. Add public TranslateOptions method signature
3. Implement ORDER BY clause generation from Sort list
4. Handle SortDirection.Ascending and Descending
5. Implement LIMIT clause generation with parameter
6. Implement OFFSET clause generation with parameter
7. Combine clauses in correct SQL order (ORDER BY, LIMIT, OFFSET)
8. Return tuple of (SQL string, parameters list)
9. Add comprehensive XML documentation
10. Build and lint verification
<!-- SECTION:PLAN:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Implemented QueryOptions translation in src/SqlTranslator.fs:

- Added public TranslateOptions method returning (SQL string, parameters list)
- Implemented ORDER BY clause generation from Sort list:
  * Handles SortDirection.Ascending → "ASC"
  * Handles SortDirection.Descending → "DESC"
  * Resolves field names using ResolveField for indexed/non-indexed fields
  * Multi-field sort: ORDER BY field1 ASC, field2 DESC
- Implemented LIMIT clause generation with @optN parameters
- Implemented OFFSET clause generation with @optN parameters
- Separate parameter counter (@opt0, @opt1) avoids conflicts with WHERE params
- Clauses combined in correct SQL order: ORDER BY, LIMIT, OFFSET
- Returns empty string for no options

- Reduced file size from 1102 to 971 lines by trimming verbose documentation
- Maintained all required XML doc comments

Build: ✅ 0 errors, 0 warnings
Lint: ✅ 0 warnings
File size: 971 lines (under 1000 limit)

Ready for Task 40: Add unit tests for SqlTranslator
<!-- SECTION:NOTES:END -->
