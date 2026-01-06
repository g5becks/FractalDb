---
id: task-134
title: Add drop tests to collection-crud.test.ts
status: Done
assignee: []
created_date: '2025-11-22 20:18'
updated_date: '2025-11-22 22:18'
labels: []
dependencies:
  - task-133
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add tests for drop covering table deletion and graceful handling when already dropped in test/integration/collection-crud.test.ts
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 All tests pass
- [ ] #2 Type checking passes
- [ ] #3 Linting passes
- [ ] #4 Tests drop collection table successfully
- [ ] #5 Tests don't throw when already dropped
<!-- AC:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Added test suite for drop.

Tests added (2 tests):
- Drop the collection successfully
- Safe to call on non-existent collection

All tests pass.
<!-- SECTION:NOTES:END -->
