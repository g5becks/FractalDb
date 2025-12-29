---
id: task-24
title: Implement FieldDef and IndexDef Records
status: Done
assignee:
  - '@assistant'
created_date: '2025-12-28 06:32'
updated_date: '2025-12-28 18:00'
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
Create FieldDef and IndexDef records in src/Schema.fs. Reference: FSHARP_PORT_DESIGN.md lines 588-600.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 FieldDef has: Name: string, Path: string option, SqlType: SqliteType, Indexed: bool, Unique: bool, Nullable: bool
- [x] #2 Add IndexDef record type
- [x] #3 IndexDef has: Name: string, Fields: string list, Unique: bool
- [x] #4 Run 'dotnet build' - build succeeds
- [x] #5 All public types and functions have XML doc comments with <summary>, and public functions include <param>, <returns>, and <example> (see doc-2 for standards)

- [x] #6 Run 'task lint' - no errors or warnings

- [x] #7 In src/Schema.fs, add FieldDef record type
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Read FSHARP_PORT_DESIGN.md lines 588-600 for record specifications
2. Add FieldDef record with 6 fields
3. Add IndexDef record with 3 fields
4. Add comprehensive XML documentation for both records
5. Run dotnet build to verify
6. Run task lint to verify
7. Mark all ACs complete
<!-- SECTION:PLAN:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Implemented FieldDef and IndexDef record types in src/Schema.fs:

FieldDef record (6 fields):
- Name: string - field name for queries
- Path: option<string> - JSON path (defaults to $.{Name})
- SqlType: SqliteType - storage type
- Indexed: bool - create index
- Unique: bool - enforce uniqueness
- Nullable: bool - allow NULL

IndexDef record (3 fields):
- Name: string - index name
- Fields: list<string> - field names in index
- Unique: bool - unique constraint

Both records include comprehensive XML documentation with detailed remarks and examples. Used prefix syntax for generic types. Build passes with 0 errors/warnings, lint passes with 0 warnings.
<!-- SECTION:NOTES:END -->
