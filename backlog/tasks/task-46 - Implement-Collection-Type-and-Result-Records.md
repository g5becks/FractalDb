---
id: task-46
title: Implement Collection Type and Result Records
status: To Do
assignee: []
created_date: '2025-12-28 06:38'
updated_date: '2025-12-28 16:36'
labels:
  - phase-3
  - storage
  - collection
dependencies:
  - task-45
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Create Collection<'T> and result types. Reference: FSHARP_PORT_DESIGN.md lines 1010-1194.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Create file src/FractalDb/Storage/Collection.fs
- [ ] #2 Add namespace FractalDb.Storage
- [ ] #3 Add opens for System.Data, FractalDb.Core, FractalDb.Json
- [ ] #4 Define Collection<'T> record: Name: string, Schema: SchemaDef<'T>, Connection: IDbConnection, IdGenerator: unit -> string, Translator: SqlTranslator<'T>, EnableCache: bool
- [ ] #5 Define InsertManyResult<'T> record: Documents: Document<'T> list, InsertedCount: int
- [ ] #6 Define UpdateResult record: MatchedCount: int, ModifiedCount: int
- [ ] #7 Define DeleteResult record: DeletedCount: int
- [ ] #8 Define ReturnDocument DU: Before | After
- [ ] #9 Define FindOptions record: Sort: (string * SortDirection) list
- [ ] #10 Define FindAndModifyOptions record: Sort, ReturnDocument, Upsert: bool
- [ ] #11 Run 'dotnet build' - build succeeds

- [ ] #12 All public types and functions have XML doc comments with <summary>, and public functions include <param>, <returns>, and <example> (see doc-2 for standards)

- [ ] #13 Run 'task lint' - no errors or warnings
<!-- AC:END -->
