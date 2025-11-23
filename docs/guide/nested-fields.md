# Nested Field Indexing

StrataDB supports indexing nested properties using **dot notation**, allowing you to create fast indexes on fields deep within your document structure.

## Overview

When defining schemas, you can use dot notation in field names to reference nested properties. StrataDB automatically converts these to JSON path expressions and creates optimized generated columns for fast querying.

## Basic Usage

### Top-Level Fields

For simple top-level fields, use the field name directly:

```typescript
import { createSchema, type Document } from 'stratadb'

type User = Document<{
  name: string
  email: string
  age: number
}>

const userSchema = createSchema<Omit<User, '_id' | 'createdAt' | 'updatedAt'>>()
  .field('name', { type: 'TEXT', indexed: true })
  .field('email', { type: 'TEXT', indexed: true, unique: true })
  .field('age', { type: 'INTEGER', indexed: true })
  .build()
```

### Nested Fields

For nested properties, use dot notation in the field name:

```typescript
type User = Document<{
  name: string
  profile?: {
    bio: string
    avatar: string
    location: string
  }
}>

const userSchema = createSchema<Omit<User, '_id' | 'createdAt' | 'updatedAt'>>()
  .field('name', { type: 'TEXT', indexed: true })
  
  // Nested fields with dot notation
  .field('profile.bio', { type: 'TEXT', indexed: false })
  .field('profile.avatar', { type: 'TEXT', indexed: false })
  .field('profile.location', { type: 'TEXT', indexed: true })
  .build()
```

**How it works:**
- Field name: `'profile.bio'`
- Automatically becomes JSON path: `'$.profile.bio'`
- Generated column name in SQLite: `_profile.bio` (if indexed)

## Explicit Path Parameter

For advanced use cases, you can use a simple field name with an explicit `path` parameter to specify a different JSON path:

### Simple Field Names with Nested Paths

Use clean field names while pointing to deeply nested data:

```typescript
type Product = Document<{
  name: string
  metadata?: {
    warehouse?: {
      location: string
      quantity: number
      lastUpdated: number
    }
  }
}>

const productSchema = createSchema<Omit<Product, '_id' | 'createdAt' | 'updatedAt'>>()
  .field('name', { type: 'TEXT', indexed: true })
  
  // Clean field names, explicit nested paths
  .field('location', { 
    path: '$.metadata.warehouse.location', 
    type: 'TEXT', 
    indexed: true 
  })
  .field('quantity', { 
    path: '$.metadata.warehouse.quantity', 
    type: 'INTEGER', 
    indexed: true 
  })
  .field('lastUpdated', { 
    path: '$.metadata.warehouse.lastUpdated', 
    type: 'INTEGER', 
    indexed: false 
  })
  .build()

// Query using the simple field names
const lowStock = await products.find({
  quantity: { $lt: 10 }
})

const nycWarehouse = await products.find({
  location: 'NYC'
})
```

**Benefits:**
- ✅ Cleaner field names in queries
- ✅ Hide complex nesting structure
- ✅ More readable query code

### Overriding Automatic Path Generation

The explicit `path` parameter takes precedence over automatic dot notation conversion:

```typescript
type Config = Document<{
  name: string
  data?: Record<string, unknown>
}>

const configSchema = createSchema<Omit<Config, '_id' | 'createdAt' | 'updatedAt'>>()
  .field('name', { type: 'TEXT', indexed: true })
  
  // Field name suggests one path, but explicit path points elsewhere
  .field('settings.theme', { 
    path: '$.data.user_settings.display_theme',  // Explicit path wins
    type: 'TEXT', 
    indexed: true 
  })
  .build()

// Stored at: $.data.user_settings.display_theme
// But queried as: settings.theme
const darkConfigs = await configs.find({
  'settings.theme': 'dark'
})
```

### When to Use Explicit Paths

**Use dot notation** (recommended):
```typescript
.field('profile.bio', { type: 'TEXT', indexed: true })
```

**Use explicit path when:**
1. You want simpler field names than the actual path
2. You're migrating from a different schema structure
3. You need to map to legacy JSON structures
4. You want to hide implementation details

```typescript
// Instead of: .field('user.profile.settings.preferences.theme', ...)
// Use clean name with explicit path:
.field('theme', { 
  path: '$.user.profile.settings.preferences.theme',
  type: 'TEXT',
  indexed: true
})
```

## Multiple Nested Fields from Same Parent

You can index multiple properties from the same nested object:

