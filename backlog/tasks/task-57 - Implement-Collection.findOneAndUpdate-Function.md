---
id: task-57
title: Implement Collection.findOneAndUpdate Function
status: To Do
assignee: []
created_date: '2025-12-28 06:40'
updated_date: '2025-12-28 16:36'
labels:
  - phase-3
  - storage
  - collection
dependencies:
  - task-56
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add atomic findOneAndUpdate operation. Reference: FSHARP_PORT_DESIGN.md lines 1136-1141.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Add 'let findOneAndUpdate (filter: Query<'T>) (update: 'T -> 'T) (options: FindAndModifyOptions) (collection: Collection<'T>) : Task<FractalResult<Document<'T> option>>'
- [ ] #2 In single transaction: find document, apply update, save, return document
- [ ] #3 If options.ReturnDocument = Before, return document before update
- [ ] #4 If options.ReturnDocument = After, return document after update
- [ ] #5 Support options.Upsert for insert if not found
- [ ] #6 Run 'dotnet build' - build succeeds

- [ ] #7 All public types and functions have XML doc comments with <summary>, and public functions include <param>, <returns>, and <example> (see doc-2 for standards)

- [ ] #8 Run 'task lint' - no errors or warnings
<!-- AC:END -->
