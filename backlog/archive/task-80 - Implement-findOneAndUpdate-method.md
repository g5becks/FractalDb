---
id: task-80
title: Implement findOneAndUpdate method
status: To Do
assignee: []
created_date: '2025-11-22 06:28'
labels:
  - mongodb-compat
  - feature
dependencies:
  - task-79
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add new findOneAndUpdate method that atomically finds and updates a document, returning the document before or after the update.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Add findOneAndUpdate signature to Collection interface in src/collection-types.ts
- [ ] #2 Add comprehensive JSDoc with examples for returnDocument option
- [ ] #3 Implement findOneAndUpdate in src/sqlite-collection.ts
- [ ] #4 Accept filter, UpdateFilter, and options (sort, returnDocument, upsert)
- [ ] #5 Support returnDocument: 'before' | 'after' (default 'after')
- [ ] #6 Unwrap $set if present
- [ ] #7 Support upsert option
- [ ] #8 Return null if no match and no upsert
- [ ] #9 Run `bun run typecheck` - should pass
- [ ] #10 Run `bun run lint` - no linter errors
<!-- AC:END -->
