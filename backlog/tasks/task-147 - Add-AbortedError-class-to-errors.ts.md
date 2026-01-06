---
id: task-147
title: Add AbortedError class to errors.ts
status: Done
assignee:
  - '@agent'
created_date: '2026-01-05 23:14'
updated_date: '2026-01-06 00:38'
labels:
  - errors
  - abort-signal
dependencies: []
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add a new AbortedError class that extends StrataDBError for aborted operations via AbortSignal. This is the foundation for all abort handling in StrataDB.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 AbortedError class extends StrataDBError
- [x] #2 category property is set to 'database'
- [x] #3 code property is set to 'OPERATION_ABORTED'
- [x] #4 reason property stores the abort reason from AbortSignal
- [x] #5 Constructor accepts message and optional reason parameters
- [x] #6 JSDoc documentation with @example showing usage with AbortController
- [x] #7 bun run check passes with no errors
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Review existing error classes in errors.ts for pattern consistency
2. Add AbortedError class extending StrataDBError with required properties
3. Add comprehensive JSDoc with @example showing AbortController usage
4. Run bun run check to verify types and linting
5. Verify the error integrates properly with existing error hierarchy
<!-- SECTION:PLAN:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Added AbortedError class to errors.ts following existing error patterns:

- Extends StrataDBError with category="database" and code="OPERATION_ABORTED"
- Includes reason property to preserve abort reason from AbortSignal
- Constructor accepts message and optional reason parameters
- Comprehensive JSDoc with example showing AbortController usage
- All checks pass (typecheck + lint)
<!-- SECTION:NOTES:END -->
