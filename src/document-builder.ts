/**
 * Build a complete document from a partial document and metadata fields.
 *
 * @typeParam T - The complete document type including metadata fields
 *
 * @param partial - Partial document data (may or may not include metadata fields)
 * @param metadata - Required metadata fields (_id, createdAt, updatedAt)
 * @returns Complete document of type T
 *
 * @remarks
 * This function is for building NEW documents (not merging with existing).
 * It performs a shallow merge of the partial data with the metadata.
 *
 * Use this when:
 * - Inserting new documents (insertOne, insertMany)
 * - Creating documents via upsert
 * - Replacing documents (replaceOne)
 *
 * The single cast inside this function is necessary because TypeScript cannot
 * statically verify that spreading a Partial<T> and adding metadata produces
 * exactly T, even though at runtime it does.
 *
 * @example
 * ```typescript
 * type User = Document<{
 *   name: string;
 *   email: string;
 * }>;
 *
 * const partial: Partial<Omit<User, "_id" | "createdAt" | "updatedAt">> = {
 *   name: 'Alice',
 *   email: 'alice@example.com'
 * };
 *
 * const doc = buildCompleteDocument<User>(partial, {
 *   _id: 'user-123',
 *   createdAt: 1000,
 *   updatedAt: 1000
 * });
 *
 * // Type: User (no casting needed!)
 * // Value: { _id: 'user-123', name: 'Alice', email: 'alice@example.com', createdAt: 1000, updatedAt: 1000 }
 * ```
 */
export function buildCompleteDocument<T extends { _id: string }>(
  partial:
    | Partial<Omit<T, "_id" | "createdAt" | "updatedAt">>
    | Omit<T, "_id" | "createdAt" | "updatedAt">,
  metadata: { _id: string; createdAt: number; updatedAt: number }
): T {
  // TypeScript cannot statically prove that spreading partial + metadata produces T
  // But at runtime this is correct for our document structure
  return { ...partial, ...metadata } as unknown as T
}
