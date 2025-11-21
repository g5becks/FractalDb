import { deepmergeCustom } from "deepmerge-ts"

/**
 * Custom deep merge function optimized for document updates.
 *
 * @remarks
 * This merge function is specifically designed for merging partial document updates
 * with existing documents in StrataDB collections. It provides the following behavior:
 *
 * **Array Handling:**
 * - Arrays are replaced, not concatenated
 * - `update: { tags: ['new'] }` completely replaces existing tags array
 *
 * **Undefined Values:**
 * - Preserved to enable field deletion
 * - `update: { field: undefined }` will set field to undefined
 *
 * **Nested Objects:**
 * - Deep merged recursively
 * - `update: { profile: { age: 30 } }` merges into existing profile object
 *
 * **Prototype Pollution Prevention:**
 * - Automatically protected by deepmerge-ts
 * - __proto__, constructor, and prototype keys are handled safely
 *
 * @example
 * ```typescript
 * const existing = {
 *   name: 'Alice',
 *   profile: { age: 25, city: 'NYC' },
 *   tags: ['user', 'active']
 * };
 *
 * const update = {
 *   profile: { age: 26 },  // Merge into profile
 *   tags: ['admin'],       // Replace tags array
 *   email: 'alice@example.com'  // Add new field
 * };
 *
 * const result = deepMerge(existing, update);
 * // {
 * //   name: 'Alice',
 * //   profile: { age: 26, city: 'NYC' },  // age updated, city preserved
 * //   tags: ['admin'],                      // array replaced
 * //   email: 'alice@example.com'          // field added
 * // }
 * ```
 *
 * @example
 * ```typescript
 * // Field deletion with undefined
 * const existing = {
 *   name: 'Bob',
 *   email: 'bob@example.com',
 *   phone: '555-1234'
 * };
 *
 * const update = {
 *   phone: undefined  // Delete phone field
 * };
 *
 * const result = deepMerge(existing, update);
 * // {
 * //   name: 'Bob',
 * //   email: 'bob@example.com',
 * //   phone: undefined  // Preserved for deletion
 * // }
 * ```
 */
export const deepMerge = deepmergeCustom({
  /**
   * Replace arrays instead of concatenating them.
   * This ensures array updates completely replace the old array.
   */
  mergeArrays: false,

  /**
   * Keep undefined values to allow field deletion.
   * By default, deepmerge-ts filters out undefined.
   */
  filterValues: false,
})

/**
 * Type-safe deep merge function for document updates.
 *
 * @typeParam T - The target document type
 * @typeParam U - The update object type (typically Partial<T>)
 *
 * @param target - The existing document to merge into
 * @param update - The partial update to merge
 * @returns A new object with merged values (inputs are not mutated)
 *
 * @remarks
 * This is a type-safe wrapper around the custom deepmerge function.
 * It ensures that the return type correctly reflects the merge operation.
 *
 * **Type Safety:**
 * - Target and update types are preserved
 * - Return type is inferred as T & U
 * - No type assertions needed in calling code
 *
 * **Immutability:**
 * - Neither target nor update is mutated
 * - Returns a new object with merged values
 * - Safe to use in concurrent operations
 *
 * @example
 * ```typescript
 * type User = {
 *   name: string;
 *   profile: {
 *     age: number;
 *     city: string;
 *   };
 *   tags: string[];
 * };
 *
 * const user: User = {
 *   name: 'Alice',
 *   profile: { age: 25, city: 'NYC' },
 *   tags: ['user']
 * };
 *
 * const update: Partial<User> = {
 *   profile: { age: 26 }
 * };
 *
 * const merged = deepMergeTyped(user, update);
 * // Type: User & Partial<User>
 * // Value: { name: 'Alice', profile: { age: 26, city: 'NYC' }, tags: ['user'] }
 * ```
 */
export function deepMergeTyped<T extends object, U extends object>(
  target: T,
  update: U
): T & U {
  return deepMerge(target, update) as T & U
}
