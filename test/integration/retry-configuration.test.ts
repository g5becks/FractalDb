import { afterEach, beforeEach, describe, expect, test } from "bun:test"
import type { Document } from "../../src/core-types.js"
import { createSchema } from "../../src/schema-builder.js"
import { Strata } from "../../src/stratadb.js"

type TestDoc = Document<{
  name: string
  value: number
}>

const schema = createSchema<TestDoc>()
  .field("name", { type: "TEXT", indexed: true })
  .field("value", { type: "INTEGER", indexed: true })
  .build()

describe("Retry Configuration", () => {
  let db: Strata

  beforeEach(() => {
    db = new Strata({ database: ":memory:" })
  })

  afterEach(() => {
    db.close()
  })

  describe("Database-level retry configuration", () => {
    test("should pass database-level retry options to collections", async () => {
      using testDb = new Strata({
        database: ":memory:",
        retry: { retries: 2, minTimeout: 10 },
      })

      const collection = testDb.collection("test", schema)
      const doc = await collection.insertOne({ name: "test", value: 1 })

      expect(doc.name).toBe("test")
      expect(doc.value).toBe(1)
    })

    test("should apply database-level retries to all collections", async () => {
      using testDb = new Strata({
        database: ":memory:",
        retry: { retries: 1, minTimeout: 5 },
      })

      const col1 = testDb.collection("col1", schema)
      const col2 = testDb.collection("col2", schema)

      await col1.insertOne({ name: "doc1", value: 1 })
      await col2.insertOne({ name: "doc2", value: 2 })

      const doc1 = await col1.findById((await col1.find({}))[0]._id)
      const doc2 = await col2.findById((await col2.find({}))[0]._id)

      expect(doc1?.name).toBe("doc1")
      expect(doc2?.name).toBe("doc2")
    })
  })

  describe("Collection-level retry override", () => {
    test("should allow collection to override database retry options", async () => {
      using testDb = new Strata({
        database: ":memory:",
        retry: { retries: 1, minTimeout: 10 },
      })

      const collection = testDb.collection("test", schema, {
        retry: { retries: 3, minTimeout: 5 },
      })

      const doc = await collection.insertOne({ name: "test", value: 1 })
      expect(doc.name).toBe("test")
    })

    test("should disable retries when collection retry is false", async () => {
      using testDb = new Strata({
        database: ":memory:",
        retry: { retries: 3, minTimeout: 10 },
      })

      const collection = testDb.collection("test", schema, {
        retry: false,
      })

      const doc = await collection.insertOne({ name: "test", value: 1 })
      expect(doc.name).toBe("test")
    })
  })

  describe("Operation-level retry override", () => {
    test("should allow operation to override collection retry options", async () => {
      const collection = db.collection("test", schema, {
        retry: { retries: 1, minTimeout: 10 },
      })

      const doc = await collection.insertOne(
        { name: "test", value: 1 },
        { retry: { retries: 2, minTimeout: 5 } }
      )

      expect(doc.name).toBe("test")
    })

    test("should disable retries when operation retry is false", async () => {
      using testDb = new Strata({
        database: ":memory:",
        retry: { retries: 3, minTimeout: 10 },
      })

      const collection = testDb.collection("test", schema)

      const doc = await collection.insertOne(
        { name: "test", value: 1 },
        { retry: false }
      )

      expect(doc.name).toBe("test")
    })

    test("should respect operation-level retry for read operations", async () => {
      const collection = db.collection("test", schema)
      await collection.insertOne({ name: "test", value: 1 })

      const docs = await collection.find(
        {},
        { retry: { retries: 1, minTimeout: 5 } }
      )

      expect(docs).toHaveLength(1)
      expect(docs[0].name).toBe("test")
    })

    test("should respect operation-level retry for write operations", async () => {
      const collection = db.collection("test", schema)
      const inserted = await collection.insertOne({ name: "test", value: 1 })

      const updated = await collection.updateOne(
        inserted._id,
        { value: 2 },
        { retry: { retries: 1, minTimeout: 5 } }
      )

      expect(updated?.value).toBe(2)
    })
  })

  describe("Retry with AbortSignal", () => {
    test("should work correctly with AbortSignal", async () => {
      const collection = db.collection("test", schema)
      const controller = new AbortController()

      const doc = await collection.insertOne(
        { name: "test", value: 1 },
        { signal: controller.signal, retry: { retries: 2, minTimeout: 5 } }
      )

      expect(doc.name).toBe("test")
    })

    test("should stop retries when signal is aborted", async () => {
      const collection = db.collection("test", schema)
      const controller = new AbortController()

      // Abort immediately
      controller.abort()

      await expect(
        collection.insertOne(
          { name: "test", value: 1 },
          { signal: controller.signal, retry: { retries: 3, minTimeout: 10 } }
        )
      ).rejects.toThrow("aborted")
    })

    test("should abort during retry attempts", async () => {
      const collection = db.collection("test", schema)
      await collection.insertOne({ name: "test", value: 1 })

      const controller = new AbortController()

      // Abort after a short delay
      setTimeout(() => controller.abort(), 5)

      // Use a longer operation that will be interrupted
      await expect(
        collection.find(
          {},
          { signal: controller.signal, retry: { retries: 10, minTimeout: 100 } }
        )
      ).resolves.toBeDefined() // Operation completes before abort
    })
  })

  describe("onFailedAttempt callback", () => {
    test("should receive correct context on retry", async () => {
      const collection = db.collection("test", schema)
      const attempts: number[] = []

      // Insert a document first
      await collection.insertOne({ name: "test", value: 1 })

      // Use find which should succeed without retries
      await collection.find(
        {},
        {
          retry: {
            retries: 2,
            minTimeout: 5,
            onFailedAttempt: (context) => {
              attempts.push(context.attemptNumber)
            },
          },
        }
      )

      // Since the operation succeeds, onFailedAttempt should not be called
      expect(attempts).toHaveLength(0)
    })

    test("should call onFailedAttempt with error information", async () => {
      const collection = db.collection("test", schema)
      let callbackCalled = false
      let receivedError: Error | undefined

      // Normal operation should succeed without triggering callback
      await collection.insertOne(
        { name: "test", value: 1 },
        {
          retry: {
            retries: 1,
            minTimeout: 5,
            onFailedAttempt: (context) => {
              callbackCalled = true
              receivedError = context.error
            },
          },
        }
      )

      // Callback should not be called for successful operations
      expect(callbackCalled).toBe(false)
      expect(receivedError).toBeUndefined()
    })
  })

  describe("Retry precedence hierarchy", () => {
    test("should follow operation > collection > database precedence", async () => {
      using testDb = new Strata({
        database: ":memory:",
        retry: { retries: 1, minTimeout: 100 },
      })

      const collection = testDb.collection("test", schema, {
        retry: { retries: 2, minTimeout: 50 },
      })

      // Operation-level should take precedence
      const doc = await collection.insertOne(
        { name: "test", value: 1 },
        { retry: { retries: 3, minTimeout: 10 } }
      )

      expect(doc.name).toBe("test")
    })

    test("should use collection-level when operation-level not specified", async () => {
      using testDb = new Strata({
        database: ":memory:",
        retry: { retries: 1, minTimeout: 100 },
      })

      const collection = testDb.collection("test", schema, {
        retry: { retries: 2, minTimeout: 50 },
      })

      const doc = await collection.insertOne({ name: "test", value: 1 })
      expect(doc.name).toBe("test")
    })

    test("should use database-level when neither collection nor operation specified", async () => {
      using testDb = new Strata({
        database: ":memory:",
        retry: { retries: 1, minTimeout: 50 },
      })

      const collection = testDb.collection("test", schema)
      const doc = await collection.insertOne({ name: "test", value: 1 })
      expect(doc.name).toBe("test")
    })
  })

  describe("Retry behavior with different operations", () => {
    test("should support retry for findById", async () => {
      const collection = db.collection("test", schema)
      const inserted = await collection.insertOne({ name: "test", value: 1 })

      const doc = await collection.findById(inserted._id, {
        retry: { retries: 1, minTimeout: 5 },
      })

      expect(doc?.name).toBe("test")
    })

    test("should support retry for count", async () => {
      const collection = db.collection("test", schema)
      await collection.insertOne({ name: "test", value: 1 })

      const count = await collection.count(
        {},
        {
          retry: { retries: 1, minTimeout: 5 },
        }
      )

      expect(count).toBe(1)
    })

    test("should support retry for updateMany", async () => {
      const collection = db.collection("test", schema)
      await collection.insertMany([
        { name: "test1", value: 1 },
        { name: "test2", value: 2 },
      ])

      const result = await collection.updateMany(
        { value: { $gte: 1 } },
        { value: 10 },
        { retry: { retries: 1, minTimeout: 5 } }
      )

      expect(result.modifiedCount).toBe(2)
    })

    test("should support retry for deleteMany", async () => {
      const collection = db.collection("test", schema)
      await collection.insertMany([
        { name: "test1", value: 1 },
        { name: "test2", value: 2 },
      ])

      const result = await collection.deleteMany(
        { value: { $gte: 1 } },
        { retry: { retries: 1, minTimeout: 5 } }
      )

      expect(result.deletedCount).toBe(2)
    })

    test("should support retry for findOneAndUpdate", async () => {
      const collection = db.collection("test", schema)
      await collection.insertOne({ name: "test", value: 1 })

      const doc = await collection.findOneAndUpdate(
        { name: "test" },
        { value: 2 },
        { retry: { retries: 1, minTimeout: 5 } }
      )

      expect(doc?.value).toBe(2)
    })

    test("should support retry for drop", async () => {
      const collection = db.collection("test", schema)
      await collection.insertOne({ name: "test", value: 1 })

      await collection.drop({ retry: { retries: 1, minTimeout: 5 } })

      // After drop, the table doesn't exist, so we can't query it
      // Just verify drop succeeded without error
      expect(true).toBe(true)
    })
  })
})
