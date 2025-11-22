/**
 * CRUD Operations Benchmarks
 *
 * Measures end-to-end performance of document operations including:
 * - Document serialization (fast-safe-stringify)
 * - Query execution against SQLite
 * - Document deserialization from JSONB
 *
 * Run with: bun bench/crud-operations.bench.ts
 */

import { bench, group, run } from "mitata"
import type { Document } from "../src/core-types.js"
import { createSchema } from "../src/schema-builder.js"
import { StrataDBClass } from "../src/stratadb.js"

// Test document type
type User = Document<{
  name: string
  email: string
  age: number
  status: "active" | "inactive" | "pending"
  role: string
  tags: string[]
  score: number
  createdAt: number
}>

// Schema with mixed indexed/non-indexed fields
const userSchema = createSchema<User>()
  .field("name", { type: "TEXT", indexed: true })
  .field("email", { type: "TEXT", indexed: true, unique: true })
  .field("age", { type: "INTEGER", indexed: true })
  .field("status", { type: "TEXT", indexed: false })
  .field("role", { type: "TEXT", indexed: true })
  .field("tags", { type: "TEXT", indexed: false })
  .field("score", { type: "INTEGER", indexed: true })
  .field("createdAt", { type: "INTEGER", indexed: true })
  .build()

// Create fresh database for each benchmark group
function createTestDb() {
  return new StrataDBClass({ database: ":memory:" })
}

// Sample user data generator
let emailCounter = 0
function generateUser(): Omit<User, "id"> {
  emailCounter += 1
  return {
    name: `User ${emailCounter}`,
    email: `user${emailCounter}@example.com`,
    age: 20 + (emailCounter % 50),
    status: "active",
    role: emailCounter % 10 === 0 ? "admin" : "user",
    tags: ["tag1", "tag2"],
    score: emailCounter * 10,
    createdAt: Date.now(),
  }
}

// Seed database with test data
async function seedDatabase(
  db: StrataDBClass,
  count: number
): Promise<string[]> {
  const users = db.collection("users", userSchema)
  const ids: string[] = []
  for (let i = 0; i < count; i += 1) {
    const result = await users.insertOne(generateUser())
    ids.push(result.document.id)
  }
  return ids
}

// ============================================================================
// SETUP: Pre-populate databases for read benchmarks
// ============================================================================

// Small dataset (100 docs)
const smallDb = createTestDb()
const smallIds = await seedDatabase(smallDb, 100)
const smallUsers = smallDb.collection("users", userSchema)

// Medium dataset (1000 docs)
const mediumDb = createTestDb()
await seedDatabase(mediumDb, 1000)
const mediumUsers = mediumDb.collection("users", userSchema)

// ============================================================================
// INSERT OPERATIONS
// ============================================================================

group("Insert Operations", () => {
  bench("insertOne - single document", async () => {
    const db = createTestDb()
    const users = db.collection("users", userSchema)
    await users.insertOne(generateUser())
    db.close()
  })

  bench("insertMany - 10 documents", async () => {
    const db = createTestDb()
    const users = db.collection("users", userSchema)
    const docs = Array.from({ length: 10 }, () => generateUser())
    await users.insertMany(docs)
    db.close()
  })

  bench("insertMany - 100 documents", async () => {
    const db = createTestDb()
    const users = db.collection("users", userSchema)
    const docs = Array.from({ length: 100 }, () => generateUser())
    await users.insertMany(docs)
    db.close()
  })
})

// ============================================================================
// FIND OPERATIONS - Small Dataset (100 docs)
// ============================================================================

group("Find Operations - Small Dataset (100 docs)", () => {
  bench("findById - single document", async () => {
    await smallUsers.findById(smallIds[50])
  })

  bench("findOne - indexed field equality", async () => {
    await smallUsers.findOne({ role: "admin" })
  })

  bench("findOne - non-indexed field", async () => {
    await smallUsers.findOne({ status: "active" })
  })

  bench("find - all documents", async () => {
    await smallUsers.find({})
  })

  bench("find - indexed range query", async () => {
    await smallUsers.find({ age: { $gte: 30, $lt: 40 } })
  })

  bench("find - with limit", async () => {
    await smallUsers.find({}, { limit: 10 })
  })

  bench("find - with sort + limit", async () => {
    await smallUsers.find({}, { sort: { createdAt: -1 }, limit: 10 })
  })

  bench("count - all documents", async () => {
    await smallUsers.count({})
  })

  bench("count - with filter", async () => {
    await smallUsers.count({ role: "admin" })
  })
})

