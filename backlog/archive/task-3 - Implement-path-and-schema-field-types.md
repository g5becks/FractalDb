---
id: task-3
title: Implement path and schema field types
status: To Do
assignee: []
created_date: '2025-11-21 01:43'
updated_date: '2025-11-21 02:01'
labels:
  - types
  - schema
dependencies:
  - task-1
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Implement the type system for JSON paths and schema field definitions, including the critical TypeScriptToSQLite type mapping that ensures compile-time type safety.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Create src/types/schema.ts file
- [ ] #2 Implement JsonPath branded type as $.
- [ ] #3 Implement SQLiteType union (TEXT, INTEGER, REAL, BOOLEAN, NUMERIC, BLOB)
- [ ] #4 Implement TypeScriptToSQLite<T> conditional type with mappings for string, number, boolean, Date, Uint8Array, ArrayBuffer, Array, and object
- [ ] #5 Implement DocumentPath<T> type using Paths from type-fest
- [ ] #6 Implement PathValue<T, P> type using Get from type-fest
- [ ] #7 Implement SchemaField<T, K> type with name, path, type, nullable, indexed, unique, and default properties
- [ ] #8 Implement CompoundIndex<T> type with name, fields, and unique properties
- [ ] #9 Implement SchemaDefinition<T> type with fields, compoundIndexes, timestamps, and validate properties
- [ ] #10 Add comprehensive TypeDoc comments for all types
- [ ] #11 Export all types from src/types/index.ts
- [ ] #12 All types compile with strict mode enabled
- [ ] #13 No use of any type
<!-- AC:END -->
