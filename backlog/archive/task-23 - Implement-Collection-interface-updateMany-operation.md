---
id: task-23
title: Implement Collection interface - updateMany operation
status: To Do
assignee: []
created_date: '2025-11-21 01:47'
updated_date: '2025-11-21 02:06'
labels:
  - collection
  - write
dependencies:
  - task-22
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Implement the updateMany method for updating multiple documents matching a query filter with partial updates applied to all matches.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Extend Collection class in src/collection/collection.ts
- [ ] #2 Implement updateMany(filter, update): Promise<{ modifiedCount: number }> method
- [ ] #3 Use QueryTranslator to generate WHERE clause from filter
- [ ] #4 Find all matching documents using find(filter)
- [ ] #5 For each document: merge update, update timestamp, validate
- [ ] #6 Use SQLite transaction for atomic batch updates
- [ ] #7 Execute UPDATE for each modified document
- [ ] #8 Count number of successfully updated documents
- [ ] #9 Return { modifiedCount: number } result object
- [ ] #10 Handle validation errors by throwing SchemaValidationError
- [ ] #11 Add TypeDoc comment explaining batch update behavior
- [ ] #12 All code compiles with strict mode
- [ ] #13 No use of any or unsafe assertions
<!-- AC:END -->
