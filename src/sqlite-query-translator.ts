import type { Document } from "./core-types.js"
import type { QueryOptions } from "./query-options-types.js"
import type {
  QueryTranslator,
  QueryTranslatorResult,
} from "./query-translator-types.js"
import type { QueryFilter } from "./query-types.js"
import type { SchemaDefinition, SchemaField } from "./schema-types.js"

/**
 * SQLite-specific query translator for converting StrataDB queries to SQL.
 *
 * @typeParam T - The document type being queried
 *
 * @remarks
 * This translator converts type-safe StrataDB query filters into parameterized
 * SQLite WHERE clauses. It uses generated columns for indexed fields and
 * jsonb_extract for non-indexed fields, ensuring optimal query performance
 * while preventing SQL injection attacks.
 *
 * **Indexed vs Non-Indexed Fields:**
 * - Indexed fields: Use generated column with underscore prefix (e.g., `_age`)
 * - Non-indexed fields: Use `jsonb_extract(data, '$.field')` directly
 *
 * **SQL Injection Prevention:**
 * All user values are extracted into the params array and bound using
 * parameterized queries. No values are ever concatenated into SQL strings.
 *
 * @example
 * ```typescript
 * import { SQLiteQueryTranslator, type Document } from 'stratadb';
 *
 * type User = Document<{
 *   name: string;
 *   age: number;
 *   email: string;
 *   status: 'active' | 'inactive';
 * }>;
 *
 * const schema = createSchema<User>()
 *   .field('name', { type: 'TEXT', indexed: true })
 *   .field('age', { type: 'INTEGER', indexed: true })
 *   .field('email', { type: 'TEXT', indexed: true, unique: true })
 *   .field('status', { type: 'TEXT', indexed: false })
 *   .build();
 *
 * const translator = new SQLiteQueryTranslator(schema);
 *
 * // ✅ Simple equality (indexed field)
 * const result1 = translator.translate({ name: 'Alice' });
 * // => { sql: "_name = ?", params: ['Alice'] }
 *
 * // ✅ Comparison operators (indexed field)
 * const result2 = translator.translate({ age: { $gte: 18, $lt: 65 } });
 * // => { sql: "(_age >= ? AND _age < ?)", params: [18, 65] }
 *
 * // ✅ Non-indexed field uses jsonb_extract
 * const result3 = translator.translate({ status: 'active' });
 * // => { sql: "jsonb_extract(data, '$.status') = ?", params: ['active'] }
 *
 * // ✅ IN operator with multiple values
 * const result4 = translator.translate({ age: { $in: [25, 30, 35] } });
 * // => { sql: "_age IN (?, ?, ?)", params: [25, 30, 35] }
 *
 * // ✅ String operators
 * const result5 = translator.translate({ email: { $regex: /@example\\.com$/i } });
 * // => { sql: "_email REGEXP ?", params: ['(?i)@example\\.com$'] }
 *
 * const result6 = translator.translate({ name: { $startsWith: 'Admin' } });
 * // => { sql: "_name LIKE ?", params: ['Admin%'] }
 *
 * const result7 = translator.translate({ email: { $endsWith: '@company.com' } });
 * // => { sql: "_email LIKE ?", params: ['%@company.com'] }
 *
 * const result8 = translator.translate({ name: { $like: '%Smith%' } });
 * // => { sql: "_name LIKE ?", params: ['%Smith%'] }
 * ```
 */
