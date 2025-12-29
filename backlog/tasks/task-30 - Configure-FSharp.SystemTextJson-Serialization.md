---
id: task-30
title: Configure FSharp.SystemTextJson Serialization
status: Done
assignee:
  - '@assistant'
created_date: '2025-12-28 06:34'
updated_date: '2025-12-28 18:14'
labels:
  - phase-2
  - json
dependencies:
  - task-29
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Set up JSON serialization with FSharp.SystemTextJson in src/Serialization.fs. Reference: FSHARP_PORT_DESIGN.md lines 85-86, 2672.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 Add namespace FractalDb.Json
- [x] #2 Create JsonSerializerOptions with JsonFSharpConverter() added to Converters
- [x] #3 Set PropertyNamingPolicy = JsonNamingPolicy.CamelCase
- [x] #4 Run 'dotnet build' - build succeeds
- [x] #5 All public types and functions have XML doc comments with <summary>, and public functions include <param>, <returns>, and <example> (see doc-2 for standards)
- [x] #6 Run 'task lint' - no errors or warnings

- [x] #7 Create file src/Serialization.fs

- [x] #8 Add module declaration: module FractalDb.Serialization
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Check if FSharp.SystemTextJson package is in project file
2. Add package reference if missing
3. Create new file src/Serialization.fs
4. Add module declaration: module FractalDb.Serialization
5. Create defaultOptions with JsonSerializerOptions configured:
   - Add JsonFSharpConverter to Converters
   - Set PropertyNamingPolicy to CamelCase
6. Add comprehensive XML documentation
7. Update src/FractalDb.fsproj to include Serialization.fs
8. Run dotnet build to verify
9. Run task lint to verify
10. Mark all ACs complete
<!-- SECTION:PLAN:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Configured FSharp.SystemTextJson serialization in src/Serialization.fs.

Implementation details:
- Created new file src/Serialization.fs with module declaration: module FractalDb.Serialization
- Added FSharp.SystemTextJson package reference (version 1.*) to src/FractalDb.fsproj
- Created defaultOptions: JsonSerializerOptions with:
  * JsonFSharpConverter() added to Converters (handles F# records, DUs, options, tuples)
  * PropertyNamingPolicy set to JsonNamingPolicy.CamelCase (PascalCase → camelCase)
- Comprehensive XML documentation with <summary>, <remarks>, and <example> sections
- Documentation explains:
  * What F# types the converter handles (records, DUs, options, lists, tuples)
  * Usage patterns for serialization and deserialization
  * Multiple examples including User records, Status DU, and Document types
- Updated src/FractalDb.fsproj to include Serialization.fs after Options.fs

Package configuration:
- FSharp.SystemTextJson 1.* (latest 1.x version)
- Native AOT compatible
- Zero CLIMutable attributes needed

Verification:
- dotnet build: Success (0 warnings, 0 errors) - package restored successfully
- task lint: Success (0 warnings)
- File size: 68 lines

Ready for next task: Task 31 - Add JSON Serialization Helper Functions
<!-- SECTION:NOTES:END -->
