---
id: task-111
title: Implement QueryTranslator.extractPropertyName helper
status: To Do
assignee:
  - '@assistant'
created_date: '2025-12-29 06:10'
updated_date: '2025-12-29 17:00'
labels:
  - query-expressions
  - translator
dependencies:
  - task-104
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Implement the extractPropertyName helper function that extracts property names from quotation expressions. Handles PropertyGet and Lambda patterns, converts PascalCase to camelCase.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 extractPropertyName function defined in QueryTranslator module
- [x] #2 Handles PropertyGet pattern
- [x] #3 Handles Lambda wrapping
- [x] #4 Converts property names to camelCase
- [x] #5 Code builds with no errors or warnings
- [x] #6 All existing tests pass

- [x] #7 XML doc comments on extractPropertyName with summary and remarks on pattern matching
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Research F# quotation patterns (Expr module) for PropertyGet and Lambda
2. Add QueryTranslator module after TranslatedQuery type in QueryExpr.fs
3. Implement extractPropertyName function with pattern matching:
   - Handle Lambda expr wrapping (strip outer lambda)
   - Handle PropertyGet pattern (extract property info)
   - Add PascalCase to camelCase conversion helper
   - Handle edge cases (nested properties with dot notation)
4. Add comprehensive XML doc comments
5. Build project and verify no errors
6. Run existing tests to ensure no regressions
<!-- SECTION:PLAN:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Implemented QueryTranslator module with extractPropertyName helper function in QueryExpr.fs.

## Implementation Details:

**Module Location**: Added internal QueryTranslator module after TranslatedQuery type (line 230-390)

**Functions Implemented**:

1. **toCamelCase** (private helper):
   - Converts PascalCase to camelCase for JSON field names
   - Handles edge cases: empty strings, single chars, already camelCase
   - Example: "CreatedAt" → "createdAt"

2. **extractPropertyName** (main function):
   - Recursive pattern matching on F# Expr quotations
   - Handles 4 patterns:
     - Lambda wrappers: strips outer lambda, processes body
     - PropertyGet with receiver: extracts property chain with dot notation
     - Static PropertyGet: extracts static property names
     - Var reference: returns empty string (identity projection)
   - Throws ArgumentException for invalid patterns
   - Supports nested properties: "profile.email"

**Key Design Decisions**:

- Used Microsoft.FSharp.Quotations.Patterns for pattern matching
- Recursive implementation handles arbitrary nesting depth
- Empty string return for Var pattern signals identity selection
- PascalCase → camelCase conversion matches JSON serialization

**Documentation**:

- Comprehensive XML doc comments (~130 lines)
- Detailed remarks explaining pattern matching strategy
- Multiple code examples for each supported pattern
- Edge case documentation

## Testing:

- Build: ✅ 0 errors, 0 warnings
- Tests: ✅ 221/227 passing (6 known ArrayOperator failures from task-127)
- No regressions introduced

## File Stats:

- QueryExpr.fs: 1,668 lines (+162 lines)
- New code: ~160 lines (including ~130 lines of XML docs)

Ready for task-112 (translatePredicate for comparison operators).
<!-- SECTION:NOTES:END -->
