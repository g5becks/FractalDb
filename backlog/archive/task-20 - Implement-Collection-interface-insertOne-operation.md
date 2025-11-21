---
id: task-20
title: Implement Collection interface - insertOne operation
status: To Do
assignee: []
created_date: '2025-11-21 01:46'
updated_date: '2025-11-21 02:05'
labels:
  - collection
  - write
dependencies:
  - task-19
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Implement the insertOne method for inserting a single document with validation, ID generation, and timestamp management.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Extend Collection class in src/collection/collection.ts
- [ ] #2 Implement insertOne(doc: DocumentInput<T>): Promise<T> method
- [ ] #3 Generate ID using idGenerator if doc.id is not provided
- [ ] #4 Validate document using schema.validate() function
- [ ] #5 Throw SchemaValidationError if validation fails with detailed message
- [ ] #6 Add createdAt and updatedAt timestamps if schema.timestamps is true
- [ ] #7 Serialize document to JSONB using JSON utilities
- [ ] #8 Execute INSERT INTO table (id, body) VALUES (?, ?)
- [ ] #9 Return complete document with generated ID and timestamps
- [ ] #10 Handle unique constraint violations with UniqueConstraintError
- [ ] #11 Add TypeDoc comment explaining insertion behavior
- [ ] #12 All code compiles with strict mode
- [ ] #13 No use of any or unsafe assertions
<!-- AC:END -->
