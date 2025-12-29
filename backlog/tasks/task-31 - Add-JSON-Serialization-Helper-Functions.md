---
id: task-31
title: Add JSON Serialization Helper Functions
status: Done
assignee:
  - '@assistant'
created_date: '2025-12-28 06:34'
updated_date: '2025-12-28 18:15'
labels:
  - phase-2
  - json
dependencies:
  - task-30
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add serialize and deserialize helper functions for Document<'T> in src/Serialization.fs. Reference: FSHARP_PORT_DESIGN.md lines 85-86.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 Add 'let deserialize<'T> (json: string) : 'T' using JsonSerializer.Deserialize with configured options
- [x] #2 Add 'let serializeToBytes<'T> (doc: 'T) : byte[]' for JSONB storage
- [x] #3 Add 'let deserializeFromBytes<'T> (bytes: byte[]) : 'T'
- [x] #4 Run 'dotnet build' - build succeeds
- [x] #5 All public types and functions have XML doc comments with <summary>, and public functions include <param>, <returns>, and <example> (see doc-2 for standards)

- [x] #6 Run 'task lint' - no errors or warnings

- [x] #7 In src/Serialization.fs, add 'let serialize<'T> (doc: 'T) : string' using JsonSerializer.Serialize with configured options
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Read current Serialization.fs to understand structure
2. Add serialize<'T> function using JsonSerializer.Serialize
3. Add deserialize<'T> function using JsonSerializer.Deserialize
4. Add serializeToBytes<'T> function using JsonSerializer.SerializeToUtf8Bytes
5. Add deserializeFromBytes<'T> function using JsonSerializer.Deserialize with ReadOnlySpan
6. Add comprehensive XML documentation for all functions
7. Run dotnet build to verify
8. Run task lint to verify
9. Mark all ACs complete
<!-- SECTION:PLAN:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Added JSON serialization helper functions to src/Serialization.fs.

Implementation details:
- Added 4 helper functions:
  * serialize<'T> value → string (JSON string serialization)
  * deserialize<'T> json → 'T (JSON string deserialization)
  * serializeToBytes<'T> value → array<byte> (UTF-8 byte array for SQLite BLOB)
  * deserializeFromBytes<'T> bytes → 'T (deserialize from UTF-8 byte array)
- All functions use the configured defaultOptions (camelCase, F# types support)
- Comprehensive XML documentation for all functions with <summary>, <param>, <returns>, <remarks>, <example>
- serializeToBytes/deserializeFromBytes are optimized for SQLite BLOB storage (no string allocation)
- Used prefix syntax for generic types: array<byte> instead of byte array
- Documentation explains JSONB usage with SQLite json_extract() and jsonb() functions

Function signatures:
- serialize: 'T → string
- deserialize: string → 'T
- serializeToBytes: 'T → array<byte>
- deserializeFromBytes: array<byte> → 'T

Verification:
- dotnet build: Success (0 warnings, 0 errors)
- task lint: Success (0 warnings)
- File size: 168 lines

Ready for next task: Task 32 - Add Unit Tests for JSON Serialization
<!-- SECTION:NOTES:END -->
