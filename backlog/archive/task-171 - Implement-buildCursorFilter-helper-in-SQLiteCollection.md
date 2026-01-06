---
id: task-171
title: Implement buildCursorFilter helper in SQLiteCollection
status: Done
assignee: []
created_date: '2025-11-23 07:30'
updated_date: '2025-11-23 15:18'
labels:
  - collection
  - phase-4
  - v0.3.0
dependencies:
  - task-170
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add a private `buildCursorFilter` method to `src/sqlite-collection.ts` that converts a CursorSpec into a QueryFilter for cursor-based pagination. Must handle both ascending and descending sort directions and tie-breaking with _id.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Create private method `buildCursorFilter(cursor: CursorSpec, sort: SortSpec<T>): QueryFilter<T>`
- [ ] #2 For 'after' cursor with ascending sort: use $gt on sort field
- [ ] #3 For 'after' cursor with descending sort: use $lt on sort field
- [ ] #4 Handle tie-breaking: (sortField > value) OR (sortField = value AND _id > cursorId)
- [ ] #5 For 'before' cursor: reverse the comparison operators
- [ ] #6 Return empty filter if neither after nor before is set
- [ ] #7 No type casting or use of 'any'
- [ ] #8 Type checking passes with `bun run typecheck`
- [ ] #9 Linting passes with `bun run lint`
<!-- AC:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Implemented buildCursorClause helper method in sqlite-collection.ts. Uses compound comparison (sortValue, _id) for stable pagination with duplicate sort values.
<!-- SECTION:NOTES:END -->
