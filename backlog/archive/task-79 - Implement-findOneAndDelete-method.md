---
id: task-79
title: Implement findOneAndDelete method
status: To Do
assignee: []
created_date: '2025-11-22 06:28'
labels:
  - mongodb-compat
  - feature
dependencies:
  - task-78
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add new findOneAndDelete method that atomically finds and deletes a document, returning the deleted document.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Add findOneAndDelete signature to Collection interface in src/collection-types.ts
- [ ] #2 Add comprehensive JSDoc with examples
- [ ] #3 Implement findOneAndDelete in src/sqlite-collection.ts
- [ ] #4 Accept filter and optional sort option
- [ ] #5 Find document with findOne, delete by _id, return the found document
- [ ] #6 Return null if no match found
- [ ] #7 Run `bun run typecheck` - should pass
- [ ] #8 Run `bun run lint` - no linter errors
<!-- AC:END -->
