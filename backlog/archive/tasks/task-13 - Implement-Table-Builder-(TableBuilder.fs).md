---
id: task-13
title: Implement Table Builder (TableBuilder.fs)
status: To Do
assignee: []
created_date: '2025-12-28 06:07'
labels:
  - storage
  - phase-2
dependencies:
  - task-4
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Create the DDL generation module for creating tables and indexes in Storage/TableBuilder.fs.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 mapSqliteType function converts SqliteType DU to SQL string
- [ ] #2 createTableSql function generates CREATE TABLE IF NOT EXISTS with _id, body, createdAt, updatedAt columns
- [ ] #3 createTableSql generates VIRTUAL generated columns for indexed fields using jsonb_extract
- [ ] #4 createIndexesSql function generates CREATE INDEX statements for indexed fields
- [ ] #5 createIndexesSql generates UNIQUE INDEX for unique fields
- [ ] #6 createIndexesSql generates compound indexes from IndexDef list
- [ ] #7 Code compiles successfully with dotnet build
<!-- AC:END -->
