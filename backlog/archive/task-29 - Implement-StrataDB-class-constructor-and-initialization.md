---
id: task-29
title: Implement StrataDB class - constructor and initialization
status: To Do
assignee: []
created_date: '2025-11-21 01:48'
updated_date: '2025-11-21 02:06'
labels:
  - database
  - core
dependencies:
  - task-10
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Create the main StrataDB class that manages the SQLite database connection and provides collection access. Implement constructor with DatabaseOptions support.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Create src/database/database.ts file
- [ ] #2 Implement StrataDB class with private sqliteDb, idGenerator, debug, and onClose properties
- [ ] #3 Implement constructor accepting DatabaseOptions
- [ ] #4 Support database option as string path (create Database instance) or factory function
- [ ] #5 Support idGenerator option for custom ID generation, default to defaultIdGenerator
- [ ] #6 Support debug option as boolean or custom logger function
- [ ] #7 Support onClose lifecycle hook
- [ ] #8 Initialize SQLite connection using bun:sqlite Database
- [ ] #9 Enable WAL mode and other pragmas via factory function
- [ ] #10 Store idGenerator for passing to collections
- [ ] #11 All code compiles with strict mode
- [ ] #12 No use of any or unsafe assertions

- [ ] #13 Add comprehensive TypeDoc comments with usage examples for constructor and options
<!-- AC:END -->
