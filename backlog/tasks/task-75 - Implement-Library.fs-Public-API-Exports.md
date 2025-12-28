---
id: task-75
title: Implement Library.fs Public API Exports
status: To Do
assignee: []
created_date: '2025-12-28 06:45'
updated_date: '2025-12-28 16:37'
labels:
  - phase-5
  - api
dependencies:
  - task-74
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Create Library.fs to export all public types. Reference: FSHARP_PORT_DESIGN.md lines 2650-2670.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Create file src/FractalDb/Library.fs
- [ ] #2 This file should be LAST in FractalDb.fsproj compile order
- [ ] #3 Re-export core types: Document, DocumentMeta, IdGenerator, Timestamp from Core/Types
- [ ] #4 Re-export operators: Query, CompareOp, StringOp, ArrayOp, ExistsOp, FieldOp
- [ ] #5 Re-export schema types: SchemaDef, FieldDef, IndexDef, SqliteType
- [ ] #6 Re-export options: QueryOptions, SortDirection, CursorSpec
- [ ] #7 Re-export errors: FractalError, FractalResult
- [ ] #8 Re-export storage: Collection, FractalDb, DbOptions
- [ ] #9 Re-export builders: query, schema, options instances with AutoOpen
- [ ] #10 Run 'dotnet build' - build succeeds

- [ ] #11 All public types and functions have XML doc comments with <summary>, and public functions include <param>, <returns>, and <example> (see doc-2 for standards)

- [ ] #12 Run 'task lint' - no errors or warnings
<!-- AC:END -->
