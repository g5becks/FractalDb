---
id: task-170
title: Update LLMS.txt with composition API
status: Done
assignee:
  - '@assistant'
created_date: '2025-12-30 21:23'
updated_date: '2025-12-30 22:29'
labels:
  - docs
dependencies:
  - task-169
priority: medium
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Update LLMS.txt to document the query composition API with <+> operator.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 Composition documented
- [x] #2 <+> operator documented in LLMS.txt

- [x] #3 QueryOps.compose documented in LLMS.txt
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Review LLMS.txt structure and find query-related sections
2. Locate existing query expression documentation
3. Add query composition subsection documenting <+> operator
4. Document QueryOps module functions (where, orderBy, skip, limit)
5. Add examples showing all three composition styles
6. Update the grepable patterns if needed
7. Verify formatting matches existing LLMS.txt style
<!-- SECTION:PLAN:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Added comprehensive query composition documentation to LLMS.txt.

**Changes Made:**

1. **New QUERY COMPOSITION section:**
   - Added complete section after QUERY EXPRESSIONS
   - Documented all three composition styles
   - Included grepable patterns for AI agents

2. **Style 1: <+> Operator Documentation:**
   - Declarative composition examples
   - Reusable query components pattern
   - Conditional composition example
   - Use cases listed

3. **Style 2: Fluent API Documentation:**
   - Moved from COLLECTION OPERATIONS to QUERY COMPOSITION
   - Added method chaining examples
   - Documented where, orderBy, skip, limit, exec methods

4. **Style 3: QueryOps Pipeline Documentation:**
   - Complete function signatures
   - Pipeline composition examples
   - Custom transformation helpers (addAdminFilter, addPagination)
   - Integration with F# pipelines

5. **Composition Rules:**
   - RULE 1: Where clauses AND logic
   - RULE 2: OrderBy appending
   - RULE 3: Skip/Take precedence
   - RULE 4: Projection precedence
   - RULE 5: Mixed style composition

6. **Style Comparison Table:**
   - When to use each style
   - Advantages of each approach
   - Recommendations

7. **Updated Grepable Patterns:**
   - Added "^OPERATOR:" pattern
   - Added "^RULE:" pattern
   - Added composition search example

**Format:**
- Consistent with existing LLMS.txt structure
- Clear section headers with ===
- Grepable keywords (OPERATOR, FUNCTION, RULE)
- Code examples for all patterns

**Verification:**
- Build succeeded with 0 warnings
- All patterns follow LLMS.txt conventions
<!-- SECTION:NOTES:END -->
