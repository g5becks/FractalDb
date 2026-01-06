---
id: task-151
title: Add $contains operator to StringOperator type
status: Done
assignee:
  - '@claude'
created_date: '2025-11-23 07:26'
updated_date: '2025-11-23 07:47'
labels:
  - query-types
  - phase-1
  - v0.3.0
dependencies: []
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add the `$contains` operator to the `StringOperator` type in `src/query-types.ts`. This is syntactic sugar for `$like: '%value%'` - the user provides just the substring without wildcards.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 StringOperator type includes `readonly $contains?: string` field
- [x] #2 Full TSDoc comment explaining it wraps value with % wildcards
- [x] #3 TSDoc includes @example showing `{ email: { $contains: '@example' } }`
- [x] #4 Type checking passes with `bun run typecheck`
- [x] #5 Linting passes with `bun run lint`
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Read current StringOperator type in query-types.ts
2. Add $contains operator with full TSDoc including @remarks and @example
3. Explain that $contains is sugar for $like: '%value%'
4. Run typecheck and lint to verify
<!-- SECTION:PLAN:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Added `$contains` operator to `StringOperator` type in `src/query-types.ts` with:
- Full TSDoc documentation including @remarks explaining it's sugar for `$like: '%value%'`
- @example showing substring search patterns (description, email domain)
- Added example combining with other string operators
- Updated main StringOperator JSDoc to list $contains as a supported operator
- Added $contains example in the main StringOperator @example block

All acceptance criteria verified:
- $contains operator added as `readonly $contains?: string`
- TSDoc explains it wraps value with % wildcards
- TSDoc includes @example with practical usage patterns
- Type checking passes
- Linting passes
<!-- SECTION:NOTES:END -->
