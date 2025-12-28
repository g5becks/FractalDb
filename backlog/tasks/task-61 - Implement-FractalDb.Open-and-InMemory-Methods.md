---
id: task-61
title: Implement FractalDb.Open and InMemory Methods
status: To Do
assignee: []
created_date: '2025-12-28 06:42'
updated_date: '2025-12-28 16:58'
labels:
  - phase-3
  - storage
  - database
dependencies:
  - task-60
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add static factory methods to FractalDb in src/Database.fs. Reference: FSHARP_PORT_DESIGN.md lines 1432-1440.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Add static member Open(path: string, ?options: DbOptions) : FractalDb
- [ ] #2 Open creates SqliteConnection with 'Data Source={path}', calls conn.Open(), returns new FractalDb
- [ ] #3 Add static member InMemory(?options: DbOptions) : FractalDb
- [ ] #4 InMemory calls Open with ':memory:' as path
- [ ] #5 Run 'dotnet build' - build succeeds

- [ ] #6 All public types and functions have XML doc comments with <summary>, and public functions include <param>, <returns>, and <example> (see doc-2 for standards)

- [ ] #7 Run 'task lint' - no errors or warnings
<!-- AC:END -->
