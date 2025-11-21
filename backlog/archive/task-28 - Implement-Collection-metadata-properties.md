---
id: task-28
title: Implement Collection metadata properties
status: To Do
assignee: []
created_date: '2025-11-21 01:48'
updated_date: '2025-11-21 02:06'
labels:
  - collection
  - metadata
dependencies:
  - task-19
priority: medium
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add readonly metadata properties to Collection class exposing collection name and schema for introspection.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Extend Collection class in src/collection/collection.ts
- [ ] #2 Add readonly name: string property returning collection name
- [ ] #3 Add readonly schema: SchemaDefinition<T> property returning schema
- [ ] #4 Implement getter methods for properties
- [ ] #5 Add TypeDoc comments explaining metadata access
- [ ] #6 Export Collection from src/collection/index.ts
- [ ] #7 All code compiles with strict mode
- [ ] #8 No use of any type
<!-- AC:END -->
