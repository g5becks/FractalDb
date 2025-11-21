---
id: task-48
title: Write integration tests for dual API patterns
status: To Do
assignee: []
created_date: '2025-11-21 01:52'
updated_date: '2025-11-21 02:07'
labels:
  - testing
  - api
  - integration
dependencies:
  - task-7
  - task-8
  - task-30
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Test both fluent and declarative API patterns to ensure they produce equivalent results and maintain type safety.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Create test/database/api-patterns.test.ts file
- [ ] #2 Test fluent API: db.collection<T>(name).field(...).build() creates working collection
- [ ] #3 Test declarative API: createSchema<T>() then db.collection<T>(name, schema) creates equivalent collection
- [ ] #4 Test both APIs produce same schema structure
- [ ] #5 Test both APIs enforce same type safety (TypeScript compile-time)
- [ ] #6 Test collection operations work identically with both APIs
- [ ] #7 Test fluent API method chaining returns correct types
- [ ] #8 All tests pass with bun test
- [ ] #9 Tests compile with strict mode
<!-- AC:END -->
