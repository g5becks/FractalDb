import type { Document } from "./core-types.js"
import type { QueryOptions } from "./query-options-types.js"
import type { QueryFilter } from "./query-types.js"

/**
 * Valid SQLite bind parameter types.
 *
 * @remarks
 * SQLite supports a limited set of types for parameterized query values.
 * This type ensures type safety when building parameterized queries.
 *
 * **Supported Types:**
 * - `string` - Text values
 * - `number` - Numeric values (integers and floats)
 * - `boolean` - Boolean values (converted to 0/1)
 * - `null` - NULL values
 * - `bigint` - Large integers
 * - `Uint8Array` - Binary data (BLOB)
 *
 * @example
 * ```typescript
 * const params: SQLiteBindValue[] = [
 *   'Alice',           // string
 *   42,                // number
 *   true,              // boolean
 *   null,              // null
 *   BigInt(123456789), // bigint
 *   new Uint8Array([1, 2, 3]) // Uint8Array
 * ];
 * ```
 */
export type SQLiteBindValue =
  | string
  | number
  | boolean
  | null
  | bigint
  | Uint8Array

/**
 * Result of translating a StrataDB query to SQL.
 *
 * @remarks
 * Query translation produces parameterized SQL to prevent SQL injection attacks.
 * The `sql` string contains placeholders (e.g., `?` or `$1`) that are safely bound
 * to the `params` array by the database driver.
 *
 * **Why Parameterized Queries?**
 * - Prevents SQL injection by separating SQL structure from user data
 * - Allows database to cache query plans for better performance
 * - Ensures proper escaping and type handling by the database driver
 *
 * @example
 * ```typescript
 * // Example translation result
 * const result: QueryTranslatorResult = {
 *   sql: "age > ? AND status = ?",
 *   params: [18, 'active']
 * };
 *
 * // Used with database query
 * const query = `SELECT * FROM users WHERE ${result.sql}`;
 * const users = db.prepare(query).all(...result.params);
 * ```
 */
export type QueryTranslatorResult = {
  /** SQL WHERE clause with parameter placeholders (e.g., ?, $1, $2) */
  readonly sql: string

  /** Array of parameter values to bind to the SQL placeholders */
  readonly params: readonly unknown[]
}

/**
 * Interface for translating StrataDB queries to SQL.
 *
 * @typeParam T - The document type being queried
 *
 * @remarks
 * Query translators convert type-safe StrataDB query filters into parameterized SQL
 * WHERE clauses. This abstraction allows different SQL generation strategies (e.g.,
 * SQLite, PostgreSQL, MySQL) while ensuring all implementations produce safe,
 * parameterized queries that prevent SQL injection.
 *
 * **Key Responsibilities:**
 * - Convert QueryFilter<T> to SQL WHERE clause syntax
 * - Generate parameter placeholders in the SQL string
 * - Extract parameter values into the params array
 * - Handle nested queries and logical operators
 * - Support all query operators ($eq, $gt, $regex, etc.)
 *
 * **SQL Injection Prevention:**
 * All translators must produce parameterized queries. User-provided values are never
 * directly concatenated into SQL strings. Instead, they are passed separately in the
 * params array and safely bound by the database driver.
 *
 * **Implementation Strategy:**
 * Translators typically work recursively:
 * 1. Start with root QueryFilter
 * 2. For each field condition, generate SQL fragment and collect params
 * 3. For logical operators ($and, $or, $nor, $not), recursively translate nested filters
 * 4. Combine all fragments with appropriate SQL operators (AND, OR, NOT)
 * 5. Return complete SQL string and params array
 *
 * @example
 * ```typescript
 * import { QueryTranslator, type Document } from 'stratadb';
 *
 * type User = Document<{
 *   name: string;
 *   age: number;
 *   email: string;
 *   status: 'active' | 'inactive';
 * }>;
 *
 * // Example translator implementation (simplified)
 * class SQLiteQueryTranslator implements QueryTranslator<User> {
 *   translate(filter: QueryFilter<User>): QueryTranslatorResult {
 *     const params: unknown[] = [];
 *
 *     // Simple example: translate age filter
 *     if ('age' in filter && typeof filter.age === 'object') {
 *       if ('$gt' in filter.age) {
 *         params.push(filter.age.$gt);
 *         return {
 *           sql: 'age > ?',
 *           params
 *         };
 *       }
 *     }
 *
 *     return { sql: '1=1', params: [] };
 *   }
 * }
 *
 * // Usage with collection
 * const translator = new SQLiteQueryTranslator();
 * const filter: QueryFilter<User> = { age: { $gt: 18 } };
 * const result = translator.translate(filter);
 *
 * console.log(result.sql);    // "age > ?"
 * console.log(result.params); // [18]
 *
 * // ✅ Safe parameterized query
 * const users = db.prepare(`SELECT * FROM users WHERE ${result.sql}`)
 *                 .all(...result.params);
 *
 * // ❌ NEVER do this (SQL injection vulnerable)
 * // const users = db.prepare(`SELECT * FROM users WHERE age > ${userInput}`)
 * //                 .all();
 * ```
 *
 * @example
 * ```typescript
 * // Complex query example
 * const filter: QueryFilter<User> = {
 *   $and: [
 *     { age: { $gte: 18, $lte: 65 } },
 *     { status: 'active' },
 *     {
 *       $or: [
 *         { name: { $regex: /^admin/i } },
 *         { email: { $like: '%@company.com' } }
 *       ]
 *     }
 *   ]
 * };
 *
 * const result = translator.translate(filter);
 *
 * // Generated SQL (simplified):
 * // "(age >= ? AND age <= ?) AND status = ? AND (name LIKE ? OR email LIKE ?)"
 *
 * // Generated params:
 * // [18, 65, 'active', '%admin%', '%@company.com']
 * ```
 */
