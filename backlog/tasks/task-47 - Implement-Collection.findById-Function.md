---
id: task-47
title: Implement Collection.findById Function
status: Done
assignee:
  - '@assistant'
created_date: '2025-12-28 06:38'
updated_date: '2025-12-28 19:07'
labels:
  - phase-3
  - storage
  - collection
dependencies:
  - task-46
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add findById read operation in src/Collection.fs. Reference: FSHARP_PORT_DESIGN.md lines 1037-1038.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 Add 'let findById (id: string) (collection: Collection<'T>) : Task<Document<'T> option>'
- [x] #2 Build SQL: SELECT _id, json(body) as body, createdAt, updatedAt FROM tableName WHERE _id = @id
- [x] #3 Use Donald Db.newCommand, Db.setParams, Db.querySingle
- [x] #4 Deserialize body JSON to 'T and construct Document<'T>
- [x] #5 Return None if not found
- [x] #6 Run 'dotnet build' - build succeeds
- [x] #7 All public types and functions have XML doc comments with <summary>, and public functions include <param>, <returns>, and <example> (see doc-2 for standards)

- [x] #8 Run 'task lint' - no errors or warnings

- [x] #9 In src/Collection.fs, add [<RequireQualifiedAccess>] module Collection
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Read design spec for findById pattern and Donald usage
2. Add open statements for System.Threading.Tasks and Donald
3. Add [<RequireQualifiedAccess>] module Collection after type definitions
4. Implement findById function: let findById (id: string) (collection: Collection<'T>) : Task<Document<'T> option>
5. Build SQL: SELECT _id, json(body) as body, createdAt, updatedAt FROM tableName WHERE _id = @id
6. Use Donald pipeline: Db.newCommand, Db.setParams, Db.querySingle
7. Handle Result<option, DbError> from Donald - convert to Task<option>
8. Deserialize JSON body to 'T using Serialization.deserialize
9. Construct Document<'T> from row data
10. Add comprehensive XML documentation with examples
11. Build and verify with dotnet build and task lint
<!-- SECTION:PLAN:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Implemented findById function in src/Collection.fs - the first CRUD operation for Collection module.

Key implementation details:
- Added [<RequireQualifiedAccess>] module Collection with comprehensive XML docs
- Function signature: let findById (id: string) (collection: Collection<'T>) : Task<option<Document<'T>>>
- SQL query: SELECT _id, json(body) as body, createdAt, updatedAt FROM tableName WHERE _id = @id
- Uses Donald Db.newCommand |> Db.setParams |> Db.querySingle pipeline
- Donald querySingle returns option<tuple> directly (NOT Result)
- Pattern match on Some/None to handle found/not-found cases
- Deserializes JSON body using Serialization.deserialize<'T> (throws on error)
- Constructs Document<'T> record with Id, Data, CreatedAt, UpdatedAt
- Returns Task.FromResult since Donald operations are synchronous
- Added open statements: System.Threading.Tasks, Donald, Serialization

Key learnings:
- Donald methods return option or Result, not Task
- deserialize<'T> throws exceptions (no Result wrapper)
- Must use Task.FromResult to wrap synchronous result in Task
- Pipeline-style with collection as last parameter for F# idioms

Verification:
- dotnet build: 0 errors, 0 warnings ✅
- task lint: 0 warnings ✅
- dotnet test: 66/66 tests passing ✅

First Collection operation complete! Establishes pattern for remaining CRUD operations.
Ready for Task 48: findOne and find functions.
<!-- SECTION:NOTES:END -->
