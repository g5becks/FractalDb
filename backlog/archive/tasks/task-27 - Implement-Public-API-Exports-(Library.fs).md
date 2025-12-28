---
id: task-27
title: Implement Public API Exports (Library.fs)
status: To Do
assignee: []
created_date: '2025-12-28 06:11'
labels:
  - api
  - phase-5
dependencies:
  - task-23
  - task-24
  - task-25
  - task-26
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Create the Library.fs file that exports all public types and functions for the FractalDb API.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 FractalDb namespace exports Document, DocumentMeta, IdGenerator, Timestamp from Core/Types.fs
- [ ] #2 FractalDb namespace exports Query, CompareOp, StringOp, ArrayOp, ExistsOp, FieldOp from Core modules
- [ ] #3 FractalDb namespace exports SchemaDef, FieldDef, IndexDef, SqliteType from Core/Schema.fs
- [ ] #4 FractalDb namespace exports QueryOptions, SortDirection, CursorSpec from Core/Options.fs
- [ ] #5 FractalDb namespace exports FractalError, FractalResult from Core/Errors.fs
- [ ] #6 FractalDb namespace exports Collection module and result types
- [ ] #7 FractalDb namespace exports FractalDb database class and DbOptions
- [ ] #8 FractalDb.Builders namespace exports query, schema, options instances
- [ ] #9 AutoOpen attributes applied for convenient access
- [ ] #10 Code compiles successfully with dotnet build
<!-- AC:END -->
