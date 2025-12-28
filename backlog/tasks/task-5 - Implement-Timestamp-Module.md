---
id: task-5
title: Implement Timestamp Module
status: To Do
assignee: []
created_date: '2025-12-28 06:28'
updated_date: '2025-12-28 16:33'
labels:
  - phase-1
  - core
  - types
dependencies:
  - task-4
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Create the Timestamp module in Core/Types.fs for Unix timestamp utilities. Reference: FSHARP_PORT_DESIGN.md lines 232-248.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 In src/FractalDb/Core/Types.fs, add module Timestamp
- [ ] #2 Add 'now' function: returns DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
- [ ] #3 Add 'toDateTimeOffset' function: takes int64, returns DateTimeOffset.FromUnixTimeMilliseconds(timestamp)
- [ ] #4 Add 'fromDateTimeOffset' function: takes DateTimeOffset, returns dto.ToUnixTimeMilliseconds()
- [ ] #5 Add 'isInRange' function: takes start: int64, end': int64, timestamp: int64, returns timestamp >= start && timestamp <= end'
- [ ] #6 Run 'dotnet build' - build succeeds

- [ ] #7 All public types and functions have XML doc comments with <summary>, and public functions include <param>, <returns>, and <example> (see doc-2 for standards)

- [ ] #8 Run 'task lint' - no errors or warnings
<!-- AC:END -->
