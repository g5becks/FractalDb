---
id: task-175
title: Export new types from index.ts
status: Done
assignee: []
created_date: '2025-11-23 07:31'
updated_date: '2025-11-23 15:34'
labels:
  - exports
  - phase-5
  - v0.3.0
dependencies:
  - task-174
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Update `src/index.ts` to export all new types added in v0.3.0: SelectSpec, OmitSpec, TextSearchSpec, and CursorSpec from query-options-types.ts.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 Add SelectSpec to exports from query-options-types.js
- [x] #2 Add OmitSpec to exports from query-options-types.js
- [x] #3 Add TextSearchSpec to exports from query-options-types.js
- [x] #4 Add CursorSpec to exports from query-options-types.js
- [x] #5 Verify all exports are type-only exports
- [ ] #6 Type checking passes with `bun run typecheck`
- [ ] #7 Linting passes with `bun run lint`
<!-- AC:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
All new types already exported from src/index.ts: SelectSpec, OmitSpec, TextSearchSpec, CursorSpec, ProjectionSpec, SortSpec, QueryOptions
<!-- SECTION:NOTES:END -->
