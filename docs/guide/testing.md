# Testing Guide

This guide covers how to effectively test code that uses StrataDB.

## Testing Setup

### Using In-Memory Databases

The most important practice for testing is using in-memory databases to ensure tests are isolated and fast:

```typescript
import { describe, test, expect, beforeEach, afterEach } from 'bun:test'
import { StrataDBClass, createSchema, type Document, type Collection } from 'stratadb'

// Define your document type for testing
type TestUser = Document<{
  name: string
  email: string
  age: number
  active: boolean
}>

// Create schema for testing
const userSchema = createSchema<TestUser>()
  .field('name', { type: 'TEXT', indexed: true })
  .field('email', { type: 'TEXT', indexed: true, unique: true })
  .field('age', { type: 'INTEGER', indexed: true })
  .build()

describe('User Service', () => {
  let db: StrataDBClass
  let users: Collection<TestUser>

  beforeEach(() => {
    // Create fresh in-memory database for each test
    db = new StrataDBClass({ database: ':memory:' })
    users = db.collection('users', userSchema)
  })

  afterEach(() => {
    // Close database after each test to free resources
    db.close()
  })

  test('should create user successfully', async () => {
    const result = await users.insertOne({
      name: 'Alice',
      email: 'alice@example.com',
      age: 30,
      active: true
    })

    expect(result.document.id).toBeDefined()
    expect(result.document.name).toBe('Alice')
    expect(result.document.email).toBe('alice@example.com')
    expect(result.document.age).toBe(30)
  })
})
```

### Test Utilities

Create reusable test utilities to reduce boilerplate in your test files:

```typescript
// test-utils.ts
import { StrataDBClass } from 'stratadb'
import type { Collection } from 'stratadb'
import type { Document } from 'stratadb'

export interface TestDatabase {
  db: StrataDBClass
  cleanup: () => void
}

/**
 * Creates a fresh in-memory database for testing
 */
export const createTestDatabase = (): TestDatabase => {
  const db = new StrataDBClass({ database: ':memory:' })
  
  return {
    db,
    cleanup: () => db.close()
  }
}

/**
 * Creates a test collection with proper cleanup
 */
export const createTestCollection = <T extends Document<unknown>>(
  db: StrataDBClass,
  collectionName: string,
  schema: any // Your schema object
): Collection<T> => {
  return db.collection(collectionName, schema)
}

/**
 * Test helper that provides database and collection with automatic cleanup
 */
export const withTestDatabase = async <T>(
  collectionName: string,
  schema: any,
  testFn: (collection: Collection<any>) => Promise<T>
): Promise<T> => {
  const { db, cleanup } = createTestDatabase()
  try {
    const collection = createTestCollection(db, collectionName, schema)
    return await testFn(collection)
  } finally {
    cleanup()
  }
}
```

## Unit Testing

### Testing Individual Operations

Test each database operation separately:

