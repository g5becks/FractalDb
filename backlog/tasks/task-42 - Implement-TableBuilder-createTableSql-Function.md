---
id: task-42
title: Implement TableBuilder - createTableSql Function
status: To Do
assignee: []
created_date: '2025-12-28 06:37'
updated_date: '2025-12-28 07:03'
labels:
  - phase-2
  - storage
  - tablebuilder
dependencies:
  - task-41
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add DDL generation for CREATE TABLE. Reference: FSHARP_PORT_DESIGN.md lines 1357-1376.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 In TableBuilder module, add 'let createTableSql (name: string) (schema: SchemaDef<'T>) : string'
- [ ] #2 Generate base columns: _id TEXT PRIMARY KEY, body BLOB NOT NULL, createdAt INTEGER NOT NULL, updatedAt INTEGER NOT NULL
- [ ] #3 For each indexed field, generate: _fieldName TYPE GENERATED ALWAYS AS (jsonb_extract(body, '$.path')) VIRTUAL
- [ ] #4 Use field.Path if set, otherwise default to '$.fieldName'
- [ ] #5 Combine into CREATE TABLE IF NOT EXISTS statement
- [ ] #6 Run 'dotnet build' - build succeeds

- [ ] #7 All public types and functions have XML doc comments with <summary>, and public functions include <param>, <returns>, and <example> (see doc-2 for standards)
<!-- AC:END -->
