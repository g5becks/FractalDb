---
id: task-64
title: Implement FractalDb.Close and IDisposable
status: To Do
assignee: []
created_date: '2025-12-28 06:42'
updated_date: '2025-12-28 16:36'
labels:
  - phase-3
  - storage
  - database
dependencies:
  - task-63
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add cleanup to FractalDb. Reference: FSHARP_PORT_DESIGN.md lines 1493-1499.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Add member this.Close() - if not disposed, close and dispose connection, set disposed=true
- [ ] #2 Implement interface IDisposable with member this.Dispose() = this.Close()
- [ ] #3 Run 'dotnet build' - build succeeds

- [ ] #4 All public types and functions have XML doc comments with <summary>, and public functions include <param>, <returns>, and <example> (see doc-2 for standards)

- [ ] #5 Run 'task lint' - no errors or warnings
<!-- AC:END -->
