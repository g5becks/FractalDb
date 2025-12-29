---
id: task-41
title: Implement TableBuilder - mapSqliteType Function
status: Done
assignee:
  - '@assistant'
created_date: '2025-12-28 06:37'
updated_date: '2025-12-28 18:45'
labels:
  - phase-2
  - storage
  - tablebuilder
dependencies:
  - task-40
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Create TableBuilder module with SQLite type mapping in src/TableBuilder.fs. Reference: FSHARP_PORT_DESIGN.md lines 1353-1398.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 Add namespace FractalDb.Storage
- [x] #2 Add module internal TableBuilder
- [x] #3 Add 'let mapSqliteType (sqlType: SqliteType) : string' function
- [x] #4 Map: Text->TEXT, Integer->INTEGER, Real->REAL, Blob->BLOB, Numeric->NUMERIC, Boolean->INTEGER
- [x] #5 Run 'dotnet build' - build succeeds
- [x] #6 All public types and functions have XML doc comments with <summary>, and public functions include <param>, <returns>, and <example> (see doc-2 for standards)
- [x] #7 Run 'task lint' - no errors or warnings

- [x] #8 Create file src/TableBuilder.fs

- [x] #9 Add module declaration: module FractalDb.TableBuilder
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Create src/TableBuilder.fs file
2. Add module declaration: module FractalDb.TableBuilder
3. Implement mapSqliteType function with pattern matching for all 6 SqliteType cases
4. Add comprehensive XML documentation following doc-2 standards
5. Add TableBuilder.fs to FractalDb.fsproj after SqlTranslator.fs
6. Build and verify with dotnet build
7. Run task lint to verify no errors/warnings
<!-- SECTION:PLAN:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Created src/TableBuilder.fs with mapSqliteType function that maps SqliteType DU cases to SQLite SQL type strings.

Key implementation details:
- All 6 SqliteType cases mapped: Text→TEXT, Integer→INTEGER, Real→REAL, Blob→BLOB, Numeric→NUMERIC, Boolean→INTEGER
- Boolean uses INTEGER storage (SQLite convention: 0=false, 1=true)
- Comprehensive XML documentation following doc-2 standards with summary, param, returns, remarks, and example tags
- Module declared as "module FractalDb.TableBuilder" (internal visibility)
- Added to FractalDb.fsproj after SqlTranslator.fs in dependency order

Verification:
- dotnet build: 0 errors, 0 warnings ✅
- task lint: 0 warnings ✅
- dotnet test: 66/66 tests passing ✅

Ready for Task 42: createTableSql function to generate CREATE TABLE statements with generated columns.
<!-- SECTION:NOTES:END -->
