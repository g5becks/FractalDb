---
id: task-67
title: Implement QueryBuilder Computation Expression
status: To Do
assignee: []
created_date: '2025-12-28 06:43'
updated_date: '2025-12-28 16:37'
labels:
  - phase-4
  - builders
dependencies:
  - task-66
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Create QueryBuilder CE for type-safe query construction. Reference: FSHARP_PORT_DESIGN.md lines 465-526.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Create file src/FractalDb/Builders/QueryBuilder.fs
- [ ] #2 Add namespace FractalDb.Builders and opens for FractalDb.Core
- [ ] #3 Define QueryBuilder class with member _.Yield(_) = Query.Empty and member _.Zero() = Query.Empty
- [ ] #4 Add [<CustomOperation("where")>] member that adds field condition with implicit AND
- [ ] #5 Add [<CustomOperation("field")>] as alias for where
- [ ] #6 Add member _.Combine merging queries with AND
- [ ] #7 Add member _.Delay(f) = f()
- [ ] #8 Add [<AutoOpen>] module with 'let query = QueryBuilder()'
- [ ] #9 Run 'dotnet build' - build succeeds

- [ ] #10 All public types and functions have XML doc comments with <summary>, and public functions include <param>, <returns>, and <example> (see doc-2 for standards)

- [ ] #11 Run 'task lint' - no errors or warnings
<!-- AC:END -->
