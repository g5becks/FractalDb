---
id: task-7
title: Add Unit Tests for Core/Types.fs
status: To Do
assignee: []
created_date: '2025-12-28 06:29'
updated_date: '2025-12-28 16:53'
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
Create unit tests for DocumentMeta, Document, IdGenerator, and Timestamp modules in tests/TypesTests.fs. Reference: FSHARP_PORT_DESIGN.md Section 10.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Add test: IdGenerator.generate returns non-empty string
- [ ] #2 Add test: IdGenerator.generate returns valid GUID format (IdGenerator.isValid returns true)
- [ ] #3 Add test: IdGenerator.isEmptyOrDefault returns true for empty string and Guid.Empty.ToString()
- [ ] #4 Add test: Timestamp.now returns value greater than 0
- [ ] #5 Add test: Timestamp.toDateTimeOffset and fromDateTimeOffset roundtrip correctly
- [ ] #6 Add test: Document.create generates non-empty Id and sets timestamps
- [ ] #7 Add test: Document.update preserves Id and CreatedAt, updates UpdatedAt
- [ ] #8 Add test: Document.map transforms Data correctly
- [ ] #9 Run 'dotnet test' - all tests pass
- [ ] #10 Run 'task lint' - no errors or warnings

- [ ] #11 Create file tests/TypesTests.fs
<!-- AC:END -->
