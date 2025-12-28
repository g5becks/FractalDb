---
id: task-159
title: Implement normalizeProjection helper in SQLiteCollection
status: Done
assignee:
  - '@claude'
created_date: '2025-11-23 07:27'
updated_date: '2025-11-23 08:02'
labels:
  - collection
  - phase-2
  - v0.3.0
dependencies:
  - task-158
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add a private `normalizeProjection` method to `src/sqlite-collection.ts` that converts `select` or `omit` arrays into `ProjectionSpec` format. Precedence: projection > select > omit.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 Create private method `normalizeProjection(options?: QueryOptions<T>): ProjectionSpec<T> | undefined`
- [x] #2 If options.projection exists, return it directly
- [x] #3 If options.select exists, convert to `{ field: 1 }` format
- [x] #4 If options.omit exists, convert to `{ field: 0 }` format
- [x] #5 Return undefined if none are set
- [x] #6 No type casting or use of 'any' - use proper generics
- [x] #7 Type checking passes with `bun run typecheck`
- [x] #8 Linting passes with `bun run lint`
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Read current sqlite-collection.ts to understand structure
2. Add private normalizeProjection method
3. Handle projection > select > omit precedence
4. Use proper type-safe conversion without any/casting
5. Run typecheck and lint
<!-- SECTION:PLAN:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Implemented normalizeProjection helper method in sqlite-collection.ts. Handles projection > select > omit precedence. Combined with task-160 implementation.
<!-- SECTION:NOTES:END -->
