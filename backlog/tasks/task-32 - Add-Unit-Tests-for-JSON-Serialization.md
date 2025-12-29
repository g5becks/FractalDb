---
id: task-32
title: Add Unit Tests for JSON Serialization
status: Done
assignee:
  - '@assistant'
created_date: '2025-12-28 06:34'
updated_date: '2025-12-28 18:17'
labels:
  - phase-2
  - testing
  - unit
dependencies:
  - task-31
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Create unit tests for JSON serialization in tests/SerializationTests.fs.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 Define simple test record type: type TestUser = { Name: string; Age: int }
- [x] #2 Add test: serialize then deserialize roundtrips TestUser correctly
- [x] #3 Add test: camelCase property naming is applied (Name becomes 'name' in JSON)
- [x] #4 Add test: F# option types serialize correctly (Some as value, None as null)
- [x] #5 Run 'dotnet test' - all tests pass
- [x] #6 Run 'task lint' - no errors or warnings

- [x] #7 Create file tests/SerializationTests.fs
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Check existing test file structure in tests/
2. Create tests/SerializationTests.fs with xUnit test module
3. Define TestUser record type for testing
4. Add test for serialize/deserialize roundtrip
5. Add test for camelCase property naming
6. Add test for F# option types (Some/None)
7. Add test for serializeToBytes/deserializeFromBytes roundtrip
8. Update tests/FractalDb.Tests.fsproj to include SerializationTests.fs
9. Run dotnet test to verify all tests pass
10. Run task lint to verify
11. Mark all ACs complete
<!-- SECTION:PLAN:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Created comprehensive unit tests for JSON serialization in tests/SerializationTests.fs.

Implementation details:
- Created tests/SerializationTests.fs with 10 test cases
- Defined TestUser record type with: Name (string), Age (int), Email (option<string>)
- Test coverage:
  * Serialize/deserialize roundtrip correctness
  * CamelCase property naming (Name → "name" in JSON)
  * Option type handling: Some → value, None → null
  * Bidirectional option conversion (null → None, value → Some)
  * Byte array serialization roundtrip (serializeToBytes/deserializeFromBytes)
  * Byte array non-empty validation
  * Equivalence between string and byte serialization
  * Edge cases (null email, empty values)
- All tests use xUnit and FsUnit.Xunit for assertions
- Updated tests/FractalDb.Tests.fsproj to include SerializationTests.fs

Test results:
- Total tests: 32 (22 QueryTests + 10 SerializationTests)
- All tests passed: 32/32 ✓
- Duration: 59ms

Verification:
- dotnet test: Success (32/32 passed)
- task lint: Success (0 warnings)
- File size: 114 lines

Ready for next task: Task 33 - Implement TranslatorResult Record and Module
<!-- SECTION:NOTES:END -->
