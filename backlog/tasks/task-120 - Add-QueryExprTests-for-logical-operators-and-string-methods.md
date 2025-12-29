---
id: task-120
title: Add QueryExprTests for logical operators and string methods
status: Done
assignee:
  - '@assistant'
created_date: '2025-12-29 06:12'
updated_date: '2025-12-29 18:38'
labels:
  - tests
  - query-expressions
dependencies:
  - task-119
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Extend QueryExprTests with tests for logical operators (AND, OR, NOT) and string methods (Contains, StartsWith, EndsWith).
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 Test for where with AND (&&)
- [x] #2 Test for where with OR (||)
- [x] #3 Test for where with NOT
- [x] #4 Test for where with nested AND/OR
- [x] #5 Test for multiple where clauses combine with AND
- [x] #6 Test for String.Contains
- [x] #7 Test for String.StartsWith
- [x] #8 Test for String.EndsWith
- [x] #9 All tests pass
<!-- AC:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Added 8 comprehensive tests for logical operators and string methods to QueryExprTests.fs:

Logical Operators (5 tests):
- AND operator (&&) - verifies translation to Query.And
- OR operator (||) - verifies translation to Query.Or
- NOT operator (not) - verifies translation to Query.Not
- Multiple where clauses - confirms they combine with AND
- Nested AND/OR - tests complex condition translation

String Methods (3 tests):
- String.Contains() - verifies translation to StringOp.Contains
- String.StartsWith() - verifies translation to StringOp.StartsWith
- String.EndsWith() - verifies translation to StringOp.EndsWith

Key Implementation Detail:
Discovered that F# compiler desugars && and || into IfThenElse expressions for short-circuit evaluation:
- a && b → if a then b else false
- a || b → if a then true else b

Added IfThenElse pattern matching to translatePredicate in QueryExpr.fs to handle these desugared forms, plus support for direct boolean property access (e.g., user.Active or not user.Active).

All 19 tests passing (11 from task-119 + 8 new tests from task-120).
<!-- SECTION:NOTES:END -->
