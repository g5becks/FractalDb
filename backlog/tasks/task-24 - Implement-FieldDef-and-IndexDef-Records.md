---
id: task-24
title: Implement FieldDef and IndexDef Records
status: To Do
assignee: []
created_date: '2025-12-28 06:32'
updated_date: '2025-12-28 16:34'
labels:
  - phase-1
  - core
  - schema
dependencies:
  - task-23
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Create FieldDef and IndexDef records in Core/Schema.fs. Reference: FSHARP_PORT_DESIGN.md lines 588-600.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 In Core/Schema.fs, add FieldDef record type
- [ ] #2 FieldDef has: Name: string, Path: string option, SqlType: SqliteType, Indexed: bool, Unique: bool, Nullable: bool
- [ ] #3 Add IndexDef record type
- [ ] #4 IndexDef has: Name: string, Fields: string list, Unique: bool
- [ ] #5 Run 'dotnet build' - build succeeds

- [ ] #6 All public types and functions have XML doc comments with <summary>, and public functions include <param>, <returns>, and <example> (see doc-2 for standards)

- [ ] #7 Run 'task lint' - no errors or warnings
<!-- AC:END -->
