---
id: task-165
title: Add search field to QueryOptions type
status: Done
assignee: []
created_date: '2025-11-23 07:29'
updated_date: '2025-11-23 15:11'
labels:
  - query-options
  - phase-3
  - v0.3.0
dependencies:
  - task-164
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Update the `QueryOptions<T>` type in `src/query-options-types.ts` to include the new `search` optional field using the TextSearchSpec type.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Add `readonly search?: TextSearchSpec<T>` to QueryOptions
- [ ] #2 TSDoc comment explaining multi-field text search behavior
- [ ] #3 Update QueryOptions main TSDoc @example to show search usage
- [ ] #4 Type checking passes with `bun run typecheck`
- [ ] #5 Linting passes with `bun run lint`
<!-- AC:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Added search field to QueryOptions<T> type. Combined with task-164 implementation.
<!-- SECTION:NOTES:END -->
