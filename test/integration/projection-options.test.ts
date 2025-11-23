/**
 * Integration tests for select/omit projection options.
 *
 * @remarks
 * These tests verify that the select and omit projection options work
 * correctly through the full StrataDB API with real database operations.
 */

import { afterEach, beforeEach, describe, expect, it } from "bun:test"
import { createSchema, type Document, Strata } from "../../src/index.js"

// ============================================================================
// Test Document Types
// ============================================================================

/**
 * User document for testing projection options.
 */
type User = Document<{
  name: string
  email: string
  password: string
  age: number
  role: "admin" | "user" | "guest"
  profile: {
    bio: string
    avatar: string
  }
}>

// ============================================================================
// Test Setup
// ============================================================================

describe("Projection Options Integration", () => {
  let db: Strata

  const createUserSchema = () =>
    createSchema<User>()
      .field("name", { type: "TEXT", indexed: true })
      .field("email", { type: "TEXT", indexed: true, unique: true })
      .field("password", { type: "TEXT", indexed: false })
      .field("age", { type: "INTEGER", indexed: true })
      .field("role", { type: "TEXT", indexed: true })
      .build()

  beforeEach(async () => {
    db = new Strata({ database: ":memory:" })
    const users = db.collection("users", createUserSchema())

    // Seed test data
    await users.insertMany([
      {
        name: "Alice Smith",
        email: "alice@example.com",
        password: "secret123",
        age: 30,
        role: "admin",
        profile: { bio: "Software engineer", avatar: "alice.png" },
      },
      {
        name: "Bob Wilson",
        email: "bob@example.com",
        password: "password456",
        age: 25,
        role: "user",
        profile: { bio: "Product manager", avatar: "bob.png" },
      },
      {
        name: "Charlie Brown",
        email: "charlie@example.com",
        password: "mypassword",
        age: 35,
        role: "user",
        profile: { bio: "Designer", avatar: "charlie.png" },
      },
      {
        name: "Diana Prince",
        email: "diana@example.com",
        password: "wonderwoman",
        age: 28,
        role: "admin",
        profile: { bio: "CTO", avatar: "diana.png" },
      },
    ])
  })

  afterEach(() => {
    db.close()
  })

  // ==========================================================================
  // SELECT TESTS
  // ==========================================================================

  describe("select option", () => {
    it("should return only selected fields for public API", async () => {
      const users = db.collection("users", createUserSchema())

      // Public API - don't return password
      const results = await users.find(
        {},
        { select: ["name", "email", "role"] }
      )

      expect(results).toHaveLength(4)
      for (const user of results) {
        expect("_id" in user).toBe(true)
        expect("name" in user).toBe(true)
        expect("email" in user).toBe(true)
        expect("role" in user).toBe(true)
        // Sensitive/unnecessary fields excluded
        expect("password" in user).toBe(false)
        expect("age" in user).toBe(false)
        expect("profile" in user).toBe(false)
      }
    })

    it("should work with filters and select", async () => {
      const users = db.collection("users", createUserSchema())

      const admins = await users.find(
        { role: "admin" },
        { select: ["name", "email"] }
      )

      expect(admins).toHaveLength(2)
      const names = admins.map((u) => u.name).sort()
      expect(names).toEqual(["Alice Smith", "Diana Prince"])
      expect("password" in admins[0]).toBe(false)
    })

    it("should work with sort, limit, and select", async () => {
      const users = db.collection("users", createUserSchema())

      const results = await users.find(
        {},
        {
          select: ["name", "age"],
          sort: { age: -1 },
          limit: 2,
        }
      )

      expect(results).toHaveLength(2)
      // Oldest users first
      expect(results[0].name).toBe("Charlie Brown")
      expect(results[0].age).toBe(35)
      expect(results[1].name).toBe("Alice Smith")
      expect(results[1].age).toBe(30)
      expect("email" in results[0]).toBe(false)
    })

    it("should work with complex query operators and select", async () => {
      const users = db.collection("users", createUserSchema())

      const results = await users.find(
        {
          $or: [{ age: { $gte: 30 } }, { role: "admin" }],
        },
        { select: ["name", "role", "age"] }
      )

      expect(results).toHaveLength(3) // Alice (admin, 30), Charlie (30+), Diana (admin)
      for (const user of results) {
        expect("name" in user).toBe(true)
        expect("role" in user).toBe(true)
        expect("age" in user).toBe(true)
        expect("email" in user).toBe(false)
        expect("password" in user).toBe(false)
      }
    })

    it("should work with string operators and select", async () => {
      const users = db.collection("users", createUserSchema())

      const results = await users.find(
        { email: { $contains: "@example" } },
        { select: ["name", "email"] }
      )

      expect(results).toHaveLength(4)
      for (const user of results) {
        expect(user.email).toContain("@example")
        expect("password" in user).toBe(false)
      }
    })
  })

  // ==========================================================================
  // OMIT TESTS
  // ==========================================================================

  describe("omit option", () => {
    it("should exclude sensitive fields using omit", async () => {
      const users = db.collection("users", createUserSchema())

      // Remove password from all results
      const results = await users.find({}, { omit: ["password"] })

      expect(results).toHaveLength(4)
      for (const user of results) {
        expect("_id" in user).toBe(true)
        expect("name" in user).toBe(true)
        expect("email" in user).toBe(true)
        expect("age" in user).toBe(true)
        expect("role" in user).toBe(true)
        expect("profile" in user).toBe(true)
        // Password excluded
        expect("password" in user).toBe(false)
      }
    })

    it("should omit multiple fields", async () => {
      const users = db.collection("users", createUserSchema())

      const results = await users.find(
        {},
        { omit: ["password", "profile", "age"] }
      )

      expect(results).toHaveLength(4)
      for (const user of results) {
        expect("name" in user).toBe(true)
        expect("email" in user).toBe(true)
        expect("role" in user).toBe(true)
        expect("password" in user).toBe(false)
        expect("profile" in user).toBe(false)
        expect("age" in user).toBe(false)
      }
    })

    it("should work with filters and omit", async () => {
      const users = db.collection("users", createUserSchema())

      const youngUsers = await users.find(
        { age: { $lt: 30 } },
        { omit: ["password"] }
      )

      expect(youngUsers).toHaveLength(2) // Bob (25) and Diana (28)
      for (const user of youngUsers) {
        expect(user.age).toBeLessThan(30)
        expect("password" in user).toBe(false)
        expect("email" in user).toBe(true)
      }
    })

    it("should work with sort, limit, skip, and omit", async () => {
      const users = db.collection("users", createUserSchema())

      const results = await users.find(
        {},
        {
          omit: ["password", "profile"],
          sort: { name: 1 },
          limit: 2,
          skip: 1,
        }
      )

      expect(results).toHaveLength(2)
      // Alphabetical: Alice, Bob, Charlie, Diana - skip Alice, take Bob and Charlie
      expect(results[0].name).toBe("Bob Wilson")
      expect(results[1].name).toBe("Charlie Brown")
      expect("password" in results[0]).toBe(false)
    })
  })

  // ==========================================================================
  // PROJECTION TESTS (MongoDB-style)
  // ==========================================================================

  describe("projection option (MongoDB-style)", () => {
    it("should include fields with projection: { field: 1 }", async () => {
      const users = db.collection("users", createUserSchema())

      const results = await users.find(
        {},
        { projection: { name: 1, email: 1 } }
      )

      expect(results).toHaveLength(4)
      for (const user of results) {
        expect("_id" in user).toBe(true)
        expect("name" in user).toBe(true)
        expect("email" in user).toBe(true)
        expect("password" in user).toBe(false)
        expect("age" in user).toBe(false)
      }
    })

    it("should exclude fields with projection: { field: 0 }", async () => {
      const users = db.collection("users", createUserSchema())

      const results = await users.find({}, { projection: { password: 0 } })

      expect(results).toHaveLength(4)
      for (const user of results) {
        expect("password" in user).toBe(false)
        expect("name" in user).toBe(true)
        expect("email" in user).toBe(true)
      }
    })

    it("should exclude _id with projection: { _id: 0 }", async () => {
      const users = db.collection("users", createUserSchema())

      const results = await users.find(
        {},
        { projection: { _id: 0, name: 1, email: 1 } }
      )

      expect(results).toHaveLength(4)
      for (const user of results) {
        expect("_id" in user).toBe(false)
        expect("name" in user).toBe(true)
        expect("email" in user).toBe(true)
      }
    })
  })

  // ==========================================================================
  // FINDONE WITH PROJECTION
  // ==========================================================================

  describe("findOne with projection options", () => {
    it("should apply select to findOne", async () => {
      const users = db.collection("users", createUserSchema())

      const user = await users.findOne(
        { email: "alice@example.com" },
        { select: ["name", "role"] }
      )

      expect(user).not.toBeNull()
      expect(user?.name).toBe("Alice Smith")
      expect(user?.role).toBe("admin")
      expect("email" in (user ?? {})).toBe(false)
      expect("password" in (user ?? {})).toBe(false)
    })

    it("should apply omit to findOne", async () => {
      const users = db.collection("users", createUserSchema())

      const user = await users.findOne(
        { name: "Bob Wilson" },
        { omit: ["password", "profile"] }
      )

      expect(user).not.toBeNull()
      expect(user?.name).toBe("Bob Wilson")
      expect(user?.email).toBe("bob@example.com")
      expect("password" in (user ?? {})).toBe(false)
      expect("profile" in (user ?? {})).toBe(false)
    })

    it("should apply projection to findOne", async () => {
      const users = db.collection("users", createUserSchema())

      const user = await users.findOne(
        { role: "admin" },
        {
          projection: { name: 1, email: 1 },
          sort: { name: 1 },
        }
      )

      expect(user).not.toBeNull()
      expect(user?.name).toBe("Alice Smith") // First alphabetically
      expect("password" in (user ?? {})).toBe(false)
    })

    it("should return null when no match with projection", async () => {
      const users = db.collection("users", createUserSchema())

      const user = await users.findOne(
        { name: "NonExistent" },
        { select: ["name"] }
      )

      expect(user).toBeNull()
    })
  })

  // ==========================================================================
  // REAL-WORLD USE CASES
  // ==========================================================================

  describe("real-world use cases", () => {
    it("should handle user list API (no passwords)", async () => {
      const users = db.collection("users", createUserSchema())

      // API endpoint: GET /users
      const userList = await users.find(
        {},
        {
          select: ["name", "email", "role"],
          sort: { name: 1 },
        }
      )

      expect(userList).toHaveLength(4)
      // Verify no sensitive data
      for (const user of userList) {
        expect("password" in user).toBe(false)
        expect("profile" in user).toBe(false)
      }
    })

    it("should handle user profile API (with nested data)", async () => {
      const users = db.collection("users", createUserSchema())

      // API endpoint: GET /users/:id/profile
      const profile = await users.findOne(
        { email: "alice@example.com" },
        { select: ["name", "email", "profile", "role"] }
      )

      expect(profile).not.toBeNull()
      expect(profile?.name).toBe("Alice Smith")
      expect(profile?.profile.bio).toBe("Software engineer")
      expect("password" in (profile ?? {})).toBe(false)
    })

    it("should handle admin dashboard (all data except passwords)", async () => {
      const users = db.collection("users", createUserSchema())

      // Admin dashboard needs most data but not passwords
      const adminView = await users.find({}, { omit: ["password"] })

      expect(adminView).toHaveLength(4)
      for (const user of adminView) {
        expect("password" in user).toBe(false)
        // All other fields present
        expect("name" in user).toBe(true)
        expect("email" in user).toBe(true)
        expect("age" in user).toBe(true)
        expect("role" in user).toBe(true)
        expect("profile" in user).toBe(true)
      }
    })

    it("should handle search with minimal fields", async () => {
      const users = db.collection("users", createUserSchema())

      // Search autocomplete - only need name and id
      const searchResults = await users.find(
        { name: { $ilike: "%al%" } },
        { select: ["name"] }
      )

      expect(searchResults.length).toBeGreaterThan(0)
      for (const result of searchResults) {
        expect("_id" in result).toBe(true)
        expect("name" in result).toBe(true)
        expect(Object.keys(result).length).toBe(2) // only _id and name
      }
    })
  })
})
