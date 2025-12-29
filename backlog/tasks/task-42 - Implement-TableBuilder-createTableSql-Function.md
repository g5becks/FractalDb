---
id: task-42
title: Implement TableBuilder - createTableSql Function
status: Done
assignee:
  - '@assistant'
created_date: '2025-12-28 06:37'
updated_date: '2025-12-28 18:47'
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
Add DDL generation for CREATE TABLE in src/TableBuilder.fs. Reference: FSHARP_PORT_DESIGN.md lines 1357-1376.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 In TableBuilder module, add 'let createTableSql (name: string) (schema: SchemaDef<'T>) : string'
- [x] #2 Generate base columns: _id TEXT PRIMARY KEY, body BLOB NOT NULL, createdAt INTEGER NOT NULL, updatedAt INTEGER NOT NULL
- [x] #3 For each indexed field, generate: _fieldName TYPE GENERATED ALWAYS AS (jsonb_extract(body, '$.path')) VIRTUAL
- [x] #4 Use field.Path if set, otherwise default to '$.fieldName'
- [x] #5 Combine into CREATE TABLE IF NOT EXISTS statement
- [x] #6 Run 'dotnet build' - build succeeds

- [x] #7 All public types and functions have XML doc comments with <summary>, and public functions include <param>, <returns>, and <example> (see doc-2 for standards)

- [x] #8 Run 'task lint' - no errors or warnings
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Read Schema.fs to understand SchemaDef<'T> and FieldDef structure
2. Implement createTableSql function with proper type signature
3. Generate base columns array: _id, body, createdAt, updatedAt
4. Filter schema.Fields for indexed fields
5. Map indexed fields to generated column definitions using mapSqliteType
6. Use field.Path or default to $.fieldName
7. Combine all columns with proper formatting and newlines
8. Add comprehensive XML documentation
9. Build and verify with dotnet build and task lint
<!-- SECTION:PLAN:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Implemented createTableSql function in src/TableBuilder.fs to generate CREATE TABLE statements with generated columns.

Key implementation details:
- Base columns: _id TEXT PRIMARY KEY, body BLOB NOT NULL, createdAt/updatedAt INTEGER NOT NULL
- Filters schema.Fields for indexed fields and generates VIRTUAL columns
- Uses mapSqliteType for proper type mapping
- Uses field.Path or defaults to $.{fieldName} for json_extract paths
- Generated column format: _fieldName TYPE GENERATED ALWAYS AS (jsonb_extract(body, 'path')) VIRTUAL
- Combines columns with proper formatting (comma-separated, indented with newlines)
- Returns CREATE TABLE IF NOT EXISTS statement
- Comprehensive XML docs with summary, param, returns, remarks, and detailed example

Verification:
- dotnet build: 0 errors, 0 warnings ✅
- task lint: 0 warnings ✅ (fixed line length warning by splitting long comment)
- dotnet test: 66/66 tests passing ✅

Ready for Task 43: createIndexesSql function to generate CREATE INDEX statements.
<!-- SECTION:NOTES:END -->
