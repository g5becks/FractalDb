---
id: task-163
title: Write integration tests for select and omit options
status: Done
assignee: []
created_date: '2025-11-23 07:27'
updated_date: '2025-11-23 08:02'
labels:
  - testing
  - phase-2
  - v0.3.0
dependencies:
  - task-162
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Create `test/integration/projection-options.test.ts` with integration tests verifying select and omit work correctly through the full Collection.find() flow.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Create new file `test/integration/projection-options.test.ts`
- [ ] #2 Test select includes only specified fields plus _id
- [ ] #3 Test select with single field
- [ ] #4 Test select with multiple fields
- [ ] #5 Test omit excludes specified fields
- [ ] #6 Test omit with multiple fields
- [ ] #7 Test projection takes precedence over select
- [ ] #8 Test select takes precedence over omit
- [ ] #9 Test with sorting and pagination combined
- [ ] #10 All tests pass with `bun test test/integration/projection-options.test.ts`
<!-- AC:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Created test/integration/projection-options.test.ts with 20 integration tests covering real-world use cases including public API responses, admin dashboards, and search autocomplete scenarios.
<!-- SECTION:NOTES:END -->
