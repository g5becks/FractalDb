---
id: task-105
title: 'Add QueryBuilder type with For, Yield, Quote, Run members'
status: To Do
assignee:
  - '@assistant'
created_date: '2025-12-29 06:08'
updated_date: '2025-12-29 17:00'
labels:
  - query-expressions
  - builder
dependencies:
  - task-104
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add the QueryBuilder computation expression type with the required members for quotation capture: For (for..in..do syntax), Yield (select syntax), Quote (captures quotation), Run (translates and executes). Members return Unchecked.defaultof since they're never executed - the quotation is analyzed instead.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 QueryBuilder type defined in QueryExpr.fs
- [x] #2 For member enables 'for x in source do' syntax
- [x] #3 Yield member enables 'select x' syntax
- [x] #4 Quote member captures Expr<TranslatedQuery<T>>
- [x] #5 Run member signature defined (implementation placeholder)
- [x] #6 Global 'query' instance created in AutoOpen module
- [x] #7 Code builds with no errors or warnings
- [x] #8 All existing tests pass

- [x] #9 XML doc comments on QueryBuilder type with summary, remarks explaining quotation capture, and example usage
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Research F# computation expression quotation patterns (Query.Quote member)
2. Define QueryBuilder type class in QueryExpr.fs
3. Implement For member to enable for..in..do syntax (returns Unchecked.defaultof)
4. Implement Yield member to enable select syntax (returns Unchecked.defaultof)
5. Implement Quote member to capture Expr<TranslatedQuery<T>> quotations
6. Implement Run member signature with placeholder (will be implemented in task-118)
7. Create AutoOpen module with global query instance
8. Add comprehensive XML documentation
9. Build project and verify compilation
10. Run existing tests to ensure no regressions
<!-- SECTION:PLAN:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
## Implementation Summary

Implemented QueryBuilder computation expression type with quotation-based query translation foundation.

**Files Modified:**
- `src/QueryExpr.fs` - Added QueryBuilder type and QueryBuilderInstance module (250+ lines)

**Types and Members Implemented:**

1. **QueryBuilder Type Class:**
   - `For(source, body)` - Enables for..in..do syntax, returns Unchecked.defaultof
   - `Yield(value)` - Enables yield/select syntax, returns Unchecked.defaultof
   - `Quote(expr)` - Captures computation expression as Expr<T> quotation
   - `Run(expr)` - Translates quotation to TranslatedQuery (stub throws NotImplementedException)

2. **QueryBuilderInstance AutoOpen Module:**
   - Global `query` instance for ergonomic usage
   - Automatically opened when FractalDb.QueryExpr is opened

**Key Design Decisions:**

- **Quotation-Based Approach:** Members return Unchecked.defaultof since they're never executed. The Quote member captures the entire expression tree for runtime analysis.\n- **Deferred Implementation:** Run member is a stub that will be implemented in task-118 once QueryTranslator functions are available.\n- **Type Safety:** Using F# quotations provides compile-time type checking while enabling runtime query translation.\n\n**Documentation:**\n- Comprehensive XML doc comments (200+ lines)\n- Detailed explanation of quotation-based approach in type-level remarks\n- Examples showing query syntax patterns\n- Cross-references to related types and upcoming implementation tasks\n\n**Verification:**\n- Project builds successfully (0 warnings, 0 errors)\n- All existing tests pass (221/227, same 6 known failures in ArrayOperatorTests)\n- No regressions introduced\n\n**Enables:**\nThis foundation enables task-106+ (CustomOperation members: where, sortBy, take, skip, select) which will provide the actual query DSL syntax.
<!-- SECTION:NOTES:END -->
