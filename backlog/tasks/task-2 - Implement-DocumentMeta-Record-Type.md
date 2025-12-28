---
id: task-2
title: Implement DocumentMeta Record Type
status: To Do
assignee: []
created_date: '2025-12-28 06:24'
updated_date: '2025-12-28 16:33'
labels:
  - phase-1
  - core
  - types
dependencies:
  - task-1
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Create the DocumentMeta record type in Core/Types.fs. This is the metadata structure attached to every document. Reference: FSHARP_PORT_DESIGN.md lines 155-159.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Create file src/FractalDb/Core/Types.fs if it does not exist
- [ ] #2 Add namespace declaration: namespace FractalDb.Core
- [ ] #3 Add 'open System' at the top
- [ ] #4 Define DocumentMeta record with exactly these fields: Id: string, CreatedAt: int64, UpdatedAt: int64
- [ ] #5 Run 'dotnet build' - build succeeds with no errors

- [ ] #6 All public types and functions have XML doc comments with <summary>, and public functions include <param>, <returns>, and <example> (see doc-2 for standards)

- [ ] #7 Run 'task lint' - no errors or warnings
<!-- AC:END -->
