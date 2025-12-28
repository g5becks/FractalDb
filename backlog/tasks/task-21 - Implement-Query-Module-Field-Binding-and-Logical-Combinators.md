---
id: task-21
title: Implement Query Module - Field Binding and Logical Combinators
status: To Do
assignee: []
created_date: '2025-12-28 06:32'
updated_date: '2025-12-28 16:55'
labels:
  - phase-1
  - core
  - query
dependencies:
  - task-20
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add field binding and logical combinator functions to Query module in src/Query.fs. Reference: FSHARP_PORT_DESIGN.md lines 406-428.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Add 'let field (name: string) (op: Query<'T>) : Query<'T>' - extracts FieldOp and rebinds with name
- [ ] #2 Add 'let all' (queries: Query<'T> list) : Query<'T> = Query.And queries'
- [ ] #3 Add 'let any (queries: Query<'T> list) : Query<'T> = Query.Or queries'
- [ ] #4 Add 'let none (queries: Query<'T> list) : Query<'T> = Query.Nor queries'
- [ ] #5 Add 'let not' (query: Query<'T>) : Query<'T> = Query.Not query'
- [ ] #6 Run 'dotnet build' - build succeeds

- [ ] #7 All public types and functions have XML doc comments with <summary>, and public functions include <param>, <returns>, and <example> (see doc-2 for standards)

- [ ] #8 Run 'task lint' - no errors or warnings
<!-- AC:END -->
