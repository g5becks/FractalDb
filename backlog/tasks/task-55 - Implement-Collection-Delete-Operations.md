---
id: task-55
title: Implement Collection Delete Operations
status: To Do
assignee: []
created_date: '2025-12-28 06:40'
updated_date: '2025-12-28 07:03'
labels:
  - phase-3
  - storage
  - collection
dependencies:
  - task-54
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add deleteById, deleteOne, deleteMany operations. Reference: FSHARP_PORT_DESIGN.md lines 1097-1121.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Add 'let deleteById (id: string) (collection: Collection<'T>) : Task<bool>'
- [ ] #2 Execute DELETE FROM tableName WHERE _id = @id, return true if deleted
- [ ] #3 Add 'let deleteOne (filter: Query<'T>) : Task<bool>' - deletes first matching
- [ ] #4 Add 'let deleteMany (filter: Query<'T>) : Task<DeleteResult>' - deletes all matching
- [ ] #5 Return DeleteResult with DeletedCount
- [ ] #6 Run 'dotnet build' - build succeeds

- [ ] #7 All public types and functions have XML doc comments with <summary>, and public functions include <param>, <returns>, and <example> (see doc-2 for standards)
<!-- AC:END -->
