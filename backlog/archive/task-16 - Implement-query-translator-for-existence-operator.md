---
id: task-16
title: Implement query translator for existence operator
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
Extend the query translator to support the  operator for checking field presence. This operator differentiates between null values and missing fields.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Extend QueryTranslator in src/query/translator.ts
- [ ] #2 Implement translateExistenceOperator() method
- [ ] #3 Handle { field: { : true } } checking json_type(body, '$.field') IS NOT NULL
- [ ] #4 Handle { field: { : false } } checking json_type(body, '$.field') IS NULL
- [ ] #5 Properly distinguish between null values (exist but are null) and missing fields
- [ ] #6 Add TypeDoc comment explaining null vs undefined behavior
- [ ] #7 All code compiles with strict mode
- [ ] #8 No use of any type
<!-- AC:END -->
