---
id: task-25
title: Implement SchemaDef Record
status: Done
assignee:
  - '@assistant'
created_date: '2025-12-28 06:33'
updated_date: '2025-12-28 18:02'
labels:
  - phase-1
  - core
  - schema
dependencies:
  - task-24
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Create SchemaDef<'T> record in src/Schema.fs. Reference: FSHARP_PORT_DESIGN.md lines 602-608.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 SchemaDef has: Fields: FieldDef list, Indexes: IndexDef list, Timestamps: bool, Validate: ('T -> Result<'T, string>) option
- [x] #2 Run 'dotnet build' - build succeeds
- [x] #3 All public types and functions have XML doc comments with <summary>, and public functions include <param>, <returns>, and <example> (see doc-2 for standards)

- [x] #4 Run 'task lint' - no errors or warnings

- [x] #5 In src/Schema.fs, add SchemaDef<'T> record type
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Read FSHARP_PORT_DESIGN.md lines 602-608 for SchemaDef specification
2. Add SchemaDef<'T> generic record with 4 fields
3. Add comprehensive XML documentation with examples
4. Run dotnet build to verify
5. Run task lint to verify
6. Mark all ACs complete
<!-- SECTION:PLAN:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Implemented SchemaDef<'T> generic record in src/Schema.fs with comprehensive XML documentation.

Implementation details:
- Added SchemaDef<'T> record with 4 fields (Fields, Indexes, Timestamps, Validate)
- Used prefix syntax for generic types: list<FieldDef>, option<('T -> Result<'T, string>)>
- Comprehensive XML documentation with <summary> and <remarks> for type and all fields
- Included 3 detailed examples showing: user schema with validation, minimal schema, and product schema with transformation
- Documentation explains when validation is called (insert/update but NOT findOneAndX operations)
- Explained that only fields needing indexing/constraints should be in Fields list

Verification:
- dotnet build: Success (0 warnings, 0 errors)
- task lint: Success (0 warnings)
- File size: 491 lines (well under 1000 line limit)

Ready for next task: Task 26 - SortDirection and CursorSpec types
<!-- SECTION:NOTES:END -->
