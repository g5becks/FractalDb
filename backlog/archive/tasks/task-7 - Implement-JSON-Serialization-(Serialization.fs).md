---
id: task-7
title: Implement JSON Serialization (Serialization.fs)
status: To Do
assignee: []
created_date: '2025-12-28 06:05'
labels:
  - json
  - phase-2
dependencies:
  - task-1
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Configure FSharp.SystemTextJson for F# record serialization in Json/Serialization.fs. Depends on Types.fs.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 JsonSerializerOptions configured with JsonFSharpConverter
- [ ] #2 JsonSerializerOptions uses camelCase property naming
- [ ] #3 Helper function to serialize Document<'T> to JSON string
- [ ] #4 Helper function to deserialize JSON string to Document<'T>
- [ ] #5 Code compiles successfully with dotnet build
<!-- AC:END -->
