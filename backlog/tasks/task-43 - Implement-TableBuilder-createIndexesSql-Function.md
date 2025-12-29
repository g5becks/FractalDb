---
id: task-43
title: Implement TableBuilder - createIndexesSql Function
status: Done
assignee:
  - '@assistant'
created_date: '2025-12-28 06:37'
updated_date: '2025-12-28 18:48'
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
Add DDL generation for CREATE INDEX statements in src/TableBuilder.fs. Reference: FSHARP_PORT_DESIGN.md lines 1379-1397.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 In TableBuilder module, add 'let createIndexesSql (name: string) (schema: SchemaDef<'T>) : string list'
- [x] #2 For each indexed field: CREATE [UNIQUE] INDEX IF NOT EXISTS idx_tableName_fieldName ON tableName(_fieldName)
- [x] #3 For each compound index in schema.Indexes: CREATE [UNIQUE] INDEX IF NOT EXISTS indexName ON tableName(_field1, _field2, ...)
- [x] #4 Return list of all index creation statements
- [x] #5 Run 'dotnet build' - build succeeds

- [x] #6 All public types and functions have XML doc comments with <summary>, and public functions include <param>, <returns>, and <example> (see doc-2 for standards)

- [x] #7 Run 'task lint' - no errors or warnings
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Review design spec (lines 1379-1397) for createIndexesSql implementation
2. Check IndexDef structure in Schema.fs
3. Implement function returning list of CREATE INDEX statements
4. Generate fieldIndexes: filter indexed fields, map to CREATE INDEX statements with UNIQUE keyword if needed
5. Generate compoundIndexes: map schema.Indexes to CREATE INDEX with multiple columns
6. Combine both lists and return
7. Add comprehensive XML documentation with examples
8. Build and verify with dotnet build and task lint
<!-- SECTION:PLAN:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Implemented createIndexesSql function in src/TableBuilder.fs to generate CREATE INDEX statements.

Key implementation details:
- Returns list<string> of CREATE INDEX statements
- Generates field indexes: filters schema.Fields for indexed fields, creates idx_{tableName}_{fieldName} indexes
- Adds UNIQUE keyword if field.Unique is true
- Indexes reference generated columns with underscore prefix (_fieldName)
- Generates compound indexes: maps schema.Indexes to CREATE INDEX with multiple columns
- Compound indexes use custom index names from IndexDef.Name
- Multiple columns joined with ", " separator, all prefixed with underscore
- All statements use IF NOT EXISTS clause
- Comprehensive XML docs with summary, param, returns, detailed remarks, and example

Verification:
- dotnet build: 0 errors, 0 warnings ✅
- task lint: 0 warnings ✅
- dotnet test: 66/66 tests passing ✅

TableBuilder.fs now has all 3 core functions: mapSqliteType, createTableSql, createIndexesSql.
Ready for Task 44: ensureTable function to execute DDL statements.
<!-- SECTION:NOTES:END -->
