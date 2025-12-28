---
id: task-47
title: Implement Collection.findById Function
status: To Do
assignee: []
created_date: '2025-12-28 06:38'
updated_date: '2025-12-28 16:57'
labels:
  - phase-3
  - storage
  - collection
dependencies:
  - task-46
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add findById read operation in src/Collection.fs. Reference: FSHARP_PORT_DESIGN.md lines 1037-1038.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Add 'let findById (id: string) (collection: Collection<'T>) : Task<Document<'T> option>'
- [ ] #2 Build SQL: SELECT _id, json(body) as body, createdAt, updatedAt FROM tableName WHERE _id = @id
- [ ] #3 Use Donald Db.newCommand, Db.setParams, Db.querySingle
- [ ] #4 Deserialize body JSON to 'T and construct Document<'T>
- [ ] #5 Return None if not found
- [ ] #6 Run 'dotnet build' - build succeeds
- [ ] #7 All public types and functions have XML doc comments with <summary>, and public functions include <param>, <returns>, and <example> (see doc-2 for standards)

- [ ] #8 Run 'task lint' - no errors or warnings

- [ ] #9 In src/Collection.fs, add [<RequireQualifiedAccess>] module Collection
<!-- AC:END -->
