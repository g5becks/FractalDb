---
id: task-30
title: Configure FSharp.SystemTextJson Serialization
status: To Do
assignee: []
created_date: '2025-12-28 06:34'
updated_date: '2025-12-28 16:56'
labels:
  - phase-2
  - json
dependencies:
  - task-29
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Set up JSON serialization with FSharp.SystemTextJson in src/Serialization.fs. Reference: FSHARP_PORT_DESIGN.md lines 85-86, 2672.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Add namespace FractalDb.Json
- [ ] #2 Create JsonSerializerOptions with JsonFSharpConverter() added to Converters
- [ ] #3 Set PropertyNamingPolicy = JsonNamingPolicy.CamelCase
- [ ] #4 Run 'dotnet build' - build succeeds
- [ ] #5 All public types and functions have XML doc comments with <summary>, and public functions include <param>, <returns>, and <example> (see doc-2 for standards)
- [ ] #6 Run 'task lint' - no errors or warnings

- [ ] #7 Create file src/Serialization.fs

- [ ] #8 Add module declaration: module FractalDb.Serialization
<!-- AC:END -->
