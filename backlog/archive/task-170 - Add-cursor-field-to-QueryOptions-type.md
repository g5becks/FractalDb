---
id: task-170
title: Add cursor field to QueryOptions type
status: Done
assignee: []
created_date: '2025-11-23 07:30'
updated_date: '2025-11-23 15:18'
labels:
  - query-options
  - phase-4
  - v0.3.0
dependencies:
  - task-169
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Update the `QueryOptions<T>` type in `src/query-options-types.ts` to include the new `cursor` optional field using the CursorSpec type.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Add `readonly cursor?: CursorSpec` to QueryOptions
- [ ] #2 TSDoc comment explaining cursor as alternative to skip for large datasets
- [ ] #3 Document that cursor requires sort to be specified
- [ ] #4 Update QueryOptions main TSDoc @example to show cursor pagination
- [ ] #5 Type checking passes with `bun run typecheck`
- [ ] #6 Linting passes with `bun run lint`
<!-- AC:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Added cursor?: CursorSpec to QueryOptions type. Combined with task-169 implementation.
<!-- SECTION:NOTES:END -->
