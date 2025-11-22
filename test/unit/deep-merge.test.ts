import { describe, expect, it } from "bun:test"
import type { Document } from "../../src/core-types.js"
import { deepMerge, mergeDocumentUpdate } from "../../src/deep-merge.js"

describe("Deep Merge Utilities", () => {
  describe("deepMerge", () => {
    it("should merge simple objects", () => {
      const obj1 = { name: "Alice", age: 30 }
      const obj2 = { email: "alice@example.com", age: 31 }

      const result = deepMerge(obj1, obj2)

      expect(result).toEqual({
        name: "Alice",
        email: "alice@example.com",
        age: 31, // obj2 value should override
      })
    })

    it("should perform deep merge of nested objects", () => {
      const obj1 = {
        name: "Alice",
        profile: { age: 30, city: "NYC", settings: { theme: "dark" } },
      }
      const obj2 = {
        profile: { age: 31, country: "USA", settings: { lang: "en" } },
      }

      const result = deepMerge(obj1, obj2)

      expect(result).toEqual({
        name: "Alice",
        profile: {
          age: 31,
          city: "NYC",
          country: "USA",
          settings: { theme: "dark", lang: "en" },
        },
      })
    })

    it("should replace arrays instead of concatenating", () => {
      const obj1 = { tags: ["user", "active"], numbers: [1, 2, 3] }
      const obj2 = { tags: ["admin"], numbers: [4, 5] }

      const result = deepMerge(obj1, obj2)

      expect(result).toEqual({
        tags: ["admin"], // Should be replaced, not concatenated
        numbers: [4, 5], // Should be replaced, not concatenated
      })
    })

    it("should preserve undefined values for field deletion", () => {
      const obj1 = { name: "Alice", email: "alice@example.com", active: true }
      const obj2 = { email: undefined, active: false }

      const result = deepMerge(obj1, obj2)

      expect(result).toEqual({
        name: "Alice",
        email: undefined, // Should be preserved to enable field deletion
        active: false,
      })
    })

    it("should handle null values", () => {
      const obj1 = { name: "Alice", value: "test" }
      const obj2 = { value: null, other: "new" }

      const result = deepMerge(obj1, obj2)

      expect(result).toEqual({
        name: "Alice",
        value: null, // Should preserve null
        other: "new",
      })
    })

    it("should handle nested undefined values", () => {
      const obj1 = {
        profile: { name: "Alice", settings: { theme: "dark", enabled: true } },
      }
      const obj2 = {
        profile: { settings: { enabled: undefined } },
      }

      const result = deepMerge(obj1, obj2)

      expect(result).toEqual({
        profile: {
          name: "Alice",
          settings: { theme: "dark", enabled: undefined },
        },
      })
    })

    it("should handle empty objects", () => {
      expect(deepMerge({}, { name: "Alice" })).toEqual({ name: "Alice" })
      expect(deepMerge({ name: "Alice" }, {})).toEqual({ name: "Alice" })
      expect(deepMerge({}, {})).toEqual({})
    })

    it("should not mutate original objects", () => {
      const obj1 = { name: "Alice", profile: { age: 30 } }
      const obj2 = { profile: { city: "NYC" } }
      const originalObj1 = { ...obj1, profile: { ...obj1.profile } }
      const originalObj2 = { ...obj2, profile: { ...obj2.profile } }

      deepMerge(obj1, obj2)

      expect(obj1).toEqual(originalObj1)
      expect(obj2).toEqual(originalObj2)
    })

    it("should handle objects with potential prototype pollution", () => {
      // deepmerge-ts should handle malicious attempts safely
      const obj1 = { name: "Alice", value: "original" }
      const obj2 = {
        __proto__: { isAdmin: true },
        constructor: { prototype: { isAdmin: true } },
        value: "updated",
      }

      const result = deepMerge(obj1, obj2)

      // The malicious properties should not affect the global prototype
      expect({} as any).not.toHaveProperty("isAdmin")

      // The result should have the expected values
      expect(result.value).toBe("updated")
      // Note: This may or may not have name depending on merge behavior
      // The important thing is that prototype pollution is prevented
    })
  })

  describe("mergeDocumentUpdate", () => {
    type TestDoc = Document<{
      name: string
      age: number
      profile: {
        city: string
        preferences: {
          theme: string
          notifications: boolean
        }
      }
      tags: string[]
    }>

    it("should merge document body with metadata", () => {
      const existingBody = {
        name: "Alice",
        age: 30,
        profile: {
          city: "NYC",
          preferences: { theme: "dark", notifications: true },
        },
        tags: ["user"],
        createdAt: 1_700_000_000_000,
        updatedAt: 1_700_000_000_000,
      }

      const metadata = {
        id: "test-123",
        updatedAt: 1_700_000_005_000,
        profile: { preferences: { notifications: false } }, // Partial update
        tags: ["admin"], // Replace array
      }

      const result = mergeDocumentUpdate<TestDoc, typeof metadata>(
        existingBody,
        metadata
      )

      expect(result).toEqual({
        id: "test-123",
        name: "Alice",
        age: 30,
        profile: {
          city: "NYC",
          preferences: { theme: "dark", notifications: false }, // Partial update merged
        },
        tags: ["admin"], // Array replaced
        createdAt: 1_700_000_000_000,
        updatedAt: 1_700_000_005_000, // Updated
      })
    })

    it("should handle document creation with metadata", () => {
      const partialBody = {
        name: "Bob",
        age: 25,
      }

      const metadata = {
        id: "test-456",
        createdAt: 1_700_000_000_000,
        updatedAt: 1_700_000_000_000,
      }

      // Use a simpler document type for this test
      type SimpleDoc = Document<{ name: string; age: number }>

      const result = mergeDocumentUpdate<SimpleDoc, typeof metadata>(
        partialBody,
        metadata
      )

      expect(result).toEqual({
        id: "test-456",
        name: "Bob",
        age: 25,
        createdAt: 1_700_000_000_000,
        updatedAt: 1_700_000_000_000,
      })
    })

    it("should preserve existing properties when merging", () => {
      const existingBody = {
        name: "Charlie",
        age: 35,
        createdAt: 1_700_000_000_000,
        updatedAt: 1_700_000_000_000,
      }

      const metadata = {
        id: "test-789",
        updatedAt: 1_700_000_010_000,
        age: 36, // Update existing field
      }

      type SimpleDoc = Document<{ name: string; age: number }>

      const result = mergeDocumentUpdate<SimpleDoc, typeof metadata>(
        existingBody,
        metadata
      )

      expect(result).toEqual({
        id: "test-789",
        name: "Charlie", // Preserved
        age: 36, // Updated
        createdAt: 1_700_000_000_000, // Preserved
        updatedAt: 1_700_000_010_000, // Updated
      })
    })

    it("should handle undefined values for field deletion", () => {
      const existingBody = {
        name: "David",
        email: "david@example.com",
        age: 40,
        createdAt: 1_700_000_000_000,
        updatedAt: 1_700_000_000_000,
      }

      const metadata = {
        id: "test-000",
        updatedAt: 1_700_000_015_000,
        email: undefined, // Mark for deletion
      }

      type UserDoc = Document<{ name: string; email?: string; age: number }>

      const result = mergeDocumentUpdate<UserDoc, typeof metadata>(
        existingBody,
        metadata
      )

      expect(result).toEqual({
        id: "test-000",
        name: "David",
        email: undefined, // Preserved as undefined for deletion
        age: 40,
        createdAt: 1_700_000_000_000,
        updatedAt: 1_700_000_015_000,
      })
    })

    it("should maintain correct return type", () => {
      const existingBody = {
        name: "Eve",
        age: 28,
        createdAt: 1_700_000_000_000,
        updatedAt: 1_700_000_000_000,
      }

      const metadata = {
        id: "test-eve",
        updatedAt: 1_700_000_020_000,
      }

      type UserDoc = Document<{ name: string; age: number }>

      const result = mergeDocumentUpdate<UserDoc, typeof metadata>(
        existingBody,
        metadata
      )

      // TypeScript should recognize the result as UserDoc type
      expect(result.id).toBe("test-eve")
      expect(result.name).toBe("Eve")
      expect(result.age).toBe(28)
      expect(result.updatedAt).toBe(1_700_000_020_000)
      expect(typeof result.id).toBe("string")
    })
  })
})
