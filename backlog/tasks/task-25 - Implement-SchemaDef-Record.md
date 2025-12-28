---
id: task-25
title: Implement SchemaDef Record
status: To Do
assignee: []
created_date: '2025-12-28 06:33'
updated_date: '2025-12-28 16:55'
labels:
  - phase-1
  - core
  - schema
dependencies:
  - task-24
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Create SchemaDef<'T> record in src/Schema.fs. Reference: FSHARP_PORT_DESIGN.md lines 602-608.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 SchemaDef has: Fields: FieldDef list, Indexes: IndexDef list, Timestamps: bool, Validate: ('T -> Result<'T, string>) option
- [ ] #2 Run 'dotnet build' - build succeeds
- [ ] #3 All public types and functions have XML doc comments with <summary>, and public functions include <param>, <returns>, and <example> (see doc-2 for standards)

- [ ] #4 Run 'task lint' - no errors or warnings

- [ ] #5 In src/Schema.fs, add SchemaDef<'T> record type
<!-- AC:END -->
