---
id: task-12
title: Implement query translator for comparison operators
status: To Do
assignee: []
created_date: '2025-11-21 01:45'
updated_date: '2025-11-21 02:03'
labels:
  - query
  - translator
dependencies:
  - task-3
  - task-4
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Create the query translator that converts QueryFilter to SQL WHERE clauses. Start with comparison operators (, , , , , , , ). This is the foundation of the query engine.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Create src/query/translator.ts file
- [ ] #2 Implement QueryTranslator<T extends Document> class with schema and tableName
- [ ] #3 Implement translate() method accepting QueryFilter<T> and returning { sql: string, params: unknown[] }
- [ ] #4 Implement translateFieldOperator() for comparison operators
- [ ] #5 Generate parameterized SQL using ? placeholders for all values
- [ ] #6 Use generated column names (_fieldname) for indexed fields
- [ ] #7 Use jsonb_extract(body, '$.path') for non-indexed fields
- [ ] #8 Handle  with = operator
- [ ] #9 Handle  with \!= operator
- [ ] #10 Handle , , ,  with >, >=, <, <= operators
- [ ] #11 Handle  with IN (?, ?, ...) syntax
- [ ] #12 Handle  with NOT IN (?, ?, ...) syntax
- [ ] #13 Properly escape field names and paths
- [ ] #14 Add error handling for invalid operators with descriptive TypeMismatchError
- [ ] #15 Add TypeDoc comments for translator methods
- [ ] #16 All code compiles with strict mode
- [ ] #17 No use of any or unsafe assertions
<!-- AC:END -->