```typescript
import { describe, test, expect, beforeEach, afterEach } from 'bun:test'
import { StrataDBClass, createSchema } from 'stratadb'

describe('User Collection Operations', () => {
  type User = Document<{ name: string; email: string }>
  
  const userSchema = createSchema<User>()
    .field('name', { type: 'TEXT', indexed: true })
    .field('email', { type: 'TEXT', indexed: true, unique: true })
    .build()

  let db: StrataDBClass
  let users: Collection<User>

  beforeEach(() => {
    db = new StrataDBClass({ database: ':memory:' })
    users = db.collection('users', userSchema)
  })

  afterEach(() => db.close())

  test('insertOne should create document with ID', async () => {
    const result = await users.insertOne({
      name: 'John Doe',
      email: 'john@example.com'
    })

    expect(result.acknowledged).toBe(true)
    expect(result.document.id).toBeDefined()
    expect(result.document.name).toBe('John Doe')
    expect(result.document.email).toBe('john@example.com')
  })

  test('findById should return document or null', async () => {
    const inserted = await users.insertOne({
      name: 'Jane Doe',
      email: 'jane@example.com'
    })

    // Test finding existing document
    const found = await users.findById(inserted.document.id)
    expect(found).toBeDefined()
    expect(found?.name).toBe('Jane Doe')

    // Test finding non-existent document
    const notFound = await users.findById('non-existent')
    expect(notFound).toBeNull()
  })

  test('find should return documents matching criteria', async () => {
    await users.insertMany([
      { name: 'Alice', email: 'alice@example.com', age: 25 },
      { name: 'Bob', email: 'bob@example.com', age: 30 },
      { name: 'Charlie', email: 'charlie@example.com', age: 35 }
    ])

    const adults = await users.find({ age: { $gte: 30 } })
    expect(adults).toHaveLength(2)
    expect(adults.map(u => u.name)).toContain('Bob')
    expect(adults.map(u => u.name)).toContain('Charlie')
  })

  test('updateOne should modify existing document', async () => {
    const inserted = await users.insertOne({
      name: 'Original Name',
      email: 'test@example.com',
      age: 25
    })

    const updated = await users.updateOne(inserted.document.id, {
      name: 'Updated Name',
      age: 26
    })

    expect(updated.modifiedCount).toBe(1)

    const check = await users.findById(inserted.document.id)
    expect(check?.name).toBe('Updated Name')
    expect(check?.age).toBe(26)
  })

  test('deleteOne should remove document', async () => {
    const inserted = await users.insertOne({
      name: 'To Delete',
      email: 'delete@example.com'
    })

    const deleted = await users.deleteOne(inserted.document.id)
    expect(deleted).toBe(true)

    const check = await users.findById(inserted.document.id)
    expect(check).toBeNull()
  })
})
```

## Integration Testing

### Testing Business Logic

Test your application's business logic that uses StrataDB:

```typescript
// user-service.ts
type User = Document<{ name: string; email: string; active: boolean }>

export class UserService {
  constructor(private users: Collection<User>) {}

  async createUser(userData: Omit<User, 'id' | 'createdAt' | 'updatedAt'>) {
    // Check if user already exists
    const existing = await this.users.findOne({ email: userData.email })
    if (existing) {
      throw new Error('User already exists')
    }

    return await this.users.insertOne(userData)
  }

  async activateUser(userId: string) {
    const user = await this.users.findById(userId)
    if (!user) {
      throw new Error('User not found')
    }

    if (user.active) {
      throw new Error('User already active')
    }

    await this.users.updateOne(userId, { active: true })
    return await this.users.findById(userId)
  }

  async getActiveUsers() {
    return await this.users.find({ active: true })
  }
}

// user-service.test.ts
import { describe, test, expect, beforeEach, afterEach } from 'bun:test'
import { UserService } from './user-service'
import { StrataDBClass, createSchema } from 'stratadb'

describe('UserService', () => {
  type User = Document<{ name: string; email: string; active: boolean }>
  
  const userSchema = createSchema<User>()
    .field('email', { type: 'TEXT', indexed: true, unique: true })
    .field('active', { type: 'INTEGER', indexed: true }) // INTEGER for boolean in SQLite
    .build()

  let db: StrataDBClass
  let users: Collection<User>
  let userService: UserService

  beforeEach(() => {
    db = new StrataDBClass({ database: ':memory:' })
    users = db.collection('users', userSchema)
    userService = new UserService(users)
  })

  afterEach(() => db.close())

  test('createUser should create new user', async () => {
    const result = await userService.createUser({
      name: 'New User',
      email: 'newuser@example.com',
      active: false
    })

    expect(result.document.id).toBeDefined()
    expect(result.document.name).toBe('New User')
    expect(result.document.email).toBe('newuser@example.com')
  })

  test('createUser should throw error for duplicate email', async () => {
    await userService.createUser({
      name: 'First User',
      email: 'unique@example.com',
      active: false
    })

    await expect(async () => {
      await userService.createUser({
        name: 'Second User',
        email: 'unique@example.com', // Same email
        active: false
      })
    }).rejects.toThrow('User already exists')
  })

  test('activateUser should activate inactive user', async () => {
    const user = await userService.createUser({
      name: 'Inactive User', 
      email: 'inactive@example.com',
      active: false
    })

    const activated = await userService.activateUser(user.document.id)

    expect(activated?.active).toBe(true)
  })

  test('activateUser should throw error for non-existent user', async () => {
    await expect(async () => {
      await userService.activateUser('non-existent-id')
    }).rejects.toThrow('User not found')
  })

  test('getActiveUsers should return only active users', async () => {
    await userService.createUser({
      name: 'Active User 1',
      email: 'active1@example.com',
      active: true
    })
    
    await userService.createUser({
      name: 'Active User 2',
      email: 'active2@example.com', 
      active: true
    })
    
    await userService.createUser({
      name: 'Inactive User',
      email: 'inactive@example.com',
      active: false
    })

    const activeUsers = await userService.getActiveUsers()
    expect(activeUsers).toHaveLength(2)
    expect(activeUsers.every(u => u.active)).toBe(true)
  })
})
```

