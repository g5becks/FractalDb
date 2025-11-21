---
id: task-37
title: Create main index.ts with public API exports
status: To Do
assignee: []
created_date: '2025-11-21 01:50'
updated_date: '2025-11-21 02:07'
labels:
  - api
  - exports
dependencies:
  - task-2
  - task-3
  - task-4
  - task-5
  - task-6
  - task-7
  - task-8
  - task-19
  - task-29
  - task-32
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Create the main entry point that exports all public types, classes, and functions for library consumers. This defines the public API surface.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Create/update src/index.ts file
- [ ] #2 Export Document, DocumentInput, DocumentUpdate, BulkWriteResult types
- [ ] #3 Export JsonPath, SQLiteType, TypeScriptToSQLite types
- [ ] #4 Export SchemaField, CompoundIndex, SchemaDefinition types
- [ ] #5 Export QueryFilter, QueryOptions, SortSpec, ProjectionSpec types
- [ ] #6 Export all error classes from errors/
- [ ] #7 Export StrataDB class and DatabaseOptions type
- [ ] #8 Export Transaction class
- [ ] #9 Export Collection class
- [ ] #10 Export createSchema function
- [ ] #11 Export SchemaBuilder type (for advanced users)
- [ ] #12 Do NOT export internal implementation details
- [ ] #13 Add TypeDoc module-level comment with library overview
- [ ] #14 All exports compile with strict mode
- [ ] #15 No use of any type in public API
<!-- AC:END -->
