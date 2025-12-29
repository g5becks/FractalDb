---
id: task-89
title: Add Collection.validate method
status: Done
assignee:
  - '@assistant'
created_date: '2025-12-29 06:00'
updated_date: '2025-12-29 07:07'
labels:
  - collection
  - api
  - validation
dependencies: []
priority: medium
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add validate method to Collection module that validates a document against the schema without inserting it. Returns FractalResult indicating success or validation errors.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 validate function takes data and collection
- [x] #2 Returns FractalResult<'T>
- [x] #3 Uses schema.Validate if defined
- [x] #4 Returns Ok data if no validator defined
- [x] #5 Code builds with no errors or warnings
- [x] #6 All existing tests pass

- [x] #7 XML doc comments on validate function with summary, params, returns, and example
<!-- AC:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Function already exists in Collection.fs (lines 3062-3174) and fully meets all requirements.

**Verification:**
- ✅ Signature: `validate (doc: 'T) (collection: Collection<'T>) : FractalResult<'T>`
- ✅ Uses `collection.Schema.Validate` if defined
- ✅ Returns `Ok doc` if no validator defined
- ✅ Converts validation errors to FractalError.Validation
- ✅ Comprehensive XML documentation with summary, params, returns, remarks, and examples
- ✅ Builds successfully with 0 errors, 0 warnings

**Implementation Details:**
- Pattern matches on `collection.Schema.Validate` (Option type)
- If Some: runs validator and maps Error(msg) to Error(FractalError.Validation(None, msg))
- If None: returns Ok(doc) directly (validation bypassed)
- Synchronous operation (no database access)
- Pure function - thread-safe for concurrent calls

**Documentation includes:**
- Comprehensive remarks on validation timing, use cases, and performance
- Multiple examples: pre-insert validation, batch validation, schema without validator
- Validator function signature documentation

**No changes required** - implementation was already complete from previous work.
<!-- SECTION:NOTES:END -->
