---
id: task-85
title: Update documentation for MongoDB compatibility changes
status: To Do
assignee: []
created_date: '2025-11-22 06:28'
labels:
  - mongodb-compat
  - docs
dependencies:
  - task-84
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Update all documentation to reflect the new MongoDB-compatible API.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Update docs/guide/collections.md with new method signatures and examples
- [ ] #2 Update docs/guide/documents.md to use _id instead of id
- [ ] #3 Update docs/guide/queries.md to remove $regex, document alternatives
- [ ] #4 Update README.md quick-start examples to use _id and new signatures
- [ ] #5 Add section documenting $set operator usage
- [ ] #6 Add section documenting findOneAnd* methods
- [ ] #7 Document that $regex is not supported, use $like/$startsWith/$endsWith
- [ ] #8 Run `bun run lint` - no linter errors in markdown files
<!-- AC:END -->
