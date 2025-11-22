/**
 * Timestamp management utilities for StrataDB documents.
 *
 * @module timestamps
 *
 * @remarks
 * StrataDB automatically manages `createdAt` and `updatedAt` timestamps on all documents.
 * Timestamps are stored as Unix timestamps (milliseconds since epoch) for:
 * - Efficient storage (8 bytes vs 24+ for Date objects)
 * - JSON serialization compatibility
 * - Immutability (numbers vs mutable Date objects)
 * - Cross-platform consistency
 * - Easy comparison and sorting
 *
 * **Automatic Timestamp Behavior:**
 * - `insertOne()` and `insertMany()`: Add both `createdAt` and `updatedAt`
 * - `updateOne()` and `updateMany()`: Update `updatedAt` only
 * - `replaceOne()`: Preserve `createdAt`, update `updatedAt`
 * - Timestamps are always managed by StrataDB (not user-provided)
 *
 * @example
 * ```typescript
 * import { Strata, nowTimestamp, timestampToDate } from 'stratadb';
 *
 * const db = new Strata({ database: ':memory:' });
 * const users = db.collection('users', schema);
 *
 * // Insert - automatically adds timestamps
 * await users.insertOne({ name: 'Alice', email: 'alice@example.com' });
 * // Document now has: { id, name, email, createdAt: 1700000000000, updatedAt: 1700000000000 }
 *
 * // Update - automatically updates updatedAt
 * await users.updateOne('user-id', { email: 'newemail@example.com' });
 * // Document now has: { ..., createdAt: 1700000000000, updatedAt: 1700000123000 }
 *
 * // Working with timestamps
 * const user = await users.findById('user-id');
 * if (user) {
 *   const created = timestampToDate(user.createdAt);
 *   console.log(`User created: ${created.toISOString()}`);
 * }
 * ```
 */

/**
 * Get the current timestamp in milliseconds since Unix epoch.
 *
 * @returns Current timestamp as number (Unix time in milliseconds)
 *
 * @remarks
 * This is the standard timestamp format used throughout StrataDB.
 * Equivalent to `Date.now()` but provided as a named function for clarity.
 *
 * **Why milliseconds?**
 * - JavaScript Date uses milliseconds as standard
 * - Provides sufficient precision for most applications
 * - Smaller storage footprint than nanoseconds
 * - Compatible with SQLite INTEGER type
 *
 * @example
 * ```typescript
 * const now = nowTimestamp();
 * console.log(now); // 1700000000000
 *
 * // Can be used for manual timestamp fields
 * await users.insertOne({
 *   name: 'Bob',
 *   lastLoginAt: nowTimestamp()
 * });
 * ```
 */
export function nowTimestamp(): number {
  return Date.now()
}

/**
 * Convert a StrataDB timestamp to a JavaScript Date object.
 *
 * @param timestamp - Unix timestamp in milliseconds
 * @returns Date object representing the timestamp
 *
 * @remarks
 * Use this when you need to work with JavaScript Date APIs for formatting,
 * timezone conversion, or date arithmetic.
 *
 * **Note:** Date objects are mutable and timezone-dependent. For storage
 * and comparison, prefer working with raw timestamps.
 *
 * @example
 * ```typescript
 * const user = await users.findById('user-id');
 * if (user) {
 *   const createdDate = timestampToDate(user.createdAt);
 *
 *   // Format for display
 *   console.log(createdDate.toLocaleDateString());
 *   console.log(createdDate.toISOString());
 *
 *   // Date arithmetic
 *   const daysSinceCreation = Math.floor(
 *     (Date.now() - user.createdAt) / (1000 * 60 * 60 * 24)
 *   );
 * }
 * ```
 */
export function timestampToDate(timestamp: number): Date {
  return new Date(timestamp)
}

