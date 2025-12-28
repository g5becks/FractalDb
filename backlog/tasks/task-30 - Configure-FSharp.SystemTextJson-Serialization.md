---
id: task-30
title: Configure FSharp.SystemTextJson Serialization
status: To Do
assignee: []
created_date: '2025-12-28 06:34'
updated_date: '2025-12-28 07:03'
labels:
  - phase-2
  - json
dependencies:
  - task-29
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Set up JSON serialization with FSharp.SystemTextJson in Json/Serialization.fs. Reference: FSHARP_PORT_DESIGN.md lines 85-86, 2672.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Create file src/FractalDb/Json/Serialization.fs
- [ ] #2 Add namespace FractalDb.Json
- [ ] #3 Add 'open System.Text.Json' and 'open System.Text.Json.Serialization'
- [ ] #4 Create JsonSerializerOptions with JsonFSharpConverter() added to Converters
- [ ] #5 Set PropertyNamingPolicy = JsonNamingPolicy.CamelCase
- [ ] #6 Run 'dotnet build' - build succeeds

- [ ] #7 All public types and functions have XML doc comments with <summary>, and public functions include <param>, <returns>, and <example> (see doc-2 for standards)
<!-- AC:END -->
