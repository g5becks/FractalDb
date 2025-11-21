/**
 * Integration tests for Collection CRUD operations.
 *
 * These tests verify that Collection operations work correctly with an actual
 * SQLite database, including document serialization, querying, and data integrity.
 */

import { afterEach, beforeEach, describe, expect, it } from "bun:test"
import {
  createSchema,
  type Document,
  StrataDBClass,
  UniqueConstraintError,
} from "../../src/index.js"

// ============================================================================
// Test Document Types
// ============================================================================

/**
 * User document for testing basic CRUD operations.
 */
type User = Document<{
  name: string
  email: string
  age: number
  active: boolean
  tags: string[]
}>

/**
 * Product document for testing nested objects and arrays.
 */
type Product = Document<{
  name: string
  price: number
  inventory: {
    stock: number
    warehouse: string
  }
  categories: string[]
}>

// ============================================================================
// Test Setup
// ============================================================================

describe("Collection CRUD Operations", () => {
  let db: StrataDBClass

  // Create fresh in-memory database before each test
  beforeEach(() => {
    db = new StrataDBClass({ database: ":memory:" })
  })

  // Close database after each test
  afterEach(() => {
    db.close()
  })

  // ==========================================================================
  // Schema Creation Helper
  // ==========================================================================

  const createUserSchema = () =>
    createSchema<User>()
      .field("name", { type: "TEXT", indexed: true })
      .field("email", { type: "TEXT", indexed: true, unique: true })
      .field("age", { type: "INTEGER", indexed: true })
      .field("active", { type: "INTEGER", indexed: false })
      .field("tags", { type: "TEXT", indexed: false })
      .build()

  const createProductSchema = () =>
    createSchema<Product>()
      .field("name", { type: "TEXT", indexed: true })
      .field("price", { type: "REAL", indexed: true })
      .field("categories", { type: "TEXT", indexed: false })
      .build()

  // ==========================================================================
  // insertOne Tests
  // ==========================================================================

  describe("insertOne", () => {
    it("should create document with auto-generated ID", async () => {
      const users = db.collection("users", createUserSchema())

      const result = await users.insertOne({
        name: "Alice",
        email: "alice@example.com",
        age: 30,
        active: true,
        tags: ["developer", "typescript"],
      })

      expect(result.document).toBeDefined()
      expect(result.document.id).toBeDefined()
      expect(typeof result.document.id).toBe("string")
      expect(result.document.id.length).toBeGreaterThan(0)
      expect(result.acknowledged).toBe(true)
    })

    it("should return the inserted document with all fields", async () => {
      const users = db.collection("users", createUserSchema())

      const result = await users.insertOne({
        name: "Bob",
        email: "bob@example.com",
        age: 25,
        active: false,
        tags: ["designer"],
      })

      expect(result.document).toBeDefined()
      expect(result.document.name).toBe("Bob")
      expect(result.document.email).toBe("bob@example.com")
      expect(result.document.age).toBe(25)
      expect(result.document.active).toBe(false)
      expect(result.document.tags).toEqual(["designer"])
    })

    it("should allow custom ID when provided", async () => {
      const users = db.collection("users", createUserSchema())

      const customId = "custom-user-123"
      const result = await users.insertOne({
        id: customId,
        name: "Charlie",
        email: "charlie@example.com",
        age: 35,
        active: true,
        tags: [],
      })

      expect(result.document.id).toBe(customId)
    })

    it("should handle nested objects correctly", async () => {
      const products = db.collection("products", createProductSchema())

      const result = await products.insertOne({
        name: "Laptop",
        price: 999.99,
        inventory: {
          stock: 50,
          warehouse: "NYC",
        },
        categories: ["electronics", "computers"],
      })

      const retrieved = await products.findById(result.document.id)
      expect(retrieved).not.toBeNull()
      expect(retrieved?.inventory.stock).toBe(50)
      expect(retrieved?.inventory.warehouse).toBe("NYC")
      expect(retrieved?.categories).toEqual(["electronics", "computers"])
    })
  })

  // ==========================================================================
  // findById Tests
  // ==========================================================================

  describe("findById", () => {
    it("should retrieve document by ID", async () => {
      const users = db.collection("users", createUserSchema())

      const inserted = await users.insertOne({
        name: "Diana",
        email: "diana@example.com",
        age: 28,
        active: true,
        tags: ["admin"],
      })

      const found = await users.findById(inserted.document.id)

      expect(found).not.toBeNull()
      expect(found?.id).toBe(inserted.document.id)
      expect(found?.name).toBe("Diana")
      expect(found?.email).toBe("diana@example.com")
    })

    it("should return null when document not found", async () => {
      const users = db.collection("users", createUserSchema())

      const found = await users.findById("non-existent-id")

      expect(found).toBeNull()
    })

    it("should preserve all document fields including arrays", async () => {
      const users = db.collection("users", createUserSchema())

      const inserted = await users.insertOne({
        name: "Eve",
        email: "eve@example.com",
        age: 22,
        active: true,
        tags: ["tag1", "tag2", "tag3"],
      })

      const found = await users.findById(inserted.document.id)

      expect(found?.tags).toEqual(["tag1", "tag2", "tag3"])
    })
  })

  // ==========================================================================
  // find Tests
  // ==========================================================================

  describe("find", () => {
    let users: ReturnType<typeof db.collection<User>>

    beforeEach(async () => {
      users = db.collection("users", createUserSchema())

      // Insert test data
      await users.insertMany([
        {
          name: "Alice",
          email: "alice@example.com",
          age: 30,
          active: true,
          tags: ["developer"],
        },
        {
          name: "Bob",
          email: "bob@example.com",
          age: 25,
          active: true,
          tags: ["designer"],
        },
        {
          name: "Charlie",
          email: "charlie@example.com",
          age: 35,
          active: false,
          tags: ["manager"],
        },
        {
          name: "Diana",
          email: "diana@example.com",
          age: 28,
          active: true,
          tags: ["developer", "lead"],
        },
      ])
    })

    it("should return all documents with empty filter", async () => {
      const results = await users.find({})

      expect(results.length).toBe(4)
    })

    it("should filter by equality", async () => {
      const results = await users.find({ name: "Alice" })

      expect(results.length).toBe(1)
      expect(results[0].name).toBe("Alice")
    })

    it("should filter with comparison operators", async () => {
      const results = await users.find({ age: { $gte: 30 } })

      expect(results.length).toBe(2) // Alice (30) and Charlie (35)
      for (const user of results) {
        expect(user.age).toBeGreaterThanOrEqual(30)
      }
    })

    it("should filter with $and operator", async () => {
      const results = await users.find({
        $and: [{ age: { $gte: 25 } }, { active: true }],
      })

      expect(results.length).toBe(3) // Alice, Bob, Diana
    })

    it("should filter with $or operator", async () => {
      const results = await users.find({
        $or: [{ name: "Alice" }, { name: "Bob" }],
      })

      expect(results.length).toBe(2)
    })

    it("should apply limit option", async () => {
      const results = await users.find({}, { limit: 2 })

      expect(results.length).toBe(2)
    })

    it("should apply skip option with limit", async () => {
      // SQLite requires LIMIT with OFFSET
      const skippedResults = await users.find({}, { skip: 2, limit: 10 })

      expect(skippedResults.length).toBe(2)
    })

    it("should apply sort option", async () => {
      const ascending = await users.find({}, { sort: { age: 1 } })
      const descending = await users.find({}, { sort: { age: -1 } })

      expect(ascending[0].age).toBeLessThanOrEqual(ascending[1].age)
      expect(descending[0].age).toBeGreaterThanOrEqual(descending[1].age)
    })
  })

  // ==========================================================================
  // findOne Tests
  // ==========================================================================

  describe("findOne", () => {
    it("should return first matching document", async () => {
      const users = db.collection("users", createUserSchema())

      await users.insertMany([
        {
          name: "Alice",
          email: "alice@example.com",
          age: 30,
          active: true,
          tags: [],
        },
        {
          name: "Bob",
          email: "bob@example.com",
          age: 25,
          active: true,
          tags: [],
        },
      ])

      const result = await users.findOne({ active: true })

      expect(result).not.toBeNull()
      expect(result?.active).toBe(true)
    })

    it("should return null when no match found", async () => {
      const users = db.collection("users", createUserSchema())

      const result = await users.findOne({ name: "NonExistent" })

      expect(result).toBeNull()
    })
  })

  // ==========================================================================
  // count Tests
  // ==========================================================================

  describe("count", () => {
    it("should return total count with empty filter", async () => {
      const users = db.collection("users", createUserSchema())

      await users.insertMany([
        {
          name: "Alice",
          email: "alice@example.com",
          age: 30,
          active: true,
          tags: [],
        },
        {
          name: "Bob",
          email: "bob@example.com",
          age: 25,
          active: false,
          tags: [],
        },
        {
          name: "Charlie",
          email: "charlie@example.com",
          age: 35,
          active: true,
          tags: [],
        },
      ])

      const count = await users.count({})

      expect(count).toBe(3)
    })

    it("should return filtered count", async () => {
      const users = db.collection("users", createUserSchema())

      await users.insertMany([
        {
          name: "Alice",
          email: "alice@example.com",
          age: 30,
          active: true,
          tags: [],
        },
        {
          name: "Bob",
          email: "bob@example.com",
          age: 25,
          active: false,
          tags: [],
        },
        {
          name: "Charlie",
          email: "charlie@example.com",
          age: 35,
          active: true,
          tags: [],
        },
      ])

      const activeCount = await users.count({ active: true })

      expect(activeCount).toBe(2)
    })
  })

  // ==========================================================================
  // updateOne Tests
  // ==========================================================================

  describe("updateOne", () => {
    it("should update document fields", async () => {
      const users = db.collection("users", createUserSchema())

      const inserted = await users.insertOne({
        name: "Frank",
        email: "frank@example.com",
        age: 40,
        active: true,
        tags: ["admin"],
      })

      const updated = await users.updateOne(inserted.document.id, {
        age: 41,
        active: false,
      })

      expect(updated).not.toBeNull()
      expect(updated?.age).toBe(41)
      expect(updated?.active).toBe(false)
      expect(updated?.name).toBe("Frank") // Unchanged fields preserved
    })

    it("should return null when document not found", async () => {
      const users = db.collection("users", createUserSchema())

      const updated = await users.updateOne("non-existent-id", { age: 99 })

      expect(updated).toBeNull()
    })

    it("should preserve id field", async () => {
      const users = db.collection("users", createUserSchema())

      const inserted = await users.insertOne({
        name: "Grace",
        email: "grace@example.com",
        age: 33,
        active: true,
        tags: [],
      })

      const originalId = inserted.document.id

      const updated = await users.updateOne(originalId, {
        name: "Grace Updated",
      })

      expect(updated?.id).toBe(originalId)
    })
  })

  // ==========================================================================
  // deleteOne Tests
  // ==========================================================================

  describe("deleteOne", () => {
    it("should delete document and return true", async () => {
      const users = db.collection("users", createUserSchema())

      const inserted = await users.insertOne({
        name: "Henry",
        email: "henry@example.com",
        age: 45,
        active: true,
        tags: [],
      })

      const deleted = await users.deleteOne(inserted.document.id)

      expect(deleted).toBe(true)

      // Verify document is actually deleted
      const found = await users.findById(inserted.document.id)
      expect(found).toBeNull()
    })

    it("should return false when document not found", async () => {
      const users = db.collection("users", createUserSchema())

      const deleted = await users.deleteOne("non-existent-id")

      expect(deleted).toBe(false)
    })
  })

  // ==========================================================================
  // Unique Constraint Tests
  // ==========================================================================

  describe("unique constraints", () => {
    it("should throw UniqueConstraintError on duplicate unique field", async () => {
      const users = db.collection("users", createUserSchema())

      await users.insertOne({
        name: "Original",
        email: "duplicate@example.com",
        age: 25,
        active: true,
        tags: [],
      })

      try {
        await users.insertOne({
          name: "Duplicate",
          email: "duplicate@example.com", // Same email
          age: 30,
          active: false,
          tags: [],
        })
        expect.unreachable("Should have thrown UniqueConstraintError")
      } catch (error) {
        expect(error).toBeInstanceOf(UniqueConstraintError)
      }
    })

    it("should include field name in UniqueConstraintError", async () => {
      const users = db.collection("users", createUserSchema())

      await users.insertOne({
        name: "First",
        email: "unique@example.com",
        age: 25,
        active: true,
        tags: [],
      })

      try {
        await users.insertOne({
          name: "Second",
          email: "unique@example.com",
          age: 30,
          active: false,
          tags: [],
        })
        expect.unreachable("Should have thrown UniqueConstraintError")
      } catch (error) {
        expect(error).toBeInstanceOf(UniqueConstraintError)
        expect((error as UniqueConstraintError).field).toBe("email")
      }
    })
  })

  // ==========================================================================
  // Edge Cases and Data Integrity
  // ==========================================================================

  describe("edge cases", () => {
    it("should handle empty arrays correctly", async () => {
      const users = db.collection("users", createUserSchema())

      const inserted = await users.insertOne({
        name: "Empty Tags",
        email: "empty@example.com",
        age: 20,
        active: true,
        tags: [],
      })

      const found = await users.findById(inserted.document.id)
      expect(found?.tags).toEqual([])
    })

    it("should handle special characters in strings", async () => {
      const users = db.collection("users", createUserSchema())

      const specialName = 'O\'Brien "The Rock" <test>'
      const inserted = await users.insertOne({
        name: specialName,
        email: "obrien@example.com",
        age: 50,
        active: true,
        tags: ["special's", '"quoted"'],
      })

      const found = await users.findById(inserted.document.id)
      expect(found?.name).toBe(specialName)
      expect(found?.tags).toEqual(["special's", '"quoted"'])
    })

    it("should handle numeric edge values", async () => {
      const users = db.collection("users", createUserSchema())

      const inserted = await users.insertOne({
        name: "Edge Numbers",
        email: "edge@example.com",
        age: 0,
        active: false,
        tags: [],
      })

      const found = await users.findById(inserted.document.id)
      expect(found?.age).toBe(0)
      expect(found?.active).toBe(false)
    })

    it("should handle unicode characters", async () => {
      const users = db.collection("users", createUserSchema())

      const unicodeName = "日本語テスト 🎉 émoji"
      const inserted = await users.insertOne({
        name: unicodeName,
        email: "unicode@example.com",
        age: 25,
        active: true,
        tags: ["日本語", "🎉"],
      })

      const found = await users.findById(inserted.document.id)
      expect(found?.name).toBe(unicodeName)
      expect(found?.tags).toEqual(["日本語", "🎉"])
    })
  })
})
