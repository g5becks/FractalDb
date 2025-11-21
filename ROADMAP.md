# StrataDB Roadmap

This document outlines future features and enhancements that are not part of the initial release but are planned for future versions.

## Version 2.0 - Production Features

### Schema Migration System
Support for evolving document schemas over time without data loss.

```typescript
// Define migrations
await db.migrate('users', {
  version: 2,
  up: async (doc) => {
    // Transform v1 schema to v2
    const [first, ...last] = doc.name.split(' ');
    return {
      ...doc,
      name: { first, last: last.join(' ') }
    };
  },
  down: async (doc) => {
    // Rollback v2 to v1
    return {
      ...doc,
      name: `${doc.name.first} ${doc.name.last}`
    };
  }
});

// Run migrations
await db.runMigrations();
```

**Implementation Notes:**
- Store migration version in collection metadata
- Support both forward and backward migrations
- Ability to migrate in batches to avoid locking
- Generate TypeScript types for each schema version

---

### Soft Delete Support
Mark documents as deleted without removing them from the database.

```typescript
interface SoftDeletable {
  deletedAt?: Date;
  deletedBy?: string;
}

const users = db.collection<User & SoftDeletable>('users',
  createSchema<User>()
    .softDelete(true) // Enable soft delete
    .build()
);

// deleteOne/deleteMany set deletedAt instead of removing
await users.deleteOne('user-id'); // Sets deletedAt: new Date()

// Find excludes soft-deleted by default
await users.find({ status: 'active' }); // Excludes deletedAt !== null

// Include soft-deleted explicitly
await users.find({ status: 'active' }, { includeSoftDeleted: true });

// Restore soft-deleted document
await users.restore('user-id'); // Sets deletedAt: null

// Permanently delete
await users.hardDelete('user-id'); // Actually removes from database
```

---

### Cursor-Based Pagination
More efficient pagination for large datasets than skip/limit.

```typescript
// First page
const page1 = await users.findWithCursor(
  { status: 'active' },
  { limit: 20, sort: { createdAt: -1 } }
);

// Next page using cursor
const page2 = await users.findWithCursor(
  { status: 'active' },
  {
    limit: 20,
    after: page1.cursor, // Opaque cursor token
    sort: { createdAt: -1 }
  }
);

// Result includes cursor for next page
console.log(page1);
// {
//   documents: [...],
//   hasNext: true,
//   cursor: 'eyJpZCI6InVzZXItMTAwIiwiY3JlYXRlZEF0IjoxNzA...'
// }
```

---

### Query Builder API
Alternative fluent API for those who prefer method chaining over object literals.

```typescript
// Object literal style (current)
await users.find({
  $and: [
    { age: { $gte: 18 } },
    { status: 'active' },
    { tags: { $in: ['developer', 'designer'] } }
  ]
});

// Fluent builder style (future)
await users.query()
  .where('age').gte(18)
  .where('status').equals('active')
  .where('tags').in(['developer', 'designer'])
  .sort('createdAt', -1)
  .limit(20)
  .execute();
```

---

### Default Values
Automatically populate fields with default values when documents are inserted.

```typescript
const posts = db.collection<Post>('posts',
  createSchema<Post>()
    .field('status', {
      path: '$.status',
      type: 'TEXT',
      default: 'draft' // Auto-populate if not provided
    })
    .field('views', {
      path: '$.views',
      type: 'INTEGER',
      default: 0
    })
    .build()
);

// Insert without status/views
await posts.insertOne({ title: 'Hello World' });
// Result: { id: '...', title: 'Hello World', status: 'draft', views: 0 }
```

---

## Version 2.5 - Advanced Features

### Partial Indexes
Create indexes on subset of documents to save space and improve performance.

```typescript
const users = db.collection<User>('users',
  createSchema<User>()
    .field('status', { path: '$.status', type: 'TEXT' })
    .partialIndex('active_users', ['status'], {
      where: { status: 'active' } // Only index active users
    })
    .build()
);

// Queries on active users use the partial index
await users.find({ status: 'active', age: { $gt: 18 } });
```

---

