---
id: task-4
title: Implement Schema Types (Schema.fs)
status: To Do
assignee: []
created_date: '2025-12-28 06:04'
labels:
  - core
  - phase-1
dependencies:
  - task-1
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Create the schema definition types in Core/Schema.fs. Depends only on Types.fs.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 SqliteType DU with Text, Integer, Real, Blob, Numeric, Boolean cases
- [ ] #2 FieldDef record with Name, Path, SqlType, Indexed, Unique, Nullable fields
- [ ] #3 IndexDef record with Name, Fields, Unique fields
- [ ] #4 SchemaDef<'T> record with Fields, Indexes, Timestamps, Validate fields
- [ ] #5 Code compiles successfully with dotnet build
<!-- AC:END -->
