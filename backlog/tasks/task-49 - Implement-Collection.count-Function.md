---
id: task-49
title: Implement Collection.count Function
status: To Do
assignee: []
created_date: '2025-12-28 06:39'
updated_date: '2025-12-28 07:03'
labels:
  - phase-3
  - storage
  - collection
dependencies:
  - task-48
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add count read operation. Reference: FSHARP_PORT_DESIGN.md lines 1054-1059.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Add 'let count (filter: Query<'T>) (collection: Collection<'T>) : Task<int>'
- [ ] #2 Build SQL: SELECT COUNT(*) as count FROM tableName WHERE {filter}
- [ ] #3 Translate filter to WHERE clause
- [ ] #4 Return integer count
- [ ] #5 Add 'let estimatedCount (collection: Collection<'T>) : Task<int>' using SQLite stats for fast estimate
- [ ] #6 Run 'dotnet build' - build succeeds

- [ ] #7 All public types and functions have XML doc comments with <summary>, and public functions include <param>, <returns>, and <example> (see doc-2 for standards)
<!-- AC:END -->
