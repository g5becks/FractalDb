---
id: task-3
title: Implement Document<'T> Record Type
status: To Do
assignee: []
created_date: '2025-12-28 06:26'
updated_date: '2025-12-28 16:33'
labels:
  - phase-1
  - core
  - types
dependencies:
  - task-2
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Create the generic Document<'T> record type in Core/Types.fs. This wraps user data with metadata. Reference: FSHARP_PORT_DESIGN.md lines 162-167.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 In src/FractalDb/Core/Types.fs, below DocumentMeta, add Document<'T> record
- [ ] #2 Document<'T> has exactly these fields: Id: string, Data: 'T, CreatedAt: int64, UpdatedAt: int64
- [ ] #3 Run 'dotnet build' - build succeeds with no errors

- [ ] #4 All public types and functions have XML doc comments with <summary>, and public functions include <param>, <returns>, and <example> (see doc-2 for standards)

- [ ] #5 Run 'task lint' - no errors or warnings
<!-- AC:END -->
