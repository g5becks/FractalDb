import { Database } from "bun:sqlite"
import { afterEach, beforeEach, describe, expect, it } from "bun:test"
import type { InsertEvent } from "../../src/collection-events.js"
import type { Document } from "../../src/core-types.js"
import { createSchema } from "../../src/schema-builder.js"
import { SQLiteCollection } from "../../src/sqlite-collection.js"

type TestUser = Document<{
  name: string
  email: string
  age: number
}>

describe("Collection Events - Registration Methods", () => {
  let db: Database
  let collection: SQLiteCollection<TestUser>

  beforeEach(() => {
    db = new Database(":memory:")
    const schema = createSchema<TestUser>()
      .field("name", { type: "TEXT", indexed: true })
      .field("email", { type: "TEXT", indexed: true, unique: true })
      .field("age", { type: "INTEGER", indexed: true })
      .build()

    collection = new SQLiteCollection(
      db,
      "users",
      schema,
      () => `test-${Date.now()}-${Math.random()}`
    )
  })

  afterEach(() => {
    db.close()
  })

  describe("on()", () => {
    it("should register listener and receive events", async () => {
      const events: InsertEvent<TestUser>[] = []

      collection.on("insert", (event) => {
        events.push(event)
      })

      await collection.insertOne({
        name: "Alice",
        email: "alice@example.com",
        age: 30,
      })

      expect(events).toHaveLength(1)
      expect(events[0].document.name).toBe("Alice")
      expect(events[0].document.email).toBe("alice@example.com")
      expect(events[0].document.age).toBe(30)
    })

    it("should return collection instance for chaining", () => {
      const result = collection.on("insert", () => {
        // no-op for testing
      })

      expect(result).toBe(collection)
    })

    it("should allow multiple listeners for same event", async () => {
      const events1: InsertEvent<TestUser>[] = []
      const events2: InsertEvent<TestUser>[] = []

      collection.on("insert", (event) => {
        events1.push(event)
      })

      collection.on("insert", (event) => {
        events2.push(event)
      })

      await collection.insertOne({
        name: "Bob",
        email: "bob@example.com",
        age: 25,
      })

      expect(events1).toHaveLength(1)
      expect(events2).toHaveLength(1)
      expect(events1[0].document._id).toBe(events2[0].document._id)
    })

    it("should support method chaining", () => {
      const listener1 = () => {
        // no-op for testing
      }
      const listener2 = () => {
        // no-op for testing
      }

      const result = collection.on("insert", listener1).on("update", listener2)

      expect(result).toBe(collection)
      expect(collection.listenerCount("insert")).toBe(1)
      expect(collection.listenerCount("update")).toBe(1)
    })
  })

  describe("once()", () => {
    it("should fire listener only once then auto-remove", async () => {
      let callCount = 0

      collection.once("insert", () => {
        callCount += 1
      })

      await collection.insertOne({
        name: "Alice",
        email: "alice@example.com",
        age: 30,
      })

      await collection.insertOne({
        name: "Bob",
        email: "bob@example.com",
        age: 25,
      })

      expect(callCount).toBe(1)
      expect(collection.listenerCount("insert")).toBe(0)
    })

    it("should return collection instance for chaining", () => {
      const result = collection.once("insert", () => {
        // no-op for testing
      })

      expect(result).toBe(collection)
    })

    it("should receive correct event data", async () => {
      let receivedEvent: InsertEvent<TestUser> | null = null

      collection.once("insert", (event) => {
        receivedEvent = event
      })

      await collection.insertOne({
        name: "Charlie",
        email: "charlie@example.com",
        age: 35,
      })

      expect(receivedEvent).not.toBeNull()
      expect(receivedEvent?.document.name).toBe("Charlie")
    })
  })

  describe("off()", () => {
    it("should remove specific listener", async () => {
      const events: InsertEvent<TestUser>[] = []
      const listener = (event: InsertEvent<TestUser>) => {
        events.push(event)
      }

      collection.on("insert", listener)

      await collection.insertOne({
        name: "Alice",
        email: "alice@example.com",
        age: 30,
      })

      expect(events).toHaveLength(1)

      collection.off("insert", listener)

      await collection.insertOne({
        name: "Bob",
        email: "bob@example.com",
        age: 25,
      })

      expect(events).toHaveLength(1)
    })

    it("should return collection instance for chaining", () => {
      const listener = () => {
        // no-op for testing
      }
      collection.on("insert", listener)

      const result = collection.off("insert", listener)

      expect(result).toBe(collection)
    })

    it("should only remove the specific listener instance", async () => {
      const events1: InsertEvent<TestUser>[] = []
      const events2: InsertEvent<TestUser>[] = []

      const listener1 = (event: InsertEvent<TestUser>) => {
        events1.push(event)
      }

      const listener2 = (event: InsertEvent<TestUser>) => {
        events2.push(event)
      }

      collection.on("insert", listener1)
      collection.on("insert", listener2)

      collection.off("insert", listener1)

      await collection.insertOne({
        name: "Alice",
        email: "alice@example.com",
        age: 30,
      })

      expect(events1).toHaveLength(0)
      expect(events2).toHaveLength(1)
    })

    it("should do nothing when removing non-existent listener", () => {
      const listener = () => {
        // no-op for testing
      }

      expect(() => collection.off("insert", listener)).not.toThrow()
    })

    it("should do nothing when no emitter exists", () => {
      const listener = () => {
        // no-op for testing
      }

      expect(() => collection.off("insert", listener)).not.toThrow()
      expect(collection.listenerCount("insert")).toBe(0)
    })
  })

  describe("removeAllListeners()", () => {
    it("should remove all listeners for specific event", async () => {
      const events1: InsertEvent<TestUser>[] = []
      const events2: InsertEvent<TestUser>[] = []

      collection.on("insert", (event) => {
        events1.push(event)
      })

      collection.on("insert", (event) => {
        events2.push(event)
      })

      collection.removeAllListeners("insert")

      await collection.insertOne({
        name: "Alice",
        email: "alice@example.com",
        age: 30,
      })

      expect(events1).toHaveLength(0)
      expect(events2).toHaveLength(0)
      expect(collection.listenerCount("insert")).toBe(0)
    })

    it("should remove all listeners for all events when no event specified", async () => {
      const insertEvents: InsertEvent<TestUser>[] = []
      const updateEvents: unknown[] = []

      collection.on("insert", (event) => {
        insertEvents.push(event)
      })

      collection.on("update", (event) => {
        updateEvents.push(event)
      })

      collection.removeAllListeners()

      await collection.insertOne({
        name: "Alice",
        email: "alice@example.com",
        age: 30,
      })

      await collection.updateOne({ name: "Alice" }, { age: 31 })

      expect(insertEvents).toHaveLength(0)
      expect(updateEvents).toHaveLength(0)
      expect(collection.listenerCount("insert")).toBe(0)
      expect(collection.listenerCount("update")).toBe(0)
    })

    it("should return collection instance for chaining", () => {
      collection.on("insert", () => {
        // no-op for testing
      })

      const result = collection.removeAllListeners("insert")

      expect(result).toBe(collection)
    })

    it("should not affect listeners for other events", async () => {
      const insertEvents: InsertEvent<TestUser>[] = []
      const updateEvents: unknown[] = []

      collection.on("insert", (event) => {
        insertEvents.push(event)
      })

      collection.on("update", (event) => {
        updateEvents.push(event)
      })

      collection.removeAllListeners("insert")

      await collection.insertOne({
        name: "Alice",
        email: "alice@example.com",
        age: 30,
      })

      await collection.updateOne({ name: "Alice" }, { age: 31 })

      expect(insertEvents).toHaveLength(0)
      expect(updateEvents).toHaveLength(1)
    })

    it("should do nothing when no emitter exists", () => {
      expect(() => collection.removeAllListeners()).not.toThrow()
      expect(() => collection.removeAllListeners("insert")).not.toThrow()
    })
  })

  describe("listenerCount()", () => {
    it("should return 0 when no listeners registered", () => {
      expect(collection.listenerCount("insert")).toBe(0)
    })

    it("should return correct count after registering listeners", () => {
      collection.on("insert", () => {
        // no-op for testing
      })
      expect(collection.listenerCount("insert")).toBe(1)

      collection.on("insert", () => {
        // no-op for testing
      })
      expect(collection.listenerCount("insert")).toBe(2)

      collection.on("insert", () => {
        // no-op for testing
      })
      expect(collection.listenerCount("insert")).toBe(3)
    })

    it("should return correct count after removing listeners", () => {
      const listener1 = () => {
        // no-op for testing
      }
      const listener2 = () => {
        // no-op for testing
      }

      collection.on("insert", listener1)
      collection.on("insert", listener2)

      expect(collection.listenerCount("insert")).toBe(2)

      collection.off("insert", listener1)

      expect(collection.listenerCount("insert")).toBe(1)

      collection.off("insert", listener2)

      expect(collection.listenerCount("insert")).toBe(0)
    })

    it("should count listeners per event separately", () => {
      collection.on("insert", () => {
        // no-op for testing
      })
      collection.on("insert", () => {
        // no-op for testing
      })
      collection.on("update", () => {
        // no-op for testing
      })

      expect(collection.listenerCount("insert")).toBe(2)
      expect(collection.listenerCount("update")).toBe(1)
      expect(collection.listenerCount("delete")).toBe(0)
    })

    it("should return 0 when no emitter exists", () => {
      expect(collection.listenerCount("insert")).toBe(0)
      expect(collection.listenerCount("update")).toBe(0)
    })
  })

  describe("listeners()", () => {
    it("should return empty array when no listeners registered", () => {
      const listeners = collection.listeners("insert")

      expect(listeners).toEqual([])
      expect(listeners).toHaveLength(0)
    })

    it("should return array of registered listeners", () => {
      const listener1 = () => {
        // no-op for testing
      }
      const listener2 = () => {
        // no-op for testing
      }
      const listener3 = () => {
        // no-op for testing
      }

      collection.on("insert", listener1)
      collection.on("insert", listener2)
      collection.on("insert", listener3)

      const listeners = collection.listeners("insert")

      expect(listeners).toHaveLength(3)
      expect(listeners).toContain(listener1)
      expect(listeners).toContain(listener2)
      expect(listeners).toContain(listener3)
    })

    it("should return listeners for specific event only", () => {
      const insertListener = () => {
        // no-op for testing
      }
      const updateListener = () => {
        // no-op for testing
      }

      collection.on("insert", insertListener)
      collection.on("update", updateListener)

      const insertListeners = collection.listeners("insert")
      const updateListeners = collection.listeners("update")

      expect(insertListeners).toHaveLength(1)
      expect(insertListeners).toContain(insertListener)
      expect(insertListeners).not.toContain(updateListener)

      expect(updateListeners).toHaveLength(1)
      expect(updateListeners).toContain(updateListener)
      expect(updateListeners).not.toContain(insertListener)
    })

    it("should return empty array for event with no listeners", () => {
      collection.on("insert", () => {
        // no-op for testing
      })

      const deleteListeners = collection.listeners("delete")

      expect(deleteListeners).toEqual([])
    })

    it("should return empty array when no emitter exists", () => {
      const listeners = collection.listeners("insert")

      expect(listeners).toEqual([])
      expect(listeners).toHaveLength(0)
    })
  })

  describe("Lazy initialization", () => {
    it("should not create emitter until first listener registration", () => {
      // Access listenerCount without registering listeners
      expect(collection.listenerCount("insert")).toBe(0)

      // Emitter should still not exist (property is private, but behavior is observable)
      const listeners = collection.listeners("insert")
      expect(listeners).toEqual([])
    })

    it("should create emitter on first on() call", () => {
      collection.on("insert", () => {
        // no-op for testing
      })

      expect(collection.listenerCount("insert")).toBe(1)
    })

    it("should create emitter on first once() call", () => {
      collection.once("insert", () => {
        // no-op for testing
      })

      expect(collection.listenerCount("insert")).toBe(1)
    })
  })

  describe("Multiple listeners receiving same event", () => {
    it("should invoke all registered listeners", async () => {
      let call1 = false
      let call2 = false
      let call3 = false

      collection.on("insert", () => {
        call1 = true
      })

      collection.on("insert", () => {
        call2 = true
      })

      collection.on("insert", () => {
        call3 = true
      })

      await collection.insertOne({
        name: "Alice",
        email: "alice@example.com",
        age: 30,
      })

      expect(call1).toBe(true)
      expect(call2).toBe(true)
      expect(call3).toBe(true)
    })

    it("should pass same event data to all listeners", async () => {
      const events: InsertEvent<TestUser>[] = []

      collection.on("insert", (event) => {
        events.push(event)
      })

      collection.on("insert", (event) => {
        events.push(event)
      })

      collection.on("insert", (event) => {
        events.push(event)
      })

      await collection.insertOne({
        name: "Bob",
        email: "bob@example.com",
        age: 25,
      })

      expect(events).toHaveLength(3)
      expect(events[0].document._id).toBe(events[1].document._id)
      expect(events[1].document._id).toBe(events[2].document._id)
      expect(events[0].document.name).toBe("Bob")
      expect(events[1].document.name).toBe("Bob")
      expect(events[2].document.name).toBe("Bob")
    })
  })
})
