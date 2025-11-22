# Security Best Practices

This guide covers security best practices for using StrataDB safely in production applications.

## Database File Security

### File Permissions

SQLite database files are regular files that need proper permissions:

```bash
# Set appropriate file permissions (readable/writable only by application user)
chmod 600 myapp.db

# Or if running under specific user context
sudo chown appuser:appuser myapp.db
chmod 600 myapp.db
```

### Database File Location

```typescript
// ✅ Good: Store database in protected directory
const db = new StrataDBClass({
  database: '/var/lib/myapp/data.db'  // Protected system directory
})

// ❌ Avoid: Database in public or temporary directories
const db = new StrataDBClass({
  database: './public/db.db'      // Accessible via web server
  // or
  database: '/tmp/app.db'         // World-readable temporary directory
})
```

### Backup Security

Database backups contain sensitive information and must be properly secured:

```bash
# Encrypt backup files
tar -czf - myapp.db | gpg --cipher-algo AES256 --compress-algo 1 --symmetric --output myapp.db.tar.gz.gpg

# Or use SQLite's built-in backup within application code
import { Database as SQLiteDatabase } from 'bun:sqlite'

const mainDb = new SQLiteDatabase('main.db')
const backupDb = new SQLiteDatabase('backup.db', { encrypted: true })
mainDb.backup(backupDb)  // Note: actual encryption requires additional setup
```

## Input Validation and Sanitization

### Query Parameter Validation

StrataDB uses parameterized queries by default, which prevents SQL injection. However, you should still validate input:

```typescript
import { z } from 'zod'

const userIdSchema = z.string().regex(/^[a-zA-Z0-9-_]{1,50}$/) // Safe pattern for IDs

const getUserById = async (unsafeId: string) => {
  // Validate the input first
  const userId = userIdSchema.parse(unsafeId)
  
  // Safe to use in query
  return await users.findById(userId)
}
```

### Dynamic Query Building

When building dynamic queries, validate the structure:

```typescript
// ❌ Don't allow direct field access without validation
const unsafeFind = async (query: unknown) => {
  // This could be dangerous if query contains malicious operators
  return await users.find(query as any)
}

// ✅ Validate query structure
const safeFind = async (query: unknown) => {
  // Use a schema to validate the query structure
  const validatedQuery = validateQuerySchema(query)
  return await users.find(validatedQuery)
}

// For search functionality, validate field names
const searchUsers = async (searchField: string, searchTerm: string) => {
  // Only allow searching on pre-approved fields
  const allowedFields = ['name', 'email', 'username'] as const
  if (!allowedFields.includes(searchField as any)) {
    throw new Error('Invalid search field')
  }
  
  const filter = { [searchField]: { $like: `%${searchTerm}%` } }
  return await users.find(filter)
}
```

## Authentication and Authorization

### Role-Based Access Control

Implement proper authentication and authorization around StrataDB operations:

```typescript
type UserRole = 'admin' | 'user' | 'guest'

interface AuthenticatedUser {
  id: string
  role: UserRole
}

// Middleware or wrapper function
const authorizedUsersOperations = {
  async find(user: AuthenticatedUser, filter: QueryFilter<User>) {
    if (user.role === 'guest') {
      // Guests can only see public information
      return await users.find({ ...filter, public: true })
    }
    
    if (user.role === 'user') {
      // Users can only see their own data or public data
      return await users.find({
        $or: [
          { ownerId: user.id },  // Their own documents
          { public: true }       // Public documents
        ]
      })
    }
    
    // Admin can see all data
    return await users.find(filter)
  },
  
  async update(user: AuthenticatedUser, id: string, update: DocumentUpdate<User>) {
    if (user.role === 'admin') {
      return await users.updateOne(id, update)
    }
    
    // Regular users can only update their own documents
    const existing = await users.findById(id)
    if (existing && existing.ownerId === user.id) {
      return await users.updateOne(id, update)
    }
    
    throw new Error('Unauthorized access')
  }
}
```

### Data Access Patterns

