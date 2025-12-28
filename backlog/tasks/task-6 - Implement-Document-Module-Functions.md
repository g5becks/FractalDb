---
id: task-6
title: Implement Document Module Functions
status: To Do
assignee: []
created_date: '2025-12-28 06:28'
updated_date: '2025-12-28 07:03'
labels:
  - phase-1
  - core
  - types
dependencies:
  - task-5
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Create the Document module with helper functions in Core/Types.fs. Reference: FSHARP_PORT_DESIGN.md lines 169-203.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 In src/FractalDb/Core/Types.fs, add module Document
- [ ] #2 Add 'create' function: takes data:'T, generates ID via IdGenerator.generate(), sets CreatedAt/UpdatedAt to Timestamp.now(), returns Document<'T>
- [ ] #3 Add 'createWithId' function: takes id:string and data:'T, sets timestamps to now(), returns Document<'T>
- [ ] #4 Add 'update' function: takes f:('T -> 'T) and doc:Document<'T>, returns new Document with f applied to Data and UpdatedAt set to now()
- [ ] #5 Add 'map' function: takes f:('T -> 'U) and doc:Document<'T>, returns Document<'U> with same Id/timestamps but transformed Data
- [ ] #6 Run 'dotnet build' - build succeeds

- [ ] #7 All public types and functions have XML doc comments with <summary>, and public functions include <param>, <returns>, and <example> (see doc-2 for standards)
<!-- AC:END -->
