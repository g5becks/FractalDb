---
id: task-5
title: Implement Timestamp Module
status: To Do
assignee: []
created_date: '2025-12-28 06:28'
updated_date: '2025-12-28 16:53'
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
Create the Timestamp module in src/Types.fs for Unix timestamp utilities. Reference: FSHARP_PORT_DESIGN.md lines 232-248.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Add 'now' function: returns DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
- [ ] #2 Add 'toDateTimeOffset' function: takes int64, returns DateTimeOffset.FromUnixTimeMilliseconds(timestamp)
- [ ] #3 Add 'fromDateTimeOffset' function: takes DateTimeOffset, returns dto.ToUnixTimeMilliseconds()
- [ ] #4 Add 'isInRange' function: takes start: int64, end': int64, timestamp: int64, returns timestamp >= start && timestamp <= end'
- [ ] #5 Run 'dotnet build' - build succeeds
- [ ] #6 All public types and functions have XML doc comments with <summary>, and public functions include <param>, <returns>, and <example> (see doc-2 for standards)

- [ ] #7 Run 'task lint' - no errors or warnings

- [ ] #8 In src/Types.fs, add module Timestamp
<!-- AC:END -->
