# Transactions

Transactions ensure multiple operations either all succeed or all fail together (atomicity).

## Basic Transaction

```typescript
const tx = db.transaction()
try {
  const users = tx.collection('users', userSchema)
  const accounts = tx.collection('accounts', accountSchema)

  await users.insertOne({ name: 'Alice', email: 'alice@example.com' })
  await accounts.insertOne({ userId: 'alice', balance: 100 })

  tx.commit()
} catch (error) {
  tx.rollback()
  throw error
}
```

## Using Symbol.dispose (Recommended)

With TypeScript 5.2+, use `using` for automatic cleanup:

```typescript
{
  using tx = db.transaction()
  const users = tx.collection('users', userSchema)

  await users.insertOne({ name: 'Alice', email: 'alice@example.com' })
  await users.insertOne({ name: 'Bob', email: 'bob@example.com' })

  tx.commit()  // Must call commit, otherwise auto-rollback
}
// Transaction automatically rolled back if commit wasn't called
```

## Execute Helper

The `execute` method handles commit/rollback automatically:

```typescript
await db.execute(async (tx) => {
  const users = tx.collection('users', userSchema)
  const logs = tx.collection('logs', logSchema)

  const result = await users.insertOne({ name: 'Alice' })
  await logs.insertOne({ action: 'user_created', userId: result.document.id })

  return result.document
})
// Commits on success, rolls back on error
```

## Error Handling

```typescript
try {
  await db.execute(async (tx) => {
    const users = tx.collection('users', userSchema)

    await users.insertOne({ email: 'alice@example.com' })
    await users.insertOne({ email: 'alice@example.com' })  // Duplicate!
  })
} catch (error) {
  // Transaction was automatically rolled back
  // First insert was also undone
  console.error('Transaction failed:', error)
}
```

## Use Cases

### Transferring Funds

```typescript
await db.execute(async (tx) => {
  const accounts = tx.collection('accounts', accountSchema)

  const from = await accounts.findById(fromId)
  const to = await accounts.findById(toId)

  if (!from || !to) throw new Error('Account not found')
  if (from.balance < amount) throw new Error('Insufficient funds')

  await accounts.updateOne(fromId, { balance: from.balance - amount })
  await accounts.updateOne(toId, { balance: to.balance + amount })
})
```

### Creating Related Records

```typescript
await db.execute(async (tx) => {
  const users = tx.collection('users', userSchema)
  const profiles = tx.collection('profiles', profileSchema)
  const settings = tx.collection('settings', settingsSchema)

  const user = await users.insertOne({ email: 'new@example.com' })

  await profiles.insertOne({
    userId: user.document.id,
    displayName: 'New User'
  })

  await settings.insertOne({
    userId: user.document.id,
    theme: 'dark',
    notifications: true
  })

  return user.document
})
```

## Nested Transactions

SQLite doesn't support true nested transactions. Calling `transaction()` while already in a transaction will use the existing transaction:

```typescript
await db.execute(async (tx) => {
  // This is the outer transaction
  await someFunction(tx)  // Pass tx to nested functions
})

async function someFunction(tx: Transaction) {
  const users = tx.collection('users', userSchema)
  // Uses the same transaction
}
```
