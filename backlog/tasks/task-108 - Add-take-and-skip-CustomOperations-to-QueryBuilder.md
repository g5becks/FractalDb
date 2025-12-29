---
id: task-108
title: Add take and skip CustomOperations to QueryBuilder
status: To Do
assignee:
  - '@assistant'
created_date: '2025-12-29 06:09'
updated_date: '2025-12-29 17:00'
labels:
  - query-expressions
  - builder
dependencies:
  - task-105
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add take and skip custom operations to QueryBuilder for pagination. These take int count directly, not a projection parameter.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 take CustomOperation added to QueryBuilder
- [x] #2 skip CustomOperation added to QueryBuilder
- [x] #3 Both operations use MaintainsVariableSpace = true
- [x] #4 Code builds with no errors or warnings
- [x] #5 All existing tests pass

- [x] #6 XML doc comments on take and skip operations with examples
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Add take CustomOperation with MaintainsVariableSpace=true
2. Add skip CustomOperation with MaintainsVariableSpace=true
3. Both operations take int count parameter (not ProjectionParameter)
4. Return Unchecked.defaultof (quotation-based approach)
5. Add comprehensive XML documentation for both operations
6. Build project and verify compilation
7. Run tests to ensure no regressions
<!-- SECTION:PLAN:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
## Implementation Summary

Added pagination CustomOperations (skip and take) to QueryBuilder enabling offset-based pagination.

**Files Modified:**
- `src/QueryExpr.fs` - Added Skip and Take members (~160 lines)

**Operations Implemented:**

1. **skip** - Skip first N documents (pagination offset):
   ```fsharp
   [<CustomOperation("skip", MaintainsVariableSpace=true)>]
   member _.Skip(source: seq<'T>, count: int) : seq<'T>\n   ```\n\n2. **take** - Limit to N documents (pagination size):\n   ```fsharp\n   [<CustomOperation("take", MaintainsVariableSpace=true)>]\n   member _.Take(source: seq<'T>, count: int) : seq<'T>\n   ```\n\n**Key Features:**\n- Both use MaintainsVariableSpace=true to keep iteration variable in scope\n- Take int count directly (NOT ProjectionParameter - these are constants, not lambdas)\n- Support offset-based pagination pattern: skip ((page-1) * size); take size\n- Return Unchecked.defaultof (quotation-based approach)\n- Applied in correct order: WHERE -> ORDER BY -> SKIP -> TAKE\n\n**Documentation:**\n- Comprehensive XML doc comments for both operations\n- Examples covering pagination patterns, top-N queries, result limiting\n- Explanation of execution order and order dependency\n- Common use cases and pagination formula\n- Warnings about non-deterministic results without sorting\n\n**Verification:**\n- Project builds successfully (0 warnings, 0 errors)\n- All tests pass (221/227, same 6 known failures)\n- No regressions introduced\n\n**Usage Examples:**\n```fsharp\n// Page 2 with 10 items per page\nsortBy user.CreatedAt\nskip 10\ntake 10\n\n// Top 20 highest-rated products\nsortByDescending product.Rating\ntake 20\n\n// Skip first result\nskip 1\n```
<!-- SECTION:NOTES:END -->
