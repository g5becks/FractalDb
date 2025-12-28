---
id: task-79
title: Implement Collection.search and distinct Functions
status: To Do
assignee: []
created_date: '2025-12-28 16:30'
labels:
  - phase-3
  - storage
  - collection
dependencies:
  - task-49
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add search and distinct read operations to Collection module. Reference: FSHARP_PORT_DESIGN.md lines 1060-1071.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Add 'let search (text: string) (fields: string list) (collection: Collection<'T>) : Task<Document<'T> list>'
- [ ] #2 Implement search using LIKE queries across specified fields
- [ ] #3 Add 'let searchWith' variant with QueryOptions for sort/limit
- [ ] #4 Add 'let distinct (field: string) (filter: Query<'T> option) (collection: Collection<'T>) : Task<obj list>'
- [ ] #5 Implement distinct using SELECT DISTINCT on the specified field
- [ ] #6 Run 'dotnet build' - build succeeds
- [ ] #7 Run 'task lint' - no errors or warnings
- [ ] #8 All public types and functions have XML doc comments with <summary>, and public functions include <param>, <returns>, and <example> (see doc-2 for standards)
<!-- AC:END -->
