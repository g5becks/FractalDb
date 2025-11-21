---
id: task-13
title: Implement query translator for string operators
status: To Do
assignee: []
created_date: '2025-11-21 01:45'
updated_date: '2025-11-21 02:03'
labels:
  - query
  - translator
dependencies:
  - task-12
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Extend the query translator to support string-specific operators (, , , ). These operators only work with string fields.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Extend QueryTranslator in src/query/translator.ts
- [ ] #2 Implement translateStringOperator() method
- [ ] #3 Handle  operator converting RegExp to SQLite REGEXP with proper escaping
- [ ] #4 Support off on off off off off off on off off on off on off on off off off on off off off on off off off off on on off off off off off on off on off off off off on off on off off off off off on off on on off off off on off off on off off on off off on on on off on on off on off off on off on off off off on off off on off on off off off on on off on off on off off on off off off on off off off off off off off off on off on off on on on off on on off off on off off on off off on on off on off on on off off off off off on off off on off off off off off on off off on on off on off off off off off off on off off off off on on off on off off off off off off off on on off off on off off off off off off off on off off off off off: 'i' flag for case-insensitive regex
- [ ] #5 Handle  operator mapping to LIKE with % wildcards
- [ ] #6 Handle  using LIKE 'value%' pattern
- [ ] #7 Handle  using LIKE '%value' pattern
- [ ] #8 Properly escape special characters in LIKE patterns (%, _)
- [ ] #9 Validate that string operators are only used with string fields
- [ ] #10 Throw TypeMismatchError when string operators used with non-string fields
- [ ] #11 Add TypeDoc comments explaining string operator behavior
- [ ] #12 All code compiles with strict mode
- [ ] #13 No use of any type
<!-- AC:END -->
