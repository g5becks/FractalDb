---
id: task-71
title: Remove broken $regex operator from query types
status: Done
assignee:
  - '@claude'
created_date: '2025-11-22 06:27'
updated_date: '2025-11-22 14:43'
labels:
  - mongodb-compat
  - breaking-change
dependencies:
  - task-70
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Remove the $regex and $options operators from the query type system since SQLite doesn't support REGEXP without native code extensions. Users should use $like, $startsWith, and $endsWith instead.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Remove `$regex` property from StringOperator type in src/query-types.ts
- [ ] #2 Remove `$options` property from StringOperator type in src/query-types.ts
- [ ] #3 Update StringOperator JSDoc to document that regex is not supported
- [ ] #4 Add note in JSDoc recommending $like, $startsWith, $endsWith as alternatives
- [ ] #5 Run `bun run typecheck` - should pass
- [ ] #6 Run `bun run lint` - no linter errors
<!-- AC:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Successfully removed  and off on off off off off off on off off on off on off on off off off on off off off on off off off off on on off off off off off on off on off off off off on off on off off off off off on off on on off off off on off off on off off on off off on on on off on on off on off off on off on off off off on off off on off on off off off on on off on off on off off on off off off on off off off off off off off off on off on off on on on off on on off off on off off on off off on on off on off on on off off off off off on off off on off off off off off on off off on on off on off off off off off off on off off off off on on off on off off off off off off off on on off off on off off off off off off off on off off off off off operators from StringOperator type in src/query-types.ts. Updated JSDoc documentation to recommend alternatives like , , and  for pattern matching. Type checking passes with no errors. Lint issues are pre-existing and unrelated to these changes.
<!-- SECTION:NOTES:END -->
