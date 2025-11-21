---
id: task-36
title: Implement SchemaBuilder build() integration with Collection
status: To Do
assignee: []
created_date: '2025-11-21 01:50'
updated_date: '2025-11-21 02:07'
labels:
  - schema
  - integration
dependencies:
  - task-7
  - task-30
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Modify SchemaBuilder.build() to return a function that creates a Collection when provided with database context. This enables the fluent API pattern.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Extend SchemaBuilder class in src/schema/builder.ts
- [ ] #2 Modify build() to return a factory function that accepts StrataDB context
- [ ] #3 Factory function should call StrataDB's internal createCollection() method
- [ ] #4 Ensure collection name is captured in closure
- [ ] #5 Maintain type safety through generic type parameter
- [ ] #6 Update StrataDB to handle SchemaBuilder factory functions
- [ ] #7 Add TypeDoc comment explaining the fluent API integration
- [ ] #8 All code compiles with strict mode
- [ ] #9 No use of any or unsafe assertions
<!-- AC:END -->
