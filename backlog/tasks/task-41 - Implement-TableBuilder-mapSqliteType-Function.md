---
id: task-41
title: Implement TableBuilder - mapSqliteType Function
status: To Do
assignee: []
created_date: '2025-12-28 06:37'
updated_date: '2025-12-28 16:57'
labels:
  - phase-2
  - storage
  - tablebuilder
dependencies:
  - task-40
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Create TableBuilder module with SQLite type mapping in src/TableBuilder.fs. Reference: FSHARP_PORT_DESIGN.md lines 1353-1398.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Add namespace FractalDb.Storage
- [ ] #2 Add module internal TableBuilder
- [ ] #3 Add 'let mapSqliteType (sqlType: SqliteType) : string' function
- [ ] #4 Map: Text->TEXT, Integer->INTEGER, Real->REAL, Blob->BLOB, Numeric->NUMERIC, Boolean->INTEGER
- [ ] #5 Run 'dotnet build' - build succeeds
- [ ] #6 All public types and functions have XML doc comments with <summary>, and public functions include <param>, <returns>, and <example> (see doc-2 for standards)
- [ ] #7 Run 'task lint' - no errors or warnings

- [ ] #8 Create file src/TableBuilder.fs

- [ ] #9 Add module declaration: module FractalDb.TableBuilder
<!-- AC:END -->