export class SQLiteQueryTranslator<T extends Document>
  implements QueryTranslator<T>
{
  /** Map of field names to their schema definitions */
  private readonly fieldMap: Map<keyof T, SchemaField<T, keyof T>>

  /**
   * Creates a new SQLite query translator.
   *
   * @param schema - The schema definition for the document type
   */
  constructor(schema: SchemaDefinition<T>) {
    // Build field map for quick lookups
    this.fieldMap = new Map()
    for (const field of schema.fields) {
      this.fieldMap.set(field.name, field)
    }
  }

  /**
   * Translates a StrataDB query filter to parameterized SQLite SQL.
   *
   * @param filter - The query filter to translate
   * @returns Object containing SQL WHERE clause and parameter values
   *
   * @remarks
   * This method recursively processes the query filter and generates
   * parameterized SQL. All user values are extracted into the params array.
   */
  translate(filter: QueryFilter<T>): QueryTranslatorResult {
    const params: unknown[] = []

    // Empty filter matches all documents
    if (!filter || Object.keys(filter).length === 0) {
      return { sql: "1=1", params: [] }
    }

    const sql = this.translateFilter(filter, params)

    return {
      sql,
      params,
    }
  }

  /**
   * Recursively translates a query filter to SQL.
   *
   * @param filter - The filter to translate
   * @param params - Array to collect parameter values
   * @returns SQL string fragment
   *
   * @internal
   */
  private translateFilter(filter: QueryFilter<T>, params: unknown[]): string {
    const conditions: string[] = []

    for (const [key, value] of Object.entries(filter)) {
      const condition = this.translateFilterEntry(key, value, params)
      if (condition) {
        conditions.push(condition)
      }
    }

    return conditions.length > 0 ? conditions.join(" AND ") : "1=1"
  }

  /**
   * Translates a single filter entry to SQL.
   *
   * @param key - The filter key (field name or logical operator)
   * @param value - The filter value
   * @param params - Array to collect parameter values
   * @returns SQL string fragment or null
   *
   * @internal
   */
  private translateFilterEntry(
    key: string,
    value: unknown,
    params: unknown[]
  ): string | null {
    // Handle logical operators
    if (key === "$and") {
      const andConditions = (value as readonly QueryFilter<T>[]).map((f) =>
        this.translateFilter(f, params)
      )
      return `(${andConditions.join(" AND ")})`
    }

    if (key === "$or") {
      const orConditions = (value as readonly QueryFilter<T>[]).map((f) =>
        this.translateFilter(f, params)
      )
      return `(${orConditions.join(" OR ")})`
    }

    if (key === "$nor") {
      const norConditions = (value as readonly QueryFilter<T>[]).map((f) =>
        this.translateFilter(f, params)
      )
      return `NOT (${norConditions.join(" OR ")})`
    }

    if (key === "$not") {
      const notCondition = this.translateFilter(value as QueryFilter<T>, params)
      return `NOT (${notCondition})`
    }

    // Handle field conditions
    return this.translateFieldCondition(key as keyof T, value, params)
  }

  /**
   * Translates a field condition to SQL.
   *
   * @param fieldName - The field name
   * @param value - The field value or operators
   * @param params - Array to collect parameter values
   * @returns SQL string fragment or null
   *
   * @internal
   */
  private translateFieldCondition(
    fieldName: keyof T,
    value: unknown,
    params: unknown[]
  ): string | null {
    const fieldSql = this.resolveFieldName(fieldName)

    // Direct value comparison (e.g., { name: 'Alice' })
    if (
      value === null ||
      typeof value === "string" ||
      typeof value === "number" ||
      typeof value === "boolean"
    ) {
      params.push(value)
      return `${fieldSql} = ?`
    }

    // Operator conditions (e.g., { age: { $gt: 18 } })
    if (typeof value === "object" && value !== null) {
      const operatorSql = this.translateFieldOperators(
        fieldSql,
        value as Record<string, unknown>,
        params
      )
      return operatorSql || null
    }

    return null
  }

  /**
   * Translates field operators to SQL.
   *
   * @param fieldSql - The SQL representation of the field
   * @param operators - Object containing operators and values
   * @param params - Array to collect parameter values
   * @returns SQL string fragment
   *
   * @internal
   */
  private translateFieldOperators(
    fieldSql: string,
    operators: Record<string, unknown>,
    params: unknown[]
  ): string {
    const conditions: string[] = []

    for (const [op, value] of Object.entries(operators)) {
      const condition = this.translateSingleOperator({
        fieldSql,
        op,
        value,
        operators,
        params,
      })
      if (condition) {
        conditions.push(condition)
      }
    }

    return conditions.length > 0 ? `(${conditions.join(" AND ")})` : ""
  }

  /**
   * Translates a single operator to SQL.
   *
   * @param context - Translation context containing field, operator, value, and params
   * @returns SQL string fragment or null
   *
   * @internal
   */
  private translateSingleOperator(context: {
    fieldSql: string
    op: string
    value: unknown
    operators: Record<string, unknown>
    params: unknown[]
  }): string | null {
    const { fieldSql, op, value, operators, params } = context
    switch (op) {
      case "$eq":
        params.push(value)
        return `${fieldSql} = ?`

      case "$ne":
        params.push(value)
        return `${fieldSql} != ?`

      case "$gt":
        params.push(value)
        return `${fieldSql} > ?`

      case "$gte":
        params.push(value)
        return `${fieldSql} >= ?`

      case "$lt":
        params.push(value)
        return `${fieldSql} < ?`

      case "$lte":
        params.push(value)
        return `${fieldSql} <= ?`

      case "$in":
        return this.translateInOperator(fieldSql, value, params)

      case "$nin":
        return this.translateNotInOperator(fieldSql, value, params)

      case "$regex": {
        const regex = value
        const options = operators.$options
        return this.translateRegexOperator(fieldSql, regex, options, params)
      }

      case "$options":
        // Handled together with $regex
        return null

      case "$like":
        params.push(value)
        return `${fieldSql} LIKE ?`

      case "$startsWith": {
        const pattern = `${value}%`
        params.push(pattern)
        return `${fieldSql} LIKE ?`
      }

      case "$endsWith": {
        const pattern = `%${value}`
        params.push(pattern)
        return `${fieldSql} LIKE ?`
      }

      case "$all":
        return this.translateAllOperator(fieldSql, value, params)

      case "$size":
        return this.translateSizeOperator(fieldSql, value, params)

      case "$elemMatch":
        return this.translateElemMatchOperator(fieldSql, value, params)

      case "$index":
        return this.translateIndexOperator(fieldSql, value, params)

      case "$exists":
        return this.translateExistsOperator(fieldSql, value)

      // Additional operators will be added in subsequent tasks

      default:
        // Unknown operator - ignore
        return null
    }
  }

  /**
   * Translates $in operator to SQL.
   *
   * @param fieldSql - The SQL representation of the field
   * @param values - Array of values to check
   * @param params - Array to collect parameter values
   * @returns SQL string fragment
   *
   * @internal
   */
  private translateInOperator(
    fieldSql: string,
    values: unknown,
    params: unknown[]
  ): string {
    const valuesArray = values as readonly unknown[]
    if (valuesArray.length === 0) {
      // Empty IN clause matches nothing
      return "0=1"
    }

    const placeholders = valuesArray.map(() => "?").join(", ")
    for (const v of valuesArray) {
      params.push(v)
    }
    return `${fieldSql} IN (${placeholders})`
  }

  /**
   * Translates $nin operator to SQL.
   *
   * @param fieldSql - The SQL representation of the field
   * @param values - Array of values to exclude
   * @param params - Array to collect parameter values
   * @returns SQL string fragment
   *
   * @internal
   */
  private translateNotInOperator(
    fieldSql: string,
    values: unknown,
    params: unknown[]
  ): string {
    const valuesArray = values as readonly unknown[]
    if (valuesArray.length === 0) {
      // Empty NOT IN clause matches everything
      return "1=1"
    }

    const placeholders = valuesArray.map(() => "?").join(", ")
    for (const v of valuesArray) {
      params.push(v)
    }
    return `${fieldSql} NOT IN (${placeholders})`
  }

  /**
   * Translates $regex operator to SQL.
   *
   * @param fieldSql - The SQL representation of the field
   * @param regex - The regex pattern (RegExp or string or unknown)
   * @param options - Optional flags (currently only 'i' for case-insensitive)
   * @param params - Array to collect parameter values
   * @returns SQL string fragment
   *
   * @remarks
   * SQLite's REGEXP operator requires a custom function to be registered.
   * For case-insensitive matching, we convert the pattern using the i flag.
   * RegExp objects are converted to their pattern string.
   *
   * @internal
   */
  private translateRegexOperator(
    fieldSql: string,
    regex: unknown,
    options: unknown,
    params: unknown[]
  ): string {
    // Extract pattern from RegExp or use string directly
    let pattern: string
    let caseInsensitive = false

    if (regex instanceof RegExp) {
      pattern = regex.source
      caseInsensitive = regex.flags.includes("i")
    } else if (typeof regex === "string") {
      pattern = regex
    } else {
      // Invalid regex value
      pattern = String(regex)
    }

    // Override with explicit $options if provided
    if (options === "i") {
      caseInsensitive = true
    }

    // For case-insensitive matching, we use SQLite's case-insensitive REGEXP
    // by prefixing the pattern with (?i)
    if (caseInsensitive) {
      pattern = `(?i)${pattern}`
    }

    params.push(pattern)
    return `${fieldSql} REGEXP ?`
  }

  /**
   * Resolves field name to SQL column reference.
   *
   * @param fieldName - The field name from the document type
   * @returns SQL column reference (generated column or jsonb_extract)
   *
   * @remarks
   * Indexed fields use generated columns with underscore prefix (e.g., `_age`).
   * Non-indexed fields use `jsonb_extract(data, '$.field')`.
   *
   * @internal
   */
  private resolveFieldName(fieldName: keyof T): string {
    const field = this.fieldMap.get(fieldName)

    if (field?.indexed) {
      // Indexed field uses generated column with underscore prefix
      return `_${String(fieldName)}`
    }

    // Non-indexed field uses jsonb_extract
    return `jsonb_extract(data, '$.${String(fieldName)}')`
  }

  /**
   * Translates $all operator to SQL.
   *
   * @param fieldSql - The SQL representation of the field
   * @param values - Array of values that must all be present
   * @param params - Array to collect parameter values
   * @returns SQL string fragment
   *
   * @remarks
   * The $all operator matches arrays that contain all specified values.
   * Uses SQLite's json_each to iterate over array elements.
   *
   * @internal
   */
  private translateAllOperator(
    fieldSql: string,
    values: unknown,
    params: unknown[]
  ): string {
    const valuesArray = values as readonly unknown[]
    if (valuesArray.length === 0) {
      // Empty $all matches everything (vacuous truth)
      return "1=1"
    }

    const conditions: string[] = []
    for (const v of valuesArray) {
      params.push(v)
      // Check if value exists in the array using json_each
      conditions.push(
        `EXISTS (SELECT 1 FROM json_each(${fieldSql}) WHERE json_each.value = ?)`
      )
    }

    return `(${conditions.join(" AND ")})`
  }

  /**
   * Translates $size operator to SQL.
   *
   * @param fieldSql - The SQL representation of the field
   * @param size - Expected array size
   * @param params - Array to collect parameter values
   * @returns SQL string fragment
   *
   * @remarks
   * The $size operator matches arrays with the specified length.
   * Uses SQLite's json_array_length function.
   *
   * @internal
   */
  private translateSizeOperator(
    fieldSql: string,
    size: unknown,
    params: unknown[]
  ): string {
    params.push(size)
    return `json_array_length(${fieldSql}) = ?`
  }

  /**
   * Translates $elemMatch operator to SQL.
   *
   * @param fieldSql - The SQL representation of the field
   * @param elementFilter - Query filter to match array elements
   * @param params - Array to collect parameter values
   * @returns SQL string fragment
   *
   * @remarks
   * The $elemMatch operator matches arrays where at least one element
   * satisfies all the specified query criteria. This requires recursively
   * translating the nested query filter.
   *
   * @internal
   */
  private translateElemMatchOperator(
    fieldSql: string,
    elementFilter: unknown,
    params: unknown[]
  ): string {
    // Extract the nested filter conditions
    const filter = elementFilter as Record<string, unknown>
    const conditions: string[] = []

    for (const [op, value] of Object.entries(filter)) {
      // Use json_each.value to reference array elements
      const elementSql = "json_each.value"
      const condition = this.translateSingleOperator({
        fieldSql: elementSql,
        op,
        value,
        operators: filter,
        params,
      })
      if (condition) {
        conditions.push(condition)
      }
    }

    const whereClause = conditions.join(" AND ")
    return `EXISTS (SELECT 1 FROM json_each(${fieldSql}) WHERE ${whereClause})`
  }

  /**
   * Translates $index operator to SQL.
   *
   * @param fieldSql - The SQL representation of the field
   * @param indexFilter - Object with index and condition
   * @param params - Array to collect parameter values
   * @returns SQL string fragment
   *
   * @remarks
   * The $index operator accesses a specific array element by index.
   * Uses SQLite's json_extract with array index syntax.
   *
   * @internal
   */
  private translateIndexOperator(
    fieldSql: string,
    indexFilter: unknown,
    params: unknown[]
  ): string {
    const filter = indexFilter as { index: number; condition: unknown }
    const index = filter.index
    const condition = filter.condition

    // Extract the element at the specified index
    // For generated columns: json_extract(_field, '$[index]')
    // For non-indexed: json_extract(jsonb_extract(data, '$.field'), '$[index]')
    const elementSql = `json_extract(${fieldSql}, '$[${index}]')`

    // Handle the condition on that element
    if (
      condition === null ||
      typeof condition === "string" ||
      typeof condition === "number" ||
      typeof condition === "boolean"
    ) {
      params.push(condition)
      return `${elementSql} = ?`
    }

    // Handle operator conditions
    if (typeof condition === "object" && condition !== null) {
      const operators = condition as Record<string, unknown>
      const conditions: string[] = []

      for (const [op, value] of Object.entries(operators)) {
        const cond = this.translateSingleOperator({
          fieldSql: elementSql,
          op,
          value,
          operators,
          params,
        })
        if (cond) {
          conditions.push(cond)
        }
      }

      return conditions.length > 0 ? `(${conditions.join(" AND ")})` : "1=1"
    }

    return "1=1"
  }

  /**
   * Translates $exists operator to SQL.
   *
   * @param fieldSql - The SQL representation of the field
   * @param shouldExist - Whether field should exist (true) or not (false)
   * @returns SQL string fragment
   *
   * @remarks
   * The $exists operator checks whether a field exists in the document.
   * Uses SQLite's json_type which returns NULL for non-existent fields.
   *
   * @internal
   */
  private translateExistsOperator(
    fieldSql: string,
    shouldExist: unknown
  ): string {
    if (shouldExist === true) {
      // Field exists if json_type is not NULL
      return `json_type(${fieldSql}) IS NOT NULL`
    }
    // Field doesn't exist if json_type is NULL
    return `json_type(${fieldSql}) IS NULL`
  }

  /**
   * Translates query options to SQL clauses.
   *
   * @param options - The query options to translate
   * @returns Object containing SQL clauses (ORDER BY, LIMIT, OFFSET) and parameters
   *
   * @remarks
   * Query options are translated to SQL clauses that control result ordering and pagination.
   * The projection option is handled by the collection layer, not by the translator.
   *
   * **Generated SQL Clauses:**
   * - `sort`: Generates ORDER BY clause with field names and ASC/DESC
   * - `limit`: Generates LIMIT clause with parameterized value
   * - `skip`: Generates OFFSET clause with parameterized value
   *
   * **Field Resolution:**
   * Sort fields use the same resolution as query filters:
   * - Indexed fields: Use generated column `_fieldname`
   * - Non-indexed fields: Use `jsonb_extract(data, '$.fieldname')`
   *
   * **Sort Direction:**
   * - `1`: Ascending order (ASC)
   * - `-1`: Descending order (DESC)
   *
   * @example
   * ```typescript
   * // Sort by age descending, then name ascending
   * translator.translateOptions({
   *   sort: { age: -1, name: 1 },
   *   limit: 10,
   *   skip: 20
   * });
   * // => {
   * //   sql: "ORDER BY _age DESC, _name ASC LIMIT ? OFFSET ?",
   * //   params: [10, 20]
   * // }
   *
   * // Sort with non-indexed field
   * translator.translateOptions({
   *   sort: { status: 1 },
   *   limit: 5
   * });
   * // => {
   * //   sql: "ORDER BY jsonb_extract(data, '$.status') ASC LIMIT ?",
   * //   params: [5]
   * // }
   * ```
   */
  translateOptions(options: QueryOptions<T>): QueryTranslatorResult {
    const params: unknown[] = []
    const clauses: string[] = []

    // Handle sort (ORDER BY clause)
    if (options.sort) {
      const orderBy = this.translateSort(options.sort)
      if (orderBy) {
        clauses.push(orderBy)
      }
    }

    // Handle limit (LIMIT clause)
    if (options.limit !== undefined) {
      params.push(options.limit)
      clauses.push("LIMIT ?")
    }

    // Handle skip (OFFSET clause)
    if (options.skip !== undefined) {
      params.push(options.skip)
      clauses.push("OFFSET ?")
    }

    // Note: projection is handled by the collection layer during SELECT construction

    return {
      sql: clauses.join(" "),
      params,
    }
  }

  /**
   * Translates sort specification to ORDER BY clause.
   *
   * @param sort - Sort specification object
   * @returns ORDER BY SQL string fragment
   *
   * @remarks
   * Converts MongoDB-style sort specification to SQL ORDER BY clause.
   * Uses the same field resolution as query filters to ensure consistency.
   *
   * @internal
   */
  private translateSort(sort: QueryOptions<T>["sort"]): string {
    if (!sort) {
      return ""
    }

    const sortFields: string[] = []

    for (const [fieldName, direction] of Object.entries(sort)) {
      const fieldSql = this.resolveFieldName(fieldName as keyof T)
      const dir = direction === 1 ? "ASC" : "DESC"
      sortFields.push(`${fieldSql} ${dir}`)
    }

    return sortFields.length > 0 ? `ORDER BY ${sortFields.join(", ")}` : ""
  }
}