## Testing Validation

### Testing Schema Validation

Test how your validation logic works:

```typescript
import { describe, test, expect, beforeEach, afterEach } from 'bun:test'
import { StrataDBClass, createSchema, wrapStandardSchema } from 'stratadb'
import { z } from 'zod'

describe('Validation Testing', () => {
  type User = Document<{ name: string; email: string; age: number }>

  // Create a Zod schema for validation
  const ZodUser = z.object({
    id: z.string(),
    name: z.string().min(1).max(50),
    email: z.string().email(),
    age: z.number().int().min(0).max(150),
    createdAt: z.number(),
    updatedAt: z.number()
  })

  const userSchema = createSchema<User>()
    .field('name', { type: 'TEXT', indexed: true })
    .field('email', { type: 'TEXT', indexed: true, unique: true })
    .validate(wrapStandardSchema<User>(ZodUser))
    .build()

  let db: StrataDBClass
  let users: Collection<User>

  beforeEach(() => {
    db = new StrataDBClass({ database: ':memory:' })
    users = db.collection('users', userSchema)
  })

  afterEach(() => db.close())

  test('should accept valid documents', async () => {
    const result = await users.insertOne({
      name: 'Valid User',
      email: 'valid@example.com',
      age: 30
    })
    expect(result.document.id).toBeDefined()
  })

  test('should reject invalid email format', async () => {
    await expect(async () => {
      await users.insertOne({
        name: 'Invalid Email User',
        email: 'not-an-email', // Invalid email
        age: 30
      })
    }).rejects.toThrow() // Should throw validation error
  })

  test('should reject invalid age', async () => {
    await expect(async () => {
      await users.insertOne({
        name: 'Invalid Age User',
        email: 'valid2@example.com',
        age: -5 // Invalid age
      })
    }).rejects.toThrow() // Should throw validation error
  })

  test('should validate with custom validation function', () => {
    // Test validation in isolation
    if (userSchema.validate) {
      const validDoc = {
        id: 'test-id',
        name: 'Valid Name',
        email: 'valid@example.com',
        age: 30,
        createdAt: 123,
        updatedAt: 123
      }
      
      const invalidDoc = {
        id: 'test-id',
        name: '', // Invalid - empty name
        email: 'valid@example.com',
        age: 30,
        createdAt: 123,
        updatedAt: 123
      }
      
      expect(userSchema.validate(validDoc)).toBe(true)
      expect(userSchema.validate(invalidDoc)).toBe(false)
    }
  })
})
```

## Mocking and Stubbing

### Creating Database Mocks

Sometimes you may want to mock the database for unit tests of dependent code:

