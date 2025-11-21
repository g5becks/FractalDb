---
id: task-6
title: Define schema definition and validation types
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
Implement the SchemaDefinition type that combines fields, indexes, timestamps, and validation. This type represents the complete configuration for a collection and is used throughout the library to ensure consistent schema enforcement.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 SchemaDefinition<T> type defined with fields, compoundIndexes, timestamps, and validate properties
- [ ] #2 validate property uses Standard Schema type predicate signature (doc: unknown) => doc is T
- [ ] #3 All properties appropriately marked as readonly
- [ ] #4 Type correctly constrains fields array to SchemaField<T, keyof T> elements
- [ ] #5 Type correctly constrains compoundIndexes to CompoundIndex<T> elements
- [ ] #6 TypeScript type checking passes with zero errors
- [ ] #7 No any types used in implementation
- [ ] #8 Complete TypeDoc comments with examples showing complete schema definition structure
<!-- AC:END -->
