---
id: task-49
title: Write integration tests with Zod validator
status: To Do
assignee: []
created_date: '2025-11-21 01:52'
updated_date: '2025-11-21 02:07'
labels:
  - testing
  - validation
  - zod
dependencies:
  - task-27
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Test Standard Schema integration with Zod to verify validation works correctly with a popular validator.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Create test/validation/zod.test.ts file
- [ ] #2 Define Zod schema for test document type
- [ ] #3 Implement type predicate using Zod schema.safeParse()
- [ ] #4 Test valid documents pass Zod validation
- [ ] #5 Test invalid documents fail validation with SchemaValidationError
- [ ] #6 Test error messages include Zod validation details
- [ ] #7 Test insertOne rejects invalid documents
- [ ] #8 Test updateOne validates merged documents
- [ ] #9 All tests pass with bun test
- [ ] #10 Tests compile with strict mode
<!-- AC:END -->
