/**
 * Default ID generator for StrataDB documents.
 *
 * @returns A unique, time-sortable UUID v7 string
 *
 * @remarks
 * Uses Bun's `randomUUIDv7()` implementation which generates UUID version 7 identifiers.
 * UUID v7 provides several important properties for database primary keys:
 *
 * **Time-Sortable:**
 * - First 48 bits encode Unix timestamp in milliseconds
 * - Documents inserted later will have lexicographically greater IDs
 * - Enables efficient B-tree indexing and range queries
 *
 * **Globally Unique:**
 * - Remaining bits contain cryptographically random data
 * - Collision probability is astronomically low (2^-74 per millisecond)
 * - Safe for distributed systems without coordination
 *
 * **URL-Safe & Compact:**
 * - Standard UUID format: `018c3f7e-8b5d-7a3c-9f2e-1a4b5c6d7e8f`
 * - 36 characters including hyphens
 * - Safe for use in URLs, filenames, and APIs
 *
 * **Performance:**
 * - Stateless function with no memory allocation
 * - Safe for concurrent use across multiple threads/workers
 * - Faster than alternatives requiring coordination (e.g., database sequences)
 *
 * **Monotonicity:**
 * - IDs generated within the same millisecond are randomly ordered
 * - IDs from different milliseconds are strictly ordered by time
 * - Provides "best-effort" monotonicity for most use cases
 *
 * @example
 * ```typescript
 * const id1 = generateId();
 * const id2 = generateId();
 *
 * // IDs are unique
 * console.log(id1 !== id2); // true
 *
 * // IDs from different times are sortable
 * await new Promise(resolve => setTimeout(resolve, 2));
 * const id3 = generateId();
 * console.log(id1 < id3); // true (lexicographic comparison)
 *
 * // Format: standard UUID v7
 * console.log(id1); // "018c3f7e-8b5d-7a3c-9f2e-1a4b5c6d7e8f"
 * ```
 *
 * @example
 * ```typescript
 * // Using with custom ID generator
 * import { StrataDB } from 'stratadb';
 *
 * const db = new StrataDB(':memory:', {
 *   idGenerator: () => `custom-${Date.now()}-${Math.random()}`
 * });
 *
 * // Or use the default
 * const db2 = new StrataDB(':memory:'); // Uses generateId internally
 * ```
 *
 * @see {@link https://www.rfc-editor.org/rfc/rfc9562.html#section-5.7 | RFC 9562 - UUID Version 7}
 * @see {@link https://bun.sh/docs/api/utils#bun-randomuuidv7 | Bun.randomUUIDv7()}
 */
export function generateId(): string {
  return Bun.randomUUIDv7()
}

/**
 * Type definition for custom ID generator functions.
 *
 * @remarks
 * Custom ID generators must return unique string identifiers.
 * While not required, time-sortable IDs are recommended for optimal
 * database performance.
 *
 * **Requirements:**
 * - Must return a unique string for each invocation
 * - Should be stateless (no mutable state)
 * - Must be safe for concurrent use
 * - Strings should be reasonably compact (< 100 chars recommended)
 *
 * **Recommendations:**
 * - Include timestamp component for sortability
 * - Include random component for uniqueness
 * - Avoid special characters that require URL encoding
 * - Consider using UUID v7, ULID, or similar standards
 *
 * @example
 * ```typescript
 * // Simple timestamp-based ID
 * const customGenerator: IdGenerator = () => {
 *   return `${Date.now()}-${Math.random().toString(36).slice(2)}`;
 * };
 *
 * // ULID-style (would need ulid library)
 * const ulidGenerator: IdGenerator = () => {
 *   return ulid(); // Lexicographically sortable, 26 chars
 * };
 *
 * // Sequential numeric (NOT recommended for distributed systems)
 * let counter = 0;
 * const sequentialGenerator: IdGenerator = () => {
 *   return String(++counter); // NOT thread-safe!
 * };
 * ```
 */
export type IdGenerator = () => string
