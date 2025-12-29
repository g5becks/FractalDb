module FractalDb.Tests.UniqueConstraintDebugTest

open System
open Xunit
open FractalDb.Types
open FractalDb.Schema
open FractalDb.Database
open FractalDb.Collection

type TestUser = { Email: string }

[<Fact>]
let ``Debug unique constraint test`` () =
    use db = FractalDb.InMemory()
    
    let schema : SchemaDef<TestUser> = {
        Fields = [
            { Name = "email"; Path = None; SqlType = SqliteType.Text; Indexed = true; Unique = true; Nullable = false }
        ]
        Indexes = []
        Timestamps = true
        Validate = None
    }
    
    let users = db.Collection<TestUser>("debug_users", schema)
    
    printfn "Inserting first user..."
    let result1 = users |> Collection.insertOne { Email = "test@test.com" } |> Async.AwaitTask |> Async.RunSynchronously
    printfn "Result 1: %A" result1
    
    printfn "\nInserting duplicate user..."
    let result2 = users |> Collection.insertOne { Email = "test@test.com" } |> Async.AwaitTask |> Async.RunSynchronously
    printfn "Result 2: %A" result2
    
    match result2 with
    | Error _ -> ()
    | Ok _ -> failwith "Expected Error but got Ok!"