```typescript
type Alert = Document<{
  name: string
  isActive: boolean
  watchlistItem?: {
    ticker: string
    symbol: string
    exchange: string
    bias: "neutral" | "bullish" | "bearish"
  }
}>

const alertSchema = createSchema<Omit<Alert, '_id' | 'createdAt' | 'updatedAt'>>()
  .field('name', { type: 'TEXT', indexed: true, unique: true })
  .field('isActive', { type: 'INTEGER', indexed: true })
  
  // Multiple fields from same nested object
  .field('watchlistItem.ticker', { type: 'TEXT', indexed: true })
  .field('watchlistItem.symbol', { type: 'TEXT', indexed: true })
  .field('watchlistItem.exchange', { type: 'TEXT', indexed: true })
  .field('watchlistItem.bias', { type: 'TEXT', indexed: true })
  .build()
```

This creates four separate indexes on different properties of `watchlistItem`.

## Deeply Nested Fields

Dot notation works at **arbitrary depth**:

```typescript
type Config = Document<{
  name: string
  settings?: {
    display?: {
      theme: string
      fontSize: number
      colors?: {
        primary: string
        secondary: string
      }
    }
  }
}>

const configSchema = createSchema<Omit<Config, '_id' | 'createdAt' | 'updatedAt'>>()
  .field('name', { type: 'TEXT', indexed: true })
  
  // Two-level nesting
  .field('settings.display.theme', { type: 'TEXT', indexed: true })
  .field('settings.display.fontSize', { type: 'INTEGER', indexed: false })
  
  // Three-level nesting
  .field('settings.display.colors.primary', { type: 'TEXT', indexed: true })
  .field('settings.display.colors.secondary', { type: 'TEXT', indexed: false })
  .build()
```

## Compound Indexes with Nested Fields

Compound indexes fully support dot notation, allowing you to create multi-column indexes across both top-level and nested fields:

```typescript
type Alert = Document<{
  name: string
  priority: "low" | "medium" | "high" | "urgent"
  isActive: boolean
  isTemplate: boolean
  templateName?: string
  watchlistItem?: {
    ticker: string
    symbol: string
  }
}>

const alertSchema = createSchema<Omit<Alert, '_id' | 'createdAt' | 'updatedAt'>>()
  .field('name', { type: 'TEXT', indexed: true, unique: true })
  .field('isActive', { type: 'INTEGER', indexed: true })
  .field('isTemplate', { type: 'INTEGER', indexed: true })
  .field('templateName', { type: 'TEXT', indexed: true })
  .field('watchlistItem.ticker', { type: 'TEXT', indexed: true })
  .field('watchlistItem.symbol', { type: 'TEXT', indexed: true })
  
  // Compound index: nested field + top-level field
  .compoundIndex('watchlist_active', ['watchlistItem.ticker', 'isActive'])
  
  // Compound index: two top-level fields
  .compoundIndex('template_name', ['isTemplate', 'templateName'])
  
  // Compound index: two nested fields
  .compoundIndex('watchlist_symbols', ['watchlistItem.ticker', 'watchlistItem.symbol'])
  .build()
```

### Compound Index Query Optimization

Field order matters in compound indexes:

```typescript
// Index: ['watchlistItem.ticker', 'isActive']

// ✅ Uses index fully
db.find({ 
  'watchlistItem.ticker': 'AAPL', 
  isActive: true 
})

// ✅ Uses index partially (first field only)
db.find({ 
  'watchlistItem.ticker': { $in: ['AAPL', 'GOOGL'] }
})

// ❌ Cannot use index (doesn't start with first field)
db.find({ 
  isActive: true 
})
```

## Querying Nested Fields

Once indexed, you can query nested fields efficiently:

```typescript
// Query by nested field
const alerts = await collection.find({
  'watchlistItem.ticker': 'AAPL'
})

// Query with operators
const users = await collection.find({
  'profile.location': { $in: ['NYC', 'SF', 'LA'] }
})

// Compound query (uses compound index)
const activeAlerts = await collection.find({
  'watchlistItem.ticker': 'AAPL',
  isActive: true
})

// Deep nesting
const configs = await collection.find({
  'settings.display.theme': 'dark'
})
```

## Best Practices

### 1. Index Only What You Query

Don't index fields you won't query by:

```typescript
// ✅ Good - Only index searchable fields
.field('profile.email', { type: 'TEXT', indexed: true })
.field('profile.bio', { type: 'TEXT', indexed: false })  // Not searchable
```

### 2. Use Compound Indexes for Common Query Patterns

If you often query by multiple fields together, create a compound index:

