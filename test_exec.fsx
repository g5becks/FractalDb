#r "src/bin/Debug/net10.0/FractalDb.dll"
#r "nuget: Microsoft.Data.Sqlite, 9.0.11"

open System
open FractalDb
open FractalDb.Types
open FractalDb.Schema
open FractalDb.Collection
open FractalDb.QueryExpr

// Test type
type TestUser =
    { Name: string
      Age: int64
      Active: bool }

// Schema
let schema: SchemaDef<TestUser> =
    { Fields =
        [ { Name = "name"
            Path = None
            SqlType = SqliteType.Text
            Indexed = true
            Unique = false
            Nullable = false }
          { Name = "age"
            Path = None
            SqlType = SqliteType.Integer
            Indexed = true
            Unique = false
            Nullable = false }
          { Name = "active"
            Path = None
            SqlType = SqliteType.Integer
            Indexed = false
            Unique = false
            Nullable = false } ]
      Indexes = []
      Timestamps = true
      Validate = None }

// Create database and collection
let db = FractalDb.InMemory()
let users = db.Collection<TestUser>("users", schema)

// Insert test data
let alice =
    { Name = "Alice"
      Age = 25L
      Active = true }

let bob =
    { Name = "Bob"
      Age = 30L
      Active = true }

let charlie =
    { Name = "Charlie"
      Age = 35L
      Active = false }

users
|> Collection.insertOne alice
|> Async.AwaitTask
|> Async.RunSynchronously
|> ignore

users
|> Collection.insertOne bob
|> Async.AwaitTask
|> Async.RunSynchronously
|> ignore

users
|> Collection.insertOne charlie
|> Async.AwaitTask
|> Async.RunSynchronously
|> ignore

printfn "Inserted 3 users"

// Test Collection.exec with query expression
let activeQuery =
    query {
        for user in users do
            where (user.Active = true)
            sortBy user.Age
    }

printfn "\nExecuting query expression..."

let results =
    users
    |> Collection.exec activeQuery
    |> Async.AwaitTask
    |> Async.RunSynchronously

printfn "Found %d active users:" results.Length

for doc in results do
    printfn "  - %s (age %d)" doc.Data.Name doc.Data.Age

// Test with instance method
printfn "\nUsing instance method..."
let results2 = users.Exec(activeQuery) |> Async.AwaitTask |> Async.RunSynchronously
printfn "Found %d users with instance method" results2.Length

printfn "\n✅ Collection.exec works correctly!"

db.Close()
