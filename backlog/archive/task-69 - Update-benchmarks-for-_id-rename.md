---
id: task-69
title: Update benchmarks for _id rename
status: In Progress
assignee:
  - '@claude'
created_date: '2025-11-22 06:26'
updated_date: '2025-11-22 14:07'
labels:
  - mongodb-compat
  - benchmarks
dependencies:
  - task-68
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Update all benchmark files to use `_id` instead of `id`.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Update bench/query-translation.bench.ts - all `.id` to `._id` if present
- [ ] #2 Update bench/crud-operations.bench.ts - all `.id` to `._id`
- [ ] #3 Update bench/batch-throughput.bench.ts - all `.id` to `._id`
- [ ] #4 Run `bun run typecheck` - should pass
- [ ] #5 Run `bun run lint` - no linter errors
<!-- AC:END -->
