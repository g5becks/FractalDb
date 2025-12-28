---
id: task-1
title: Implement Core Types (Types.fs)
status: To Do
assignee: []
created_date: '2025-12-28 06:04'
labels:
  - core
  - phase-1
dependencies: []
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Create the foundational document types, ID generation, and timestamp utilities in Core/Types.fs. This is the base module with no dependencies on other FractalDb modules.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 DocumentMeta record type defined with Id, CreatedAt, UpdatedAt fields
- [ ] #2 Document<'T> record type defined wrapping user data with metadata
- [ ] #3 Document module with create, createWithId, update, and map functions
- [ ] #4 IdGenerator module with generate (Guid.CreateVersion7), isEmptyOrDefault, isValid functions
- [ ] #5 Timestamp module with now, toDateTimeOffset, fromDateTimeOffset, isInRange functions
- [ ] #6 Code compiles successfully with dotnet build
<!-- AC:END -->
