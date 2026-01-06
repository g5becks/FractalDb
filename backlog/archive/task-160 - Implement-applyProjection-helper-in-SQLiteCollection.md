---
id: task-160
title: Implement applyProjection helper in SQLiteCollection
status: Done
assignee: []
created_date: '2025-11-23 07:27'
updated_date: '2025-11-23 08:02'
labels:
  - collection
  - phase-2
  - v0.3.0
dependencies:
  - task-159
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add a private `applyProjection` method to `src/sqlite-collection.ts` that filters document fields based on a ProjectionSpec. Include mode (1) keeps only specified fields; exclude mode (0) removes specified fields.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Create private method `applyProjection(docs: readonly T[], projection: ProjectionSpec<T>): readonly T[]`
- [ ] #2 Detect include mode by checking if any value is 1
- [ ] #3 In include mode: keep _id plus fields with value 1
- [ ] #4 In exclude mode: remove fields with value 0
- [ ] #5 Always preserve _id unless explicitly excluded
- [ ] #6 Return properly typed array
- [ ] #7 No type casting or use of 'any'
- [ ] #8 Type checking passes with `bun run typecheck`
- [ ] #9 Linting passes with `bun run lint`
<!-- AC:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Implemented applyProjection, applyIncludeProjection, and applyExcludeProjection helper methods in sqlite-collection.ts. Refactored into three methods to keep cognitive complexity under 15.
<!-- SECTION:NOTES:END -->
