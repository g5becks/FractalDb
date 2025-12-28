---
id: task-48
title: Implement Collection.findOne and find Functions
status: To Do
assignee: []
created_date: '2025-12-28 06:38'
updated_date: '2025-12-28 16:36'
labels:
  - phase-3
  - storage
  - collection
dependencies:
  - task-47
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add findOne and find read operations. Reference: FSHARP_PORT_DESIGN.md lines 1040-1052.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Add 'let findOne (filter: Query<'T>) (collection: Collection<'T>) : Task<Document<'T> option>'
- [ ] #2 Translate filter to SQL WHERE clause using collection.Translator
- [ ] #3 Execute query with LIMIT 1, return first result or None
- [ ] #4 Add 'let findOneWith' variant with QueryOptions
- [ ] #5 Add 'let find (filter: Query<'T>) (collection: Collection<'T>) : Task<Document<'T> list>'
- [ ] #6 Execute query returning all matching documents
- [ ] #7 Add 'let findWith' variant with QueryOptions for sort/limit/skip
- [ ] #8 Run 'dotnet build' - build succeeds

- [ ] #9 All public types and functions have XML doc comments with <summary>, and public functions include <param>, <returns>, and <example> (see doc-2 for standards)

- [ ] #10 Run 'task lint' - no errors or warnings
<!-- AC:END -->
