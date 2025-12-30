---
title: Transactions
category: Guides
categoryindex: 2
index: 3
---

# Transactions

FractalDb provides full ACID transaction support using the `db.Transact { }` computation expression. Group multiple operations that either all succeed or all fail together.

## Why Use Transactions?

Transactions are essential for:

1. **Atomicity** - Multiple operations succeed or fail as a unit
2. **Consistency** - Database stays in a valid state
3. **Data Integrity** - Related changes are applied together
4. **Performance** - Batch operations are faster in transactions (4x speedup)

## Basic Transaction Usage

### Using db.Transact

The `db.Transact` computation expression provides a clean way to execute operations in a transaction:

```fsharp
open FractalDb
open FractalDb.Collection

let! result =
    db.Transact {
        // All operations share the same transaction
        let! user = users |> Collection.insertOne { Name = "Alice"; Email = "alice@test.com"; Age = 30; Active = true }
        let! post = posts |> Collection.insertOne { Title = "Hello"; AuthorId = user.Id }

        // Return both documents
        return (user, post)
    }

match result with
| Ok (user, post) ->
    printfn "Transaction committed: user %s, post %s" user.Id post.Id
| Error err ->
    printfn "Transaction rolled back: %s" err.Message
```

**Key Points:**
- Returns `Task<Result<'T, FractalError>>`
- Automatically commits on success (Ok)
- Automatically rolls back on error (Error) or exception

## Transaction Behavior

### Automatic Commit

When all operations succeed (return Ok), the transaction commits:

```fsharp
let! result =
    db.Transact {
        let! doc1 = collection |> Collection.insertOne data1
        let! doc2 = collection |> Collection.insertOne data2
        return (doc1.Id, doc2.Id)  // Both committed
    }
```

### Automatic Rollback on Error

When any operation returns Error, the transaction rolls back:

```fsharp
let! result =
    db.Transact {
        let! doc1 = collection |> Collection.insertOne validData
        // If this fails (e.g., duplicate key), doc1 is also rolled back
        let! doc2 = collection |> Collection.insertOne duplicateData
        return (doc1, doc2)
    }

match result with
| Ok _ -> printfn "Success"
| Error err -> printfn "Rolled back: %s" err.Message
```

### Explicit Error Return

You can explicitly return an error to trigger rollback:

```fsharp
let! result =
    db.Transact {
        let! doc = collection |> Collection.insertOne data

        // Validation after insert
        if doc.Data.Amount > 10000 then
            return! Task.FromResult(Error(FractalError.Validation(None, "Amount too large")))
        else
            return doc
    }
```

## Transaction Patterns

### Transfer Between Accounts

```fsharp
let transfer fromId toId amount =
    db.Transact {
        // Get both accounts
        let! fromAccount = accounts |> Collection.findById fromId
        let! toAccount = accounts |> Collection.findById toId

        match fromAccount, toAccount with
        | Some from, Some to' when from.Data.Balance >= amount ->
            // Update both accounts
            let! _ = accounts |> Collection.updateById fromId (fun a ->
                { a with Balance = a.Balance - amount })
            let! _ = accounts |> Collection.updateById toId (fun a ->
                { a with Balance = a.Balance + amount })
            return Ok amount
        | Some from, Some _ ->
            return! Task.FromResult(Error(FractalError.Validation(None, "Insufficient funds")))
        | _ ->
            return! Task.FromResult(Error(FractalError.NotFound "Account not found"))
    }
```

### Batch Insert with Validation

```fsharp
let insertWithValidation items =
    db.Transact {
        let! doc1 = collection |> Collection.insertOne items.[0]
        let! doc2 = collection |> Collection.insertOne items.[1]
        let! doc3 = collection |> Collection.insertOne items.[2]

        // All three inserted or none
        return [doc1; doc2; doc3]
    }
```

### Multi-Collection Operations

```fsharp
let createUserWithProfile userData profileData =
    db.Transact {
        let! user = users |> Collection.insertOne userData
        let! profile = profiles |> Collection.insertOne { profileData with UserId = user.Id }
        return (user, profile)
    }
```

