#r "src/bin/Debug/net10.0/FractalDb.dll"
#r "nuget: Microsoft.Data.Sqlite, 10.0.0"

open FractalDb.Types
open FractalDb.Schema
open FractalDb.Database
open FractalDb.Collection

type User = {
    Name: string
    Email: string
}

let userSchema : SchemaDef<User> = {
    Fields = [
        { Name = "email"; Path = None; SqlType = SqliteType.Text; Indexed = false; Unique = true; Nullable = false }
    ]
    Indexes = []
    Timestamps = true
    Validate = None
}

let db = FractalDb.InMemory()
let users = db.Collection<User>("users", userSchema)

printfn "Inserting first user..."
let result1 = users |> Collection.insertOne { Name = "User1"; Email = "test@test.com" } |> Async.AwaitTask |> Async.RunSynchronously
printfn "Result 1: %A" result1

printfn "\nInserting duplicate user..."
let result2 = users |> Collection.insertOne { Name = "User2"; Email = "test@test.com" } |> Async.AwaitTask |> Async.RunSynchronously
printfn "Result 2: %A" result2

db.Close()
