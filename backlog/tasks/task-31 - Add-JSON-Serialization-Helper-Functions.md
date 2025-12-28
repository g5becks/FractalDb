---
id: task-31
title: Add JSON Serialization Helper Functions
status: To Do
assignee: []
created_date: '2025-12-28 06:34'
updated_date: '2025-12-28 16:35'
labels:
  - phase-2
  - json
dependencies:
  - task-30
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add serialize and deserialize helper functions for Document<'T>. Reference: FSHARP_PORT_DESIGN.md lines 85-86.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 In Json/Serialization.fs, add 'let serialize<'T> (doc: 'T) : string' using JsonSerializer.Serialize with configured options
- [ ] #2 Add 'let deserialize<'T> (json: string) : 'T' using JsonSerializer.Deserialize with configured options
- [ ] #3 Add 'let serializeToBytes<'T> (doc: 'T) : byte[]' for JSONB storage
- [ ] #4 Add 'let deserializeFromBytes<'T> (bytes: byte[]) : 'T'
- [ ] #5 Run 'dotnet build' - build succeeds

- [ ] #6 All public types and functions have XML doc comments with <summary>, and public functions include <param>, <returns>, and <example> (see doc-2 for standards)

- [ ] #7 Run 'task lint' - no errors or warnings
<!-- AC:END -->
