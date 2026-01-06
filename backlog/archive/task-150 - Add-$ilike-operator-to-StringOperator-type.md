---
id: task-150
title: Add $ilike operator to StringOperator type
status: Done
assignee:
  - '@claude'
created_date: '2025-11-23 07:26'
updated_date: '2025-11-23 07:44'
labels:
  - query-types
  - phase-1
  - v0.3.0
dependencies: []
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add the `$ilike` operator to the `StringOperator` type in `src/query-types.ts` for case-insensitive LIKE pattern matching. This is the type definition only - translation will be added in a separate task.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 StringOperator type includes `readonly $ilike?: string` field
- [x] #2 Full TSDoc comment with @remarks explaining case-insensitivity
- [x] #3 TSDoc includes @example showing usage like `{ name: { $ilike: '%alice%' } }`
- [x] #4 Type checking passes with `bun run typecheck`
- [x] #5 Linting passes with `bun run lint`
<!-- AC:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Added `$ilike` operator to `StringOperator` type in `src/query-types.ts` with:
- Full TSDoc documentation including @remarks explaining COLLATE NOCASE behavior
- @example showing case-insensitive name and email domain search patterns
- Updated main StringOperator JSDoc to list $ilike as a supported operator
- Uses same wildcard syntax as $like (% and _)

All acceptance criteria verified:
- $ilike operator added as `readonly $ilike?: string`
- TSDoc includes @remarks about SQLite COLLATE NOCASE
- TSDoc includes @example with practical usage patterns
- Type checking passes
- Linting passes
<!-- SECTION:NOTES:END -->
