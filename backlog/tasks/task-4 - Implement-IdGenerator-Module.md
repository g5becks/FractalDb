---
id: task-4
title: Implement IdGenerator Module
status: To Do
assignee: []
created_date: '2025-12-28 06:28'
updated_date: '2025-12-28 07:03'
labels:
  - phase-1
  - core
  - types
dependencies:
  - task-3
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Create the IdGenerator module in Core/Types.fs for generating UUID v7 IDs. Reference: FSHARP_PORT_DESIGN.md lines 208-227.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 In src/FractalDb/Core/Types.fs, add module IdGenerator
- [ ] #2 Add 'generate' function that returns Guid.CreateVersion7().ToString()
- [ ] #3 Add 'isEmptyOrDefault' function: returns true if string is null/empty or equals Guid.Empty.ToString()
- [ ] #4 Add 'isValid' function: uses Guid.TryParse to check if string is valid GUID format
- [ ] #5 Run 'dotnet build' - build succeeds

- [ ] #6 All public types and functions have XML doc comments with <summary>, and public functions include <param>, <returns>, and <example> (see doc-2 for standards)
<!-- AC:END -->
