/**
 * Error message builder utilities for consistent, actionable error messages.
 *
 * @remarks
 * This module provides helper functions to build clear error messages
 * that follow consistent formatting and provide actionable guidance.
 *
 * Error Message Structure:
 * 1. What happened (the error)
 * 2. Context (field, value, expected type)
 * 3. Why it happened (if applicable)
 * 4. How to fix it (actionable suggestion)
 *
 * @internal
 */

/**
 * Formats a value for display in error messages.
 *
 * @param value - The value to format
 * @returns Formatted string representation
 *
 * @internal
 */
export function formatValue(value: unknown): string {
  if (value === null) {
    return "null"
  }
  if (value === undefined) {
    return "undefined"
  }
  if (typeof value === "string") {
    return `"${value}"`
  }
  if (typeof value === "number" || typeof value === "boolean") {
    return String(value)
  }
  if (typeof value === "bigint") {
    return `${value}n`
  }
  if (value instanceof Date) {
    return `Date(${value.toISOString()})`
  }
  if (value instanceof RegExp) {
    return String(value)
  }
  if (Array.isArray(value)) {
    return `Array(${value.length})`
  }
  if (typeof value === "object") {
    return "Object"
  }
  return String(value)
}

/**
 * Gets the type name of a value for error messages.
 *
 * @param value - The value to get the type of
 * @returns Human-readable type name
 *
 * @internal
 */
export function getTypeName(value: unknown): string {
  if (value === null) {
    return "null"
  }
  if (value === undefined) {
    return "undefined"
  }
  if (Array.isArray(value)) {
    return "array"
  }
  if (value instanceof Date) {
    return "Date"
  }
  if (value instanceof RegExp) {
    return "RegExp"
  }
  if (value instanceof Uint8Array) {
    return "Uint8Array"
  }
  return typeof value
}

/**
 * Builds a validation error message with field, expected type, and actual value.
 *
 * @param field - The field that failed validation
 * @param expectedType - The expected type or description
 * @param actualValue - The actual value that failed
 * @param suggestion - Optional suggestion for fixing the error
 * @returns Formatted error message
 *
 * @example
 * ```typescript
 * buildValidationMessage('age', 'number', 'thirty')
 * // "Validation failed for field 'age': expected number, got string \"thirty\""
 * ```
 *
 * @internal
 */
export function buildValidationMessage(
  field: string,
  expectedType: string,
  actualValue: unknown,
  suggestion?: string
): string {
  const actualType = getTypeName(actualValue)
  const formattedValue = formatValue(actualValue)

  let message = `Validation failed for field '${field}': expected ${expectedType}, got ${actualType}`

  if (formattedValue !== actualType) {
    message += ` ${formattedValue}`
  }

  if (suggestion) {
    message += `. ${suggestion}`
  }

  return message
}

/**
 * Builds a unique constraint error message with upsert suggestion.
 *
 * @param field - The field with unique constraint violation
 * @param value - The duplicate value
 * @param collectionName - Optional collection name for context
 * @returns Formatted error message with suggestion
 *
 * @example
 * ```typescript
 * buildUniqueConstraintMessage('email', 'user@example.com', 'users')
 * // "Duplicate value for unique field 'email': \"user@example.com\".
 * //  A document with this value already exists.
 * //  Use updateOne() with upsert option to update existing documents,
 * //  or check for existence before inserting."
 * ```
 *
 * @internal
 */
export function buildUniqueConstraintMessage(
  field: string,
  value: unknown,
  collectionName?: string
): string {
  const formattedValue = formatValue(value)
  const location = collectionName ? ` in collection '${collectionName}'` : ""

  return (
    `Duplicate value for unique field '${field}': ${formattedValue}${location}. ` +
    "A document with this value already exists. " +
    "Use updateOne() with { upsert: true } to update existing documents, " +
    "or use findOne() to check for existence before inserting."
  )
}

/**
 * Builds a type mismatch error message with operator suggestions.
 *
 * @param field - The field with type mismatch
 * @param expectedType - The expected type for the field
 * @param actualType - The actual type provided
 * @param operator - Optional operator that caused the mismatch
 * @returns Formatted error message with suggestions
 *
 * @example
 * ```typescript
 * buildTypeMismatchMessage('name', 'string', 'number', '$gt')
 * // "Type mismatch for field 'name': expected string, got number.
 * //  The operator '$gt' is not valid for string fields.
 * //  For string fields, use operators like $eq, $in, $like, $startsWith, $endsWith."
 * ```
 *
 * @internal
 */
export function buildTypeMismatchMessage(
  field: string,
  expectedType: string,
  actualType: string,
  operator?: string
): string {
  let message = `Type mismatch for field '${field}': expected ${expectedType}, got ${actualType}`

  if (operator) {
    message += `. The operator '${operator}' is not valid for ${expectedType} fields`

    // Provide operator suggestions based on type
    const suggestions = getOperatorSuggestions(expectedType)
    if (suggestions) {
      message += `. For ${expectedType} fields, use operators like ${suggestions}`
    }
  }

  return message
}

/**
 * Gets operator suggestions for a given type.
 *
 * @param type - The field type
 * @returns Comma-separated list of suggested operators
 *
 * @internal
 */
