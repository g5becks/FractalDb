---
id: task-32
title: Implement Transaction class
status: To Do
assignee: []
created_date: '2025-11-21 01:49'
updated_date: '2025-11-21 02:06'
labels:
  - database
  - transactions
dependencies:
  - task-29
  - task-19
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Create the Transaction class that provides collection access within a transaction scope with explicit commit/rollback and Symbol.dispose support.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Create src/database/transaction.ts file
- [ ] #2 Implement Transaction class with private sqliteDb and committed/rolledback flags
- [ ] #3 Implement collection<T extends Document>(name): Collection<T> method
- [ ] #4 Return Collection instances that use the transaction's SQLite connection
- [ ] #5 Implement commit(): Promise<void> method executing COMMIT
- [ ] #6 Set committed flag to true
- [ ] #7 Throw error if already committed or rolled back
- [ ] #8 Implement rollback(): Promise<void> method executing ROLLBACK
- [ ] #9 Set rolledback flag to true
- [ ] #10 Throw error if already committed or rolled back
- [ ] #11 Implement [Symbol.dispose](): void method for automatic rollback
- [ ] #12 Rollback if not yet committed when disposed
- [ ] #13 Add TypeDoc comments explaining transaction lifecycle
- [ ] #14 Export Transaction from src/database/index.ts
- [ ] #15 All code compiles with strict mode
- [ ] #16 No use of any or unsafe assertions
<!-- AC:END -->
