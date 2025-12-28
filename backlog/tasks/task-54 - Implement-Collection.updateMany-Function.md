---
id: task-54
title: Implement Collection.updateMany Function
status: To Do
assignee: []
created_date: '2025-12-28 06:40'
updated_date: '2025-12-28 07:03'
labels:
  - phase-3
  - storage
  - collection
dependencies:
  - task-53
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add updateMany batch operation. Reference: FSHARP_PORT_DESIGN.md lines 1115-1117.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Add 'let updateMany (filter: Query<'T>) (update: 'T -> 'T) (collection: Collection<'T>) : Task<FractalResult<UpdateResult>>'
- [ ] #2 Find all documents matching filter
- [ ] #3 Apply update function to each, update timestamps
- [ ] #4 Execute batch UPDATE in transaction
- [ ] #5 Return UpdateResult with MatchedCount and ModifiedCount
- [ ] #6 Run 'dotnet build' - build succeeds

- [ ] #7 All public types and functions have XML doc comments with <summary>, and public functions include <param>, <returns>, and <example> (see doc-2 for standards)
<!-- AC:END -->
