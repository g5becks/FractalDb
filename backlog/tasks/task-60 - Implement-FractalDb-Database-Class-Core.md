---
id: task-60
title: Implement FractalDb Database Class - Core
status: To Do
assignee: []
created_date: '2025-12-28 06:41'
updated_date: '2025-12-28 16:36'
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
Create main FractalDb database class. Reference: FSHARP_PORT_DESIGN.md lines 1406-1456.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Create file src/FractalDb/Storage/Database.fs
- [ ] #2 Add namespace FractalDb.Storage
- [ ] #3 Define DbOptions record: IdGenerator: unit -> string, EnableCache: bool
- [ ] #4 Add DbOptions module with 'let defaults' using IdGenerator.generate and EnableCache=false
- [ ] #5 Define FractalDb class with private constructor taking SqliteConnection and DbOptions
- [ ] #6 Add private collections: ConcurrentDictionary<string, obj>
- [ ] #7 Add private mutable disposed: bool = false
- [ ] #8 Run 'dotnet build' - build succeeds

- [ ] #9 All public types and functions have XML doc comments with <summary>, and public functions include <param>, <returns>, and <example> (see doc-2 for standards)

- [ ] #10 Run 'task lint' - no errors or warnings
<!-- AC:END -->
