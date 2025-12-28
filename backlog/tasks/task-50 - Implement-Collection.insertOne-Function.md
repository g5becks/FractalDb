---
id: task-50
title: Implement Collection.insertOne Function
status: To Do
assignee: []
created_date: '2025-12-28 06:39'
updated_date: '2025-12-28 16:57'
labels:
  - phase-3
  - storage
  - collection
dependencies:
  - task-79
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add insertOne write operation in src/Collection.fs. Reference: FSHARP_PORT_DESIGN.md lines 1076-1078.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Add 'let insertOne (doc: 'T) (collection: Collection<'T>) : Task<FractalResult<Document<'T>>>'
- [ ] #2 Generate ID using collection.IdGenerator() if needed
- [ ] #3 Create Document<'T> with generated ID and timestamps from Timestamp.now()
- [ ] #4 Serialize document Data to JSON
- [ ] #5 Execute INSERT INTO tableName (_id, body, createdAt, updatedAt) VALUES (@id, jsonb(@body), @created, @updated)
- [ ] #6 Return Ok Document on success, Error UniqueConstraint on constraint violation
- [ ] #7 Run 'dotnet build' - build succeeds

- [ ] #8 All public types and functions have XML doc comments with <summary>, and public functions include <param>, <returns>, and <example> (see doc-2 for standards)

- [ ] #9 Run 'task lint' - no errors or warnings
<!-- AC:END -->
