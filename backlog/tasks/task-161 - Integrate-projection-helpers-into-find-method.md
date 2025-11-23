---
id: task-161
title: Integrate projection helpers into find method
status: Done
assignee: []
created_date: '2025-11-23 07:27'
updated_date: '2025-11-23 08:02'
labels:
  - collection
  - phase-2
  - v0.3.0
dependencies:
  - task-160
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Update the `find` method in `src/sqlite-collection.ts` to use `normalizeProjection` and `applyProjection` to process select/omit options before returning results.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Call normalizeProjection(options) in find method
- [ ] #2 If projection is returned, call applyProjection on results
- [ ] #3 Projection is applied after all other processing (sort, limit, skip)
- [ ] #4 Existing projection option still works unchanged
- [ ] #5 Type checking passes with `bun run typecheck`
- [ ] #6 Linting passes with `bun run lint`
<!-- AC:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Updated find method to call normalizeProjection and applyProjection to apply select/omit/projection options to query results.
<!-- SECTION:NOTES:END -->
