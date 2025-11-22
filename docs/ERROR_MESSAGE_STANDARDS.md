# Error Message Standards

This document defines the standards and best practices for error messages in StrataDB. Good error messages significantly improve developer experience by providing clear, actionable guidance.

## Error Message Structure

All error messages should follow this structure:

1. **What happened** - Clearly state what went wrong
2. **Context** - Provide relevant details (field names, values, expected types)
3. **Why it happened** - Explain the cause (if not obvious)
4. **How to fix it** - Provide actionable suggestions

### Example

```typescript
// Bad
throw new Error("Validation failed")

// Good
throw new ValidationError(
  "Validation failed for field 'email': expected valid email address, got string \"invalid\". " +
  "Use format: user@example.com",
  "email",
  "invalid"
)
```

## Error Categories

### 1. Validation Errors

**Purpose**: Indicate that data doesn't match expected schema or constraints

**Requirements**:
- ✅ Include field name
- ✅ Include expected type/format
- ✅ Include actual value (safely formatted)
- ✅ Suggest how to fix

**Examples**:

```typescript
// Field validation
new ValidationError(
  "Validation failed for field 'age': expected number, got string \"thirty\"",
  "age",
  "thirty"
)

// Document validation
new ValidationError(
  "Document validation failed: Document does not match the schema. " +
  "Check that all required fields are present and have correct types.",
  undefined,
  doc
)

// Context-specific validation
new ValidationError(
  "Document validation failed in batch insert at index 5: Document does not match the schema. " +
  "Check that all required fields are present and have correct types.",
  undefined,
  doc
)
```

### 2. Unique Constraint Errors

**Purpose**: Indicate duplicate values for unique fields

**Requirements**:
- ✅ Include field name
- ✅ Include duplicate value
- ✅ Include collection name (when available)
- ✅ Suggest upsert or existence check

**Example**:

```typescript
new UniqueConstraintError(
  "Duplicate value for unique field 'email': \"user@example.com\" in collection 'users'. " +
  "A document with this value already exists. " +
  "Use updateOne() with { upsert: true } to update existing documents, " +
  "or use findOne() to check for existence before inserting.",
  "email",
  "user@example.com"
)
```

### 3. Type Mismatch Errors

**Purpose**: Indicate wrong types in queries or operations

**Requirements**:
- ✅ Include field name
- ✅ Include expected type
- ✅ Include actual type
- ✅ Suggest correct operators for the type

**Example**:

```typescript
new TypeMismatchError(
  "Type mismatch for field 'age': expected number, got string. " +
  "The operator '$gt' is not valid for string fields. " +
  "For number fields, use operators like $eq, $gt, $gte, $lt, $lte, $in, $ne.",
  {
    field: "age",
    expectedType: "number",
    actualType: "string",
    operator: "$gt"
  }
)
```

### 4. Query Errors

**Purpose**: Indicate problems with query syntax or operators

**Requirements**:
- ✅ Include operator that failed
- ✅ Explain why it failed
- ✅ Suggest valid alternatives

**Example**:

```typescript
new QueryError(
  "Query error with operator '$invalidOp': Operator not recognized. " +
  "Use valid operators like $eq, $gt, $in, $like.",
  JSON.stringify(query)
)
```

### 5. Database Errors

**Purpose**: Indicate database-level errors (SQLite, constraints, etc.)

**Requirements**:
- ✅ Include operation that failed
- ✅ Include SQLite error code (when available)
- ✅ Include SQLite error name
- ✅ Provide guidance based on error code

**Example**:

```typescript
new DatabaseError(
  "Database error during INSERT operation (SQLite error 19: SQLITE_CONSTRAINT). " +
  "UNIQUE constraint failed: users.email. " +
  "This usually indicates a constraint violation (unique, foreign key, not null, etc.).",
  19
)
```

## Message Builder Utilities

Use the helper functions in `src/error-messages.ts` to build consistent messages:

