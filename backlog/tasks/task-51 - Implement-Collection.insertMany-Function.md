---
id: task-51
title: Implement Collection.insertMany Function
status: To Do
assignee: []
created_date: '2025-12-28 06:39'
updated_date: '2025-12-28 16:57'
labels:
  - phase-3
  - storage
  - collection
dependencies:
  - task-50
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add insertMany batch operation in src/Collection.fs. Reference: FSHARP_PORT_DESIGN.md lines 1106-1113.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Add 'let insertMany (docs: 'T list) (collection: Collection<'T>) : Task<FractalResult<InsertManyResult<'T>>>'
- [ ] #2 Wrap all inserts in a transaction
- [ ] #3 For each doc, call insertOne logic
- [ ] #4 If any fails and ordered=true (default), rollback and return error
- [ ] #5 Add 'insertManyWith' variant with ordered: bool parameter
- [ ] #6 Return InsertManyResult with Documents list and InsertedCount
- [ ] #7 Run 'dotnet build' - build succeeds

- [ ] #8 All public types and functions have XML doc comments with <summary>, and public functions include <param>, <returns>, and <example> (see doc-2 for standards)

- [ ] #9 Run 'task lint' - no errors or warnings
<!-- AC:END -->
