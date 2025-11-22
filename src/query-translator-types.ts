import type { SQLQueryBindings } from "bun:sqlite"
import type { Document } from "./core-types.js"
import type { QueryOptions } from "./query-options-types.js"
import type { QueryFilter } from "./query-types.js"

/**
 * Valid SQLite bind parameter types.
 *
 * @remarks
 * Re-exported from `bun:sqlite` for consistency with Bun's SQLite bindings.
 * This ensures proper type compatibility when spreading query parameters.
 *
 * @see {@link https://bun.sh/docs/api/sqlite#query-parameters}
 */
export type SQLiteBindValue = SQLQueryBindings

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
 * **Debugging with toString():**
 * The result includes a `toString()` method for easy debugging and introspection.
 * This is useful for troubleshooting queries and reporting issues.
 *
 * @example
 * ```typescript
 * // Example translation result
 * const result: QueryTranslatorResult = {
 *   sql: "age > ? AND status = ?",
 *   params: [18, 'active'],
 *   toString: () => "..."
 * };
 *
 * // Used with database query
 * const query = `SELECT * FROM users WHERE ${result.sql}`;
 * const users = db.prepare(query).all(...result.params);
 *
 * // Debug output
 * console.log(result.toString());
 * // SQL: age > ? AND status = ?
 * // Parameters: [18, "active"]
 * ```
 */
export type QueryTranslatorResult = {
  /** SQL WHERE clause with parameter placeholders (e.g., ?, $1, $2) */
  readonly sql: string

  /** Array of parameter values to bind to the SQL placeholders */
  readonly params: SQLiteBindValue[]

  /**
   * Returns a formatted string representation of the query for debugging.
   *
   * @remarks
   * Useful for logging, troubleshooting, and GitHub issue reports.
   * Shows both the SQL and the parameter values.
   *
   * @example
   * ```typescript
   * const result = translator.translate({ age: { $gte: 21 } });
   * console.log(result.toString());
   * // SQL: jsonb_extract(body, '$.age') >= ?
   * // Parameters: [21]
   * ```
   */
  toString(): string
}

/**
 * Creates a QueryTranslatorResult with toString() for debugging.
 *
 * @param sql - The SQL string with placeholders
 * @param params - The parameter values to bind
 * @returns QueryTranslatorResult with toString() method
 *
 * @example
 * ```typescript
 * const result = createQueryResult("age > ?", [18]);
 * console.log(result.toString());
 * // SQL: age > ?
 * // Parameters: [18]
 * ```
 */
export function createQueryResult(
  sql: string,
  params: SQLiteBindValue[]
): QueryTranslatorResult {
  return {
    sql,
    params,
    toString(): string {
      const paramStr = JSON.stringify(params)
      return `SQL: ${sql}\nParameters: ${paramStr}`
    },
  }
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
 * - Support all query operators ($eq, $gt, $like, $startsWith, $endsWith, etc.)
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
 *         { name: { $startsWith: 'admin' } },
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
 * // [18, 65, 'active', 'admin%', '%@company.com']
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
   * translator.translate({ email: { $endsWith: '@example.com' } });
   * // => { sql: "email LIKE ?", params: ['%@example.com'] }
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
