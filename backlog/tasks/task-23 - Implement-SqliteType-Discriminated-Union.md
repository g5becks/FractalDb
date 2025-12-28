---
id: task-23
title: Implement SqliteType Discriminated Union
status: To Do
assignee: []
created_date: '2025-12-28 06:32'
updated_date: '2025-12-28 07:03'
labels:
  - phase-1
  - core
  - schema
dependencies:
  - task-21
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Create SqliteType DU in Core/Schema.fs for SQLite column types. Reference: FSHARP_PORT_DESIGN.md lines 580-587.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Create file src/FractalDb/Core/Schema.fs
- [ ] #2 Add namespace FractalDb.Core
- [ ] #3 Add [<RequireQualifiedAccess>] attribute to SqliteType
- [ ] #4 Define SqliteType DU with cases: Text, Integer, Real, Blob, Numeric, Boolean
- [ ] #5 Run 'dotnet build' - build succeeds

- [ ] #6 All public types and functions have XML doc comments with <summary>, and public functions include <param>, <returns>, and <example> (see doc-2 for standards)
<!-- AC:END -->