### `buildValidationMessage()`

```typescript
buildValidationMessage(
  "age",                    // field name
  "number",                 // expected type
  "thirty",                 // actual value
  "Use a numeric value"     // optional suggestion
)
// "Validation failed for field 'age': expected number, got string \"thirty\".
//  Use a numeric value"
```

### `buildUniqueConstraintMessage()`

```typescript
buildUniqueConstraintMessage(
  "email",                 // field name
  "user@example.com",      // duplicate value
  "users"                  // optional collection name
)
// "Duplicate value for unique field 'email': \"user@example.com\" in collection 'users'.
//  A document with this value already exists.
//  Use updateOne() with { upsert: true } to update existing documents,
//  or use findOne() to check for existence before inserting."
```

### `buildTypeMismatchMessage()`

```typescript
buildTypeMismatchMessage(
  "age",        // field name
  "number",     // expected type
  "string",     // actual type
  "$gt"         // optional operator
)
// "Type mismatch for field 'age': expected number, got string.
//  The operator '$gt' is not valid for string fields.
//  For number fields, use operators like $eq, $gt, $gte, $lt, $lte, $in, $ne."
```

### `buildQueryErrorMessage()`

```typescript
buildQueryErrorMessage(
  "$invalidOp",                           // operator
  "Operator not recognized",              // reason
  "Use valid operators like $eq, $gt"     // optional suggestion
)
// "Query error with operator '$invalidOp': Operator not recognized.
//  Use valid operators like $eq, $gt"
```

### `buildDatabaseErrorMessage()`

```typescript
buildDatabaseErrorMessage(
  "INSERT",                                // operation
  19,                                      // SQLite error code
  "UNIQUE constraint failed: users.email"  // optional details
)
// "Database error during INSERT operation (SQLite error 19: SQLITE_CONSTRAINT).
//  UNIQUE constraint failed: users.email.
//  This usually indicates a constraint violation (unique, foreign key, not null, etc.)."
```

### `buildDocumentValidationMessage()`

```typescript
buildDocumentValidationMessage(
  "user-123",           // optional document ID
  "Standard Schema",    // optional validator name
  "Email format invalid"  // optional details
)
// "Document validation failed for id 'user-123' using Standard Schema validator:
//  Email format invalid.
//  Check that all required fields are present and have correct types."
```

## Format Utilities

### `formatValue(value: unknown): string`

Safely formats values for display in error messages:

```typescript
formatValue(null)          // "null"
formatValue(undefined)     // "undefined"
formatValue("test")        // '"test"'
formatValue(42)            // "42"
formatValue(true)          // "true"
formatValue([1, 2, 3])     // "Array(3)"
formatValue({ a: 1 })      // "Object"
formatValue(/test/i)       // "/test/i"
```

### `getTypeName(value: unknown): string`

Gets human-readable type names:

```typescript
getTypeName(null)          // "null"
getTypeName([])            // "array"
getTypeName(new Date())    // "Date"
getTypeName(/test/)        // "RegExp"
getTypeName({})            // "object"
```

## Best Practices

### 1. Be Specific

```typescript
// ❌ Bad - too vague
throw new Error("Operation failed")

// ✅ Good - specific about what failed
throw new ValidationError(
  "Validation failed for field 'email': expected valid email address, got string \"invalid\"",
  "email",
  "invalid"
)
```

### 2. Provide Context

```typescript
// ❌ Bad - no context
throw new Error("Duplicate value")

// ✅ Good - includes field, value, and collection
throw new UniqueConstraintError(
  "Duplicate value for unique field 'email': \"user@example.com\" in collection 'users'",
  "email",
  "user@example.com"
)
```

### 3. Suggest Solutions

```typescript
// ❌ Bad - no guidance
throw new Error("Constraint violation")

// ✅ Good - suggests alternatives
throw new UniqueConstraintError(
  "Duplicate value for unique field 'email'. " +
  "Use updateOne() with { upsert: true } to update existing documents, " +
  "or use findOne() to check for existence before inserting.",
  "email",
  value
)
```

