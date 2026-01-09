---
id: task-212
title: Add event cleanup on database close
status: To Do
assignee: []
created_date: '2026-01-09 16:20'
labels:
  - feature
  - events
  - implementation
dependencies:
  - task-211
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Update Strata.close() to clean up event resources for all collections before closing the database.

## Instructions

1. Open `src/stratadb.ts`
2. The Strata class needs to track collections to clean them up. Check if there's already a collection registry/cache.
3. Update the `close()` method to iterate over all collections and call `cleanupEvents()`:
   ```typescript
   close(): void {
     // Clean up event listeners for all collections
     for (const collection of this.collections.values()) {
       collection.cleanupEvents()
     }
     
     this.onCloseCallback?.()
     this.sqliteDb.close()
   }
   ```
4. If collections aren't already tracked, add a Map to track them:
   ```typescript
   private collections = new Map<string, SQLiteCollection<any>>()
   ```
   And update the `collection()` method to cache collections in this map.
5. Commit changes after completion

## Why This Matters
- Database close should clean up all resources
- Prevents memory leaks from listeners on closed database
- Matches expected disposal semantics (close = full cleanup)
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Strata class tracks created collections (if not already)
- [ ] #2 close() iterates over collections and calls cleanupEvents()
- [ ] #3 Event cleanup happens before database connection closes
- [ ] #4 bun run typecheck passes with zero errors
- [ ] #5 bun run lint:fix produces no new warnings or errors
- [ ] #6 Changes are committed with descriptive message
<!-- AC:END -->
