---
id: task-36
title: Implement Collection validation methods
status: Done
assignee: []
created_date: '2025-11-21 02:58'
updated_date: '2025-11-21 22:21'
labels:
  - collection
  - validation
dependencies: []
priority: medium
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Implement document validation methods using schema validators. These methods provide both async and sync validation, throwing descriptive errors when validation fails and returning typed documents when successful.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 validate method accepts unknown parameter and returns Promise<T>
- [x] #2 validate method calls schema validate function asynchronously
- [x] #3 validateSync method accepts unknown parameter and returns T synchronously
- [x] #4 validateSync method calls schema validate function synchronously
- [x] #5 Both methods throw ValidationError with field and value context on validation failure
- [x] #6 Both methods act as type guards, narrowing unknown to T on success
- [x] #7 TypeScript type checking passes with zero errors
- [x] #8 No any types used in implementation
- [x] #9 Complete TypeDoc comments with examples showing validation error handling
<!-- AC:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Completed Collection validation methods implementation and comprehensive testing.

## Implementation
- Both validate() and validateSync() methods were already implemented in SQLiteCollection
- Methods accept unknown input and return properly typed documents
- Type predicates narrow unknown to T on successful validation
- Throws ValidationError with descriptive context on validation failure

## Testing
- Created test/integration/validation.test.ts with 18 comprehensive tests
- Covers valid/invalid document validation for both async and sync methods
- Tests edge cases: null, undefined, primitives, arrays, missing fields
- Validates custom business rules and type narrowing behavior
- All tests passing (18 pass, 0 fail)

## Type Safety
- Zero type casts in implementation
- Complete TypeDoc documentation with examples
- Type checking passes with no errors
<!-- SECTION:NOTES:END -->
