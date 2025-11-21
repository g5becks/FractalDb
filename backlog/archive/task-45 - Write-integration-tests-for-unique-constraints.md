---
id: task-45
title: Write integration tests for unique constraints
status: To Do
assignee: []
created_date: '2025-11-21 01:52'
updated_date: '2025-11-21 02:07'
labels:
  - testing
  - collection
  - constraints
dependencies:
  - task-20
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Test that unique constraints are properly enforced and UniqueConstraintError is thrown with appropriate context.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Create test/collection/constraints.test.ts file
- [ ] #2 Test unique field constraint prevents duplicate values
- [ ] #3 Test insertOne throws UniqueConstraintError with field name and value
- [ ] #4 Test insertMany reports unique constraint violations in errors array
- [ ] #5 Test compound unique index prevents duplicate combinations
- [ ] #6 Test updateOne respects unique constraints
- [ ] #7 Error messages include helpful context about constraint violation
- [ ] #8 All tests pass with bun test
- [ ] #9 Tests compile with strict mode
<!-- AC:END -->
