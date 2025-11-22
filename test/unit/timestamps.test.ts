import { describe, expect, it } from "bun:test"
import {
  dateToTimestamp,
  isTimestampInRange,
  isValidTimestamp,
  nowTimestamp,
  timestampDiff,
  timestampToDate,
} from "../../src/timestamps.js"

describe("Timestamp Utilities", () => {
  describe("nowTimestamp", () => {
    it("should return a number", () => {
      const timestamp = nowTimestamp()
      expect(typeof timestamp).toBe("number")
    })

    it("should return current timestamp (approximate)", () => {
      const before = Date.now()
      const timestamp = nowTimestamp()
      const after = Date.now()

      expect(timestamp).toBeGreaterThanOrEqual(before)
      expect(timestamp).toBeLessThanOrEqual(after + 10) // Allow small buffer for execution time
    })

    it("should return integer milliseconds", () => {
      const timestamp = nowTimestamp()
      expect(Number.isInteger(timestamp)).toBe(true)
    })
  })

  describe("timestampToDate", () => {
    it("should convert timestamp to Date object", () => {
      const timestamp = 1_700_000_000_000 // Example timestamp
      const date = timestampToDate(timestamp)

      expect(date).toBeInstanceOf(Date)
      expect(date.getTime()).toBe(timestamp)
    })

    it("should handle recent timestamps", () => {
      const now = Date.now()
      const date = timestampToDate(now)

      expect(date.getTime()).toBe(now)
      expect(date).toBeInstanceOf(Date)
    })

    it("should handle epoch timestamp", () => {
      const date = timestampToDate(0)
      expect(date.getTime()).toBe(0)
      expect(date.getUTCFullYear()).toBe(1970)
    })
  })

  describe("dateToTimestamp", () => {
    it("should convert Date object to timestamp", () => {
      const date = new Date()
      const timestamp = dateToTimestamp(date)

      expect(typeof timestamp).toBe("number")
      expect(timestamp).toBe(date.getTime())
    })

    it("should handle specific date", () => {
      const date = new Date("2024-01-01T00:00:00.000Z")
      const timestamp = dateToTimestamp(date)

      // Calculate the correct timestamp for 2024-01-01T00:00:00.000Z
      expect(timestamp).toBe(Date.UTC(2024, 0, 1, 0, 0, 0, 0)) // January is 0-indexed
    })

    it("should preserve milliseconds", () => {
      const date = new Date("2024-01-01T12:34:56.789Z")
      const timestamp = dateToTimestamp(date)

      // Calculate the correct timestamp
      expect(timestamp).toBe(Date.UTC(2024, 0, 1, 12, 34, 56, 789)) // January is 0-indexed
    })
  })

  describe("isTimestampInRange", () => {
    it("should return true for timestamp within range", () => {
      const timestamp = 1_700_000_000_000
      const start = 1_699_000_000_000
      const end = 1_701_000_000_000

      expect(isTimestampInRange(timestamp, start, end)).toBe(true)
    })

    it("should return false for timestamp before range", () => {
      const timestamp = 1_698_000_000_000
      const start = 1_699_000_000_000
      const end = 1_701_000_000_000

      expect(isTimestampInRange(timestamp, start, end)).toBe(false)
    })

    it("should return false for timestamp after range", () => {
      const timestamp = 1_702_000_000_000
      const start = 1_699_000_000_000
      const end = 1_701_000_000_000

      expect(isTimestampInRange(timestamp, start, end)).toBe(false)
    })

    it("should return true for timestamp equal to start", () => {
      const timestamp = 1_700_000_000_000
      const start = 1_700_000_000_000
      const end = 1_701_000_000_000

      expect(isTimestampInRange(timestamp, start, end)).toBe(true)
    })

    it("should return true for timestamp equal to end", () => {
      const timestamp = 1_701_000_000_000
      const start = 1_700_000_000_000
      const end = 1_701_000_000_000

      expect(isTimestampInRange(timestamp, start, end)).toBe(true)
    })
  })

  describe("timestampDiff", () => {
    it("should calculate positive difference", () => {
      const timestamp1 = 1_700_000_000_000
      const timestamp2 = 1_700_000_005_000 // 5 seconds later

      expect(timestampDiff(timestamp1, timestamp2)).toBe(5000)
      expect(timestampDiff(timestamp2, timestamp1)).toBe(5000) // Order should not matter
    })

    it("should return 0 for same timestamps", () => {
      const timestamp = 1_700_000_000_000

      expect(timestampDiff(timestamp, timestamp)).toBe(0)
    })

    it("should handle large differences", () => {
      const timestamp1 = 0
      const timestamp2 = 1_000_000_000_000 // About 31 years later

      expect(timestampDiff(timestamp1, timestamp2)).toBe(1_000_000_000_000)
    })
  })

  describe("isValidTimestamp", () => {
    it("should return true for valid positive timestamp", () => {
      expect(isValidTimestamp(1_700_000_000_000)).toBe(true)
    })

    it("should return false for non-number values", () => {
      expect(isValidTimestamp("123")).toBe(false)
      expect(isValidTimestamp(null)).toBe(false)
      expect(isValidTimestamp(undefined)).toBe(false)
      expect(isValidTimestamp({})).toBe(false)
      expect(isValidTimestamp([])).toBe(false)
    })

    it("should return false for negative numbers", () => {
      expect(isValidTimestamp(-1)).toBe(false)
      expect(isValidTimestamp(-1_000_000_000_000)).toBe(false)
    })

    it("should return false for non-finite numbers", () => {
      expect(isValidTimestamp(Number.POSITIVE_INFINITY)).toBe(false)
      expect(isValidTimestamp(Number.NEGATIVE_INFINITY)).toBe(false)
      expect(isValidTimestamp(Number.NaN)).toBe(false)
    })

    it("should return false for timestamps outside reasonable range", () => {
      // Before 1970 (negative)
      expect(isValidTimestamp(-1)).toBe(false)

      // After year 3000 (in the future)
      expect(isValidTimestamp(32_503_680_000_001)).toBe(false) // Just after year 3000
    })

    it("should return true for timestamps at range boundaries", () => {
      // Beginning of Unix time
      expect(isValidTimestamp(0)).toBe(true)

      // End boundary (year 3000)
      expect(isValidTimestamp(32_503_680_000_000)).toBe(true)
    })

    it("should return true for current timestamp", () => {
      expect(isValidTimestamp(Date.now())).toBe(true)
    })
  })
})
