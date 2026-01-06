---
id: task-166
title: Implement buildSearchFilter helper in SQLiteCollection
status: Done
assignee: []
created_date: '2025-11-23 07:29'
updated_date: '2025-11-23 15:11'
labels:
  - collection
  - phase-3
  - v0.3.0
dependencies:
  - task-165
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add a private `buildSearchFilter` method to `src/sqlite-collection.ts` that converts a TextSearchSpec into a QueryFilter with $or conditions across all specified fields.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Create private method `buildSearchFilter(search: TextSearchSpec<T>): QueryFilter<T>`
- [ ] #2 Use $ilike operator when caseSensitive is false (default)
- [ ] #3 Use $like operator when caseSensitive is true
- [ ] #4 Build $or array with one condition per field
- [ ] #5 Each condition uses `%${search.text}%` pattern
- [ ] #6 Support dot notation field paths for nested fields
- [ ] #7 No type casting or use of 'any'
- [ ] #8 Type checking passes with `bun run typecheck`
- [ ] #9 Linting passes with `bun run lint`
<!-- AC:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Implemented buildSearchClause helper method in sqlite-collection.ts. Generates OR-connected LIKE clauses for each field with optional COLLATE NOCASE.
<!-- SECTION:NOTES:END -->
