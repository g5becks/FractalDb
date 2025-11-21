---
id: task-8
title: Define string and array query operators
status: To Do
assignee: []
created_date: '2025-11-21 02:30'
labels:
  - query
  - types
dependencies: []
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Implement specialized operator types for string and array fields. String operators enable pattern matching and text search, while array operators provide MongoDB-like array query capabilities with full type safety.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 StringOperator type defined with regex, options, like, startsWith, and endsWith operators
- [ ] #2 ArrayOperator<T> type defined extracting array element type using conditional type inference
- [ ] #3 ArrayOperator includes all, size, elemMatch, and index operators
- [ ] #4 ArrayOperator constrained to only work on array types using conditional types
- [ ] #5 elemMatch operator uses QueryFilter recursively for nested queries
- [ ] #6 TypeScript type checking passes with zero errors
- [ ] #7 No any types used in implementation
- [ ] #8 Complete TypeDoc comments with examples showing string pattern matching and array operations
<!-- AC:END -->
