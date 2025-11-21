---
id: task-7
title: Implement SchemaBuilder fluent API
status: To Do
assignee: []
created_date: '2025-11-21 01:44'
updated_date: '2025-11-21 02:02'
labels:
  - schema
  - builder
dependencies:
  - task-3
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Create the SchemaBuilder class that provides a type-safe fluent API for defining collection schemas. The builder validates TypeScript-to-SQLite type mappings at compile time and constructs SchemaDefinition objects.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Create src/schema/builder.ts file
- [ ] #2 Implement SchemaBuilder<T extends Document> class with private fields array and options object
- [ ] #3 Implement field<K extends keyof T>() method with type parameter and options
- [ ] #4 Validate that options.type matches TypeScriptToSQLite<T[K]> at compile time
- [ ] #5 Default path to $.{fieldName} when path is omitted
- [ ] #6 Implement compoundIndex() method accepting name, fields array, and unique option
- [ ] #7 Implement timestamps() method to enable/disable automatic timestamp management
- [ ] #8 Implement validate() method accepting type predicate function
- [ ] #9 Implement build() method returning immutable SchemaDefinition<T>
- [ ] #10 All methods return this for method chaining except build()
- [ ] #11 Export SchemaBuilder from src/schema/index.ts
- [ ] #12 Builder compiles with strict mode
- [ ] #13 No use of any or unsafe assertions

- [ ] #14 Add comprehensive TypeDoc comments with usage examples for all public methods
<!-- AC:END -->