```typescript
import type { Collection } from 'stratadb'
import type { Document } from 'stratadb'

// Mock collection for unit testing without database
export const createMockCollection = <T extends Document<unknown>>(): Collection<T> => {
  const items: T[] = []
  
  return {
    name: 'mock-collection',
    schema: {} as any,
    
    // Mock insertOne
    insertOne: async (doc: any) => {
      const newDoc = {
        ...doc,
        id: `mock-id-${Date.now()}-${Math.random()}`,
        createdAt: Date.now(),
        updatedAt: Date.now()
      }
      items.push(newDoc)
      return {
        acknowledged: true as const,
        document: newDoc
      }
    },
    
    // Mock findById
    findById: async (id: string) => {
      return items.find(item => item.id === id) || null
    },
    
    // Mock find
    find: async (filter: any) => {
      let result = [...items]
      
      // Simple filter implementation for mocks
      if (filter && typeof filter === 'object') {
        for (const [key, value] of Object.entries(filter)) {
          if (typeof value === 'object' && value !== null) {
            // Handle operators like { $gte: 18 }
            const operator = Object.keys(value)[0]
            const operand = Object.values(value)[0]
            
            if (operator === '$gte') {
              result = result.filter(item => (item as any)[key] >= operand)
            } else if (operator === '$lte') {
              result = result.filter(item => (item as any)[key] <= operand)
            }
            // Add more operators as needed for your tests
          } else {
            // Simple equality filter
            result = result.filter(item => (item as any)[key] === value)
          }
        }
      }
      
      return result
    },
    
    // Add other methods as needed
    findOne: async (filter: any) => {
      const results = await (this as Collection<T>).find(filter)
      return results[0] || null
    },
    
    updateOne: async (id: string, update: any) => {
      const index = items.findIndex(item => item.id === id)
      if (index === -1) {
        return { matchedCount: 0, modifiedCount: 0 }
      }
      
      items[index] = {
        ...items[index],
        ...update,
        updatedAt: Date.now()
      }
      
      return { matchedCount: 1, modifiedCount: 1 }
    },
    
    deleteOne: async (id: string) => {
      const initialLength = items.length
      const newItems = items.filter(item => item.id !== id)
      const deleted = initialLength > newItems.length
      items.length = 0  // Clear array
      items.push(...newItems)  // Add back filtered items
      return deleted
    },
    
    count: async (filter: any) => {
      const results = await (this as Collection<T>).find(filter)
      return results.length
    }
  } as Collection<T>
}

// Usage in tests
import { describe, test, expect } from 'bun:test'

describe('UserService with Mock Database', () => {
  test('should create user with mock database', async () => {
    const mockUsers = createMockCollection<User>()
    const userService = new UserService(mockUsers)
    
    const result = await userService.createUser({
      name: 'Mock User',
      email: 'mock@example.com',
      active: true
    })
    
    expect(result.document.id).toBeDefined()
    expect(result.document.name).toBe('Mock User')
  })
})
```

## Performance Testing

### Testing Query Performance

Add performance testing to ensure your queries remain efficient:

```typescript
import { describe, test, expect } from 'bun:test'

describe('Performance Testing', () => {
  test('large dataset performance', async () => {
    // This is just an example - you'd implement with real database
    const startTime = performance.now()
    
    // Insert large dataset
    // Perform operations
    // Measure time
    
    const endTime = performance.now()
    const executionTime = endTime - startTime
    
    // Assert performance requirements
    expect(executionTime).toBeLessThan(1000) // Should complete in under 1 second
  })
})
```

## Test Patterns and Anti-Patterns

### Good Patterns

1. **Always use in-memory databases** for tests
2. **Create fresh database for each test** to ensure isolation
3. **Clean up database connections** after tests
4. **Test both positive and negative cases** for validation
5. **Use descriptive test names** that indicate expected behavior

### Anti-Patterns to Avoid

```typescript
// ❌ Don't use shared databases between tests
let sharedDb: StrataDBClass
let sharedUsers: Collection<User>

beforeAll(() => {
  sharedDb = new StrataDBClass({ database: './shared-test.db' }) // Shared state
  sharedUsers = sharedDb.collection('users', userSchema)
})

// ❌ Don't forget to close databases
test('some test', async () => {
  const db = new StrataDBClass({ database: ':memory:' })
  const users = db.collection('users', userSchema)
  // Using db and users but never closing
  // This can cause resource leaks
})

// ❌ Don't test implementation details
test('should call database.insertOne', () => {
  // Testing internal implementation rather than behavior
  // Use integration tests instead
})
```

## Running Tests

### Test Configuration

For Bun tests with StrataDB:

```json
// In package.json or similar configuration
{
  "scripts": {
    "test": "bun test",
    "test:watch": "bun test --watch",
    "test:coverage": "bun test --coverage"
  }
}
```

### Parallel Testing

Bun supports parallel test execution. Since each test uses an isolated in-memory database, tests can safely run in parallel:

```bash
# Run tests in parallel (Bun default behavior)
bun test

# Or explicitly
bun test --parallel
```

By following these testing patterns and best practices, you can ensure your StrataDB applications are well-tested and reliable.