---
id: task-82
title: Update tests for filter-based operations
status: To Do
assignee: []
created_date: '2025-11-22 06:28'
labels:
  - mongodb-compat
  - tests
dependencies:
  - task-81
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Update all existing tests to use the new filter-based signatures for deleteOne, updateOne, and replaceOne.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Update deleteOne tests to use filter syntax: `deleteOne({ _id: id })`
- [ ] #2 Update deleteOne tests to check DeleteResult instead of boolean
- [ ] #3 Update updateOne tests to use filter syntax: `updateOne({ _id: id }, { $set: ... })`
- [ ] #4 Update updateOne tests to check UpdateResult instead of document
- [ ] #5 Update replaceOne tests to use filter syntax: `replaceOne({ _id: id }, doc)`
- [ ] #6 Update replaceOne tests to check UpdateResult instead of document
- [ ] #7 Add new tests for filter-based matching (not just _id)
- [ ] #8 Run `bun run typecheck` - should pass
- [ ] #9 Run `bun run lint` - no linter errors
<!-- AC:END -->
