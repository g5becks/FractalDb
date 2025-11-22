---
id: task-14
title: Implement Standard Schema validator wrapper
status: Done
assignee:
  - '@claude'
created_date: '2025-11-21 02:55'
updated_date: '2025-11-21 06:10'
labels:
  - validation
dependencies: []
priority: medium
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Create a wrapper that integrates Standard Schema-compatible validators (Zod, Valibot, ArkType, etc.) with StrataDB's validation system. This enables any Standard Schema validator to work seamlessly with StrataDB while converting validation failures to StrataDB error types.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 Validator wrapper accepts Standard Schema-compatible objects with tilde-v property
- [x] #2 Wrapper provides validate method that calls Standard Schema validation
- [x] #3 Successful validation returns type predicate (doc is T)
- [x] #4 Validation failures are converted to ValidationError with field and value context
- [x] #5 Wrapper extracts detailed error information from Standard Schema failure objects
- [x] #6 TypeScript type checking passes with zero errors
- [x] #7 No any types used in implementation
- [x] #8 Complete TypeDoc comments with examples using Zod and ArkType validators
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Copy Standard Schema types into src/standard-schema.ts
2. Create validator wrapper function that accepts Standard Schema objects
3. Implement validation logic that calls schema['~standard'].validate()
4. Convert Standard Schema failure results to StrataDB ValidationError
5. Extract field paths and error messages from issue objects
6. Return type predicate function for successful validation
7. Add comprehensive TypeDoc with Zod and ArkType examples
8. Verify TypeScript compilation and linting
<!-- SECTION:PLAN:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
## Implementation Summary

Successfully implemented Standard Schema validator wrapper for StrataDB, enabling integration with Zod, Valibot, ArkType, and other Standard Schema-compatible validators.

## Implementation Details

**Created src/standard-schema.ts** (95 lines):
- Complete Standard Schema v1 specification types
- Copied directly from official specification
- Includes StandardSchemaV1 type with ~standard property
- Namespace with Props, Result, Issue, PathSegment types
- Type inference utilities (InferInput, InferOutput)
- Uses namespace for backwards compatibility with spec

**Created src/validator.ts** (210 lines):

1. **wrapStandardSchema<T>() function**:
   - Accepts any StandardSchemaV1-compatible schema
   - Returns type predicate: (doc: unknown) => doc is T
   - Only supports synchronous validation (throws on Promise)
   - Converts Standard Schema failures to StrataDB ValidationError

2. **Error Conversion Logic**:
   - Extracts first issue from Standard Schema failure result
   - Calls extractFieldPath() to get field name from issue path
   - Calls extractValueAtPath() to get failing value from document
   - Creates ValidationError with message, field, and value

3. **extractFieldPath() helper**:
   - Converts Standard Schema path array to dot-notation string
   - Handles both PropertyKey and PathSegment objects
   - Returns "unknown" if path is empty/undefined

4. **extractValueAtPath() helper**:
   - Navigates document following Standard Schema path
   - Returns value at the specified path
   - Returns current value if path cannot be fully navigated

## Integration Examples

Comprehensive TypeDoc documentation includes examples for:
- Zod validator integration
- Valibot validator integration
- ArkType validator integration
- Error handling with ValidationError

## Technical Features

- ✅ Zero any types throughout
- ✅ Full type safety with type predicates
- ✅ Proper error context extraction (field + value)
- ✅ Synchronous validation only (explicit error on async)
- ✅ Compatible with all Standard Schema v1 libraries
- ✅ Detailed error messages from schema validators

## Files Created

- **src/standard-schema.ts** (95 lines)
- **src/validator.ts** (210 lines)

## Verification

- ✅ TypeScript compilation: Zero errors
- ✅ Biome linting: All checks pass
- ✅ Zero any types
- ✅ Comprehensive TypeDoc documentation
- ✅ All acceptance criteria verified
<!-- SECTION:NOTES:END -->
