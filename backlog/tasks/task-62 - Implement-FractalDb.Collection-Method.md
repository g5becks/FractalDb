---
id: task-62
title: Implement FractalDb.Collection Method
status: To Do
assignee: []
created_date: '2025-12-28 06:42'
updated_date: '2025-12-28 16:58'
labels:
  - phase-3
  - storage
  - database
dependencies:
  - task-61
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add Collection getter to FractalDb in src/Database.fs. Reference: FSHARP_PORT_DESIGN.md lines 1443-1456.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Add member this.Collection<'T>(name: string, schema: SchemaDef<'T>) : Collection<'T>
- [ ] #2 Use collections.GetOrAdd to cache collection instance
- [ ] #3 Create Collection record with Name, Schema, Connection, IdGenerator, Translator, EnableCache
- [ ] #4 Call TableBuilder.ensureTable to create table/indexes if needed
- [ ] #5 Return Collection<'T>
- [ ] #6 Run 'dotnet build' - build succeeds

- [ ] #7 All public types and functions have XML doc comments with <summary>, and public functions include <param>, <returns>, and <example> (see doc-2 for standards)

- [ ] #8 Run 'task lint' - no errors or warnings
<!-- AC:END -->
