---
id: task-67
title: Update all integration tests for _id rename
status: Done
assignee: []
created_date: '2025-11-22 06:26'
updated_date: '2025-11-22 07:55'
labels:
  - mongodb-compat
  - tests
dependencies:
  - task-66
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Update all integration test files to use `_id` instead of `id` in assertions and test data.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Update test/integration/collection-crud.test.ts - all `.id` to `._id`
- [ ] #2 Update test/integration/batch-operations.test.ts - all `.id` to `._id`
- [ ] #3 Update test/integration/transactions.test.ts - all `.id` to `._id`
- [ ] #4 Update test/integration/validation.test.ts - all `.id` to `._id`
- [ ] #5 Update test/integration/complex-queries.test.ts - all `.id` to `._id`
- [ ] #6 Update test/integration/standard-schema-validators.test.ts - all `.id` to `._id`
- [ ] #7 Update test/integration/symbol-dispose.test.ts - all `.id` to `._id`
- [ ] #8 Update test/integration/cache-configuration.test.ts - all `.id` to `._id`
- [ ] #9 Run `bun run typecheck` - should pass
- [ ] #10 Run `bun run lint` - no linter errors
<!-- AC:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Updated all 8 integration test files for _id rename: collection-crud.test.ts, batch-operations.test.ts, transactions.test.ts, validation.test.ts, complex-queries.test.ts, standard-schema-validators.test.ts, symbol-dispose.test.ts, and cache-configuration.test.ts. Fixed all field access patterns, test data, and validator schema definitions. Updated Zod, Valibot, and Arktype schemas to use _id field. All 146 integration tests now pass.
<!-- SECTION:NOTES:END -->
