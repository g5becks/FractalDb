---
id: task-11
title: Define query options types
status: Done
assignee:
  - '@claude'
created_date: '2025-11-21 02:31'
updated_date: '2025-11-21 05:48'
labels:
  - query
  - types
dependencies: []
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Implement types for query sorting, pagination, and field projection. These options control result ordering, limiting, skipping, and field selection while maintaining type safety across all operations.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 SortSpec<T> type defined mapping document fields to 1 (ascending) or -1 (descending)
- [x] #2 ProjectionSpec<T> type defined mapping document fields to 1 (include) or 0 (exclude)
- [x] #3 QueryOptions<T> type defined with sort, limit, skip, and projection properties
- [x] #4 All properties appropriately marked as readonly
- [x] #5 Types work correctly with nested document structures
- [x] #6 TypeScript type checking passes with zero errors
- [x] #7 No any types used in implementation
- [x] #8 Complete TypeDoc comments with examples showing pagination and field projection patterns
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Create new file src/query-options-types.ts
2. Define SortSpec<T> type for field ordering
3. Define ProjectionSpec<T> type for field selection
4. Define QueryOptions<T> combining all options
5. Add comprehensive TypeDoc documentation
6. Verify TypeScript compilation
7. Run linting checks
<!-- SECTION:PLAN:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
## Implementation Summary

Successfully implemented query options types for sorting, pagination, and field projection with complete type safety.

## Changes Made

### 1. Created New File
- Created src/query-options-types.ts
- Organized all query option types in dedicated module

### 2. SortSpec<T> Type
- Maps document fields to 1 (ascending) or -1 (descending)
- Supports multi-field sorting with order precedence
- All properties readonly for immutability
- Works with all document field types

### 3. ProjectionSpec<T> Type
- Maps document fields to 1 (include) or 0 (exclude)
- Supports MongoDB-style field selection
- All properties readonly
- Handles id field exclusion correctly

### 4. QueryOptions<T> Type
- Combines sort, limit, skip, and projection
- All properties optional and readonly
- Enables pagination patterns
- Supports performance optimization

## Documentation
Comprehensive TypeDoc comments including:
- Detailed explanations of each option
- Multiple usage examples for each type
- Pagination patterns and helpers
- Performance optimization examples
- MongoDB-style syntax guidance
- Best practices for field projection

## Type Safety Features
- All fields type-checked against document structure
- Readonly modifiers prevent accidental mutation
- Zero `any` types throughout
- Works with nested document structures
- Full TypeScript inference

## Usage Examples Provided
- Single and multi-field sorting
- Pagination with skip/limit
- Field inclusion and exclusion
- Complex pagination helpers
- Performance optimization patterns

## Verification
- ✅ TypeScript compilation: Pass
- ✅ Biome/Ultracite linting: Pass
- ✅ All acceptance criteria met
- ✅ Zero `any` types
- ✅ Readonly properties
- ✅ Comprehensive documentation

## Files Created
- src/query-options-types.ts (new file)

## Design Notes
- Kept types simple and MongoDB-compatible
- All properties optional for flexibility
- Readonly ensures immutability
- Clear examples demonstrate common patterns
<!-- SECTION:NOTES:END -->
