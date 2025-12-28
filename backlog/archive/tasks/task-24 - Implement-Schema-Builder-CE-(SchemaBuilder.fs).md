---
id: task-24
title: Implement Schema Builder CE (SchemaBuilder.fs)
status: To Do
assignee: []
created_date: '2025-12-28 06:10'
labels:
  - builders
  - phase-4
dependencies:
  - task-4
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Create the SchemaBuilder computation expression in Builders/SchemaBuilder.fs for declarative schema definition.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 SchemaBuilder<'T> class with Yield returning empty SchemaDef
- [ ] #2 field CustomOperation accepts name, SqliteType, and optional indexed/unique/nullable/path
- [ ] #3 indexed CustomOperation as shorthand for indexed field
- [ ] #4 unique CustomOperation as shorthand for unique indexed field
- [ ] #5 timestamps CustomOperation enables automatic timestamp management
- [ ] #6 compoundIndex CustomOperation adds compound index with fields list
- [ ] #7 validate CustomOperation adds validation function
- [ ] #8 Global schema<'T> instance via AutoOpen module
- [ ] #9 Code compiles successfully with dotnet build
<!-- AC:END -->
