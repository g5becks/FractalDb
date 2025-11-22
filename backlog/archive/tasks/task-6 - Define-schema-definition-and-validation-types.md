---
id: task-6
title: Define schema definition and validation types
status: Done
assignee:
  - '@claude'
created_date: '2025-11-21 02:30'
updated_date: '2025-11-21 04:56'
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
- [x] #1 SchemaDefinition<T> type defined with fields, compoundIndexes, timestamps, and validate properties
- [x] #2 validate property uses Standard Schema type predicate signature (doc: unknown) => doc is T
- [x] #3 All properties appropriately marked as readonly
- [x] #4 Type correctly constrains fields array to SchemaField<T, keyof T> elements
- [x] #5 Type correctly constrains compoundIndexes to CompoundIndex<T> elements
- [x] #6 TypeScript type checking passes with zero errors
- [x] #7 No any types used in implementation
- [x] #8 Complete TypeDoc comments with examples showing complete schema definition structure
<!-- AC:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Implemented comprehensive SchemaDefinition<T> type that combines all schema configuration elements:

- Added TimestampConfig type for automatic timestamp management with both boolean and object configuration options
- Created SchemaDefinition<T> type with readonly properties for fields, compoundIndexes, timestamps, and validation
- Used Standard Schema type predicate signature for validation: (doc: unknown) => doc is T
- Ensured type safety with proper constraints: SchemaField<T, keyof T>[] and CompoundIndex<T>[]
- Maintained zero 'any' types throughout implementation
- Added comprehensive TypeDoc documentation with examples showing complete schema structure
- Fixed linting issues using consistent readonly T[] syntax instead of ReadonlyArray<T>
- All TypeScript compilation passes with zero errors

The SchemaDefinition type now serves as the foundation for type-safe collection creation, combining field definitions, compound indexes, timestamp configuration, and validation logic into a single cohesive interface.
<!-- SECTION:NOTES:END -->