### 4. Use Consistent Formatting

- Start with what went wrong
- Use single quotes for field names: `'fieldName'`
- Use double quotes for values: `"value"`
- Use backticks for operators: `` `$operator` ``
- Separate multiple sentences with periods
- Use commas for lists: `$eq, $gt, $in`

### 5. Handle Edge Cases

```typescript
// Handle missing field names
const message = field
  ? `Duplicate value for unique field '${field}': ${formatValue(value)}`
  : "Unique constraint violation occurred"

// Handle undefined values safely
const value = field ? (doc as Record<string, unknown>)[field] : undefined
```

## SQLite Error Codes

Common SQLite error codes and their meanings:

| Code | Name | Description | Guidance |
|------|------|-------------|----------|
| 1 | SQLITE_ERROR | Generic error | Check SQL syntax and query structure |
| 5 | SQLITE_BUSY | Database locked | Retry after short delay |
| 8 | SQLITE_READONLY | Read-only database | Check file permissions |
| 13 | SQLITE_FULL | Disk full | Free up disk space |
| 14 | SQLITE_CANTOPEN | Can't open database | Check file path and permissions |
| 19 | SQLITE_CONSTRAINT | Constraint violation | Check unique, foreign key, not null constraints |
| 20 | SQLITE_MISMATCH | Type mismatch | Verify data types match schema |
| 26 | SQLITE_NOTADB | Not a database file | Verify file is valid SQLite database |

## Testing Error Messages

All error message builders have comprehensive tests in `test/unit/error-messages.test.ts`:

```typescript
it("should build actionable unique constraint message", () => {
  const message = buildUniqueConstraintMessage(
    "email",
    "user@example.com",
    "users"
  )

  expect(message).toContain("Duplicate value for unique field 'email'")
  expect(message).toContain('"user@example.com"')
  expect(message).toContain("in collection 'users'")
  expect(message).toContain("Use updateOne() with { upsert: true }")
})
```

## Checklist for New Error Messages

When adding or updating error messages:

- [ ] Message states what went wrong clearly
- [ ] Includes relevant context (field, value, type, etc.)
- [ ] Explains why it happened (if not obvious)
- [ ] Provides actionable suggestions
- [ ] Uses consistent formatting
- [ ] Uses message builder utilities when applicable
- [ ] Safely formats user values
- [ ] Handles edge cases (missing fields, undefined values)
- [ ] Has test coverage
- [ ] Follows the structure: what, context, why, how

## Examples by Error Type

### ValidationError
```typescript
new ValidationError(
  "Validation failed for field 'age': expected number, got string \"thirty\"",
  "age",
  "thirty"
)
```

### UniqueConstraintError
```typescript
new UniqueConstraintError(
  "Duplicate value for unique field 'email': \"user@example.com\" in collection 'users'. " +
  "Use updateOne() with { upsert: true } or findOne() to check existence.",
  "email",
  "user@example.com"
)
```

### TypeMismatchError
```typescript
new TypeMismatchError(
  "Type mismatch for field 'age': expected number, got string. " +
  "For number fields, use operators like $eq, $gt, $gte.",
  { field: "age", expectedType: "number", actualType: "string" }
)
```

### QueryError
```typescript
new QueryError(
  "Query error with operator '$invalidOp': Operator not recognized. " +
  "Use valid operators like $eq, $gt, $in.",
  JSON.stringify(query)
)
```

### DatabaseError
```typescript
new DatabaseError(
  "Database error during INSERT (SQLite error 19: SQLITE_CONSTRAINT). " +
  "This usually indicates a constraint violation.",
  19
)
```

## Maintenance

- Review error messages during code reviews
- Update message builders when adding new error types
- Keep SQLite error code mappings current
- Add tests for new error scenarios
- Update this document when standards change
