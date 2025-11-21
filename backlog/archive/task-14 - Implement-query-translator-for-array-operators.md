---
id: task-14
title: Implement query translator for array operators
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
Extend the query translator to support array operators (, , , ). These operators enable querying array fields and elements.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Extend QueryTranslator in src/query/translator.ts
- [ ] #2 Implement translateArrayOperator() method
- [ ] #3 Handle  operator using jsonb_each() to check all values present in array
- [ ] #4 Handle  operator using json_array_length() function
- [ ] #5 Handle  operator with nested query conditions using jsonb_each()
- [ ] #6 Handle  operator for accessing specific array elements by position
- [ ] #7 Support negative array indexing using [#-N] SQLite syntax
- [ ] #8 Properly generate subqueries for complex array conditions
- [ ] #9 Validate array operators only used with array fields
- [ ] #10 Add TypeDoc comments explaining array operator SQL generation
- [ ] #11 All code compiles with strict mode
- [ ] #12 No use of any or unsafe type assertions
<!-- AC:END -->
