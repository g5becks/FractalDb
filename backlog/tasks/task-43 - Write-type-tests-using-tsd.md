---
id: task-43
title: Write type tests using tsd
status: To Do
assignee: []
created_date: '2025-11-21 02:59'
labels:
  - testing
dependencies: []
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Create comprehensive type-level tests that verify compile-time type safety guarantees. These tests ensure the type system prevents invalid queries and operations, catching type errors before runtime.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Type tests verify DocumentInput makes id optional while requiring all other fields
- [ ] #2 Type tests verify DocumentUpdate makes all fields optional and excludes id
- [ ] #3 Type tests verify comparison operators reject invalid type combinations
- [ ] #4 Type tests verify string operators only available on string fields
- [ ] #5 Type tests verify array operators only available on array fields
- [ ] #6 Type tests verify nested path types correctly extract property types
- [ ] #7 Type tests verify invalid paths cause compilation errors
- [ ] #8 All type tests pass using tsd expectType and expectError assertions
- [ ] #9 Complete TypeDoc comments explaining what each type test validates
<!-- AC:END -->
