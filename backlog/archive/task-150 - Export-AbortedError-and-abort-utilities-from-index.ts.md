---
id: task-150
title: Export AbortedError and abort utilities from index.ts
status: Done
assignee:
  - '@agent'
created_date: '2026-01-05 23:19'
updated_date: '2026-01-06 00:42'
labels:
  - abort-signal
  - exports
dependencies:
  - task-148
priority: medium
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Update src/index.ts to export AbortedError from errors.ts and throwIfAborted from abort-utils.ts for public API access.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 AbortedError is exported from index.ts
- [x] #2 throwIfAborted is exported from index.ts
- [x] #3 createAbortPromise is exported from index.ts (for advanced use cases)
- [x] #4 bun run check passes with no errors
- [x] #5 bun run build succeeds
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Read current index.ts to understand export structure
2. Add export for AbortedError from errors.ts
3. Add exports for throwIfAborted and createAbortPromise from abort-utils.ts
4. Run bun run check to verify
5. Run bun run build to ensure build succeeds
<!-- SECTION:PLAN:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Updated index.ts to export abort-related functionality:

- Exported AbortedError from errors.ts
- Exported throwIfAborted from abort-utils.ts
- Exported createAbortPromise from abort-utils.ts for advanced use cases
- Added biome-ignore comment for barrel file warning
- All checks pass (typecheck + lint)
- Build succeeds
<!-- SECTION:NOTES:END -->