// ============================================================================
// UPDATE OPERATIONS
// ============================================================================

group("Update Operations", () => {
  bench("updateOne - single field", async () => {
    const db = createTestDb()
    const users = db.collection("users", userSchema)
    await users.insertOne(generateUser())
    await users.updateOne(
      { name: { $like: "User%" } },
      { $set: { score: 999 } }
    )
    db.close()
  })

  bench("updateOne - multiple fields", async () => {
    const db = createTestDb()
    const users = db.collection("users", userSchema)
    await users.insertOne(generateUser())
    await users.updateOne(
      { name: { $like: "User%" } },
      { $set: { score: 999, status: "inactive", role: "admin" } }
    )
    db.close()
  })

  bench("updateMany - 10 documents", async () => {
    const db = createTestDb()
    const users = db.collection("users", userSchema)
    const docs = Array.from({ length: 10 }, () => generateUser())
    await users.insertMany(docs)
    await users.updateMany({ status: "active" }, { $set: { score: 500 } })
    db.close()
  })
})

// ============================================================================
// DELETE OPERATIONS
// ============================================================================

group("Delete Operations", () => {
  bench("deleteOne - by filter", async () => {
    const db = createTestDb()
    const users = db.collection("users", userSchema)
    await users.insertOne(generateUser())
    await users.deleteOne({ name: { $like: "User%" } })
    db.close()
  })

  bench("deleteMany - 10 documents", async () => {
    const db = createTestDb()
    const users = db.collection("users", userSchema)
    const docs = Array.from({ length: 10 }, () => generateUser())
    await users.insertMany(docs)
    await users.deleteMany({ status: "active" })
    db.close()
  })
})

// ============================================================================
// TRANSACTION OPERATIONS
// ============================================================================

group("Transaction Operations", () => {
  bench("execute() with single insert", async () => {
    const db = createTestDb()
    await db.execute(async (tx) => {
      const users = tx.collection("users", userSchema)
      await users.insertOne(generateUser())
    })
    db.close()
  })

  bench("execute() with 10 inserts", async () => {
    const db = createTestDb()
    await db.execute(async (tx) => {
      const users = tx.collection("users", userSchema)
      for (let i = 0; i < 10; i += 1) {
        await users.insertOne(generateUser())
      }
    })
    db.close()
  })

  bench("manual transaction - commit", async () => {
    const db = createTestDb()
    const tx = db.transaction()
    const users = tx.collection("users", userSchema)
    await users.insertOne(generateUser())
    tx.commit()
    db.close()
  })

  bench("manual transaction - rollback", async () => {
    const db = createTestDb()
    const tx = db.transaction()
    const users = tx.collection("users", userSchema)
    await users.insertOne(generateUser())
    tx.rollback()
    db.close()
  })
})

// ============================================================================
// COMPLEX QUERY OPERATIONS - Medium Dataset (1000 docs)
// ============================================================================

group("Complex Queries - Medium Dataset (1000 docs)", () => {
  bench("find - $and with 2 conditions", async () => {
    await mediumUsers.find({
      $and: [{ status: "active" }, { age: { $gte: 30 } }],
    })
  })

  bench("find - $or with 3 conditions", async () => {
    await mediumUsers.find({
      $or: [{ role: "admin" }, { score: { $gt: 500 } }, { age: { $lt: 25 } }],
    })
  })

  bench("find - nested logical operators", async () => {
    await mediumUsers.find({
      $and: [
        { status: "active" },
        { $or: [{ role: "admin" }, { score: { $gte: 300 } }] },
      ],
    })
  })

  bench("find - $in operator", async () => {
    await mediumUsers.find({ age: { $in: [25, 30, 35, 40, 45] } })
  })

  bench("find - sort + skip + limit (pagination)", async () => {
    await mediumUsers.find({}, { sort: { score: -1 }, skip: 100, limit: 20 })
  })
})

await run({
  silent: false,
  avg: true,
  json: false,
  colors: true,
  min_max: true,
  percentiles: true,
})

// Cleanup
smallDb.close()
mediumDb.close()
