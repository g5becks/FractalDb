---
id: task-169
title: Add CursorSpec type to query-options-types.ts
status: Done
assignee:
  - '@claude'
created_date: '2025-11-23 07:30'
updated_date: '2025-11-23 15:18'
labels:
  - query-options
  - phase-4
  - v0.3.0
dependencies:
  - task-168
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add the `CursorSpec` type to `src/query-options-types.ts` for cursor-based pagination. This type defines the cursor position using document ID and sort field value for efficient pagination without skip.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 Create CursorSpec type with `after?: string`, `before?: string`, `sortField: string`, `sortValue: unknown`
- [x] #2 Full TSDoc with @remarks explaining cursor pagination benefits over skip/limit
- [x] #3 @example showing cursor usage for forward pagination
- [x] #4 @example showing cursor usage for backward pagination
- [x] #5 Document that sort option is required when using cursor
- [x] #6 Export type from the file
- [x] #7 Type checking passes with `bun run typecheck`
- [x] #8 Linting passes with `bun run lint`
<!-- AC:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Added CursorSpec type with after/before cursor properties. Full TSDoc with examples for forward and backward pagination.
<!-- SECTION:NOTES:END -->
