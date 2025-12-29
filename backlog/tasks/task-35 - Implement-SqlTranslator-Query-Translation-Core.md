---
id: task-35
title: Implement SqlTranslator - Query Translation Core
status: Done
assignee:
  - '@assistant'
created_date: '2025-12-28 06:35'
updated_date: '2025-12-28 18:25'
labels:
  - phase-2
  - storage
  - translator
dependencies:
  - task-34
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Implement TranslateQuery method for logical operators in src/SqlTranslator.fs. Reference: FSHARP_PORT_DESIGN.md lines 1635-1665.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 Add private mutable paramCounter for generating @p1, @p2, etc.
- [x] #2 Add private nextParam() function incrementing counter and returning param name
- [x] #3 Add private TranslateQuery method taking Query<'T> and returning TranslatorResult
- [x] #4 Handle Query.Empty - return TranslatorResult.empty (SQL: '1=1')
- [x] #5 Handle Query.Field - call TranslateFieldOp (stub for now)
- [x] #6 Handle Query.And - combine results with ' AND ', wrap in parentheses
- [x] #7 Handle Query.Or - combine results with ' OR ', wrap in parentheses
- [x] #8 Handle Query.Nor - combine with OR, wrap with 'NOT (...)'
- [x] #9 Handle Query.Not - wrap single result with 'NOT (...)'
- [x] #10 Run 'dotnet build' - build succeeds

- [x] #11 All public types and functions have XML doc comments with <summary>, and public functions include <param>, <returns>, and <example> (see doc-2 for standards)

- [x] #12 Run 'task lint' - no errors or warnings
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Add mutable paramCounter field to SqlTranslator class
2. Add private nextParam() method to generate @p0, @p1, etc.
3. Implement private TranslateQuery method with pattern matching
4. Handle Query.Empty case (return TranslatorResult.empty)
5. Handle Query.Field case (stub call to TranslateFieldOp)
6. Handle Query.And - combine results with AND logic
7. Handle Query.Or - combine results with OR logic
8. Handle Query.Nor - combine with OR then wrap in NOT
9. Handle Query.Not - wrap result in NOT
10. Update public Translate method to use TranslateQuery
11. Add comprehensive XML documentation
12. Build and lint verification
<!-- SECTION:PLAN:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Implemented complete query translation core in src/SqlTranslator.fs:

- Added mutable paramCounter field to track parameter generation
- Implemented NextParam() method for generating @p0, @p1, etc.
- Implemented TranslateQuery method with full pattern matching:
  * Query.Empty → returns "1=1" (matches all)
  * Query.Field → resolves field and delegates to TranslateFieldOp stub
  * Query.And → combines sub-queries with AND, wrapped in parentheses
  * Query.Or → combines sub-queries with OR, wrapped in parentheses  
  * Query.Nor → combines with OR then wraps in NOT
  * Query.Not → wraps single query in NOT
- Updated public Translate() method to reset paramCounter and call TranslateQuery
- Added TranslateFieldOp stub for Task 36
- All logical operators properly combine SQL fragments and flatten parameters
- Comprehensive XML documentation added for all methods

Build: ✅ 0 errors, 0 warnings
Lint: ✅ 0 warnings
File size: 395 lines (under 1000 limit)

Ready for Task 36: Implement TranslateFieldOp for comparison operators
<!-- SECTION:NOTES:END -->
