---
id: task-158
title: Add select and omit fields to QueryOptions type
status: Done
assignee:
  - '@claude'
created_date: '2025-11-23 07:27'
updated_date: '2025-11-23 07:54'
labels:
  - query-options
  - phase-2
  - v0.3.0
dependencies:
  - task-157
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Update the `QueryOptions<T>` type in `src/query-options-types.ts` to include the new `select` and `omit` optional fields using the SelectSpec and OmitSpec types.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 Add `readonly select?: SelectSpec<T>` to QueryOptions
- [x] #2 Add `readonly omit?: OmitSpec<T>` to QueryOptions
- [x] #3 TSDoc comments on each field explaining usage
- [x] #4 Update QueryOptions main TSDoc @example to show select/omit usage
- [x] #5 Type checking passes with `bun run typecheck`
- [x] #6 Linting passes with `bun run lint`
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Add select and omit fields to QueryOptions type
2. Add TSDoc comments for each field
3. Update QueryOptions main TSDoc @example to show select/omit usage
4. Run typecheck and lint
<!-- SECTION:PLAN:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Updated `QueryOptions<T>` type in `src/query-options-types.ts`:
- Added `readonly select?: SelectSpec<T>` field
- Added `readonly omit?: OmitSpec<T>` field  
- Added TSDoc comments explaining mutual exclusivity with projection
- Updated main @remarks section to document all field selection options
- Added @example showing select and omit usage patterns

All acceptance criteria verified:
- Type checking passes
- Linting passes
<!-- SECTION:NOTES:END -->
