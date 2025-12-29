---
id: task-111
title: Implement QueryTranslator.extractPropertyName helper
status: Done
assignee:
  - '@assistant'
created_date: '2025-12-29 06:10'
updated_date: '2025-12-29 17:20'
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
Implemented QueryTranslator internal module with extractPropertyName and toCamelCase helper functions. Supports 4 quotation patterns: Lambda wrappers, PropertyGet with receiver (nested fields), static PropertyGet, Var identity. Handles dot notation for nested properties. Added 210+ lines comprehensive XML documentation. Build successful 0 errors/warnings.
<!-- SECTION:NOTES:END -->
