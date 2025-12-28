---
id: task-167
title: Integrate text search into find method
status: Done
assignee: []
created_date: '2025-11-23 07:29'
updated_date: '2025-11-23 15:11'
labels:
  - collection
  - phase-3
  - v0.3.0
dependencies:
  - task-166
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Update the `find` method in `src/sqlite-collection.ts` to process the `search` option by merging the search filter with the existing filter using $and when both are present.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Check for options.search in find method
- [ ] #2 Call buildSearchFilter when search is provided
- [ ] #3 If filter is empty, use search filter directly
- [ ] #4 If filter has conditions, wrap both in $and
- [ ] #5 Search is applied before translation to SQL
- [ ] #6 Type checking passes with `bun run typecheck`
- [ ] #7 Linting passes with `bun run lint`
<!-- AC:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Integrated buildSearchClause into find method. Search clause is combined with filter using AND logic. Also extracted buildSortClause helper to reduce cognitive complexity.
<!-- SECTION:NOTES:END -->
