---
id: task-43
title: Implement TableBuilder - createIndexesSql Function
status: To Do
assignee: []
created_date: '2025-12-28 06:37'
updated_date: '2025-12-28 07:03'
labels:
  - phase-2
  - storage
  - tablebuilder
dependencies:
  - task-42
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add DDL generation for CREATE INDEX statements. Reference: FSHARP_PORT_DESIGN.md lines 1379-1397.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 In TableBuilder module, add 'let createIndexesSql (name: string) (schema: SchemaDef<'T>) : string list'
- [ ] #2 For each indexed field: CREATE [UNIQUE] INDEX IF NOT EXISTS idx_tableName_fieldName ON tableName(_fieldName)
- [ ] #3 For each compound index in schema.Indexes: CREATE [UNIQUE] INDEX IF NOT EXISTS indexName ON tableName(_field1, _field2, ...)
- [ ] #4 Return list of all index creation statements
- [ ] #5 Run 'dotnet build' - build succeeds

- [ ] #6 All public types and functions have XML doc comments with <summary>, and public functions include <param>, <returns>, and <example> (see doc-2 for standards)
<!-- AC:END -->
