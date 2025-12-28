---
id: task-69
title: Implement SchemaBuilder Computation Expression
status: To Do
assignee: []
created_date: '2025-12-28 06:43'
updated_date: '2025-12-28 16:37'
labels:
  - phase-4
  - builders
dependencies:
  - task-68
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Create SchemaBuilder CE for declarative schema definition. Reference: FSHARP_PORT_DESIGN.md lines 574-685.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Create file src/FractalDb/Builders/SchemaBuilder.fs
- [ ] #2 Add namespace FractalDb.Builders
- [ ] #3 Define SchemaBuilder<'T> class with Yield returning empty SchemaDef
- [ ] #4 Add [<CustomOperation("field")>] with name, SqliteType, optional indexed/unique/nullable/path
- [ ] #5 Add [<CustomOperation("indexed")>] as shorthand for indexed field
- [ ] #6 Add [<CustomOperation("unique")>] as shorthand for unique indexed field
- [ ] #7 Add [<CustomOperation("timestamps")>] enabling auto timestamps
- [ ] #8 Add [<CustomOperation("compoundIndex")>] for compound indexes
- [ ] #9 Add [<CustomOperation("validate")>] for validation function
- [ ] #10 Add [<AutoOpen>] module with 'let schema<'T> = SchemaBuilder<'T>()'
- [ ] #11 Run 'dotnet build' - build succeeds

- [ ] #12 All public types and functions have XML doc comments with <summary>, and public functions include <param>, <returns>, and <example> (see doc-2 for standards)

- [ ] #13 Run 'task lint' - no errors or warnings
<!-- AC:END -->