Prevent unauthorized data access by implementing proper access controls:

```typescript
// ❌ Don't do this - allows users to access any document
const getUser = async (userId: string) => {
  return await users.findById(userId) // Any user can fetch any document
}

// ✅ Do this - validate ownership first
const getMyUser = async (currentUserId: string, requestedUserId: string) => {
  if (currentUserId !== requestedUserId) {
    throw new Error('Unauthorized: Cannot access other user data')
  }
  
  return await users.findById(requestedUserId)
}
```

## Injection Prevention

### SQL Injection

StrataDB is designed to prevent SQL injection through parameterized queries, but be careful with raw SQL:

```typescript
// ✅ StrataDB queries are safe by default
await users.find({ email: userInput }) // Safe - parameterized

// ❌ Avoid raw SQL unless absolutely necessary
const badExample = async (userInput: string) => {
  // Never do this - opens SQL injection
  const result = db.prepare(`SELECT * FROM users WHERE email = '${userInput}'`).all()
}

// ✅ If you must use raw SQL, use parameterized queries
const safeRawQuery = async (email: string) => {
  const result = db.prepare('SELECT * FROM users WHERE email = ?').all(email)
}
```

### Command Injection

When using StrataDB in systems that execute commands, prevent command injection:

```typescript
// ❌ Dangerous if userId comes from user input
const dangerousBackup = async (userId: string) => {
  // This could inject shell commands if userId contains command characters
  const command = `cp /db/${userId}.db /backup/${userId}.db.backup`
  // Don't execute this directly
}

// ✅ Validate and sanitize inputs
const safeBackup = async (userId: string) => {
  // Validate the userId pattern
  if (!/^[a-zA-Z0-9_-]+$/.test(userId)) {
    throw new Error('Invalid user ID format')
  }
  
  // Use safe file operations
  await Bun.write(`./backup/${userId}.db.backup`, await Bun.file(`./db/${userId}.db`))
}
```

## Sensitive Data Handling

### Data Encryption at Rest

While SQLite provides basic file security, you may want additional encryption for sensitive data:

```typescript
import { createCipheriv, createDecipheriv } from 'crypto'

// Function to encrypt sensitive fields
const encryptField = (data: string, key: Buffer, iv: Buffer): string => {
  const cipher = createCipheriv('aes-256-cbc', key, iv)
  let encrypted = cipher.update(data, 'utf8', 'hex')
  encrypted += cipher.final('hex')
  return encrypted
}

// Function to decrypt sensitive fields
const decryptField = (encryptedData: string, key: Buffer, iv: Buffer): string => {
  const decipher = createDecipheriv('aes-256-cbc', key, iv)
  let decrypted = decipher.update(encryptedData, 'hex', 'utf8')
  decrypted += decipher.final('utf8')
  return decrypted
}

// Example schema with encrypted fields
type SecureUser = Document<{
  name: string  // Public field
  encryptedSsn: string  // Encrypted sensitive field
}>

const secureSchema = createSchema<SecureUser>()
  .field('name', { type: 'TEXT', indexed: true })
  .build()

// Store encrypted data
const createSecureUser = async (name: string, ssn: string) => {
  const encryptedSsn = encryptField(ssn, encryptionKey, iv)
  return await secureUsers.insertOne({
    name,
    encryptedSsn
  })
}

// Retrieve and decrypt data
const getSecureUser = async (id: string) => {
  const user = await secureUsers.findById(id)
  if (user) {
    return {
      ...user,
      ssn: decryptField(user.encryptedSsn, encryptionKey, iv)
    }
  }
  return user
}
```

### Data Masking for Logging

Never log sensitive data directly:

```typescript
// ❌ Never log sensitive information directly
const badLog = async (user: User) => {
  console.log('User login:', user) // Might log passwords, SSN, etc.
}

// ✅ Mask sensitive fields before logging
const safeLog = async (user: User) => {
  const safeUser = {
    id: user.id,
    email: maskEmail(user.email), // Hide full email
    name: user.name
    // Don't include sensitive fields like ssn, password, etc.
  }
  console.log('User login:', safeUser)
}

const maskEmail = (email: string): string => {
  const [local, domain] = email.split('@')
  return `${local.substring(0, 2)}***@${domain}`
}
```

