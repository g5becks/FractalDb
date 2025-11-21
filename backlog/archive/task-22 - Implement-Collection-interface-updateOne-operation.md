---
id: task-22
title: Implement Collection interface - updateOne operation
status: To Do
assignee: []
created_date: '2025-11-21 01:47'
updated_date: '2025-11-21 02:06'
labels:
  - collection
  - write
dependencies:
  - task-19
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Implement the updateOne method for updating a single document by ID with partial updates and upsert support.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Extend Collection class in src/collection/collection.ts
- [ ] #2 Implement updateOne(id, update, options): Promise<T | null> method
- [ ] #3 Accept DocumentUpdate<T> for partial field updates
- [ ] #4 Support options.upsert boolean flag
- [ ] #5 Read existing document using findById()
- [ ] #6 Return null if document not found and upsert is false
- [ ] #7 Merge update with existing document using deep merge
- [ ] #8 Update updatedAt timestamp if schema.timestamps is true
- [ ] #9 Validate merged document using schema.validate()
- [ ] #10 Execute UPDATE table SET body = ? WHERE id = ?
- [ ] #11 If upsert is true and document not found, perform insertOne with provided id
- [ ] #12 Return updated/inserted document
- [ ] #13 Add TypeDoc comment explaining update and upsert behavior
- [ ] #14 All code compiles with strict mode
- [ ] #15 No use of any or unsafe assertions
<!-- AC:END -->
