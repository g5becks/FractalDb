import { afterEach, beforeEach, describe, expect, it } from "bun:test"
import type { Document } from "../../src/core-types.js"
import { ValidationError } from "../../src/errors.js"
import { createSchema } from "../../src/schema-builder.js"
import { StrataDBClass } from "../../src/stratadb.js"

describe("Collection Validation Methods", () => {
  let db: StrataDBClass

  beforeEach(() => {
    db = new StrataDBClass({ database: ":memory:" })
  })

  afterEach(() => {
    db.close()
  })

  // Test type for validation
  type User = Document<{
    name: string
    email: string
    age: number
  }>

  // Helper function to create a validator
  const createUserValidator =
    (minAge = 18) =>
    (doc: unknown): doc is User => {
      if (typeof doc !== "object" || doc === null) {
        return false
      }
      const obj = doc as Record<string, unknown>

      return (
        typeof obj.name === "string" &&
        obj.name.length > 0 &&
        typeof obj.email === "string" &&
        obj.email.includes("@") &&
        typeof obj.age === "number" &&
        obj.age >= minAge
      )
    }

  describe("validate (async)", () => {
    it("should validate and return typed document when valid", async () => {
      const schema = createSchema<User>()
        .field("name", { type: "TEXT", indexed: true })
        .field("email", { type: "TEXT" })
        .field("age", { type: "INTEGER" })
        .validate(createUserValidator())
        .build()

      const users = db.collection("users", schema)

      const validData: unknown = {
        name: "Alice",
        email: "alice@example.com",
        age: 30,
      }

      const result = await users.validate(validData)

      // Type should be narrowed to User
      expect(result.name).toBe("Alice")
      expect(result.email).toBe("alice@example.com")
      expect(result.age).toBe(30)
    })

    it("should throw ValidationError when document is invalid", async () => {
      const schema = createSchema<User>()
        .field("name", { type: "TEXT" })
        .field("email", { type: "TEXT" })
        .field("age", { type: "INTEGER" })
        .validate(createUserValidator())
        .build()

      const users = db.collection("users", schema)

      const invalidData: unknown = {
        name: "Bob",
        email: "not-an-email", // Missing @
        age: 25,
      }

      try {
        await users.validate(invalidData)
        expect.unreachable("Should have thrown ValidationError")
      } catch (error) {
        expect(error).toBeInstanceOf(ValidationError)
        expect((error as ValidationError).message).toBe(
          "Document validation failed"
        )
      }
    })

    it("should throw ValidationError for missing required fields", async () => {
      const schema = createSchema<User>()
        .field("name", { type: "TEXT" })
        .field("email", { type: "TEXT" })
        .field("age", { type: "INTEGER" })
        .validate(createUserValidator())
        .build()

      const users = db.collection("users", schema)

      const incompleteData: unknown = {
        name: "Charlie",
        // Missing email and age
      }

      try {
        await users.validate(incompleteData)
        expect.unreachable("Should have thrown ValidationError")
      } catch (error) {
        expect(error).toBeInstanceOf(ValidationError)
      }
    })

    it("should throw ValidationError for invalid types", async () => {
      const schema = createSchema<User>()
        .field("name", { type: "TEXT" })
        .field("email", { type: "TEXT" })
        .field("age", { type: "INTEGER" })
        .validate(createUserValidator())
        .build()

      const users = db.collection("users", schema)

      const wrongTypes: unknown = {
        name: 123, // Should be string
        email: "valid@example.com",
        age: "30", // Should be number
      }

      try {
        await users.validate(wrongTypes)
        expect.unreachable("Should have thrown ValidationError")
      } catch (error) {
        expect(error).toBeInstanceOf(ValidationError)
      }
    })

    it("should accept document when no validator is configured", async () => {
      const schema = createSchema<User>()
        .field("name", { type: "TEXT" })
        .field("email", { type: "TEXT" })
        .field("age", { type: "INTEGER" })
        .build() // No validator

      const users = db.collection("users", schema)

      const anyData: unknown = {
        name: "Dave",
        email: "not-validated",
        age: "not-a-number", // Would fail with validator
      }

      // Should not throw since no validator
      const result = await users.validate(anyData)
      expect(result).toBeDefined()
    })

    it("should throw ValidationError for null input", async () => {
      const schema = createSchema<User>()
        .field("name", { type: "TEXT" })
        .field("email", { type: "TEXT" })
        .field("age", { type: "INTEGER" })
        .validate(createUserValidator())
        .build()

      const users = db.collection("users", schema)

      try {
        await users.validate(null)
        expect.unreachable("Should have thrown ValidationError")
      } catch (error) {
        expect(error).toBeInstanceOf(ValidationError)
      }
    })

    it("should throw ValidationError for undefined input", async () => {
      const schema = createSchema<User>()
        .field("name", { type: "TEXT" })
        .field("email", { type: "TEXT" })
        .field("age", { type: "INTEGER" })
        .validate(createUserValidator())
        .build()

      const users = db.collection("users", schema)

      try {
        await users.validate(undefined)
        expect.unreachable("Should have thrown ValidationError")
      } catch (error) {
        expect(error).toBeInstanceOf(ValidationError)
      }
    })

    it("should throw ValidationError for primitive input", async () => {
      const schema = createSchema<User>()
        .field("name", { type: "TEXT" })
        .field("email", { type: "TEXT" })
        .field("age", { type: "INTEGER" })
        .validate(createUserValidator())
        .build()

      const users = db.collection("users", schema)

      try {
        await users.validate("string")
        expect.unreachable("Should have thrown ValidationError for string")
      } catch (error) {
        expect(error).toBeInstanceOf(ValidationError)
      }

      try {
        await users.validate(123)
        expect.unreachable("Should have thrown ValidationError for number")
      } catch (error) {
        expect(error).toBeInstanceOf(ValidationError)
      }

      try {
        await users.validate(true)
        expect.unreachable("Should have thrown ValidationError for boolean")
      } catch (error) {
        expect(error).toBeInstanceOf(ValidationError)
      }
    })

    it("should validate custom business rules", async () => {
      // Validator requires age >= 21
      const schema = createSchema<User>()
        .field("name", { type: "TEXT" })
        .field("email", { type: "TEXT" })
        .field("age", { type: "INTEGER" })
        .validate(createUserValidator(21))
        .build()

      const users = db.collection("users", schema)

      const youngUser: unknown = {
        name: "Eve",
        email: "eve@example.com",
        age: 20, // Less than 21
      }

      try {
        await users.validate(youngUser)
        expect.unreachable("Should have thrown ValidationError for age < 21")
      } catch (error) {
        expect(error).toBeInstanceOf(ValidationError)
      }

      const oldEnoughUser: unknown = {
        name: "Frank",
        email: "frank@example.com",
        age: 21,
      }

      const result = await users.validate(oldEnoughUser)
      expect(result.age).toBe(21)
    })
  })

  describe("validateSync (synchronous)", () => {
    it("should validate and return typed document when valid", () => {
      const schema = createSchema<User>()
        .field("name", { type: "TEXT" })
        .field("email", { type: "TEXT" })
        .field("age", { type: "INTEGER" })
        .validate(createUserValidator())
        .build()

      const users = db.collection("users", schema)

      const validData: unknown = {
        name: "George",
        email: "george@example.com",
        age: 35,
      }

      const result = users.validateSync(validData)

      expect(result.name).toBe("George")
      expect(result.email).toBe("george@example.com")
      expect(result.age).toBe(35)
    })

    it("should throw ValidationError when document is invalid", () => {
      const schema = createSchema<User>()
        .field("name", { type: "TEXT" })
        .field("email", { type: "TEXT" })
        .field("age", { type: "INTEGER" })
        .validate(createUserValidator())
        .build()

      const users = db.collection("users", schema)

      const invalidData: unknown = {
        name: "", // Empty name
        email: "helen@example.com",
        age: 28,
      }

      expect(() => users.validateSync(invalidData)).toThrow(ValidationError)
      expect(() => users.validateSync(invalidData)).toThrow(
        "Document validation failed"
      )
    })

    it("should throw ValidationError for missing required fields", () => {
      const schema = createSchema<User>()
        .field("name", { type: "TEXT" })
        .field("email", { type: "TEXT" })
        .field("age", { type: "INTEGER" })
        .validate(createUserValidator())
        .build()

      const users = db.collection("users", schema)

      const incompleteData: unknown = {
        name: "Ian",
        email: "ian@example.com",
        // Missing age
      }

      expect(() => users.validateSync(incompleteData)).toThrow(ValidationError)
    })

    it("should accept document when no validator is configured", () => {
      const schema = createSchema<User>()
        .field("name", { type: "TEXT" })
        .field("email", { type: "TEXT" })
        .field("age", { type: "INTEGER" })
        .build() // No validator

      const users = db.collection("users", schema)

      const anyData: unknown = {
        name: "Jane",
        email: "invalid-email",
        age: -5, // Would fail with validator
      }

      // Should not throw since no validator
      const result = users.validateSync(anyData)
      expect(result).toBeDefined()
    })

    it("should throw ValidationError for null input", () => {
      const schema = createSchema<User>()
        .field("name", { type: "TEXT" })
        .field("email", { type: "TEXT" })
        .field("age", { type: "INTEGER" })
        .validate(createUserValidator())
        .build()

      const users = db.collection("users", schema)

      expect(() => users.validateSync(null)).toThrow(ValidationError)
    })

    it("should throw ValidationError for primitive input", () => {
      const schema = createSchema<User>()
        .field("name", { type: "TEXT" })
        .field("email", { type: "TEXT" })
        .field("age", { type: "INTEGER" })
        .validate(createUserValidator())
        .build()

      const users = db.collection("users", schema)

      expect(() => users.validateSync("string")).toThrow(ValidationError)
      expect(() => users.validateSync(123)).toThrow(ValidationError)
      expect(() => users.validateSync(false)).toThrow(ValidationError)
    })

    it("should validate arrays correctly", () => {
      const schema = createSchema<User>()
        .field("name", { type: "TEXT" })
        .field("email", { type: "TEXT" })
        .field("age", { type: "INTEGER" })
        .validate(createUserValidator())
        .build()

      const users = db.collection("users", schema)

      // Arrays should fail validation
      expect(() => users.validateSync([])).toThrow(ValidationError)
      expect(() =>
        users.validateSync([
          { name: "Kate", email: "kate@example.com", age: 30 },
        ])
      ).toThrow(ValidationError)
    })
  })

  describe("type narrowing", () => {
    it("validate should narrow unknown to T", async () => {
      const schema = createSchema<User>()
        .field("name", { type: "TEXT" })
        .field("email", { type: "TEXT" })
        .field("age", { type: "INTEGER" })
        .validate(createUserValidator())
        .build()

      const users = db.collection("users", schema)

      const unknownData: unknown = {
        name: "Laura",
        email: "laura@example.com",
        age: 42,
      }

      // After validation, TypeScript knows this is User
      const user = await users.validate(unknownData)

      // These should all be accessible with correct types
      const name: string = user.name
      const email: string = user.email
      const age: number = user.age

      expect(name).toBe("Laura")
      expect(email).toBe("laura@example.com")
      expect(age).toBe(42)
    })

    it("validateSync should narrow unknown to T", () => {
      const schema = createSchema<User>()
        .field("name", { type: "TEXT" })
        .field("email", { type: "TEXT" })
        .field("age", { type: "INTEGER" })
        .validate(createUserValidator())
        .build()

      const users = db.collection("users", schema)

      const unknownData: unknown = {
        name: "Mike",
        email: "mike@example.com",
        age: 55,
      }

      // After validation, TypeScript knows this is User
      const user = users.validateSync(unknownData)

      // These should all be accessible with correct types
      const name: string = user.name
      const email: string = user.email
      const age: number = user.age

      expect(name).toBe("Mike")
      expect(email).toBe("mike@example.com")
      expect(age).toBe(55)
    })
  })
})
