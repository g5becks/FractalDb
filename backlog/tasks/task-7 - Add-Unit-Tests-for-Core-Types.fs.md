---
id: task-7
title: Add Unit Tests for Core/Types.fs
status: To Do
assignee: []
created_date: '2025-12-28 06:29'
updated_date: '2025-12-28 16:37'
labels:
  - phase-1
  - testing
  - unit
dependencies:
  - task-77
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Create unit tests for DocumentMeta, Document, IdGenerator, and Timestamp modules. Reference: FSHARP_PORT_DESIGN.md Section 10.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Create file tests/FractalDb.Tests/Unit/TypesTests.fs
- [ ] #2 Add test: IdGenerator.generate returns non-empty string
- [ ] #3 Add test: IdGenerator.generate returns valid GUID format (IdGenerator.isValid returns true)
- [ ] #4 Add test: IdGenerator.isEmptyOrDefault returns true for empty string and Guid.Empty.ToString()
- [ ] #5 Add test: Timestamp.now returns value greater than 0
- [ ] #6 Add test: Timestamp.toDateTimeOffset and fromDateTimeOffset roundtrip correctly
- [ ] #7 Add test: Document.create generates non-empty Id and sets timestamps
- [ ] #8 Add test: Document.update preserves Id and CreatedAt, updates UpdatedAt
- [ ] #9 Add test: Document.map transforms Data correctly
- [ ] #10 Run 'dotnet test' - all tests pass

- [ ] #11 Run 'task lint' - no errors or warnings
<!-- AC:END -->
