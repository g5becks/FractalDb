---
id: task-5
title: Define schema field and index types
status: To Do
assignee: []
created_date: '2025-11-21 02:30'
labels:
  - schema
  - types
dependencies: []
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Implement type-safe schema definition types that map TypeScript types to SQLite column types. The TypeScriptToSQLite mapping ensures compile-time validation that schema field types match document property types, preventing runtime type mismatches.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 SQLiteType union type defined with all SQLite column types (TEXT, INTEGER, REAL, BOOLEAN, NUMERIC, BLOB)
- [ ] #2 TypeScriptToSQLite<T> conditional type maps TypeScript types to compatible SQLite types
- [ ] #3 SchemaField<T, K> type defined with name, path, type, nullable, indexed, unique, and default properties
- [ ] #4 CompoundIndex<T> type defined with name, fields array, and unique option
- [ ] #5 Type constraint ensures SchemaField type property matches TypeScriptToSQLite<T[K]>
- [ ] #6 TypeScript type checking passes with zero errors
- [ ] #7 No any types used in implementation
- [ ] #8 Complete TypeDoc comments with examples showing correct and incorrect field type mappings
<!-- AC:END -->
