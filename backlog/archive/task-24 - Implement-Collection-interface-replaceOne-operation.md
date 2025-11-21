---
id: task-24
title: Implement Collection interface - replaceOne operation
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
Implement the replaceOne method for completely replacing a document while preserving its ID. Unlike updateOne, this replaces the entire document body.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Extend Collection class in src/collection/collection.ts
- [ ] #2 Implement replaceOne(id, doc): Promise<T | null> method
- [ ] #3 Accept Except<T, 'id'> excluding id field from replacement document
- [ ] #4 Verify document exists using findById()
- [ ] #5 Return null if document not found
- [ ] #6 Preserve original document ID
- [ ] #7 Update updatedAt timestamp if schema.timestamps is true
- [ ] #8 Preserve createdAt from original document
- [ ] #9 Validate replacement document using schema.validate()
- [ ] #10 Execute UPDATE table SET body = ? WHERE id = ?
- [ ] #11 Return replaced document with preserved ID and timestamps
- [ ] #12 Add TypeDoc comment explaining replace vs update difference
- [ ] #13 All code compiles with strict mode
- [ ] #14 No use of any or unsafe assertions
<!-- AC:END -->
