---
id: task-59
title: Implement Collection Utility Operations
status: To Do
assignee: []
created_date: '2025-12-28 06:41'
updated_date: '2025-12-28 07:03'
labels:
  - phase-3
  - storage
  - collection
dependencies:
  - task-58
priority: medium
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add drop and validate utility operations. Reference: FSHARP_PORT_DESIGN.md lines 1152-1157.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Add 'let drop (collection: Collection<'T>) : Task<unit>' - executes DROP TABLE IF EXISTS
- [ ] #2 Add 'let validate (doc: 'T) (collection: Collection<'T>) : FractalResult<'T>'
- [ ] #3 If schema has Validate function, run it and convert Result<'T, string> to FractalResult
- [ ] #4 If no validator, return Ok doc
- [ ] #5 Run 'dotnet build' - build succeeds

- [ ] #6 All public types and functions have XML doc comments with <summary>, and public functions include <param>, <returns>, and <example> (see doc-2 for standards)
<!-- AC:END -->
