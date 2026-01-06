---
id: task-162
title: Write unit tests for projection helpers
status: Done
assignee: []
created_date: '2025-11-23 07:27'
updated_date: '2025-11-23 08:02'
labels:
  - testing
  - phase-2
  - v0.3.0
dependencies:
  - task-161
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Create `test/unit/projection-helpers.test.ts` with unit tests for the normalizeProjection and applyProjection methods, testing the conversion logic independently of database operations.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Create new file `test/unit/projection-helpers.test.ts`
- [ ] #2 Test normalizeProjection returns projection when provided
- [ ] #3 Test normalizeProjection converts select array to projection
- [ ] #4 Test normalizeProjection converts omit array to projection
- [ ] #5 Test normalizeProjection precedence (projection > select > omit)
- [ ] #6 Test applyProjection in include mode keeps only specified fields
- [ ] #7 Test applyProjection in exclude mode removes specified fields
- [ ] #8 Test applyProjection always keeps _id unless explicitly excluded
- [ ] #9 All tests pass with `bun test test/unit/projection-helpers.test.ts`
<!-- AC:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Created test/unit/projection-helpers.test.ts with 22 tests covering select, omit, projection options, precedence rules, and edge cases.
<!-- SECTION:NOTES:END -->
