# Indexes

Indexes dramatically improve query performance by creating B-tree structures for fast lookups.

## How Indexes Work in StrataDB

StrataDB stores documents as JSONB but creates **generated columns** for indexed fields:

```sql
-- What StrataDB creates for an indexed 'email' field:
CREATE TABLE users (
  _id TEXT PRIMARY KEY,
  body BLOB NOT NULL,                    -- JSONB document
  createdAt INTEGER,                     -- if timestamps enabled
  updatedAt INTEGER,                     -- if timestamps enabled
  _email TEXT GENERATED ALWAYS AS (jsonb_extract(body, '$.email')) VIRTUAL
);
CREATE UNIQUE INDEX idx_users_email ON users(_email);
```

This gives you:
- **Document flexibility**: Store any JSON structure
- **Query performance**: B-tree indexes on specific fields
- **Zero storage overhead**: Generated columns are VIRTUAL

## Creating Indexes

```typescript
const userSchema = createSchema<User>()
  // Indexed field - creates generated column + index
  .field('email', { type: 'TEXT', indexed: true })

  // Non-indexed field - stored in JSONB only
  .field('bio', { type: 'TEXT' })

  .build()
```

## Index Types

### Single Field Index

```typescript
.field('email', { type: 'TEXT', indexed: true })
```

**Good for:**
- Exact match: `{ email: 'alice@example.com' }`
- Range queries: `{ age: { $gte: 18 } }`
- Sorting: `{ sort: { createdAt: -1 } }`

### Unique Index

```typescript
.field('email', { type: 'TEXT', indexed: true, unique: true })
```

Enforces uniqueness and improves lookup performance.

### Compound Index

```typescript
.compoundIndex('age_status', ['age', 'status'])
```

**Good for queries on multiple fields:**

```typescript
// ✅ Uses compound index (left-to-right)
await users.find({ age: 30, status: 'active' })
await users.find({ age: { $gte: 18 } })

// ❌ Cannot use compound index (doesn't start with 'age')
await users.find({ status: 'active' })
```

### Unique Compound Index

```typescript
// One email per tenant
.compoundIndex('email_tenant', ['email', 'tenantId'], { unique: true })
```

## Performance Comparison

| Query Type | Indexed | Non-indexed |
|------------|---------|-------------|
| Exact match | O(log n) | O(n) |
| Range query | O(log n + k) | O(n) |
| Sort | O(n log n) using index | O(n log n) |
| Full scan | O(n) | O(n) |

Where `n` = total documents, `k` = matching documents

## When to Index

**Do index:**
- Fields used in `find()` filters frequently
- Fields used for sorting
- Fields with unique constraints
- Foreign key-like references

**Don't index:**
- Fields rarely queried
- Fields with low cardinality (e.g., boolean)
- Large text fields (use full-text search instead)
- Fields only used in returned data, not filters

## Checking Index Usage

StrataDB automatically uses indexes for queries on indexed fields. For debugging, you can access the underlying SQLite database:

```typescript
// Get query plan
const plan = db.sqliteDb
  .prepare('EXPLAIN QUERY PLAN SELECT * FROM users WHERE _email = ?')
  .all('alice@example.com')

console.log(plan)
// Shows: SEARCH users USING INDEX idx_users_email (_email=?)
```

## Index Limitations

1. **Nested field indexes require explicit paths:**
   ```typescript
   .field('authorName', {
     path: '$.author.name',
     type: 'TEXT',
     indexed: true
   })
   ```

2. **Array field indexing is limited** - consider denormalization

3. **Indexes add write overhead** - each insert/update must update indexes
