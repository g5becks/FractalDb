---
id: task-44
title: Implement TableBuilder - ensureTable Function
status: Done
assignee:
  - '@assistant'
created_date: '2025-12-28 06:37'
updated_date: '2025-12-28 18:50'
labels:
  - phase-2
  - storage
  - tablebuilder
dependencies:
  - task-43
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add function to ensure table and indexes exist in src/TableBuilder.fs. Reference: FSHARP_PORT_DESIGN.md lines 1454.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 In TableBuilder module, add 'let ensureTable (conn: IDbConnection) (name: string) (schema: SchemaDef<'T>) : unit'
- [x] #2 Execute createTableSql to create table
- [x] #3 Execute each statement from createIndexesSql to create indexes
- [x] #4 Use Donald Db.newCommand and Db.exec for execution
- [x] #5 Run 'dotnet build' - build succeeds

- [x] #6 All public types and functions have XML doc comments with <summary>, and public functions include <param>, <returns>, and <example> (see doc-2 for standards)

- [x] #7 Run 'task lint' - no errors or warnings
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Add Donald and Microsoft.Data.Sqlite package references to FractalDb.fsproj
2. Read design spec for ensureTable implementation pattern
3. Add open statements for System.Data and Donald in TableBuilder.fs
4. Implement ensureTable function with IDbConnection parameter
5. Call createTableSql to get CREATE TABLE statement
6. Execute table creation using Donald Db.newCommand and Db.exec
7. Call createIndexesSql to get index statements list
8. Iterate and execute each index statement using Donald
9. Add comprehensive XML documentation
10. Build and verify with dotnet build and task lint
<!-- SECTION:PLAN:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Implemented ensureTable function in src/TableBuilder.fs to execute DDL statements and ensure tables/indexes exist.

Key implementation details:
- Added Donald (v10.*) and Microsoft.Data.Sqlite (v9.*) package references to FractalDb.fsproj
- Added open statements: System.Data and Donald
- Function signature: let ensureTable (conn: IDbConnection) (name: string) (schema: SchemaDef<'T>) : unit
- Calls createTableSql to generate CREATE TABLE statement
- Executes table creation using Donald pipeline: conn |> Db.newCommand sql |> Db.exec |> ignore
- Calls createIndexesSql to get list of index statements
- Iterates each index statement with List.iter, executes using Donald
- All statements use IF NOT EXISTS, making function idempotent
- Comprehensive XML docs with summary, param, returns, detailed remarks, exception handling notes, and example

Verification:
- dotnet build: 0 errors, 0 warnings ✅ (restored new packages)
- task lint: 0 warnings ✅
- dotnet test: 66/66 tests passing ✅

TableBuilder.fs module complete (281 lines) with all 4 functions: mapSqliteType, createTableSql, createIndexesSql, ensureTable.
Ready for Task 45: Transaction type implementation.
<!-- SECTION:NOTES:END -->
