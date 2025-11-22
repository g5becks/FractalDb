---
id: task-81
title: Implement findOneAndReplace method
status: To Do
assignee: []
created_date: '2025-11-22 06:28'
labels:
  - mongodb-compat
  - feature
dependencies:
  - task-80
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add new findOneAndReplace method that atomically finds and replaces a document, returning the document before or after the replacement.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Add findOneAndReplace signature to Collection interface in src/collection-types.ts
- [ ] #2 Add comprehensive JSDoc with examples
- [ ] #3 Implement findOneAndReplace in src/sqlite-collection.ts
- [ ] #4 Accept filter, replacement document, and options (sort, returnDocument, upsert)
- [ ] #5 Support returnDocument: 'before' | 'after' (default 'after')
- [ ] #6 Support upsert option
- [ ] #7 Return null if no match and no upsert
- [ ] #8 Run `bun run typecheck` - should pass
- [ ] #9 Run `bun run lint` - no linter errors
<!-- AC:END -->
