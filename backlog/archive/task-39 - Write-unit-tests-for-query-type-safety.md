---
id: task-39
title: Write unit tests for query type safety
status: To Do
assignee: []
created_date: '2025-11-21 01:50'
updated_date: '2025-11-21 02:07'
labels:
  - testing
  - types
  - query
dependencies:
  - task-4
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Create comprehensive tests verifying that query operators are type-safe and only allow valid operator/field type combinations.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Create test/types/query.test.ts file
- [ ] #2 Test comparison operators only work with compatible types
- [ ] #3 Test , , ,  are rejected for string fields at type level
- [ ] #4 Test string operators (, ) only work with string fields
- [ ] #5 Test array operators only work with array fields
- [ ] #6 Test QueryFilter allows nested property access with correct types
- [ ] #7 Test logical operators (, , , ) with nested conditions
- [ ] #8 Use type-level assertions to verify compile errors for invalid queries
- [ ] #9 All tests pass with bun test
- [ ] #10 Tests compile with strict mode
<!-- AC:END -->
