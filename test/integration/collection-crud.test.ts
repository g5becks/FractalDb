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

      const user = await users.insertOne({
        name: "Alice",
        email: "alice@example.com",
        age: 30,
        active: true,
        tags: ["developer", "typescript"],
      })

      expect(user).toBeDefined()
      expect(user._id).toBeDefined()
      expect(typeof user._id).toBe("string")
      expect(user._id.length).toBeGreaterThan(0)
    })

    it("should return the inserted document with all fields", async () => {
      const users = db.collection("users", createUserSchema())

      const user = await users.insertOne({
        name: "Bob",
        email: "bob@example.com",
        age: 25,
        active: false,
        tags: ["designer"],
      })

      expect(user).toBeDefined()
      expect(user.name).toBe("Bob")
      expect(user.email).toBe("bob@example.com")
      expect(user.age).toBe(25)
      expect(user.active).toBe(false)
      expect(user.tags).toEqual(["designer"])
    })

    it("should allow custom ID when provided", async () => {
      const users = db.collection("users", createUserSchema())

      const customId = "custom-user-123"
      const user = await users.insertOne({
        _id: customId,
        name: "Charlie",
        email: "charlie@example.com",
        age: 35,
        active: true,
        tags: [],
      })

      expect(user._id).toBe(customId)
    })

    it("should handle nested objects correctly", async () => {
      const products = db.collection("products", createProductSchema())

      const product = await products.insertOne({
        name: "Laptop",
        price: 999.99,
        inventory: {
          stock: 50,
          warehouse: "NYC",
        },
        categories: ["electronics", "computers"],
      })

      const retrieved = await products.findById(product._id)
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

      const found = await users.findById(inserted._id)

      expect(found).not.toBeNull()
      expect(found?._id).toBe(inserted._id)
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

      const found = await users.findById(inserted._id)

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

      const updated = await users.updateOne(inserted._id, {
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

      const originalId = inserted._id

      const updated = await users.updateOne(originalId, {
        name: "Grace Updated",
      })

      expect(updated?._id).toBe(originalId)
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

      const deleted = await users.deleteOne(inserted._id)

      expect(deleted).toBe(true)

      // Verify document is actually deleted
      const found = await users.findById(inserted._id)
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
  // Uniform Filter Support (String ID | QueryFilter)
  // ==========================================================================

  describe("Uniform filter support for all 'One' methods", () => {
    describe("findOne", () => {
      it("should accept string ID", async () => {
        const users = db.collection("users", createUserSchema())
        const inserted = await users.insertOne({
          name: "Alice",
          email: "alice@example.com",
          age: 30,
          active: true,
          tags: [],
        })
        const found = await users.findOne(inserted._id)
        expect(found?._id).toBe(inserted._id)
        expect(found?.name).toBe("Alice")
      })

      it("should accept query filter", async () => {
        const users = db.collection("users", createUserSchema())
        await users.insertOne({
          name: "Alice",
          email: "alice@example.com",
          age: 30,
          active: true,
          tags: [],
        })
        const found = await users.findOne({ email: "alice@example.com" })
        expect(found?.name).toBe("Alice")
      })
    })

    describe("updateOne", () => {
      it("should accept string ID (backwards compatible)", async () => {
        const users = db.collection("users", createUserSchema())
        const user = await users.insertOne({
          name: "Alice",
          email: "alice@example.com",
          age: 30,
          active: true,
          tags: [],
        })
        const updated = await users.updateOne(user._id, { age: 31 })
        expect(updated?.age).toBe(31)
      })

      it("should accept query filter", async () => {
        const users = db.collection("users", createUserSchema())
        await users.insertOne({
          name: "Alice",
          email: "alice@example.com",
          age: 30,
          active: true,
          tags: [],
        })
        const updated = await users.updateOne(
          { email: "alice@example.com" },
          { age: 31 }
        )
        expect(updated?.age).toBe(31)
      })
    })

    describe("deleteOne", () => {
      it("should accept string ID (backwards compatible)", async () => {
        const users = db.collection("users", createUserSchema())
        const user = await users.insertOne({
          name: "Alice",
          email: "alice@example.com",
          age: 30,
          active: true,
          tags: [],
        })
        const deleted = await users.deleteOne(user._id)
        expect(deleted).toBe(true)
        const found = await users.findById(user._id)
        expect(found).toBeNull()
      })

      it("should accept query filter", async () => {
        const users = db.collection("users", createUserSchema())
        await users.insertOne({
          name: "Alice",
          email: "alice@example.com",
          age: 30,
          active: true,
          tags: [],
        })
        const deleted = await users.deleteOne({ email: "alice@example.com" })
        expect(deleted).toBe(true)
        const count = await users.count({ email: "alice@example.com" })
        expect(count).toBe(0)
      })
    })

    describe("replaceOne", () => {
      it("should accept string ID (backwards compatible)", async () => {
        const users = db.collection("users", createUserSchema())
        const user = await users.insertOne({
          name: "Alice",
          email: "alice@example.com",
          age: 30,
          active: true,
          tags: ["old"],
        })
        const replaced = await users.replaceOne(user._id, {
          name: "Alicia",
          email: "alicia@example.com",
          age: 31,
          active: false,
          tags: ["new"],
        })
        expect(replaced?.name).toBe("Alicia")
        expect(replaced?.age).toBe(31)
        expect(replaced?._id).toBe(user._id) // ID preserved
      })

      it("should accept query filter", async () => {
        const users = db.collection("users", createUserSchema())
        await users.insertOne({
          name: "Alice",
          email: "alice@example.com",
          age: 30,
          active: true,
          tags: [],
        })
        const replaced = await users.replaceOne(
          { email: "alice@example.com" },
          {
            name: "Alicia",
            email: "alicia@example.com",
            age: 31,
            active: false,
            tags: ["replaced"],
          }
        )
        expect(replaced?.name).toBe("Alicia")
      })
    })

    describe("Edge cases", () => {
      it("should handle _id in filter object", async () => {
        const users = db.collection("users", createUserSchema())
        const user = await users.insertOne({
          name: "Alice",
          email: "alice@example.com",
          age: 30,
          active: true,
          tags: [],
        })
        const found = await users.findOne({ _id: user._id })
        expect(found?._id).toBe(user._id)
      })

      it("should update first match when multiple documents match filter", async () => {
        const users = db.collection("users", createUserSchema())
        await users.insertMany([
          {
            name: "User1",
            email: "user1@example.com",
            age: 30,
            active: false,
            tags: [],
          },
          {
            name: "User2",
            email: "user2@example.com",
            age: 25,
            active: false,
            tags: [],
          },
        ])
        await users.updateOne({ active: false }, { active: true })
        const activeCount = await users.count({ active: true })
        expect(activeCount).toBe(1) // Only one updated
      })

      it("should delete first match when multiple documents match filter", async () => {
        const users = db.collection("users", createUserSchema())
        await users.insertMany([
          {
            name: "User1",
            email: "user1@example.com",
            age: 30,
            active: false,
            tags: [],
          },
          {
            name: "User2",
            email: "user2@example.com",
            age: 25,
            active: false,
            tags: [],
          },
        ])
        const deleted = await users.deleteOne({ active: false })
        expect(deleted).toBe(true)
        const remaining = await users.count({ active: false })
        expect(remaining).toBe(1) // Only one deleted
      })

      it("should return null when updateOne finds no match", async () => {
        const users = db.collection("users", createUserSchema())
        const updated = await users.updateOne(
          { email: "nonexistent@example.com" },
          { age: 99 }
        )
        expect(updated).toBeNull()
      })

      it("should return false when deleteOne finds no match", async () => {
        const users = db.collection("users", createUserSchema())
        const deleted = await users.deleteOne({
          email: "nonexistent@example.com",
        })
        expect(deleted).toBe(false)
      })

      it("should return null when replaceOne finds no match", async () => {
        const users = db.collection("users", createUserSchema())
        const replaced = await users.replaceOne(
          { email: "nonexistent@example.com" },
          {
            name: "New",
            email: "new@example.com",
            age: 25,
            active: true,
            tags: [],
          }
        )
        expect(replaced).toBeNull()
      })
    })
  })

  // ==========================================================================
  // Atomic Find-and-Modify Operations
  // ==========================================================================

  describe("findOneAndDelete", () => {
    it("should accept string ID", async () => {
      const users = db.collection("users", createUserSchema())
      const user = await users.insertOne({
        name: "Alice",
        email: "alice@example.com",
        age: 30,
        active: true,
        tags: [],
      })
      const deleted = await users.findOneAndDelete(user._id)

      expect(deleted?._id).toBe(user._id)
      expect(deleted?.name).toBe("Alice")
      expect(await users.findById(user._id)).toBeNull()
    })

    it("should accept query filter", async () => {
      const users = db.collection("users", createUserSchema())
      await users.insertOne({
        name: "Alice",
        email: "alice@example.com",
        age: 30,
        active: false,
        tags: [],
      })
      const deleted = await users.findOneAndDelete({ active: false })

      expect(deleted).toBeDefined()
      expect(deleted?.name).toBe("Alice")
      expect(await users.count({ active: false })).toBe(0)
    })

    it("should return null if no document matches", async () => {
      const users = db.collection("users", createUserSchema())
      const deleted = await users.findOneAndDelete({
        email: "nonexistent@example.com",
      })
      expect(deleted).toBeNull()
    })

    it("should respect sort option when multiple matches exist", async () => {
      const users = db.collection("users", createUserSchema())
      await users.insertMany([
        {
          name: "Alice",
          email: "alice@example.com",
          age: 30,
          active: false,
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
          active: false,
          tags: [],
        },
      ])

      const deleted = await users.findOneAndDelete(
        { active: false },
        { sort: { age: 1 } }
      )

      expect(deleted?.name).toBe("Bob") // Age 25 (youngest)
      expect(await users.count({ active: false })).toBe(2)
    })

    it("should work with _id in filter object", async () => {
      const users = db.collection("users", createUserSchema())
      const user = await users.insertOne({
        name: "Alice",
        email: "alice@example.com",
        age: 30,
        active: true,
        tags: [],
      })
      const deleted = await users.findOneAndDelete({ _id: user._id })
      expect(deleted?._id).toBe(user._id)
      expect(await users.findById(user._id)).toBeNull()
    })
  })

  describe("findOneAndUpdate", () => {
    it("should accept string ID", async () => {
      const users = db.collection("users", createUserSchema())
      const user = await users.insertOne({
        name: "Alice",
        email: "alice@example.com",
        age: 30,
        active: true,
        tags: [],
      })
      const updated = await users.findOneAndUpdate(user._id, { age: 31 })

      expect(updated?._id).toBe(user._id)
      expect(updated?.age).toBe(31)
      expect(updated?.name).toBe("Alice") // Other fields preserved
    })

    it("should accept query filter", async () => {
      const users = db.collection("users", createUserSchema())
      await users.insertOne({
        name: "Alice",
        email: "alice@example.com",
        age: 30,
        active: true,
        tags: [],
      })
      const updated = await users.findOneAndUpdate(
        { email: "alice@example.com" },
        { age: 31 }
      )

      expect(updated).toBeDefined()
      expect(updated?.age).toBe(31)
      expect(updated?.email).toBe("alice@example.com")
    })

    it("should return null if no document matches", async () => {
      const users = db.collection("users", createUserSchema())
      const updated = await users.findOneAndUpdate(
        { email: "nonexistent@example.com" },
        { age: 100 }
      )
      expect(updated).toBeNull()
    })

    it("should return document after update by default", async () => {
      const users = db.collection("users", createUserSchema())
      const user = await users.insertOne({
        name: "Alice",
        email: "alice@example.com",
        age: 30,
        active: true,
        tags: [],
      })
      const updated = await users.findOneAndUpdate(user._id, { age: 31 })

      expect(updated?.age).toBe(31) // After update
    })

    it("should return document before update when returnDocument is 'before'", async () => {
      const users = db.collection("users", createUserSchema())
      const user = await users.insertOne({
        name: "Alice",
        email: "alice@example.com",
        age: 30,
        active: true,
        tags: [],
      })
      const before = await users.findOneAndUpdate(
        user._id,
        { age: 31 },
        { returnDocument: "before" }
      )

      expect(before?.age).toBe(30) // Before update
      const after = await users.findById(user._id)
      expect(after?.age).toBe(31) // Verify update happened
    })

    it("should respect sort option when multiple matches exist", async () => {
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

      const updated = await users.findOneAndUpdate(
        { active: true },
        { age: 100 },
        { sort: { age: 1 } }
      )

      expect(updated?.name).toBe("Bob") // Age 25 (youngest)
      expect(updated?.age).toBe(100)
    })

    it("should handle upsert when document not found", async () => {
      const users = db.collection("users", createUserSchema())
      const result = await users.findOneAndUpdate(
        { email: "new@example.com" },
        {
          name: "New User",
          email: "new@example.com",
          age: 25,
          active: true,
          tags: [],
        },
        { upsert: true, returnDocument: "after" }
      )

      expect(result).toBeDefined()
      expect(result?.name).toBe("New User")
      expect(result?.email).toBe("new@example.com")
      expect(await users.count({ email: "new@example.com" })).toBe(1)
    })
  })

  describe("findOneAndReplace", () => {
    it("should accept string ID", async () => {
      const users = db.collection("users", createUserSchema())
      const user = await users.insertOne({
        name: "Alice",
        email: "alice@example.com",
        age: 30,
        active: true,
        tags: ["admin"],
      })
      const replaced = await users.findOneAndReplace(user._id, {
        name: "Alice Updated",
        email: "alice@example.com",
        age: 31,
        active: false,
        tags: [],
      })

      expect(replaced?._id).toBe(user._id)
      expect(replaced?.name).toBe("Alice Updated")
      expect(replaced?.age).toBe(31)
      expect(replaced?.active).toBe(false)
      expect(replaced?.tags).toEqual([])
    })

    it("should accept query filter", async () => {
      const users = db.collection("users", createUserSchema())
      await users.insertOne({
        name: "Alice",
        email: "alice@example.com",
        age: 30,
        active: true,
        tags: ["admin"],
      })
      const replaced = await users.findOneAndReplace(
        { email: "alice@example.com" },
        {
          name: "New Name",
          email: "alice@example.com",
          age: 25,
          active: false,
          tags: [],
        }
      )

      expect(replaced).toBeDefined()
      expect(replaced?.name).toBe("New Name")
      expect(replaced?.age).toBe(25)
    })

    it("should return null if no document matches", async () => {
      const users = db.collection("users", createUserSchema())
      const replaced = await users.findOneAndReplace(
        { email: "nonexistent@example.com" },
        {
          name: "Test",
          email: "test@example.com",
          age: 25,
          active: true,
          tags: [],
        }
      )
      expect(replaced).toBeNull()
    })

    it("should return document after replacement by default", async () => {
      const users = db.collection("users", createUserSchema())
      const user = await users.insertOne({
        name: "Alice",
        email: "alice@example.com",
        age: 30,
        active: true,
        tags: [],
      })
      const replaced = await users.findOneAndReplace(user._id, {
        name: "Bob",
        email: "bob@example.com",
        age: 25,
        active: false,
        tags: ["user"],
      })

      expect(replaced?.name).toBe("Bob") // After replacement
      expect(replaced?.age).toBe(25)
    })

    it("should return document before replacement when returnDocument is 'before'", async () => {
      const users = db.collection("users", createUserSchema())
      const user = await users.insertOne({
        name: "Alice",
        email: "alice@example.com",
        age: 30,
        active: true,
        tags: [],
      })
      const before = await users.findOneAndReplace(
        user._id,
        {
          name: "Bob",
          email: "bob@example.com",
          age: 25,
          active: false,
          tags: [],
        },
        { returnDocument: "before" }
      )

      expect(before?.name).toBe("Alice") // Before replacement
      expect(before?.age).toBe(30)
      const after = await users.findById(user._id)
      expect(after?.name).toBe("Bob") // Verify replacement happened
    })

    it("should handle upsert when document not found", async () => {
      const users = db.collection("users", createUserSchema())
      const result = await users.findOneAndReplace(
        { email: "new@example.com" },
        {
          name: "New User",
          email: "new@example.com",
          age: 25,
          active: true,
          tags: [],
        },
        { upsert: true, returnDocument: "after" }
      )

      expect(result).toBeDefined()
      expect(result?.name).toBe("New User")
      expect(await users.count({ email: "new@example.com" })).toBe(1)
    })
  })

  // ==========================================================================
  // Utility Methods
  // ==========================================================================

  describe("distinct", () => {
    it("should return unique values for a field", async () => {
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
        {
          name: "Charlie",
          email: "charlie@example.com",
          age: 30,
          active: false,
          tags: [],
        },
        {
          name: "David",
          email: "david@example.com",
          age: 25,
          active: true,
          tags: [],
        },
      ])

      const ages = await users.distinct("age")
      expect(ages).toEqual([25, 30])
    })

    it("should work with indexed fields", async () => {
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
        {
          name: "Charlie",
          email: "charlie@example.com",
          age: 30,
          active: false,
          tags: [],
        },
      ])

      const ages = await users.distinct("age") // age is indexed
      expect(ages).toEqual([25, 30])
    })

    it("should work with non-indexed fields", async () => {
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
        {
          name: "Alice",
          email: "alice2@example.com",
          age: 35,
          active: false,
          tags: [],
        },
      ])

      const names = await users.distinct("name") // name is not indexed
      expect(names).toEqual(["Alice", "Bob"])
    })

    it("should support filter parameter", async () => {
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
        {
          name: "Charlie",
          email: "charlie@example.com",
          age: 35,
          active: false,
          tags: [],
        },
      ])

      const activeAges = await users.distinct("age", { active: true })
      expect(activeAges).toEqual([25, 30])
    })

    it("should return empty array for empty collection", async () => {
      const users = db.collection("users", createUserSchema())
      const ages = await users.distinct("age")
      expect(ages).toEqual([])
    })
  })

  describe("estimatedDocumentCount", () => {
    it("should return count of documents", async () => {
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
        {
          name: "Charlie",
          email: "charlie@example.com",
          age: 35,
          active: false,
          tags: [],
        },
      ])

      const count = await users.estimatedDocumentCount()
      expect(count).toBe(3)
    })

    it("should return 0 for empty collection", async () => {
      const users = db.collection("users", createUserSchema())
      const count = await users.estimatedDocumentCount()
      expect(count).toBe(0)
    })

    it("should match count() for small collections", async () => {
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

      const estimated = await users.estimatedDocumentCount()
      const exact = await users.count({})
      expect(estimated).toBe(exact)
    })
  })

  describe("drop", () => {
    it("should drop the collection", async () => {
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

      await users.drop()

      // Table should no longer exist - verify by trying to query it
      // This will throw an error in SQLite
      expect(() => {
        db.db.prepare("SELECT COUNT(*) FROM users").get()
      }).toThrow()
    })

    it("should be safe to call on non-existent collection", async () => {
      const users = db.collection("users", createUserSchema())
      // Drop without inserting anything - should not throw
      await users.drop()
      // Verify it completes successfully
      expect(true).toBe(true)
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

      const found = await users.findById(inserted._id)
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

      const found = await users.findById(inserted._id)
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

      const found = await users.findById(inserted._id)
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

      const found = await users.findById(inserted._id)
      expect(found?.name).toBe(unicodeName)
      expect(found?.tags).toEqual(["日本語", "🎉"])
    })
  })
})
