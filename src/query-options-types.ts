/**
 * Sort specification for query result ordering.
 *
 * @typeParam T - The document type being queried
 *
 * @remarks
 * Defines sort order for query results using MongoDB-style sort syntax.
 * Each field can be sorted in ascending (1) or descending (-1) order.
 * Multiple fields can be specified for multi-level sorting.
 *
 * Sort order matters: fields are sorted left-to-right as specified.
 *
 * @example
 * ```typescript
 * type User = Document<{
 *   name: string;
 *   age: number;
 *   createdAt: Date;
 *   status: 'active' | 'inactive';
 * }>;
 *
 * // ✅ Single field sort
 * const byAge: SortSpec<User> = {
 *   age: 1  // Ascending order
 * };
 *
 * // ✅ Descending order
 * const byNewest: SortSpec<User> = {
 *   createdAt: -1  // Most recent first
 * };
 *
 * // ✅ Multi-field sort
 * const byStatusThenAge: SortSpec<User> = {
 *   status: 1,   // First by status (ascending)
 *   age: -1      // Then by age (descending)
 * };
 *
 * // ✅ Complex sort
 * const complexSort: SortSpec<User> = {
 *   status: 1,      // Active users first
 *   createdAt: -1,  // Then newest first
 *   name: 1         // Then alphabetically
 * };
 *
 * // Usage with collection
 * const results = await users.find(
 *   { status: 'active' },
 *   { sort: { createdAt: -1, name: 1 } }
 * );
 * ```
 */
export type SortSpec<T> = {
  readonly [K in keyof T]?: 1 | -1
}

/**
 * Projection specification for field selection in query results.
 *
 * @typeParam T - The document type being queried
 *
 * @remarks
 * Controls which fields are included in or excluded from query results.
 * Uses MongoDB-style projection syntax:
 * - `1` or `true`: Include the field
 * - `0` or `false`: Exclude the field
 *
 * **Important rules:**
 * - Cannot mix inclusion and exclusion (except for `id` field)
 * - Either specify fields to include OR fields to exclude
 * - The `id` field is always included unless explicitly excluded
 *
 * @example
 * ```typescript
 * type User = Document<{
 *   name: string;
 *   email: string;
 *   password: string;
 *   age: number;
 *   createdAt: Date;
 * }>;
 *
 * // ✅ Include specific fields (exclude all others)
 * const publicFields: ProjectionSpec<User> = {
 *   name: 1,
 *   email: 1,
 *   age: 1
 *   // password, createdAt excluded
 * };
 *
 * // ✅ Exclude specific fields (include all others)
 * const withoutPassword: ProjectionSpec<User> = {
 *   password: 0
 *   // name, email, age, createdAt included
 * };
 *
 * // ✅ Exclude multiple fields
 * const minimalUser: ProjectionSpec<User> = {
 *   password: 0,
 *   createdAt: 0
 * };
 *
 * // ✅ Exclude id field
 * const noId: ProjectionSpec<User> = {
 *   id: 0,
 *   name: 1,
 *   email: 1
 * };
 *
 * // Usage with collection
 * const users = await collection.find(
 *   { status: 'active' },
 *   { projection: { name: 1, email: 1 } }
 * );
 * ```
 */
export type ProjectionSpec<T> = {
  readonly [K in keyof T]?: 1 | 0
}

/**
 * Complete query options for controlling result set behavior.
 *
 * @typeParam T - The document type being queried
 *
 * @remarks
 * Combines all query result control options:
 * - `sort`: Order results by one or more fields
 * - `limit`: Maximum number of results to return
 * - `skip`: Number of results to skip (for pagination)
 * - `projection`: Which fields to include/exclude
 *
 * These options work together to enable:
 * - Pagination (limit + skip)
 * - Sorting (sort)
 * - Field selection (projection)
 * - Performance optimization (limit + projection)
 *
 * @example
 * ```typescript
 * type User = Document<{
 *   name: string;
 *   email: string;
 *   age: number;
 *   status: 'active' | 'inactive';
 *   createdAt: Date;
 * }>;
 *
 * // ✅ Pagination with sorting
 * const page2: QueryOptions<User> = {
 *   sort: { createdAt: -1 },  // Newest first
 *   limit: 20,                // 20 per page
 *   skip: 20                  // Skip first page
 * };
 *
 * // ✅ Sorted results with field selection
 * const publicUsers: QueryOptions<User> = {
 *   sort: { name: 1 },
 *   projection: {
 *     name: 1,
 *     email: 1
 *     // Excludes age, status, createdAt
 *   }
 * };
 *
 * // ✅ Top 10 most recent
 * const recent: QueryOptions<User> = {
 *   sort: { createdAt: -1 },
 *   limit: 10
 * };
 *
 * // ✅ Complex pagination pattern
 * function getPage(pageNum: number, pageSize: number): QueryOptions<User> {
 *   return {
 *     sort: { createdAt: -1, name: 1 },
 *     limit: pageSize,
 *     skip: (pageNum - 1) * pageSize,
 *     projection: { name: 1, email: 1, status: 1 }
 *   };
 * }
 *
 * // ✅ Performance optimization
 * const optimized: QueryOptions<User> = {
 *   limit: 100,              // Limit results
 *   projection: {            // Only needed fields
 *     name: 1,
 *     status: 1
 *   }
 * };
 *
 * // Usage with collection
 * const results = await users.find(
 *   { status: 'active' },
 *   {
 *     sort: { createdAt: -1 },
 *     limit: 20,
 *     skip: 0,
 *     projection: { name: 1, email: 1 }
 *   }
 * );
 *
 * // Pagination helper
 * async function fetchPage(page: number, perPage: number = 20) {
 *   return await users.find(
 *     {},
 *     {
 *       sort: { createdAt: -1 },
 *       limit: perPage,
 *       skip: (page - 1) * perPage
 *     }
 *   );
 * }
 * ```
 */
export type QueryOptions<T> = {
  /** Sort order for results (MongoDB-style: 1 = asc, -1 = desc) */
  readonly sort?: SortSpec<T>

  /** Maximum number of documents to return */
  readonly limit?: number

  /** Number of documents to skip (for pagination) */
  readonly skip?: number

  /** Which fields to include (1) or exclude (0) from results */
  readonly projection?: ProjectionSpec<T>
}
