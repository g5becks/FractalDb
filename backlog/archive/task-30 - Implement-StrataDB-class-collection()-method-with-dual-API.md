---
id: task-30
title: Implement StrataDB class - collection() method with dual API
status: To Do
assignee: []
created_date: '2025-11-21 01:48'
updated_date: '2025-11-21 02:06'
labels:
  - database
  - api
dependencies:
  - task-29
  - task-7
  - task-19
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Implement the collection() method with function overloads supporting both fluent and declarative API patterns. This is the primary entry point for accessing collections.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Extend StrataDB class in src/database/database.ts
- [ ] #2 Implement collection<T extends Document>(name): SchemaBuilder<T> overload for fluent API
- [ ] #3 Return new SchemaBuilder instance with collection name bound
- [ ] #4 Implement collection<T extends Document>(name, schema): Collection<T> overload for declarative API
- [ ] #5 Create or access existing table using schema
- [ ] #6 Execute generateTableSQL() to create table and indexes
- [ ] #7 Return new Collection instance with name, schema, sqliteDb, and idGenerator
- [ ] #8 Add private createTable() helper method
- [ ] #9 Add TypeDoc comments explaining both API patterns with examples
- [ ] #10 All code compiles with strict mode
- [ ] #11 No use of any or unsafe assertions

- [ ] #12 Add comprehensive TypeDoc comments with usage examples explaining both API patterns
<!-- AC:END -->
