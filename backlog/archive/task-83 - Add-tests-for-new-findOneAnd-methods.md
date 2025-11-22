---
id: task-83
title: Add tests for new findOneAnd* methods
status: To Do
assignee: []
created_date: '2025-11-22 06:28'
labels:
  - mongodb-compat
  - tests
dependencies:
  - task-82
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add comprehensive tests for findOneAndDelete, findOneAndUpdate, and findOneAndReplace methods.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Add findOneAndDelete tests: basic usage, with sort, no match returns null
- [ ] #2 Add findOneAndUpdate tests: basic, returnDocument before/after, upsert, $set syntax
- [ ] #3 Add findOneAndReplace tests: basic, returnDocument before/after, upsert
- [ ] #4 Test that operations are atomic (document returned matches what was modified)
- [ ] #5 Run `bun run typecheck` - should pass
- [ ] #6 Run `bun run lint` - no linter errors
- [ ] #7 Run `bun test` - all tests must pass
<!-- AC:END -->
