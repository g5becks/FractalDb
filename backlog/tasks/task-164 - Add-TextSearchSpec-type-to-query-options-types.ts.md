---
id: task-164
title: Add TextSearchSpec type to query-options-types.ts
status: Done
assignee:
  - '@claude'
created_date: '2025-11-23 07:29'
updated_date: '2025-11-23 15:11'
labels:
  - query-options
  - phase-3
  - v0.3.0
dependencies:
  - task-163
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add the `TextSearchSpec<T>` type to `src/query-options-types.ts` for multi-field text search configuration. This type defines the search text, fields to search, and case sensitivity option.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 Create TextSearchSpec<T> type with `text: string`, `fields: readonly (keyof T | string)[]`, `caseSensitive?: boolean`
- [x] #2 Full TSDoc with @typeParam, @remarks explaining multi-field search
- [x] #3 @example showing search config with nested field paths
- [x] #4 Support dot notation in fields array for nested fields
- [x] #5 Export type from the file
- [x] #6 Type checking passes with `bun run typecheck`
- [x] #7 Linting passes with `bun run lint`
<!-- AC:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Added TextSearchSpec<T> type with text, fields, and caseSensitive properties. Full TSDoc with examples for multi-field search.
<!-- SECTION:NOTES:END -->
