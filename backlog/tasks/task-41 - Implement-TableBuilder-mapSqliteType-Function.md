---
id: task-41
title: Implement TableBuilder - mapSqliteType Function
status: To Do
assignee: []
created_date: '2025-12-28 06:37'
updated_date: '2025-12-28 16:35'
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
Create TableBuilder module with SQLite type mapping. Reference: FSHARP_PORT_DESIGN.md lines 1353-1398.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Create file src/FractalDb/Storage/TableBuilder.fs
- [ ] #2 Add namespace FractalDb.Storage
- [ ] #3 Add opens for FractalDb.Core
- [ ] #4 Add module internal TableBuilder
- [ ] #5 Add 'let mapSqliteType (sqlType: SqliteType) : string' function
- [ ] #6 Map: Text->TEXT, Integer->INTEGER, Real->REAL, Blob->BLOB, Numeric->NUMERIC, Boolean->INTEGER
- [ ] #7 Run 'dotnet build' - build succeeds

- [ ] #8 All public types and functions have XML doc comments with <summary>, and public functions include <param>, <returns>, and <example> (see doc-2 for standards)

- [ ] #9 Run 'task lint' - no errors or warnings
<!-- AC:END -->
