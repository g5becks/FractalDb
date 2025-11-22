---
id: task-84
title: Add tests for $set operator
status: To Do
assignee: []
created_date: '2025-11-22 06:28'
labels:
  - mongodb-compat
  - tests
dependencies:
  - task-83
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add tests verifying $set operator works correctly in all update methods.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Test updateOne with $set syntax
- [ ] #2 Test updateOne with direct partial syntax (backwards compat)
- [ ] #3 Test updateMany with $set syntax
- [ ] #4 Test findOneAndUpdate with $set syntax
- [ ] #5 Verify both syntaxes produce same results
- [ ] #6 Run `bun run typecheck` - should pass
- [ ] #7 Run `bun run lint` - no linter errors
- [ ] #8 Run `bun test` - all tests must pass
<!-- AC:END -->