/**
 * Convert a JavaScript Date object to a StrataDB timestamp.
 *
 * @param date - Date object to convert
 * @returns Unix timestamp in milliseconds
 *
 * @remarks
 * Use this when you have a Date object (from user input, external API, etc.)
 * that needs to be stored as a timestamp in StrataDB.
 *
 * @example
 * ```typescript
 * // Store a specific date
 * const birthDate = new Date('1990-01-01');
 * await users.insertOne({
 *   name: 'Charlie',
 *   birthDate: dateToTimestamp(birthDate)
 * });
 *
 * // Store user's selected date from form
 * const selectedDate = new Date(formInput.date);
 * await events.insertOne({
 *   title: 'Conference',
 *   eventDate: dateToTimestamp(selectedDate)
 * });
 * ```
 */
export function dateToTimestamp(date: Date): number {
  return date.getTime()
}

/**
 * Check if a timestamp is within a given time range.
 *
 * @param timestamp - The timestamp to check
 * @param start - Start of the range (inclusive)
 * @param end - End of the range (inclusive)
 * @returns True if timestamp is within range
 *
 * @remarks
 * Useful for filtering documents by time ranges without converting to Date objects.
 * All comparisons are done with numbers for maximum performance.
 *
 * @example
 * ```typescript
 * // Find users created in the last 30 days
 * const thirtyDaysAgo = nowTimestamp() - (30 * 24 * 60 * 60 * 1000);
 * const now = nowTimestamp();
 *
 * const recentUsers = await users.find({
 *   // Using query operators (preferred)
 *   createdAt: { $gte: thirtyDaysAgo, $lte: now }
 * });
 *
 * // Or filter in memory if needed
 * const allUsers = await users.find({});
 * const filtered = allUsers.filter(u =>
 *   isTimestampInRange(u.createdAt, thirtyDaysAgo, now)
 * );
 * ```
 */
export function isTimestampInRange(
  timestamp: number,
  start: number,
  end: number
): boolean {
  return timestamp >= start && timestamp <= end
}

/**
 * Calculate the difference between two timestamps in milliseconds.
 *
 * @param timestamp1 - First timestamp
 * @param timestamp2 - Second timestamp
 * @returns Absolute difference in milliseconds
 *
 * @remarks
 * Returns the absolute difference, so order doesn't matter.
 * Useful for calculating durations, age, time since events, etc.
 *
 * @example
 * ```typescript
 * const user = await users.findById('user-id');
 * if (user) {
 *   // How long ago was the user created?
 *   const ageMs = timestampDiff(nowTimestamp(), user.createdAt);
 *   const ageDays = Math.floor(ageMs / (1000 * 60 * 60 * 24));
 *   console.log(`Account is ${ageDays} days old`);
 *
 *   // Time between creation and last update
 *   const timeSinceUpdate = timestampDiff(user.createdAt, user.updatedAt);
 *   const hoursSinceUpdate = Math.floor(timeSinceUpdate / (1000 * 60 * 60));
 * }
 * ```
 */
export function timestampDiff(timestamp1: number, timestamp2: number): number {
  return Math.abs(timestamp1 - timestamp2)
}

/**
 * Type guard to check if a value is a valid timestamp.
 *
 * @param value - Value to check
 * @returns True if value is a valid timestamp number
 *
 * @remarks
 * A valid timestamp is a positive number that represents a reasonable date.
 * This function checks if the value is a number and falls within a plausible
 * range (after 1970 and before year 3000).
 *
 * @example
 * ```typescript
 * const input: unknown = getUserInput();
 *
 * if (isValidTimestamp(input)) {
 *   // TypeScript now knows input is a number
 *   const date = timestampToDate(input);
 *   console.log(date.toISOString());
 * } else {
 *   throw new Error('Invalid timestamp provided');
 * }
 * ```
 */
export function isValidTimestamp(value: unknown): value is number {
  if (typeof value !== "number") {
    return false
  }

  // Check if number is finite and positive
  if (!Number.isFinite(value) || value < 0) {
    return false
  }

  // Check if timestamp is in a reasonable range
  // After 1970-01-01 and before 3000-01-01
  const minTimestamp = 0
  const maxTimestamp = 32_503_680_000_000 // 3000-01-01

  return value >= minTimestamp && value <= maxTimestamp
}
