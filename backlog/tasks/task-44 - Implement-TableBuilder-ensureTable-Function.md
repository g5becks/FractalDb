---
id: task-44
title: Implement TableBuilder - ensureTable Function
status: To Do
assignee: []
created_date: '2025-12-28 06:37'
updated_date: '2025-12-28 07:03'
labels:
  - phase-2
  - storage
  - tablebuilder
dependencies:
  - task-43
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add function to ensure table and indexes exist. Reference: FSHARP_PORT_DESIGN.md lines 1454.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 In TableBuilder module, add 'let ensureTable (conn: IDbConnection) (name: string) (schema: SchemaDef<'T>) : unit'
- [ ] #2 Execute createTableSql to create table
- [ ] #3 Execute each statement from createIndexesSql to create indexes
- [ ] #4 Use Donald Db.newCommand and Db.exec for execution
- [ ] #5 Run 'dotnet build' - build succeeds

- [ ] #6 All public types and functions have XML doc comments with <summary>, and public functions include <param>, <returns>, and <example> (see doc-2 for standards)
<!-- AC:END -->
