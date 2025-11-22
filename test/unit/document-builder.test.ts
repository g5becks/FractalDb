import { describe, expect, it } from "bun:test"
import type { Document } from "../../src/core-types.js"
import { buildCompleteDocument } from "../../src/document-builder.js"

describe("Document Builder", () => {
  type TestUser = Document<{
    name: string
    email: string
    age: number
    active: boolean
    tags: string[]
  }>

  describe("buildCompleteDocument", () => {
    it("should build complete document from partial data and metadata", () => {
      const partial: Partial<
        Omit<TestUser, "_id" | "createdAt" | "updatedAt">
      > = {
        name: "Alice",
        email: "alice@example.com",
        age: 30,
        active: true,
        tags: ["user"],
      }

      const metadata = {
        _id: "user-123",
        createdAt: 1_700_000_000_000,
        updatedAt: 1_700_000_000_000,
      }

      const result = buildCompleteDocument<TestUser>(partial, metadata)

      expect(result).toEqual({
        _id: "user-123",
        name: "Alice",
        email: "alice@example.com",
        age: 30,
        active: true,
        tags: ["user"],
        createdAt: 1_700_000_000_000,
        updatedAt: 1_700_000_000_000,
      })

      // Verify type
      expect(typeof result._id).toBe("string")
      expect(typeof result.name).toBe("string")
      expect(typeof result.age).toBe("number")
      expect(typeof result.active).toBe("boolean")
    })

    it("should handle minimal partial data", () => {
      const partial = {
        name: "Bob",
      }

      const metadata = {
        _id: "user-456",
        createdAt: 1_700_000_005_000,
        updatedAt: 1_700_000_005_000,
      }

      type MinimalUser = Document<{ name: string }>

      const result = buildCompleteDocument<MinimalUser>(partial, metadata)

      expect(result).toEqual({
        _id: "user-456",
        name: "Bob",
        createdAt: 1_700_000_005_000,
        updatedAt: 1_700_000_005_000,
      })
    })

    it("should handle empty partial data", () => {
      const partial: Partial<
        Omit<TestUser, "_id" | "createdAt" | "updatedAt">
      > = {}

      const metadata = {
        _id: "user-empty",
        createdAt: 1_700_000_010_000,
        updatedAt: 1_700_000_010_000,
      }

      type EmptyUser = Document<Record<string, never>>

      const result = buildCompleteDocument<EmptyUser>(partial, metadata)

      expect(result).toEqual({
        _id: "user-empty",
        createdAt: 1_700_000_010_000,
        updatedAt: 1_700_000_010_000,
      })
    })

    it("should override partial data with metadata fields", () => {
      // If partial contains metadata fields, metadata should take precedence
      const partial: Partial<
        Omit<TestUser, "_id" | "createdAt" | "updatedAt">
      > & {
        _id?: string
        createdAt?: number
        updatedAt?: number
      } = {
        name: "Charlie",
        _id: "partial-id", // This should be overridden
        createdAt: 1_600_000_000_000, // This should be overridden
      }

      const metadata = {
        _id: "correct-id",
        createdAt: 1_700_000_000_000,
        updatedAt: 1_700_000_015_000,
      }

      const result = buildCompleteDocument<TestUser>(partial, metadata)

      expect(result).toEqual({
        _id: "correct-id", // Metadata value should win
        name: "Charlie",
        createdAt: 1_700_000_000_000, // Metadata value should win
        updatedAt: 1_700_000_015_000,
        email: undefined, // From partial but undefined
        age: undefined, // From partial but undefined
        active: undefined, // From partial but undefined
        tags: undefined, // From partial but undefined
      })
    })

    it("should handle partial with all required fields", () => {
      const partial = {
        name: "Diana",
        email: "diana@example.com",
        age: 25,
        active: true,
        tags: ["admin", "premium"],
      }

      const metadata = {
        _id: "user-789",
        createdAt: 1_700_000_020_000,
        updatedAt: 1_700_000_020_000,
      }

      const result = buildCompleteDocument<TestUser>(partial, metadata)

      expect(result).toEqual({
        _id: "user-789",
        name: "Diana",
        email: "diana@example.com",
        age: 25,
        active: true,
        tags: ["admin", "premium"],
        createdAt: 1_700_000_020_000,
        updatedAt: 1_700_000_020_000,
      })
    })

    it("should handle partial as complete object without metadata fields", () => {
      const partial: Omit<TestUser, "_id" | "createdAt" | "updatedAt"> = {
        name: "Eve",
        email: "eve@example.com",
        age: 35,
        active: false,
        tags: ["inactive"],
      }

      const metadata = {
        _id: "user-999",
        createdAt: 1_700_000_025_000,
        updatedAt: 1_700_000_025_000,
      }

      const result = buildCompleteDocument<TestUser>(partial, metadata)

      expect(result).toEqual({
        _id: "user-999",
        name: "Eve",
        email: "eve@example.com",
        age: 35,
        active: false,
        tags: ["inactive"],
        createdAt: 1_700_000_025_000,
        updatedAt: 1_700_000_025_000,
      })
    })

    it("should maintain correct type inference", () => {
      const partial = { name: "Frank" }

      const metadata = {
        _id: "user-type-test",
        createdAt: 1_700_000_030_000,
        updatedAt: 1_700_000_030_000,
      }

      type SimpleUser = Document<{ name: string }>

      const result = buildCompleteDocument<SimpleUser>(partial, metadata)

      // TypeScript should correctly infer that result is of type SimpleUser
      expect(result._id).toBe("user-type-test")
      expect(result.name).toBe("Frank")
      expect(result.createdAt).toBe(1_700_000_030_000)
      expect(result.updatedAt).toBe(1_700_000_030_000)
    })

    it("should preserve nested objects in partial data", () => {
      type UserWithProfile = Document<{
        name: string
        profile: {
          age: number
          settings: {
            theme: string
          }
        }
      }>

      const partial = {
        name: "Grace",
        profile: {
          age: 28,
          settings: {
            theme: "light",
          },
        },
      }

      const metadata = {
        _id: "user-nested",
        createdAt: 1_700_000_035_000,
        updatedAt: 1_700_000_035_000,
      }

      const result = buildCompleteDocument<UserWithProfile>(partial, metadata)

      expect(result).toEqual({
        _id: "user-nested",
        name: "Grace",
        profile: {
          age: 28,
          settings: {
            theme: "light",
          },
        },
        createdAt: 1_700_000_035_000,
        updatedAt: 1_700_000_035_000,
      })

      // Ensure nested objects are properly preserved
      expect(result.profile.age).toBe(28)
      expect(result.profile.settings.theme).toBe("light")
    })
  })
})
