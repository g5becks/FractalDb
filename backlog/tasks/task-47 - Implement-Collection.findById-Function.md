---
id: task-47
title: Implement Collection.findById Function
status: To Do
assignee: []
created_date: '2025-12-28 06:38'
updated_date: '2025-12-28 16:36'
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
Add findById read operation. Reference: FSHARP_PORT_DESIGN.md lines 1037-1038.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 In Collection.fs, add [<RequireQualifiedAccess>] module Collection
- [ ] #2 Add 'let findById (id: string) (collection: Collection<'T>) : Task<Document<'T> option>'
- [ ] #3 Build SQL: SELECT _id, json(body) as body, createdAt, updatedAt FROM tableName WHERE _id = @id
- [ ] #4 Use Donald Db.newCommand, Db.setParams, Db.querySingle
- [ ] #5 Deserialize body JSON to 'T and construct Document<'T>
- [ ] #6 Return None if not found
- [ ] #7 Run 'dotnet build' - build succeeds

- [ ] #8 All public types and functions have XML doc comments with <summary>, and public functions include <param>, <returns>, and <example> (see doc-2 for standards)

- [ ] #9 Run 'task lint' - no errors or warnings
<!-- AC:END -->