### Full-Text Search
SQLite FTS5 integration for text search capabilities.

```typescript
const posts = db.collection<Post>('posts',
  createSchema<Post>()
    .field('title', { path: '$.title', type: 'TEXT' })
    .field('content', { path: '$.content', type: 'TEXT' })
    .fullTextSearch(['title', 'content']) // Enable FTS5
    .build()
);

// Full-text search queries
await posts.search('typescript database', {
  fields: ['title', 'content'],
  highlight: true,
  limit: 10
});
// Returns: { title: '<b>TypeScript</b> for <b>databases</b>', ... }
```

---

### Relationships & References
Support for document references with automatic population.

```typescript
interface Post extends BaseDocument {
  title: string;
  authorId: string; // Reference to User
}

const posts = db.collection<Post>('posts',
  createSchema<Post>()
    .field('authorId', {
      path: '$.authorId',
      type: 'TEXT',
      ref: 'users' // Reference to users collection
    })
    .build()
);

// Populate references
const post = await posts.findOne(
  { id: 'post-1' },
  { populate: ['authorId'] } // Joins with users collection
);
// Result: { ..., author: { id: 'user-1', name: 'Alice', email: '...' } }

// Cascading deletes
await users.deleteOne('user-1', { cascade: ['posts'] });
// Deletes user and all their posts
```

---

### Aggregation Pipeline
MongoDB-style aggregation for complex data transformations.

```typescript
const results = await users.aggregate([
  { $match: { status: 'active' } },
  { $group: {
      _id: '$country',
      count: { $count: {} },
      avgAge: { $avg: '$age' }
    }
  },
  { $sort: { count: -1 } },
  { $limit: 10 }
]);
// Result: [{ _id: 'USA', count: 1500, avgAge: 32.5 }, ...]
```

---

## Version 3.0 - Enterprise Features

### Observability & Monitoring

#### Query Performance Metrics
```typescript
// Enable metrics collection
using db = new StrataDB({
  database: './app.db',
  metrics: {
    enabled: true,
    slowQueryThreshold: 100 // Log queries >100ms
  }
});

// Get metrics
const metrics = await db.getMetrics();
console.log(metrics);
// {
//   queries: {
//     total: 10_000,
//     avgExecutionTime: 2.3,
//     p50: 1.5,
//     p95: 8.2,
//     p99: 15.7,
//     slowQueries: 23
//   },
//   collections: {
//     users: { documents: 1000, size: '10MB', avgQueryTime: 2.1 },
//     posts: { documents: 5000, size: '50MB', avgQueryTime: 3.2 }
//   },
//   indexes: {
//     idx_users_email: { hitRate: 0.95, size: '2MB' },
//     idx_users_age: { hitRate: 0.12, size: '500KB' } // Low hit rate!
//   }
// }
```

#### Slow Query Log
```typescript
db.on('slowQuery', (event) => {
  logger.warn('Slow query detected', {
    collection: event.collection,
    query: event.query,
    executionTime: event.executionTime,
    timestamp: event.timestamp
  });
});
```

#### Index Recommendations
```typescript
const recommendations = await db.analyzeQueries({ days: 7 });
console.log(recommendations);
// [
//   {
//     type: 'CREATE_INDEX',
//     collection: 'users',
//     field: 'status',
//     reason: 'Field queried in 80% of queries but not indexed',
//     estimatedImprovement: '60% faster queries'
//   },
//   {
//     type: 'DROP_INDEX',
//     collection: 'users',
//     index: 'idx_users_age',
//     reason: 'Index hit rate <15% over 7 days',
//     estimatedSavings: '500KB disk space'
//   }
// ]
```

---

### Backup & Recovery

#### Incremental Backups
```typescript
// Full backup
await db.backup('./backups/full-2024-01-15.sqlite');

// Incremental backup (WAL checkpoint)
await db.backup('./backups/incremental-2024-01-15-12h.wal', {
  incremental: true,
  compress: true
});

// Point-in-time recovery
await db.restore('./backups/full-2024-01-15.sqlite', {
  pointInTime: new Date('2024-01-15T14:30:00Z'),
  incrementalBackups: ['./backups/incremental-*.wal']
});
```

