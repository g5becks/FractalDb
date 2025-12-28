---
id: task-52
title: Implement Collection.updateById and updateOne Functions
status: To Do
assignee: []
created_date: '2025-12-28 06:40'
updated_date: '2025-12-28 16:57'
labels:
  - phase-3
  - storage
  - collection
dependencies:
  - task-51
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add update operations in src/Collection.fs. Reference: FSHARP_PORT_DESIGN.md lines 1080-1091.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Add 'let updateById (id: string) (update: 'T -> 'T) (collection: Collection<'T>) : Task<FractalResult<Document<'T> option>>'
- [ ] #2 Find document by ID, apply update function to Data, set new UpdatedAt timestamp
- [ ] #3 Serialize updated Data and execute UPDATE statement
- [ ] #4 Return Ok Some for success, Ok None if not found
- [ ] #5 Add 'let updateOne' taking Query<'T> instead of ID
- [ ] #6 Add 'let updateOneWith' with upsert: bool option
- [ ] #7 Run 'dotnet build' - build succeeds

- [ ] #8 All public types and functions have XML doc comments with <summary>, and public functions include <param>, <returns>, and <example> (see doc-2 for standards)

- [ ] #9 Run 'task lint' - no errors or warnings
<!-- AC:END -->
