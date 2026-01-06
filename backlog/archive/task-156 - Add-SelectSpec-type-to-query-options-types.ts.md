---
id: task-156
title: Add SelectSpec type to query-options-types.ts
status: Done
assignee:
  - '@claude'
created_date: '2025-11-23 07:27'
updated_date: '2025-11-23 07:53'
labels:
  - query-options
  - phase-2
  - v0.3.0
dependencies:
  - task-155
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add the `SelectSpec<T>` type to `src/query-options-types.ts` for array-based field inclusion. This provides a cleaner alternative to `projection: { name: 1, email: 1 }`.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 Create `SelectSpec<T> = readonly (keyof T)[]` type
- [x] #2 Full TSDoc with @typeParam, @remarks, and @example
- [x] #3 Example shows `{ select: ['name', 'email'] }` usage
- [x] #4 Export type from the file
- [x] #5 Type checking passes with `bun run typecheck`
- [x] #6 Linting passes with `bun run lint`
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Read current query-options-types.ts to understand existing types
2. Add SelectSpec<T> type with full TSDoc
3. Export the type
4. Run typecheck and lint to verify
<!-- SECTION:PLAN:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Added `SelectSpec<T>` type to `src/query-options-types.ts`:
- Type: `readonly (keyof T)[]` for array-based field selection
- Full TSDoc with @typeParam, @remarks explaining usage patterns
- @example showing `{ select: ['name', 'email'] }` usage patterns
- Exported from `src/index.ts`

All acceptance criteria verified:
- Type checking passes
- Linting passes
<!-- SECTION:NOTES:END -->
