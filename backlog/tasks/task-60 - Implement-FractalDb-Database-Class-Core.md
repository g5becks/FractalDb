---
id: task-60
title: Implement FractalDb Database Class - Core
status: To Do
assignee: []
created_date: '2025-12-28 06:41'
updated_date: '2025-12-28 16:58'
labels:
  - phase-3
  - storage
  - database
dependencies:
  - task-59
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Create main FractalDb database class in src/Database.fs. Reference: FSHARP_PORT_DESIGN.md lines 1406-1456.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Add namespace FractalDb.Storage
- [ ] #2 Add DbOptions module with 'let defaults' using IdGenerator.generate and EnableCache=false
- [ ] #3 Define FractalDb class with private constructor taking SqliteConnection and DbOptions
- [ ] #4 Add private collections: ConcurrentDictionary<string, obj>
- [ ] #5 Add private mutable disposed: bool = false
- [ ] #6 Run 'dotnet build' - build succeeds
- [ ] #7 All public types and functions have XML doc comments with <summary>, and public functions include <param>, <returns>, and <example> (see doc-2 for standards)
- [ ] #8 Run 'task lint' - no errors or warnings

- [ ] #9 Create file src/Database.fs

- [ ] #10 Add module declaration: module FractalDb.Database
<!-- AC:END -->