```typescript
// If you often query: { ticker: X, isActive: true }
.compoundIndex('ticker_active', ['watchlistItem.ticker', 'isActive'])
```

### 3. Consider Field Order in Compound Indexes

Put the most selective field first:

```typescript
// If ticker is more selective than isActive
.compoundIndex('ticker_active', ['watchlistItem.ticker', 'isActive'])

// NOT: .compoundIndex('active_ticker', ['isActive', 'watchlistItem.ticker'])
```

### 4. Use Unique Compound Indexes for Business Constraints

Enforce uniqueness across multiple fields:

```typescript
.field('email', { type: 'TEXT', indexed: true })
.field('tenantId', { type: 'TEXT', indexed: true })
.compoundIndex('email_tenant', ['email', 'tenantId'], { unique: true })
```

## How It Works Internally

When you define a nested field like `'profile.bio'`:

1. **JSON Path Generation**: Field name is converted to `'$.profile.bio'`
2. **Generated Column**: SQLite creates a generated column using `json_extract(body, '$.profile.bio')`
3. **Index Creation**: If indexed, SQLite creates an index on the generated column
4. **Query Translation**: Queries on `'profile.bio'` are translated to use the generated column

This provides native SQL performance for nested field queries!

## Complete Example

Here's a real-world example combining all features:

```typescript
import { createSchema, type Document, StrataDB } from 'stratadb'

// Define your document type
type Alert = Document<{
  name: string
  priority: "low" | "medium" | "high" | "urgent"
  bias: "neutral" | "bullish" | "bearish"
  isActive: boolean
  logAlways: boolean
  isTemplate: boolean
  templateName?: string
  activatedBy?: string
  description?: string
  watchlistItem?: {
    ticker: string
    symbol: string
    exchange: string
    bias: "neutral" | "bullish" | "bearish"
  }
}>

// Create schema with nested field indexes
const alertSchema = createSchema<Omit<Alert, '_id' | 'createdAt' | 'updatedAt'>>()
  // Core fields
  .field('name', { type: 'TEXT', indexed: true, unique: true })
  .field('isActive', { type: 'INTEGER', indexed: true })
  .field('isTemplate', { type: 'INTEGER', indexed: true })
  .field('templateName', { type: 'TEXT', indexed: true })
  .field('priority', { type: 'TEXT', indexed: true })
  .field('bias', { type: 'TEXT', indexed: true })

  // Nested watchlistItem fields
  .field('watchlistItem.ticker', { type: 'TEXT', indexed: true })
  .field('watchlistItem.symbol', { type: 'TEXT', indexed: true })
  .field('watchlistItem.exchange', { type: 'TEXT', indexed: true })
  .field('watchlistItem.bias', { type: 'TEXT', indexed: true })

  // Compound indexes for common queries
  .compoundIndex('watchlist_active', ['watchlistItem.ticker', 'isActive'])
  .compoundIndex('template_name', ['isTemplate', 'templateName'])
  .compoundIndex('ticker_priority', ['watchlistItem.ticker', 'priority'])

  // Auto-manage timestamps
  .timestamps(true)
  .build()

// Use with database
const db = new StrataDB({ database: './alerts.db' })
const alerts = db.collection<Alert>('alerts', alertSchema)

// Insert document
await alerts.insertOne({
  name: 'AAPL High Price',
  priority: 'high',
  bias: 'bullish',
  isActive: true,
  logAlways: false,
  isTemplate: false,
  watchlistItem: {
    ticker: 'AAPL',
    symbol: 'AAPL',
    exchange: 'NASDAQ',
    bias: 'bullish'
  }
})

// Query by nested field (uses index!)
const appleAlerts = await alerts.find({
  'watchlistItem.ticker': 'AAPL'
})

// Query with compound index
const activeAppleAlerts = await alerts.find({
  'watchlistItem.ticker': 'AAPL',
  isActive: true
})

// Update nested field
await alerts.updateOne('alert-123', {
  'watchlistItem.bias': 'bearish'
})
```

## Summary

- ✅ **Dot Notation**: Use `'field.nested.property'` syntax
- ✅ **Automatic Path Conversion**: Dots become JSON paths (`$.field.nested.property`)
- ✅ **Arbitrary Depth**: Works at any nesting level
- ✅ **Compound Indexes**: Mix top-level and nested fields
- ✅ **Fast Queries**: Generated columns provide native SQL performance
- ✅ **Type Safe**: Full TypeScript support throughout

Nested field indexing makes StrataDB perfect for complex document structures while maintaining the performance of indexed SQL queries!