export type QueryTranslator<T extends Document> = {
  /**
   * Translates a StrataDB query filter to parameterized SQL.
   *
   * @param filter - The query filter to translate
   * @returns Object containing SQL WHERE clause and parameter values
   *
   * @remarks
   * The translate method must produce safe, parameterized SQL that prevents SQL injection.
   * All user-provided values must be extracted into the params array, not concatenated
   * into the SQL string.
   *
   * **Parameter Placeholders:**
   * Different databases use different placeholder syntax:
   * - SQLite: `?` for positional parameters
   * - PostgreSQL: `$1`, `$2`, `$3` for numbered parameters
   * - MySQL: `?` for positional parameters
   *
   * **Empty Filter:**
   * If the filter is empty (matches all documents), implementations typically return:
   * - `{ sql: "1=1", params: [] }` or
   * - `{ sql: "TRUE", params: [] }`
   *
   * @example
   * ```typescript
   * // Simple field equality
   * translator.translate({ name: 'Alice' });
   * // => { sql: "name = ?", params: ['Alice'] }
   *
   * // Comparison operators
   * translator.translate({ age: { $gte: 18, $lt: 65 } });
   * // => { sql: "(age >= ? AND age < ?)", params: [18, 65] }
   *
   * // String operators
   * translator.translate({ email: { $regex: /@example\\.com$/ } });
   * // => { sql: "email REGEXP ?", params: ['@example\\.com$'] }
   *
   * // Array operators
   * translator.translate({ tags: { $all: ['admin', 'verified'] } });
   * // => { sql: "(tags LIKE ? AND tags LIKE ?)", params: ['%admin%', '%verified%'] }
   *
   * // Logical operators
   * translator.translate({
   *   $and: [
   *     { status: 'active' },
   *     { age: { $gt: 18 } }
   *   ]
   * });
   * // => { sql: "(status = ? AND age > ?)", params: ['active', 18] }
   * ```
   */
  translate(filter: QueryFilter<T>): QueryTranslatorResult

  /**
   * Translates query options (sort, limit, skip, projection) to SQL.
   *
   * @param options - The query options to translate
   * @returns Object containing SQL clauses for ORDER BY, LIMIT, OFFSET
   *
   * @remarks
   * Query options are translated to SQL clauses that control result ordering,
   * pagination, and field selection. Unlike filter translation, options generally
   * don't require parameterization since they contain structural information
   * rather than user data.
   *
   * **Generated SQL Clauses:**
   * - `sort`: Generates ORDER BY clause
   * - `limit`: Generates LIMIT clause
   * - `skip`: Generates OFFSET clause
   * - `projection`: Affects SELECT clause (handled by collection implementation)
   *
   * @example
   * ```typescript
   * translator.translateOptions({
   *   sort: { createdAt: -1, name: 1 },
   *   limit: 20,
   *   skip: 40
   * });
   * // => {
   * //   sql: "ORDER BY createdAt DESC, name ASC LIMIT ? OFFSET ?",
   * //   params: [20, 40]
   * // }
   * ```
   */
  translateOptions(options: QueryOptions<T>): QueryTranslatorResult
}