#### Export/Import
```typescript
// Export collection to JSONL
await users.export('./exports/users.jsonl', {
  format: 'jsonl', // One JSON object per line
  filter: { status: 'active' },
  compress: true
});

// Import from JSONL
await users.import('./exports/users.jsonl', {
  format: 'jsonl',
  upsert: true, // Update existing, insert new
  validate: true // Run schema validation
});
```

---

### Security Features

#### Row-Level Security
```typescript
const users = db.collection<User>('users', schema, {
  rowLevelSecurity: (doc, context) => {
    // Only return users in same organization
    return doc.organizationId === context.user.organizationId;
  }
});

// All queries automatically filtered by RLS
const results = await users.find(
  { status: 'active' },
  { context: { user: currentUser } }
);
// Only returns users in currentUser's organization
```

#### Audit Logging
```typescript
const auditLog = db.collection<AuditLog>('audit_log', schema);

db.on('write', async (event) => {
  await auditLog.insertOne({
    collection: event.collection,
    operation: event.operation, // 'insert', 'update', 'delete'
    documentId: event.documentId,
    userId: event.context?.userId,
    timestamp: new Date(),
    changes: event.changes // Before/after values
  });
});
```

#### Field Masking
```typescript
db.on('query', (event) => {
  // Automatically redact sensitive fields in logs
  const sanitized = maskSensitiveFields(event.query, ['password', 'ssn', 'creditCard']);
  logger.info('Query executed', { query: sanitized });
});
```

---

## Version 4.0 - Distributed Features

### Replication
SQLite replication support for read replicas.

```typescript
// Primary database
const primary = new StrataDB({
  database: './primary.db',
  role: 'primary',
  replication: {
    replicas: ['./replica1.db', './replica2.db']
  }
});

// Replica database (read-only)
const replica = new StrataDB({
  database: './replica1.db',
  role: 'replica',
  replication: {
    primary: './primary.db',
    syncInterval: 5000 // Sync every 5 seconds
  }
});

// Writes go to primary
await primary.collection('users').insertOne({ name: 'Alice' });

// Reads can use replica
await replica.collection('users').find({ status: 'active' });
```

---

### Multi-Tenancy
Built-in support for multi-tenant applications.

```typescript
const db = new StrataDB({
  database: './app.db',
  multiTenant: {
    enabled: true,
    tenantIdField: 'tenantId'
  }
});

// All operations automatically scoped to tenant
const users = db.collection<User>('users', schema);

// Queries automatically filtered by tenant
await users.find(
  { status: 'active' },
  { tenant: 'tenant-123' }
);
// Adds WHERE tenantId = 'tenant-123' to query
```

---

## Community Requests

Features requested by the community that are under consideration:

1. **GraphQL Integration**: Auto-generate GraphQL schema from collections
2. **REST API Generator**: Auto-generate REST endpoints
3. **Real-time Subscriptions**: WebSocket-based change notifications
4. **Encryption at Rest**: Transparent database encryption (SQLCipher)
5. **Time-Series Support**: Optimized storage for time-series data
6. **Geospatial Queries**: Support for geo queries (within radius, etc.)
7. **Schema Versioning UI**: Web UI for managing schema versions
8. **Query Playground**: Interactive query builder with live preview
9. **VS Code Extension**: IntelliSense for queries, schema validation

---

## How to Contribute

We welcome contributions! Here's how you can help:

1. **Vote on features**: Star features you'd like to see in GitHub Discussions
2. **Submit proposals**: Open an issue with detailed use cases
3. **Contribute code**: Pick up features marked "help wanted"
4. **Share use cases**: Tell us how you're using StrataDB

## Feature Request Template

When requesting a feature, please include:

1. **Use case**: What problem does this solve?
2. **API proposal**: How should the API look?
3. **Alternatives**: What workarounds exist today?
4. **Impact**: How many users would benefit?

---

**Note**: This roadmap is subject to change based on community feedback and project priorities. Features may be added, removed, or re-prioritized.
