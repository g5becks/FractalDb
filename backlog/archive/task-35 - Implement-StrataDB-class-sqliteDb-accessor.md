---
id: task-35
title: Implement StrataDB class - sqliteDb accessor
status: To Do
assignee: []
created_date: '2025-11-21 01:49'
updated_date: '2025-11-21 02:06'
labels:
  - database
  - api
dependencies:
  - task-29
priority: medium
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add readonly property exposing the underlying SQLite database instance for advanced use cases requiring direct SQLite access.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Extend StrataDB class in src/database/database.ts
- [ ] #2 Add readonly sqliteDb: SQLiteDatabase property getter
- [ ] #3 Return private _sqliteDb instance
- [ ] #4 Add TypeDoc comment warning that direct SQL access bypasses type safety
- [ ] #5 Export StrataDB from src/database/index.ts
- [ ] #6 All code compiles with strict mode
- [ ] #7 No use of any type
<!-- AC:END -->
