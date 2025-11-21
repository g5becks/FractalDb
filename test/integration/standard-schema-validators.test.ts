import { afterEach, beforeEach, describe, expect, it } from "bun:test"
import { type } from "arktype"
import {
  email,
  integer,
  minLength,
  minValue,
  number,
  object,
  pipe,
  string,
} from "valibot"
import { z } from "zod"
import type { Document } from "../../src/core-types.js"
import { ValidationError } from "../../src/errors.js"
import { createSchema } from "../../src/schema-builder.js"
import { StrataDBClass } from "../../src/stratadb.js"
import { wrapStandardSchema } from "../../src/validator.js"

describe("Standard Schema Validator Integration Tests", () => {
  let db: StrataDBClass

  beforeEach(() => {
    db = new StrataDBClass({ database: ":memory:" })
  })

  afterEach(() => {
    db.close()
  })

  // Test type for validators
  type User = Document<{
    name: string
    email: string
    age: number
  }>

  describe("Zod validator integration", () => {
    it("should successfully validate valid documents with Zod", async () => {
      const UserZodSchema = z.object({
        id: z.string(),
        name: z.string().min(1),
        email: z.string().email(),
        age: z.number().int().min(0),
        createdAt: z.number(),
        updatedAt: z.number(),
      })

      const schema = createSchema<User>()
        .field("name", { type: "TEXT", indexed: true })
        .field("email", { type: "TEXT", indexed: true, unique: true })
        .field("age", { type: "INTEGER", indexed: true })
        .validate(wrapStandardSchema<User>(UserZodSchema))
        .build()

      const users = db.collection("users", schema)

      const result = await users.insertOne({
        name: "Alice",
        email: "alice@example.com",
        age: 30,
      })

      expect(result.document.id).toBeDefined()

      const found = await users.findById(result.document.id)
      expect(found).toBeDefined()
      expect(found?.name).toBe("Alice")
      expect(found?.email).toBe("alice@example.com")
      expect(found?.age).toBe(30)
    })

    it("should prevent invalid documents from being inserted with Zod", async () => {
      const UserZodSchema = z.object({
        id: z.string(),
        name: z.string().min(1),
        email: z.string().email(),
        age: z.number().int().min(0),
        createdAt: z.number(),
        updatedAt: z.number(),
      })

      const schema = createSchema<User>()
        .field("name", { type: "TEXT", indexed: true })
        .field("email", { type: "TEXT", indexed: true })
        .field("age", { type: "INTEGER", indexed: true })
        .validate(wrapStandardSchema<User>(UserZodSchema))
        .build()

      const users = db.collection("users", schema)

      try {
        await users.insertOne({
          name: "",
          email: "invalid-email",
          age: -5,
        })
        expect.unreachable("Should have thrown ValidationError")
      } catch (error) {
        expect(error).toBeInstanceOf(ValidationError)
      }
    })

    it("should provide field-level error details with Zod", async () => {
      const UserZodSchema = z.object({
        id: z.string(),
        name: z.string().min(1),
        email: z.string().email(),
        age: z.number().int().min(18),
        createdAt: z.number(),
        updatedAt: z.number(),
      })

      const schema = createSchema<User>()
        .field("name", { type: "TEXT", indexed: true })
        .field("email", { type: "TEXT", indexed: true })
        .field("age", { type: "INTEGER", indexed: true })
        .validate(wrapStandardSchema<User>(UserZodSchema))
        .build()

      const users = db.collection("users", schema)

      try {
        await users.insertOne({
          name: "Bob",
          email: "bob@example.com",
          age: 15, // Below minimum age of 18
        })
        expect.unreachable("Should have thrown ValidationError")
      } catch (error) {
        expect(error).toBeInstanceOf(ValidationError)
        const validationError = error as ValidationError
        expect(validationError.field).toBe("age")
        expect(validationError.value).toBe(15)
        expect(validationError.message).toContain("18")
      }
    })

    it("should handle nested validation errors with Zod", async () => {
      const UserZodSchema = z.object({
        id: z.string(),
        name: z.string().min(1, "Name is required"),
        email: z.string().email("Invalid email format"),
        age: z.number().int().min(0),
        createdAt: z.number(),
        updatedAt: z.number(),
      })

      const schema = createSchema<User>()
        .field("name", { type: "TEXT", indexed: true })
        .field("email", { type: "TEXT", indexed: true })
        .field("age", { type: "INTEGER", indexed: true })
        .validate(wrapStandardSchema<User>(UserZodSchema))
        .build()

      const users = db.collection("users", schema)

      try {
        await users.insertOne({
          name: "Charlie",
          email: "not-an-email",
          age: 25,
        })
        expect.unreachable("Should have thrown ValidationError")
      } catch (error) {
        expect(error).toBeInstanceOf(ValidationError)
        const validationError = error as ValidationError
        expect(validationError.field).toBe("email")
        expect(validationError.message).toContain("email")
      }
    })
  })

  describe("Valibot validator integration", () => {
    it("should successfully validate valid documents with Valibot", async () => {
      const UserValibotSchema = object({
        id: string(),
        name: pipe(string(), minLength(1)),
        email: pipe(string(), email()),
        age: pipe(number(), integer(), minValue(0)),
        createdAt: number(),
        updatedAt: number(),
      })

      const schema = createSchema<User>()
        .field("name", { type: "TEXT", indexed: true })
        .field("email", { type: "TEXT", indexed: true, unique: true })
        .field("age", { type: "INTEGER", indexed: true })
        .validate(wrapStandardSchema<User>(UserValibotSchema))
        .build()

      const users = db.collection("users", schema)

      const result = await users.insertOne({
        name: "Diana",
        email: "diana@example.com",
        age: 28,
      })

      expect(result.document.id).toBeDefined()

      const found = await users.findById(result.document.id)
      expect(found).toBeDefined()
      expect(found?.name).toBe("Diana")
      expect(found?.email).toBe("diana@example.com")
      expect(found?.age).toBe(28)
    })

    it("should prevent invalid documents from being inserted with Valibot", async () => {
      const UserValibotSchema = object({
        id: string(),
        name: pipe(string(), minLength(1)),
        email: pipe(string(), email()),
        age: pipe(number(), integer(), minValue(0)),
        createdAt: number(),
        updatedAt: number(),
      })

      const schema = createSchema<User>()
        .field("name", { type: "TEXT", indexed: true })
        .field("email", { type: "TEXT", indexed: true })
        .field("age", { type: "INTEGER", indexed: true })
        .validate(wrapStandardSchema<User>(UserValibotSchema))
        .build()

      const users = db.collection("users", schema)

      try {
        await users.insertOne({
          name: "Eve",
          email: "invalid-email",
          age: 22,
        })
        expect.unreachable("Should have thrown ValidationError")
      } catch (error) {
        expect(error).toBeInstanceOf(ValidationError)
      }
    })

    it("should provide field-level error details with Valibot", async () => {
      const UserValibotSchema = object({
        id: string(),
        name: pipe(string(), minLength(1)),
        email: pipe(string(), email()),
        age: pipe(number(), integer(), minValue(21)),
        createdAt: number(),
        updatedAt: number(),
      })

      const schema = createSchema<User>()
        .field("name", { type: "TEXT", indexed: true })
        .field("email", { type: "TEXT", indexed: true })
        .field("age", { type: "INTEGER", indexed: true })
        .validate(wrapStandardSchema<User>(UserValibotSchema))
        .build()

      const users = db.collection("users", schema)

      try {
        await users.insertOne({
          name: "Frank",
          email: "frank@example.com",
          age: 18, // Below minimum age of 21
        })
        expect.unreachable("Should have thrown ValidationError")
      } catch (error) {
        expect(error).toBeInstanceOf(ValidationError)
        const validationError = error as ValidationError
        expect(validationError.field).toBe("age")
        expect(validationError.value).toBe(18)
      }
    })

    it("should handle validation errors correctly with Valibot", async () => {
      const UserValibotSchema = object({
        id: string(),
        name: pipe(string(), minLength(1, "Name cannot be empty")),
        email: pipe(string(), email("Invalid email address")),
        age: pipe(number(), integer()),
        createdAt: number(),
        updatedAt: number(),
      })

      const schema = createSchema<User>()
        .field("name", { type: "TEXT", indexed: true })
        .field("email", { type: "TEXT", indexed: true })
        .field("age", { type: "INTEGER", indexed: true })
        .validate(wrapStandardSchema<User>(UserValibotSchema))
        .build()

      const users = db.collection("users", schema)

      try {
        await users.insertOne({
          name: "",
          email: "grace@example.com",
          age: 35,
        })
        expect.unreachable("Should have thrown ValidationError")
      } catch (error) {
        expect(error).toBeInstanceOf(ValidationError)
        const validationError = error as ValidationError
        expect(validationError.field).toBe("name")
        expect(validationError.message).toContain("empty")
      }
    })
  })

  describe("ArkType validator integration", () => {
    it("should successfully validate valid documents with ArkType", async () => {
      const UserArkSchema = type({
        id: "string",
        name: "string>0",
        email: "string.email",
        age: "number%1>=0",
        createdAt: "number",
        updatedAt: "number",
      })

      const schema = createSchema<User>()
        .field("name", { type: "TEXT", indexed: true })
        .field("email", { type: "TEXT", indexed: true, unique: true })
        .field("age", { type: "INTEGER", indexed: true })
        .validate(wrapStandardSchema<User>(UserArkSchema))
        .build()

      const users = db.collection("users", schema)

      const result = await users.insertOne({
        name: "Henry",
        email: "henry@example.com",
        age: 42,
      })

      expect(result.document.id).toBeDefined()

      const found = await users.findById(result.document.id)
      expect(found).toBeDefined()
      expect(found?.name).toBe("Henry")
      expect(found?.email).toBe("henry@example.com")
      expect(found?.age).toBe(42)
    })

    it("should prevent invalid documents from being inserted with ArkType", async () => {
      const UserArkSchema = type({
        id: "string",
        name: "string>0",
        email: "string.email",
        age: "number%1>=0",
        createdAt: "number",
        updatedAt: "number",
      })

      const schema = createSchema<User>()
        .field("name", { type: "TEXT", indexed: true })
        .field("email", { type: "TEXT", indexed: true })
        .field("age", { type: "INTEGER", indexed: true })
        .validate(wrapStandardSchema<User>(UserArkSchema))
        .build()

      const users = db.collection("users", schema)

      try {
        await users.insertOne({
          name: "Isabel",
          email: "not-valid-email",
          age: 29,
        })
        expect.unreachable("Should have thrown ValidationError")
      } catch (error) {
        expect(error).toBeInstanceOf(ValidationError)
      }
    })

    it("should provide field-level error details with ArkType", async () => {
      const UserArkSchema = type({
        id: "string",
        name: "string>0",
        email: "string.email",
        age: "number%1>=18",
        createdAt: "number",
        updatedAt: "number",
      })

      const schema = createSchema<User>()
        .field("name", { type: "TEXT", indexed: true })
        .field("email", { type: "TEXT", indexed: true })
        .field("age", { type: "INTEGER", indexed: true })
        .validate(wrapStandardSchema<User>(UserArkSchema))
        .build()

      const users = db.collection("users", schema)

      try {
        await users.insertOne({
          name: "Jack",
          email: "jack@example.com",
          age: 16, // Below minimum age of 18
        })
        expect.unreachable("Should have thrown ValidationError")
      } catch (error) {
        expect(error).toBeInstanceOf(ValidationError)
        const validationError = error as ValidationError
        expect(validationError.field).toBe("age")
        expect(validationError.value).toBe(16)
      }
    })

    it("should handle validation errors correctly with ArkType", async () => {
      const UserArkSchema = type({
        id: "string",
        name: "string>0",
        email: "string.email",
        age: "number%1>=0",
        createdAt: "number",
        updatedAt: "number",
      })

      const schema = createSchema<User>()
        .field("name", { type: "TEXT", indexed: true })
        .field("email", { type: "TEXT", indexed: true })
        .field("age", { type: "INTEGER", indexed: true })
        .validate(wrapStandardSchema<User>(UserArkSchema))
        .build()

      const users = db.collection("users", schema)

      try {
        await users.insertOne({
          name: "",
          email: "kate@example.com",
          age: 27,
        })
        expect.unreachable("Should have thrown ValidationError")
      } catch (error) {
        expect(error).toBeInstanceOf(ValidationError)
        const validationError = error as ValidationError
        expect(validationError.field).toBe("name")
      }
    })
  })

  describe("Cross-validator consistency", () => {
    it("should enforce the same validation rules across all validators", async () => {
      const testData = {
        name: "Laura",
        email: "laura@example.com",
        age: 33,
      }

      // Zod schema
      const zodSchema = z.object({
        id: z.string(),
        name: z.string().min(1),
        email: z.string().email(),
        age: z.number().int().min(0),
        createdAt: z.number(),
        updatedAt: z.number(),
      })

      const zodDbSchema = createSchema<User>()
        .field("name", { type: "TEXT", indexed: true })
        .field("email", { type: "TEXT", indexed: true })
        .field("age", { type: "INTEGER", indexed: true })
        .validate(wrapStandardSchema<User>(zodSchema))
        .build()

      const zodUsers = db.collection("zod_users", zodDbSchema)
      const zodResult = await zodUsers.insertOne(testData)
      expect(zodResult.document.id).toBeDefined()

      // Valibot schema
      const valibotSchema = object({
        id: string(),
        name: pipe(string(), minLength(1)),
        email: pipe(string(), email()),
        age: pipe(number(), integer(), minValue(0)),
        createdAt: number(),
        updatedAt: number(),
      })

      const valibotDbSchema = createSchema<User>()
        .field("name", { type: "TEXT", indexed: true })
        .field("email", { type: "TEXT", indexed: true })
        .field("age", { type: "INTEGER", indexed: true })
        .validate(wrapStandardSchema<User>(valibotSchema))
        .build()

      const valibotUsers = db.collection("valibot_users", valibotDbSchema)
      const valibotResult = await valibotUsers.insertOne(testData)
      expect(valibotResult.document.id).toBeDefined()

      // ArkType schema
      const arkSchema = type({
        id: "string",
        name: "string>0",
        email: "string.email",
        age: "number%1>=0",
        createdAt: "number",
        updatedAt: "number",
      })

      const arkDbSchema = createSchema<User>()
        .field("name", { type: "TEXT", indexed: true })
        .field("email", { type: "TEXT", indexed: true })
        .field("age", { type: "INTEGER", indexed: true })
        .validate(wrapStandardSchema<User>(arkSchema))
        .build()

      const arkUsers = db.collection("ark_users", arkDbSchema)
      const arkResult = await arkUsers.insertOne(testData)
      expect(arkResult.document.id).toBeDefined()

      // All should have successfully inserted the same data
      const zodFound = await zodUsers.findById(zodResult.document.id)
      const valibotFound = await valibotUsers.findById(
        valibotResult.document.id
      )
      const arkFound = await arkUsers.findById(arkResult.document.id)

      expect(zodFound?.name).toBe(testData.name)
      expect(valibotFound?.name).toBe(testData.name)
      expect(arkFound?.name).toBe(testData.name)
    })

    it("should reject the same invalid data across all validators", async () => {
      const invalidData = {
        name: "",
        email: "invalid-email",
        age: -5,
      }

      // Zod
      const zodSchema = z.object({
        id: z.string(),
        name: z.string().min(1),
        email: z.string().email(),
        age: z.number().int().min(0),
        createdAt: z.number(),
        updatedAt: z.number(),
      })

      const zodDbSchema = createSchema<User>()
        .field("name", { type: "TEXT", indexed: true })
        .validate(wrapStandardSchema<User>(zodSchema))
        .build()

      const zodUsers = db.collection("zod_invalid", zodDbSchema)

      try {
        await zodUsers.insertOne(invalidData)
        expect.unreachable("Zod should have rejected")
      } catch (error) {
        expect(error).toBeInstanceOf(ValidationError)
      }

      // Valibot
      const valibotSchema = object({
        id: string(),
        name: pipe(string(), minLength(1)),
        email: pipe(string(), email()),
        age: pipe(number(), integer(), minValue(0)),
        createdAt: number(),
        updatedAt: number(),
      })

      const valibotDbSchema = createSchema<User>()
        .field("name", { type: "TEXT", indexed: true })
        .validate(wrapStandardSchema<User>(valibotSchema))
        .build()

      const valibotUsers = db.collection("valibot_invalid", valibotDbSchema)

      try {
        await valibotUsers.insertOne(invalidData)
        expect.unreachable("Valibot should have rejected")
      } catch (error) {
        expect(error).toBeInstanceOf(ValidationError)
      }

      // ArkType
      const arkSchema = type({
        id: "string",
        name: "string>0",
        email: "string.email",
        age: "number%1>=0",
        createdAt: "number",
        updatedAt: "number",
      })

      const arkDbSchema = createSchema<User>()
        .field("name", { type: "TEXT", indexed: true })
        .validate(wrapStandardSchema<User>(arkSchema))
        .build()

      const arkUsers = db.collection("ark_invalid", arkDbSchema)

      try {
        await arkUsers.insertOne(invalidData)
        expect.unreachable("ArkType should have rejected")
      } catch (error) {
        expect(error).toBeInstanceOf(ValidationError)
      }
    })
  })
})