function getOperatorSuggestions(type: string): string | null {
  const suggestions: Record<string, string> = {
    string: "$eq, $in, $like, $startsWith, $endsWith, $ne",
    number: "$eq, $gt, $gte, $lt, $lte, $in, $ne",
    integer: "$eq, $gt, $gte, $lt, $lte, $in, $ne",
    boolean: "$eq, $ne",
    array: "$in, $all, $elemMatch, $size",
  }

  return suggestions[type.toLowerCase()] || null
}

/**
 * Builds a query error message with operator and context.
 *
 * @param operator - The query operator that caused the error
 * @param reason - The reason for the error
 * @param suggestion - Optional suggestion for fixing the error
 * @returns Formatted error message
 *
 * @example
 * ```typescript
 * buildQueryErrorMessage('$invalidOp', 'Operator not recognized',
 *   'Use valid operators like $eq, $gt, $in')
 * // "Query error with operator '$invalidOp': Operator not recognized.
 * //  Use valid operators like $eq, $gt, $in"
 * ```
 *
 * @internal
 */
export function buildQueryErrorMessage(
  operator: string,
  reason: string,
  suggestion?: string
): string {
  let message = `Query error with operator '${operator}': ${reason}`

  if (suggestion) {
    message += `. ${suggestion}`
  }

  return message
}

/**
 * Builds a database error message with SQLite error code context.
 *
 * @param operation - The operation that failed
 * @param sqliteCode - Optional SQLite error code
 * @param details - Optional additional details
 * @returns Formatted error message
 *
 * @example
 * ```typescript
 * buildDatabaseErrorMessage('INSERT', 19, 'CONSTRAINT failed')
 * // "Database error during INSERT operation (SQLite error 19: SQLITE_CONSTRAINT).
 * //  CONSTRAINT failed. This usually indicates a constraint violation
 * //  (unique, foreign key, etc.)"
 * ```
 *
 * @internal
 */
export function buildDatabaseErrorMessage(
  operation: string,
  sqliteCode?: number,
  details?: string
): string {
  let message = `Database error during ${operation} operation`

  if (sqliteCode !== undefined) {
    message += ` (SQLite error ${sqliteCode}: ${getSQLiteErrorName(sqliteCode)})`
  }

  if (details) {
    message += `. ${details}`
  }

  // Add general guidance based on error code
  const guidance = getSQLiteErrorGuidance(sqliteCode)
  if (guidance) {
    message += `. ${guidance}`
  }

  return message
}

/**
 * Gets the SQLite error name for a given error code.
 *
 * @param code - SQLite error code
 * @returns Error name
 *
 * @internal
 */
function getSQLiteErrorName(code?: number): string {
  if (code === undefined) {
    return "UNKNOWN"
  }

  const errorNames: Record<number, string> = {
    1: "SQLITE_ERROR",
    2: "SQLITE_INTERNAL",
    3: "SQLITE_PERM",
    4: "SQLITE_ABORT",
    5: "SQLITE_BUSY",
    6: "SQLITE_LOCKED",
    7: "SQLITE_NOMEM",
    8: "SQLITE_READONLY",
    9: "SQLITE_INTERRUPT",
    10: "SQLITE_IOERR",
    11: "SQLITE_CORRUPT",
    13: "SQLITE_FULL",
    14: "SQLITE_CANTOPEN",
    19: "SQLITE_CONSTRAINT",
    20: "SQLITE_MISMATCH",
    21: "SQLITE_MISUSE",
    23: "SQLITE_NOLFS",
    24: "SQLITE_AUTH",
    26: "SQLITE_NOTADB",
  }

  return errorNames[code] || `UNKNOWN_${code}`
}

/**
 * Gets guidance for a given SQLite error code.
 *
 * @param code - SQLite error code
 * @returns Guidance message or null
 *
 * @internal
 */
function getSQLiteErrorGuidance(code?: number): string | null {
  if (code === undefined) {
    return null
  }

  const guidance: Record<number, string> = {
    19: "This usually indicates a constraint violation (unique, foreign key, not null, etc.)",
    5: "The database is locked by another process. Try again after a short delay",
    8: "Attempting to write to a read-only database",
    13: "Disk is full",
    14: "Cannot open the database file. Check file path and permissions",
    26: "The file is not a valid SQLite database",
  }

  return guidance[code] || null
}

/**
 * Builds a document validation error message with context.
 *
 * @param documentId - Optional document ID
 * @param validatorName - Optional validator name (e.g., 'Standard Schema')
 * @param details - Optional validation details
 * @returns Formatted error message
 *
 * @example
 * ```typescript
 * buildDocumentValidationMessage('user-123', 'Standard Schema', 'Email format invalid')
 * // "Document validation failed for id 'user-123' using Standard Schema validator:
 * //  Email format invalid. Check that all required fields are present and have correct types."
 * ```
 *
 * @internal
 */
export function buildDocumentValidationMessage(
  documentId?: string,
  validatorName?: string,
  details?: string
): string {
  let message = "Document validation failed"

  if (documentId) {
    message += ` for id '${documentId}'`
  }

  if (validatorName) {
    message += ` using ${validatorName} validator`
  }

  if (details) {
    message += `: ${details}`
  }

  message +=
    ". Check that all required fields are present and have correct types."

  return message
}