## API Security

### Rate Limiting

Protect against abuse by implementing rate limiting around database operations:

```typescript
// Simple in-memory rate limiter
const rateLimits = new Map<string, { count: number, resetTime: number }>()

const checkRateLimit = (identifier: string, maxRequests: number, windowMs: number): boolean => {
  const now = Date.now()
  const limit = rateLimits.get(identifier)
  
  if (!limit || now > limit.resetTime) {
    rateLimits.set(identifier, { 
      count: 1, 
      resetTime: now + windowMs 
    })
    return true
  }
  
  if (limit.count >= maxRequests) {
    return false // Rate limit exceeded
  }
  
  rateLimits.set(identifier, {
    count: limit.count + 1,
    resetTime: limit.resetTime
  })
  
  return true
}

// Use in your API endpoints
const createUser = async (req: Request) => {
  const clientIp = req.headers.get('X-Forwarded-For') || req.headers.get('X-Real-IP') || 'unknown'
  
  if (!checkRateLimit(clientIp, 10, 60000)) { // 10 requests per minute
    throw new Error('Rate limit exceeded')
  }
  
  // Proceed with database operation
  return await users.insertOne(req.body)
}
```

### Input Size Limits

Prevent attacks through very large documents:

```typescript
// Validate document size before insertion
const MAX_DOC_SIZE = 1024 * 1024 // 1MB max size
const insertWithSizeCheck = async (doc: User) => {
  const serialized = JSON.stringify(doc)
  const size = new Blob([serialized]).size
  
  if (size > MAX_DOC_SIZE) {
    throw new Error(`Document too large: ${size} bytes, max allowed is ${MAX_DOC_SIZE}`)
  }
  
  return await users.insertOne(doc)
}
```

## Configuration Security

### Environment Variables

Secure database configuration:

```typescript
// ✅ Load sensitive config from environment variables
const db = new StrataDBClass({
  database: process.env.DB_PATH || './app.db',
  // For production, use absolute paths that are secured
  onClose: () => console.log('Database closed securely')
})

// Don't hardcode sensitive paths
// ❌ const db = new StrataDBClass({ database: './db.sqlite' }) // Path might be guessable
// ✅ const db = new StrataDBClass({ database: process.env.DB_PATH }) // Secure
```

## Audit and Monitoring

### Access Logging

Keep track of database access for security monitoring:

```typescript
// Audit wrapper for database operations
class AuditedCollection<T extends Document<unknown>> {
  constructor(private collection: Collection<T>, private logger: any) {}
  
  async insertOne(doc: DocumentInput<T>, userId?: string) {
    this.logger.info('DB Insert', { 
      collection: this.collection.name, 
      userId,
      timestamp: new Date().toISOString()
    })
    
    return await this.collection.insertOne(doc)
  }
  
  async updateOne(id: string, update: DocumentUpdate<T>, userId?: string) {
    this.logger.info('DB Update', { 
      collection: this.collection.name, 
      documentId: id,
      userId,
      timestamp: new Date().toISOString()
    })
    
    return await this.collection.updateOne(id, update)
  }
  
  // ... other methods
}

// Usage
const auditedUsers = new AuditedCollection(users, logger)
```

## Common Security Mistakes to Avoid

1. **Never expose database files**: Don't make database files accessible via web servers
2. **Validate user inputs**: Always validate before using in queries
3. **Don't store secrets**: Don't store passwords, API keys, or other secrets without encryption
4. **Use HTTPS**: Always use HTTPS in production to encrypt data in transit
5. **Regular backups**: Maintain secure, encrypted backups for disaster recovery
6. **Update regularly**: Keep StrataDB and Bun updated for security patches

By following these security best practices, you can ensure that your StrataDB implementation is secure and resilient against common attack vectors.