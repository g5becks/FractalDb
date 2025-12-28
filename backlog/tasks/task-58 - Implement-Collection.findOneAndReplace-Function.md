---
id: task-58
title: Implement Collection.findOneAndReplace Function
status: To Do
assignee: []
created_date: '2025-12-28 06:41'
updated_date: '2025-12-28 16:58'
labels:
  - phase-3
  - storage
  - collection
dependencies:
  - task-57
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add atomic findOneAndReplace operation in src/Collection.fs. Reference: FSHARP_PORT_DESIGN.md lines 1143-1147.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Add 'let findOneAndReplace (filter: Query<'T>) (doc: 'T) (options: FindAndModifyOptions) (collection: Collection<'T>) : Task<FractalResult<Document<'T> option>>'
- [ ] #2 Similar to findOneAndUpdate but replaces entire document
- [ ] #3 Preserve Id and CreatedAt from original
- [ ] #4 Support ReturnDocument.Before and After options
- [ ] #5 Run 'dotnet build' - build succeeds

- [ ] #6 All public types and functions have XML doc comments with <summary>, and public functions include <param>, <returns>, and <example> (see doc-2 for standards)

- [ ] #7 Run 'task lint' - no errors or warnings
<!-- AC:END -->