## Error Handling in Transactions

### Try-With for Exceptions

```fsharp
let! result =
    db.Transact {
        try
            let! doc = collection |> Collection.insertOne data
            return doc
        with ex ->
            return! Task.FromResult(Error(FractalError.Database ex.Message))
    }
```

### Handling Specific Errors

```fsharp
let! result =
    db.Transact {
        let! insertResult = collection |> Collection.insertOne data

        // The computation expression handles Result automatically
        // If insertResult is Error, transaction rolls back
        return insertResult
    }

match result with
| Ok doc -> printfn "Created: %s" doc.Id
| Error (FractalError.Duplicate (field, value)) ->
    printfn "Duplicate %s: %s" field value
| Error (FractalError.Validation (field, msg)) ->
    printfn "Validation error: %s" msg
| Error err ->
    printfn "Error: %s" err.Message
```

## Performance Considerations

### Transactions Improve Batch Performance

Transactions reduce overhead for multiple operations:

| Scenario | Time | Speedup |
|----------|------|---------|
| 5 inserts without transaction | 1.76 ms | Baseline |
| 5 inserts in transaction | 442 us | **4x faster** |

### Best Practices

1. **Keep transactions short** - Long transactions hold locks
2. **Batch related operations** - Group writes that should be atomic
3. **Use insertMany for bulk inserts** - Already uses internal transaction
4. **Don't nest transactions** - `insertMany` has its own transaction, don't wrap it

### Avoid Nested Transactions

SQLite doesn't support nested transactions. Don't wrap `insertMany` in a transaction:

```fsharp
// WRONG - causes nested transaction error
let! result =
    db.Transact {
        let! batch = collection |> Collection.insertMany items  // Already transactional!
        return batch
    }

// CORRECT - use insertMany directly
let! result = collection |> Collection.insertMany items
```

## Transaction vs No Transaction

Use transactions when:
- Multiple operations must succeed or fail together
- You need atomicity across collections
- You're doing read-modify-write operations

Don't use transactions when:
- Single operation (already atomic)
- Using `insertMany` (has internal transaction)
- Read-only operations

## Complete Example

```fsharp
open FractalDb
open FractalDb.Schema
open FractalDb.Collection

type Account = { Name: string; Balance: decimal }
type Transfer = { FromId: string; ToId: string; Amount: decimal; Timestamp: int64 }

let accountSchema = schema<Account> {
    field "name" SqliteType.Text
    indexed "balance" SqliteType.Real
    timestamps
}

let transferSchema = schema<Transfer> {
    indexed "fromId" SqliteType.Text
    indexed "toId" SqliteType.Text
    timestamps
}

let db = FractalDb.Open("bank.db")
let accounts = db.Collection<Account>("accounts", accountSchema)
let transfers = db.Collection<Transfer>("transfers", transferSchema)

let performTransfer fromId toId amount =
    db.Transact {
        let! fromAccount = accounts |> Collection.findById fromId
        let! toAccount = accounts |> Collection.findById toId

        match fromAccount, toAccount with
        | Some from, Some to' when from.Data.Balance >= amount ->
            // Debit source
            let! _ = accounts |> Collection.updateById fromId (fun a ->
                { a with Balance = a.Balance - amount })
            // Credit destination
            let! _ = accounts |> Collection.updateById toId (fun a ->
                { a with Balance = a.Balance + amount })
            // Record transfer
            let! transfer = transfers |> Collection.insertOne {
                FromId = fromId
                ToId = toId
                Amount = amount
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            }
            return transfer
        | Some _, Some _ ->
            return! Task.FromResult(Error(FractalError.Validation(None, "Insufficient funds")))
        | _ ->
            return! Task.FromResult(Error(FractalError.NotFound "Account not found"))
    }

// Usage
task {
    let! result = performTransfer "acc-001" "acc-002" 100m
    match result with
    | Ok transfer -> printfn "Transfer complete: %s" transfer.Id
    | Error err -> printfn "Transfer failed: %s" err.Message
}
```
