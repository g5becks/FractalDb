---
id: task-168
title: Update README.md with query composition examples
status: Done
assignee:
  - '@assistant'
created_date: '2025-12-30 21:22'
updated_date: '2025-12-30 22:27'
labels:
  - docs
dependencies:
  - task-167
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Update README.md to document the query composition feature with the <+> operator.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 README documents the <+> operator for query composition
- [x] #2 Example shows combining filter, sort, and paging queries
- [x] #3 QueryOps.compose pipeline function documented
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Review the <+> operator implementation in QueryExpr.fs (line 523-524)
2. Review QueryOps module functions (where, orderBy, skip, limit)
3. Add a new "Query Composition" section in README.md after line 133
4. Document the <+> operator with examples
5. Document QueryOps pipeline functions for composition
6. Show example combining filter, sort, and paging queries using both styles
7. Verify markdown formatting is correct
<!-- SECTION:PLAN:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Added comprehensive query composition documentation to README.md.

**Changes Made:**

1. **Enhanced Quick Start example (lines 100-133):**
   - Added Option 1: <+> Operator with reusable query parts example
   - Reorganized existing Fluent API as Option 2
   - Reorganized existing Pipeline API as Option 3

2. **New "Query Composition" section (after Query Expressions):**
   - Documented all three composition styles with detailed examples
   - Style 1: <+> operator for combining separate queries
   - Style 2: Fluent API with method chaining
   - Style 3: QueryOps pipeline with |> operators
   - Added guidance on when to use each style

3. **QueryOps Functions table:**
   - Documented where, orderBy, skip, limit functions
   - Included full function signatures
   - Described behavior of each function

4. **Composition Rules section:**
   - Where clauses combine with AND
   - OrderBy clauses append for secondary sorting
   - Skip/Take precedence (last wins)
   - Projection precedence (last wins)

**Key Examples:**
- Reusable query components with <+> operator
- Combining filter, sort, and paging queries
- All three composition styles demonstrated

**Verification:**
- Build succeeded with 0 warnings
- Markdown formatting verified
- Examples align with actual implementation in QueryExpr.fs
<!-- SECTION:NOTES:END -->
