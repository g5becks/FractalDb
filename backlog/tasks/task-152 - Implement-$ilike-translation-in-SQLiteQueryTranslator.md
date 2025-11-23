---
id: task-152
title: Implement $ilike translation in SQLiteQueryTranslator
status: Done
assignee:
  - '@claude'
created_date: '2025-11-23 07:26'
updated_date: '2025-11-23 07:47'
labels:
  - query-translator
  - phase-1
  - v0.3.0
dependencies:
  - task-150
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add the SQL translation for the `$ilike` operator in `src/sqlite-query-translator.ts`. The operator should translate to `LIKE ? COLLATE NOCASE` for case-insensitive matching in SQLite.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 Add case '$ilike' to translateSingleOperator method
- [x] #2 Translation produces `${fieldSql} LIKE ? COLLATE NOCASE`
- [x] #3 Parameter value is pushed to params array using toBindValue()
- [x] #4 No type casting or use of 'any'
- [x] #5 Type checking passes with `bun run typecheck`
- [x] #6 Linting passes with `bun run lint`
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Read current SQLiteQueryTranslator translateSingleOperator method
2. Add case for '$ilike' operator using LIKE with COLLATE NOCASE
3. Add TSDoc comment explaining the translation
4. Run typecheck and lint to verify
<!-- SECTION:PLAN:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Added `$ilike` case to `translateSingleOperator` method in `src/sqlite-query-translator.ts`:
- Translates to `${fieldSql} LIKE ? COLLATE NOCASE` for case-insensitive matching
- Parameter value pushed using `toBindValue()` as with other operators
- Added comment explaining the COLLATE NOCASE behavior
- Updated class TSDoc @example block to show $ilike translation

All acceptance criteria verified:
- $ilike case added to translateSingleOperator
- Translation produces `${fieldSql} LIKE ? COLLATE NOCASE`
- Parameter value pushed using toBindValue()
- No type casting or use of 'any'
- Type checking passes
- Linting passes
<!-- SECTION:NOTES:END -->
