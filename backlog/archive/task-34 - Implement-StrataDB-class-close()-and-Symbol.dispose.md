---
id: task-34
title: Implement StrataDB class - close() and Symbol.dispose
status: To Do
assignee: []
created_date: '2025-11-21 01:49'
updated_date: '2025-11-21 02:06'
labels:
  - database
  - lifecycle
dependencies:
  - task-29
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Implement database lifecycle methods for proper connection cleanup with async close() and automatic Symbol.dispose support for using declarations.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Extend StrataDB class in src/database/database.ts
- [ ] #2 Implement close(): Promise<void> method
- [ ] #3 Call onClose hook if provided, passing sqliteDb instance
- [ ] #4 Await onClose promise if it returns a promise
- [ ] #5 Close SQLite connection using sqliteDb.close()
- [ ] #6 Mark database as closed to prevent further operations
- [ ] #7 Implement [Symbol.dispose](): void synchronous method
- [ ] #8 Call close() synchronously (note: onClose may not await properly in dispose)
- [ ] #9 Enable using declarations for automatic cleanup
- [ ] #10 Add TypeDoc comments explaining cleanup behavior and using pattern
- [ ] #11 All code compiles with strict mode
- [ ] #12 No use of any or unsafe assertions
<!-- AC:END -->
