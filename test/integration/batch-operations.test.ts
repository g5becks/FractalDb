/**
 * Integration tests for batch operations (insertMany, updateMany, deleteMany).
 *
 * These tests verify that batch operations maintain data integrity,
 * handle errors correctly, and provide detailed results.
 */

import { afterEach, beforeEach, describe, expect, it } from "bun:test"
import { createSchema, type Document, StrataDBClass } from "../../src/index.js"

// ============================================================================
// Test Document Types
// ============================================================================

type User = Document<{
  name: string
  email: string
  age: number
  status: string
}>

// ============================================================================
// Test Setup
// ============================================================================

describe("Batch Operations", () => {
  let db: StrataDBClass

  beforeEach(() => {
    db = new StrataDBClass({ database: ":memory:" })
  })

  afterEach(() => {
    db.close()
  })

  const createUserSchema = () =>
    createSchema<User>()
      .field("name", { type: "TEXT", indexed: true })
      .field("email", { type: "TEXT", indexed: true, unique: true })
      .field("age", { type: "INTEGER", indexed: true })
      .field("status", { type: "TEXT", indexed: true })
      .build()

  // ==========================================================================
  // insertMany Tests
  // ==========================================================================

  describe("insertMany", () => {
    it("should insert multiple documents successfully", async () => {
      const users = db.collection("users", createUserSchema())

      const result = await users.insertMany([
        {
          name: "Alice",
          email: "alice@example.com",
          age: 30,
          status: "active",
        },
        { name: "Bob", email: "bob@example.com", age: 25, status: "active" },
        {
          name: "Charlie",
          email: "charlie@example.com",
          age: 35,
          status: "inactive",
        },
      ])

      expect(result.insertedCount).toBe(3)
      expect(result.documents.length).toBe(3)
    })

    it("should generate unique IDs for each document", async () => {
      const users = db.collection("users", createUserSchema())

      const result = await users.insertMany([
        {
          name: "Alice",
          email: "alice@example.com",
          age: 30,
          status: "active",
        },
        { name: "Bob", email: "bob@example.com", age: 25, status: "active" },
      ])

      const ids = result.documents.map((d) => d._id)
      expect(new Set(ids).size).toBe(2) // All IDs are unique
    })

    it("should verify documents are retrievable after insertMany", async () => {
      const users = db.collection("users", createUserSchema())

      const result = await users.insertMany([
        {
          name: "Alice",
          email: "alice@example.com",
          age: 30,
          status: "active",
        },
        { name: "Bob", email: "bob@example.com", age: 25, status: "active" },
      ])

      for (const doc of result.documents) {
        const retrieved = await users.findById(doc._id)
        expect(retrieved).not.toBeNull()
        expect(retrieved?.name).toBe(doc.name)
      }
    })

    describe("ordered insertMany (default)", () => {
      it("should stop at first error when ordered is true", async () => {
        const users = db.collection("users", createUserSchema())

        // Insert first document
        await users.insertOne({
          name: "Existing",
          email: "duplicate@example.com",
          age: 20,
          status: "active",
        })

        // Try to insert batch with duplicate in the middle
        const result = await users.insertMany([
          {
            name: "Alice",
            email: "alice@example.com",
            age: 30,
            status: "active",
          },
          {
            name: "Duplicate",
            email: "duplicate@example.com",
            age: 25,
            status: "active",
          }, // Will fail
          {
            name: "Charlie",
            email: "charlie@example.com",
            age: 35,
            status: "active",
          }, // Won't be attempted
        ])

        // With ordered=true (default), stops at first error
        // Only Alice should be inserted
        expect(result.insertedCount).toBe(1)
        expect(result.documents.length).toBe(1)
        expect(result.documents[0].name).toBe("Alice")
      })
    })

    describe("unordered insertMany", () => {
      it("should continue after errors when ordered is false", async () => {
        const users = db.collection("users", createUserSchema())

        // Insert first document
        await users.insertOne({
          name: "Existing",
          email: "duplicate@example.com",
          age: 20,
          status: "active",
        })

        // Try to insert batch with duplicate in the middle
        const result = await users.insertMany(
          [
            {
              name: "Alice",
              email: "alice@example.com",
              age: 30,
              status: "active",
            },
            {
              name: "Duplicate",
              email: "duplicate@example.com",
              age: 25,
              status: "active",
            }, // Will fail
            {
              name: "Charlie",
              email: "charlie@example.com",
              age: 35,
              status: "active",
            }, // Should be inserted
          ],
          { ordered: false }
        )

        // With ordered=false, continues after error
        // Alice and Charlie should be inserted
        expect(result.insertedCount).toBe(2)
        expect(result.documents.length).toBe(2)

        const names = result.documents.map((d) => d.name)
        expect(names).toContain("Alice")
        expect(names).toContain("Charlie")
      })

      it("should handle multiple errors when ordered is false", async () => {
        const users = db.collection("users", createUserSchema())

        // Insert documents that will conflict
        await users.insertOne({
          name: "Existing1",
          email: "dup1@example.com",
          age: 20,
          status: "active",
        })
        await users.insertOne({
          name: "Existing2",
          email: "dup2@example.com",
          age: 21,
          status: "active",
        })

        // Try to insert batch with multiple duplicates
        const result = await users.insertMany(
          [
            {
              name: "Valid1",
              email: "valid1@example.com",
              age: 30,
              status: "active",
            },
            {
              name: "Dup1",
              email: "dup1@example.com",
              age: 25,
              status: "active",
            }, // Fail
            {
              name: "Valid2",
              email: "valid2@example.com",
              age: 35,
              status: "active",
            },
            {
              name: "Dup2",
              email: "dup2@example.com",
              age: 28,
              status: "active",
            }, // Fail
            {
              name: "Valid3",
              email: "valid3@example.com",
              age: 40,
              status: "active",
            },
          ],
          { ordered: false }
        )

        // Should insert 3 valid documents
        expect(result.insertedCount).toBe(3)
        expect(result.documents.length).toBe(3)
      })
    })

    it("should handle empty array gracefully", async () => {
      const users = db.collection("users", createUserSchema())

      const result = await users.insertMany([])

      expect(result.insertedCount).toBe(0)
      expect(result.documents.length).toBe(0)
    })

    it("should handle large batch insert", async () => {
      const users = db.collection("users", createUserSchema())

      const docs = Array.from({ length: 100 }, (_, i) => ({
        name: `User${i}`,
        email: `user${i}@example.com`,
        age: 20 + (i % 50),
        status: i % 2 === 0 ? "active" : "inactive",
      }))

      const result = await users.insertMany(docs)

      expect(result.insertedCount).toBe(100)

      const count = await users.count({})
      expect(count).toBe(100)
    })
  })

  // ==========================================================================
  // updateMany Tests
  // ==========================================================================

  describe("updateMany", () => {
    beforeEach(async () => {
      const users = db.collection("users", createUserSchema())

      await users.insertMany([
        {
          name: "Alice",
          email: "alice@example.com",
          age: 30,
          status: "active",
        },
        { name: "Bob", email: "bob@example.com", age: 25, status: "active" },
        {
          name: "Charlie",
          email: "charlie@example.com",
          age: 35,
          status: "inactive",
        },
        {
          name: "Diana",
          email: "diana@example.com",
          age: 28,
          status: "active",
        },
        { name: "Eve", email: "eve@example.com", age: 45, status: "inactive" },
      ])
    })

    it("should update all matching documents", async () => {
      const users = db.collection("users", createUserSchema())

      const result = await users.updateMany(
        { status: "active" },
        { status: "pending" }
      )

      expect(result.matchedCount).toBe(3)
      expect(result.modifiedCount).toBe(3)

      // Verify updates
      const pending = await users.find({ status: "pending" })
      expect(pending.length).toBe(3)
    })

    it("should return zero counts when no documents match", async () => {
      const users = db.collection("users", createUserSchema())

      const result = await users.updateMany(
        { status: "nonexistent" },
        { status: "updated" }
      )

      expect(result.matchedCount).toBe(0)
      expect(result.modifiedCount).toBe(0)
    })

    it("should update documents with complex filter", async () => {
      const users = db.collection("users", createUserSchema())

      const result = await users.updateMany(
        { $and: [{ status: "active" }, { age: { $gte: 28 } }] },
        { status: "senior" }
      )

      // Alice (30, active) and Diana (28, active) match
      expect(result.modifiedCount).toBe(2)

      const seniors = await users.find({ status: "senior" })
      expect(seniors.length).toBe(2)
    })

    it("should preserve non-updated fields", async () => {
      const users = db.collection("users", createUserSchema())

      await users.updateMany({ name: "Alice" }, { status: "updated" })

      const alice = await users.findOne({ name: "Alice" })
      expect(alice?.name).toBe("Alice")
      expect(alice?.email).toBe("alice@example.com")
      expect(alice?.age).toBe(30)
      expect(alice?.status).toBe("updated")
    })

    it("should update all documents with empty filter", async () => {
      const users = db.collection("users", createUserSchema())

      const result = await users.updateMany({}, { status: "archived" })

      expect(result.modifiedCount).toBe(5)

      const archived = await users.find({ status: "archived" })
      expect(archived.length).toBe(5)
    })
  })

  // ==========================================================================
  // deleteMany Tests
  // ==========================================================================

  describe("deleteMany", () => {
    beforeEach(async () => {
      const users = db.collection("users", createUserSchema())

      await users.insertMany([
        {
          name: "Alice",
          email: "alice@example.com",
          age: 30,
          status: "active",
        },
        { name: "Bob", email: "bob@example.com", age: 25, status: "active" },
        {
          name: "Charlie",
          email: "charlie@example.com",
          age: 35,
          status: "inactive",
        },
        {
          name: "Diana",
          email: "diana@example.com",
          age: 28,
          status: "active",
        },
        { name: "Eve", email: "eve@example.com", age: 45, status: "inactive" },
      ])
    })

    it("should delete all matching documents", async () => {
      const users = db.collection("users", createUserSchema())

      const result = await users.deleteMany({ status: "inactive" })

      expect(result.deletedCount).toBe(2)

      // Verify deletions
      const remaining = await users.find({})
      expect(remaining.length).toBe(3)

      const inactive = await users.find({ status: "inactive" })
      expect(inactive.length).toBe(0)
    })

    it("should return zero count when no documents match", async () => {
      const users = db.collection("users", createUserSchema())

      const result = await users.deleteMany({ status: "nonexistent" })

      expect(result.deletedCount).toBe(0)
    })

    it("should delete documents with complex filter", async () => {
      const users = db.collection("users", createUserSchema())

      const result = await users.deleteMany({
        $or: [{ age: { $lt: 26 } }, { age: { $gt: 40 } }],
      })

      // Bob (25) and Eve (45) match
      expect(result.deletedCount).toBe(2)

      const remaining = await users.find({})
      expect(remaining.length).toBe(3)
    })

    it("should delete all documents with empty filter", async () => {
      const users = db.collection("users", createUserSchema())

      const result = await users.deleteMany({})

      expect(result.deletedCount).toBe(5)

      const remaining = await users.find({})
      expect(remaining.length).toBe(0)
    })

    it("should handle deletion with comparison operators", async () => {
      const users = db.collection("users", createUserSchema())

      const result = await users.deleteMany({ age: { $gte: 30 } })

      // Alice (30), Charlie (35), Eve (45) match
      expect(result.deletedCount).toBe(3)

      const remaining = await users.find({})
      expect(remaining.length).toBe(2)

      for (const user of remaining) {
        expect(user.age).toBeLessThan(30)
      }
    })
  })

  // ==========================================================================
  // Atomicity Tests
  // ==========================================================================

  describe("atomicity", () => {
    it("should maintain data integrity after batch operations", async () => {
      const users = db.collection("users", createUserSchema())

      // Insert batch
      await users.insertMany([
        {
          name: "Alice",
          email: "alice@example.com",
          age: 30,
          status: "active",
        },
        { name: "Bob", email: "bob@example.com", age: 25, status: "active" },
        {
          name: "Charlie",
          email: "charlie@example.com",
          age: 35,
          status: "inactive",
        },
      ])

      // Update batch
      await users.updateMany({ status: "active" }, { status: "updated" })

      // Delete batch
      await users.deleteMany({ status: "inactive" })

      // Verify final state
      const all = await users.find({})
      expect(all.length).toBe(2)

      for (const user of all) {
        expect(user.status).toBe("updated")
      }
    })

    it("should handle concurrent-like operations correctly", async () => {
      const users = db.collection("users", createUserSchema())

      // Insert many documents
      const docs = Array.from({ length: 50 }, (_, i) => ({
        name: `User${i}`,
        email: `user${i}@example.com`,
        age: 20 + i,
        status: "initial",
      }))

      await users.insertMany(docs)

      // Run multiple batch operations
      await users.updateMany({ age: { $lt: 30 } }, { status: "young" })
      await users.updateMany(
        { age: { $gte: 30, $lt: 50 } },
        { status: "middle" }
      )
      await users.updateMany({ age: { $gte: 50 } }, { status: "senior" })

      // Verify all documents have correct status
      const young = await users.count({ status: "young" })
      const middle = await users.count({ status: "middle" })
      const senior = await users.count({ status: "senior" })
      const initial = await users.count({ status: "initial" })

      expect(young).toBe(10) // ages 20-29
      expect(middle).toBe(20) // ages 30-49
      expect(senior).toBe(20) // ages 50-69
      expect(initial).toBe(0) // none should remain
    })
  })
})
