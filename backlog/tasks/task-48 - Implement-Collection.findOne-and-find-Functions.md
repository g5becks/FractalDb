---
id: task-48
title: Implement Collection.findOne and find Functions
status: Done
assignee:
  - '@assistant'
created_date: '2025-12-28 06:38'
updated_date: '2025-12-28 19:15'
labels:
  - phase-3
  - storage
  - collection
dependencies:
  - task-47
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add findOne and find read operations in src/Collection.fs. Reference: FSHARP_PORT_DESIGN.md lines 1040-1052.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 Add 'let findOne (filter: Query<'T>) (collection: Collection<'T>) : Task<Document<'T> option>'
- [x] #2 Translate filter to SQL WHERE clause using collection.Translator
- [x] #3 Execute query with LIMIT 1, return first result or None
- [x] #4 Add 'let findOneWith' variant with QueryOptions
- [x] #5 Add 'let find (filter: Query<'T>) (collection: Collection<'T>) : Task<Document<'T> list>'
- [x] #6 Execute query returning all matching documents
- [x] #7 Add 'let findWith' variant with QueryOptions for sort/limit/skip
- [x] #8 Run 'dotnet build' - build succeeds

- [x] #9 All public types and functions have XML doc comments with <summary>, and public functions include <param>, <returns>, and <example> (see doc-2 for standards)

- [x] #10 Run 'task lint' - no errors or warnings
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Review SqlTranslator usage for converting Query<'T> to SQL
2. Implement findOne: translate filter, build SELECT with LIMIT 1, use Db.querySingle
3. Implement findOneWith: same as findOne but include QueryOptions (sort/limit/skip)
4. Implement find: translate filter, build SELECT, use Db.query for list results
5. Implement findWith: same as find but include QueryOptions
6. Extract common helper for building base SELECT and deserializing rows
7. Handle empty Query (Query.Empty) as no WHERE clause
8. Add comprehensive XML documentation for all 4 functions
9. Build and verify with dotnet build and task lint
<!-- SECTION:PLAN:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Implemented all four find functions in src/Collection.fs:

- Added private helper toSqlType() to convert obj parameters to Donald SqlType cases
- findOne: translates Query<T> to SQL WHERE with LIMIT 1, returns option<Document<T>>
- findOneWith: extends findOne with QueryOptions support (sort/limit/skip)
- find: returns all matching documents as list<Document<T>>
- findWith: extends find with QueryOptions support

All functions use SqlTranslator.Translate() and SqlTranslator.TranslateOptions() for query building. Private helper rowToDocument extracts and deserializes query results. Comprehensive XML documentation added per doc-2 standards.

Build: 0 errors, 0 warnings
Lint: 0 warnings
Tests: 66/66 passing
<!-- SECTION:NOTES:END -->
