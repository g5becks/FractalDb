---
id: task-41
title: Write integration tests for SchemaBuilder
status: To Do
assignee: []
created_date: '2025-11-21 01:51'
updated_date: '2025-11-21 02:07'
labels:
  - testing
  - schema
  - integration
dependencies:
  - task-7
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Test the SchemaBuilder fluent API with real schema definitions, verifying field validation and schema construction.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Create test/schema/builder.test.ts file
- [ ] #2 Test field() method adds fields with correct types
- [ ] #3 Test path defaults to $.{fieldName} when omitted
- [ ] #4 Test compoundIndex() creates compound indexes
- [ ] #5 Test timestamps() enables timestamp management
- [ ] #6 Test validate() accepts type predicate function
- [ ] #7 Test build() returns immutable SchemaDefinition
- [ ] #8 Test that invalid type mappings cause TypeScript errors at compile time
- [ ] #9 All tests pass with bun test
- [ ] #10 Tests compile with strict mode
- [ ] #11 No use of any type in tests
<!-- AC:END -->
