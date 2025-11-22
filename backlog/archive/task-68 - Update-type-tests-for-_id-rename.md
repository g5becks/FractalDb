---
id: task-68
title: Update type tests for _id rename
status: Done
assignee:
  - '@claude'
created_date: '2025-11-22 06:26'
updated_date: '2025-11-22 14:05'
labels:
  - mongodb-compat
  - tests
dependencies:
  - task-67
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Update the tsd type tests to use `_id` instead of `id`.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Update test/type/core-types.test-d.ts - all `id` references to `_id`
- [ ] #2 Run `bun run typecheck` - should pass
- [ ] #3 Run `bun run lint` - no linter errors
<!-- AC:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Updated test/type/core-types.test-d.ts for _id rename. Changed all DocumentInput, DocumentUpdate, and query type references from 'id' to '_id'. Updated type assertions, test data, and comments. Type checking passes with no errors. Lint issues are pre-existing and unrelated to _id rename changes.
<!-- SECTION:NOTES:END -->
