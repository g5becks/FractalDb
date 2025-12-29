---
id: task-117
title: Implement QueryExecutor.execute function
status: To Do
assignee:
  - '@assistant'
created_date: '2025-12-29 06:12'
updated_date: '2025-12-29 17:00'
labels:
  - query-expressions
  - executor
dependencies:
  - task-115
  - task-116
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Implement the execute function that takes TranslatedQuery<T> and executes it against the database. Converts to QueryOptions and uses existing Collection.find functions.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 execute function defined in QueryExecutor module
- [ ] #2 Converts OrderBy list to QueryOptions sort
- [ ] #3 Applies Skip and Take as QueryOptions
- [ ] #4 Calls Collection.findWith with translated Query
- [ ] #5 Maps results to return just Data (T seq)
- [ ] #6 Returns Task<T seq>
- [ ] #7 Code builds with no errors or warnings
- [ ] #8 All existing tests pass

- [ ] #9 XML doc comments on execute function with summary, params, returns, and example
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Study Collection.find and Collection.findWith functions in Collection.fs
2. Study QueryOptions structure in Options.fs
3. Design QueryExecutor module structure
4. Implement execute function:
   - Take TranslatedQuery<'T> and Collection<'T>
   - Convert OrderBy to QueryOptions.Sort
   - Convert Skip/Take to QueryOptions.Skip/Limit
   - Convert Projection to QueryOptions.Select/Omit
   - Call Collection.findWith with query and options
   - Extract .Data from Document<'T> results
   - Return Task<seq<'T>>
5. Add comprehensive XML documentation
6. Build and verify
7. Run tests
<!-- SECTION:PLAN:END -->
