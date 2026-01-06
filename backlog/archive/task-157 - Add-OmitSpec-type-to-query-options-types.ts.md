---
id: task-157
title: Add OmitSpec type to query-options-types.ts
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
  - task-156
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add the `OmitSpec<T>` type to `src/query-options-types.ts` for array-based field exclusion. This provides a cleaner alternative to `projection: { password: 0 }`.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 Create `OmitSpec<T> = readonly (keyof T)[]` type
- [x] #2 Full TSDoc with @typeParam, @remarks, and @example
- [x] #3 Example shows `{ omit: ['password', 'ssn'] }` usage
- [x] #4 Export type from the file
- [x] #5 Type checking passes with `bun run typecheck`
- [x] #6 Linting passes with `bun run lint`
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Add OmitSpec<T> type after SelectSpec<T>
2. Add full TSDoc with @typeParam, @remarks, and @example
3. Export from index.ts
4. Run typecheck and lint
<!-- SECTION:PLAN:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Added `OmitSpec<T>` type to `src/query-options-types.ts`:
- Type: `readonly (keyof T)[]` for array-based field exclusion
- Full TSDoc with @typeParam, @remarks explaining usage patterns
- @example showing `{ omit: ['password', 'ssn'] }` usage patterns
- Exported from `src/index.ts`

All acceptance criteria verified:
- Type checking passes
- Linting passes
<!-- SECTION:NOTES:END -->
