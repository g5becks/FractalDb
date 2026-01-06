---
id: task-172
title: Integrate cursor pagination into find method
status: Done
assignee: []
created_date: '2025-11-23 07:30'
updated_date: '2025-11-23 15:18'
labels:
  - collection
  - phase-4
  - v0.3.0
dependencies:
  - task-171
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Update the `find` method in `src/sqlite-collection.ts` to process the `cursor` option by merging the cursor filter with the existing filter. Cursor requires sort to be specified.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Check for options.cursor in find method
- [ ] #2 Only process cursor if options.sort is also provided
- [ ] #3 Call buildCursorFilter when cursor and sort are provided
- [ ] #4 Merge cursor filter with existing filter using $and
- [ ] #5 Cursor filter is applied before translation to SQL
- [ ] #6 Type checking passes with `bun run typecheck`
- [ ] #7 Linting passes with `bun run lint`
<!-- AC:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Integrated cursor pagination into find method. Cursor requires sort to be provided. Generates SQL WHERE clause for efficient cursor-based pagination.
<!-- SECTION:NOTES:END -->
