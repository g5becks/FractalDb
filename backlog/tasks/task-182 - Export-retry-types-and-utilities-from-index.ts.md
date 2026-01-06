---
id: task-182
title: Export retry types and utilities from index.ts
status: In Progress
assignee:
  - '@agent'
created_date: '2026-01-06 00:24'
updated_date: '2026-01-06 03:19'
labels:
  - retry
  - exports
dependencies:
  - task-181
priority: medium
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Update src/index.ts to export RetryOptions, RetryContext from retry-types.ts and defaultShouldRetry, withRetry, mergeRetryOptions from retry-utils.ts.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 RetryOptions type is exported from index.ts
- [ ] #2 RetryContext type is exported from index.ts
- [ ] #3 defaultShouldRetry function is exported from index.ts
- [ ] #4 withRetry function is exported from index.ts
- [ ] #5 mergeRetryOptions function is exported from index.ts
- [ ] #6 bun run check passes with no errors
- [ ] #7 bun run build succeeds
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Check current exports in index.ts
2. Add retry type exports from retry-types.ts
3. Add retry utility exports from retry-utils.ts
4. Run bun run check and bun run build
<!-- SECTION:PLAN:END -->
