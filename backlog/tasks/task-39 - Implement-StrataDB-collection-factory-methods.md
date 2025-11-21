---
id: task-39
title: Implement StrataDB collection factory methods
status: To Do
assignee: []
created_date: '2025-11-21 02:58'
labels:
  - database
dependencies: []
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Implement the overloaded collection methods that support both fluent schema building and pre-built schema passing. These methods create Collection instances with proper table initialization and schema management.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 collection method with schema parameter creates Collection instance directly
- [ ] #2 collection method without schema parameter returns SchemaBuilder instance
- [ ] #3 SchemaBuilder build method creates Collection instance with built schema
- [ ] #4 Collection creation initializes table with generated columns for indexed fields
- [ ] #5 Method properly handles collection name as table name in SQLite
- [ ] #6 Collection instances receive schema, translator, and database connection references
- [ ] #7 TypeScript type checking passes with zero errors
- [ ] #8 No any types used in implementation
- [ ] #9 Complete TypeDoc comments with examples showing both API styles
<!-- AC:END -->
