---
id: task-22
title: Implement Database Class (Database.fs)
status: To Do
assignee: []
created_date: '2025-12-28 06:09'
labels:
  - storage
  - phase-3
dependencies:
  - task-21
  - task-13
  - task-14
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Create the main FractalDb database class in Storage/Database.fs.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 DbOptions record with IdGenerator and EnableCache fields
- [ ] #2 DbOptions.defaults provides default configuration
- [ ] #3 FractalDb class with private constructor taking SqliteConnection and options
- [ ] #4 FractalDb.Open static method creates instance from file path
- [ ] #5 FractalDb.InMemory static method creates in-memory database
- [ ] #6 FractalDb.Collection<'T> method returns Collection<'T> for a name and schema
- [ ] #7 Collection caching via ConcurrentDictionary
- [ ] #8 FractalDb.Transaction method creates manual Transaction
- [ ] #9 FractalDb.Execute method runs function in auto-commit/rollback transaction
- [ ] #10 FractalDb.ExecuteTransaction method handles Result-based transactions
- [ ] #11 IDisposable implementation for cleanup
- [ ] #12 Code compiles successfully with dotnet build
<!-- AC:END -->
