import { afterEach, beforeEach, describe, expect, it } from "bun:test"
import type { Document } from "../../src/core-types.js"
import { AbortedError } from "../../src/errors.js"
import { createSchema } from "../../src/schema-builder.js"
import { Strata } from "../../src/stratadb.js"

type TestDoc = Document<{
  name: string
  value: number
}>

describe("AbortSignal Integration - Read Operations", () => {
  let db: Strata
  let collection: ReturnType<typeof db.collection<TestDoc>>

  beforeEach(() => {
    db = new Strata({ database: ":memory:" })
    const schema = createSchema<TestDoc>()
      .field("name", { type: "TEXT", indexed: true })
      .field("value", { type: "INTEGER", indexed: true })
      .build()
    collection = db.collection("test", schema)

    // Insert test data
    collection.insertOne({ name: "test1", value: 1 })
    collection.insertOne({ name: "test2", value: 2 })
    collection.insertOne({ name: "test3", value: 3 })
  })

  afterEach(() => {
    db.close()
  })

  it("find throws AbortedError when signal is pre-aborted", async () => {
    const controller = new AbortController()
    controller.abort()

    try {
      await collection.find({}, { signal: controller.signal })
      expect.unreachable("Should have thrown")
    } catch (error) {
      expect(error).toBeInstanceOf(AbortedError)
    }
  })

  it("findOne throws AbortedError when signal is pre-aborted", async () => {
    const controller = new AbortController()
    controller.abort()

    try {
      await collection.findOne({}, { signal: controller.signal })
      expect.unreachable("Should have thrown")
    } catch (error) {
      expect(error).toBeInstanceOf(AbortedError)
    }
  })

  it("findById throws AbortedError when signal is pre-aborted", async () => {
    const controller = new AbortController()
    controller.abort()

    try {
      await collection.findById("test-id", { signal: controller.signal })
      expect.unreachable("Should have thrown")
    } catch (error) {
      expect(error).toBeInstanceOf(AbortedError)
    }
  })

  it("count throws AbortedError when signal is pre-aborted", async () => {
    const controller = new AbortController()
    controller.abort()

    try {
      await collection.count({}, { signal: controller.signal })
      expect.unreachable("Should have thrown")
    } catch (error) {
      expect(error).toBeInstanceOf(AbortedError)
    }
  })

  it("search throws AbortedError when signal is pre-aborted", async () => {
    const controller = new AbortController()
    controller.abort()

    try {
      await collection.search("test", ["name"], { signal: controller.signal })
      expect.unreachable("Should have thrown")
    } catch (error) {
      expect(error).toBeInstanceOf(AbortedError)
    }
  })

  it("distinct throws AbortedError when signal is pre-aborted", async () => {
    const controller = new AbortController()
    controller.abort()

    try {
      await collection.distinct("name", {}, { signal: controller.signal })
      expect.unreachable("Should have thrown")
    } catch (error) {
      expect(error).toBeInstanceOf(AbortedError)
    }
  })

  it("estimatedDocumentCount throws AbortedError when signal is pre-aborted", async () => {
    const controller = new AbortController()
    controller.abort()

    try {
      await collection.estimatedDocumentCount({ signal: controller.signal })
      expect.unreachable("Should have thrown")
    } catch (error) {
      expect(error).toBeInstanceOf(AbortedError)
    }
  })

  it("operations complete successfully when signal is not aborted", async () => {
    const controller = new AbortController()

    const findResult = await collection.find({}, { signal: controller.signal })
    expect(findResult.length).toBe(3)

    const findOneResult = await collection.findOne(
      {},
      { signal: controller.signal }
    )
    expect(findOneResult).not.toBeNull()

    const countResult = await collection.count(
      {},
      { signal: controller.signal }
    )
    expect(countResult).toBe(3)

    const searchResult = await collection.search("test", ["name"], {
      signal: controller.signal,
    })
    expect(searchResult.length).toBe(3)

    const distinctResult = await collection.distinct(
      "name",
      {},
      {
        signal: controller.signal,
      }
    )
    expect(distinctResult.length).toBe(3)

    const estimatedCount = await collection.estimatedDocumentCount({
      signal: controller.signal,
    })
    expect(estimatedCount).toBe(3)
  })
})
