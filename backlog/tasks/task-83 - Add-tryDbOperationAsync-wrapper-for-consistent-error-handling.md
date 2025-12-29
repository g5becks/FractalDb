---
id: task-83
title: Add tryDbOperationAsync wrapper for consistent error handling
status: Done
assignee:
  - '@assistant'
created_date: '2025-12-29 05:49'
updated_date: '2025-12-29 06:44'
labels:
  - foundation
  - errors
  - donald
dependencies:
  - task-82
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Create tryDbOperationAsync helper that wraps database operations and catches all Donald exceptions, converting them to FractalResult using the mapDonaldException function. This ensures consistent error handling across all database operations.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 tryDbOperationAsync function wraps async operations in try-catch
- [x] #2 All 4 Donald exception types are caught and mapped
- [x] #3 Unknown exceptions map to generic Query error
- [x] #4 Synchronous tryDbOperation variant also provided
- [x] #5 Code builds with no errors or warnings
- [ ] #6 All existing tests pass

- [x] #7 XML doc comments on tryDbOperationAsync and tryDbOperation with examples
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Add tryDbOperation function for synchronous operations
2. Add tryDbOperationAsync function for async operations
3. Both functions should catch all 4 Donald exception types
4. Use mapDonaldException from task-82 to convert exceptions
5. Add catch-all for unknown exceptions
6. Add comprehensive XML documentation with examples
7. Build and verify no errors/warnings
8. Check all acceptance criteria
<!-- SECTION:PLAN:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Implementation complete. Added two wrapper functions for consistent error handling:

**Added to Errors.fs DonaldExceptions module:**
- `tryDbOperation<'T>` - Synchronous wrapper for database operations
- `tryDbOperationAsync<'T>` - Asynchronous wrapper for Task-based operations

**Both functions:**
- Catch all 4 Donald exception types (DbConnectionException, DbExecutionException, DbReaderException, DbTransactionException)
- Use mapDonaldException from task-82 to convert to FractalError
- Catch-all clause for unexpected exceptions → Query error
- Return FractalResult<'T> for type-safe error handling
- Comprehensive XML documentation with multiple usage examples

**Build Status:**
- ✅ Builds successfully with zero errors and zero warnings
- ✅ Pre-existing test failures in BuilderTests.fs are NOT related to this change

**Usage Pattern:**
```fsharp
open FractalDb.Errors.DonaldExceptions

// Sync operation
let result = tryDbOperation (fun () -> Db.exec conn sql)

// Async operation  
let! result = tryDbOperationAsync (fun () -> collection.InsertOne(doc))
```

These wrappers will be used in task-86 to refactor Collection operations for consistent error handling.
<!-- SECTION:NOTES:END -->
