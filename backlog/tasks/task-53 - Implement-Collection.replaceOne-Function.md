---
id: task-53
title: Implement Collection.replaceOne Function
status: To Do
assignee: []
created_date: '2025-12-28 06:40'
updated_date: '2025-12-28 16:57'
labels:
  - phase-3
  - storage
  - collection
dependencies:
  - task-52
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add replaceOne operation in src/Collection.fs. Reference: FSHARP_PORT_DESIGN.md lines 1093-1095.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Add 'let replaceOne (filter: Query<'T>) (doc: 'T) (collection: Collection<'T>) : Task<FractalResult<Document<'T> option>>'
- [ ] #2 Find document matching filter
- [ ] #3 Replace Data entirely with new doc (preserving Id and CreatedAt)
- [ ] #4 Update UpdatedAt to now()
- [ ] #5 Return updated Document or None if not found
- [ ] #6 Run 'dotnet build' - build succeeds

- [ ] #7 All public types and functions have XML doc comments with <summary>, and public functions include <param>, <returns>, and <example> (see doc-2 for standards)

- [ ] #8 Run 'task lint' - no errors or warnings
<!-- AC:END -->
