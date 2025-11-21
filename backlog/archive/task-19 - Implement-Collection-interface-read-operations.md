---
id: task-19
title: Implement Collection interface - read operations
status: To Do
assignee: []
created_date: '2025-11-21 01:46'
updated_date: '2025-11-21 02:05'
labels:
  - collection
  - read
dependencies:
  - task-2
  - task-3
  - task-9
  - task-10
  - task-11
  - task-12
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Create the Collection class implementing read operations (findById, find, findOne, count). These are the core query methods that users interact with.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Create src/collection/collection.ts file
- [ ] #2 Implement Collection<T extends Document> class with constructor accepting name, schema, sqliteDb, idGenerator
- [ ] #3 Implement findById(id: string): Promise<T | null> method
- [ ] #4 Query using SELECT id, body FROM table WHERE id = ?
- [ ] #5 Deserialize JSONB body to typed document
- [ ] #6 Return null if document not found
- [ ] #7 Implement find(filter, options): Promise<ReadonlyArray<T>> method
- [ ] #8 Use QueryTranslator to generate WHERE clause from filter
- [ ] #9 Apply sort, limit, skip from options
- [ ] #10 Return array of deserialized documents
- [ ] #11 Implement findOne(filter, options): Promise<T | null> method
- [ ] #12 Identical to find but with LIMIT 1, return first result or null
- [ ] #13 Implement count(filter): Promise<number> method
- [ ] #14 Generate SELECT COUNT(*) query with filter
- [ ] #15 Add TypeDoc comments for all public methods
- [ ] #16 All code compiles with strict mode
- [ ] #17 No use of any or unsafe assertions except for SQLite row types

- [ ] #18 Add comprehensive TypeDoc comments with usage examples for all public methods
<!-- AC:END -->
