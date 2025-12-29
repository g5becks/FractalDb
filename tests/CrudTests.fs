module FractalDb.Tests.CrudTests

// Suppress linter warnings for test code
// fsharplint:disable FL0072

open System
open System.Threading.Tasks
open Xunit
open FsUnit.Xunit
open FractalDb.Types
open FractalDb.Errors
open FractalDb.Schema
open FractalDb.Query
open FractalDb.Collection
open FractalDb.Database

/// <summary>
/// Integration tests for basic CRUD operations.
/// Tests insertOne, findById, find, updateById, and deleteById operations.
/// </summary>

type User = {
    Name: string
    Email: string
    Age: int
    Active: bool
    Tags: list<string>
}

let userSchema : SchemaDef<User> = {
    Fields = [
        { Name = "name"; Path = None; SqlType = SqliteType.Text; Indexed = true; Unique = false; Nullable = false }
        { Name = "email"; Path = None; SqlType = SqliteType.Text; Indexed = true; Unique = true; Nullable = false }
        { Name = "age"; Path = None; SqlType = SqliteType.Integer; Indexed = true; Unique = false; Nullable = false }
        { Name = "active"; Path = None; SqlType = SqliteType.Integer; Indexed = false; Unique = false; Nullable = false }
    ]
    Indexes = []
    Timestamps = true
    Validate = None
}

/// <summary>
/// Test fixture providing shared in-memory database and Users collection.
/// </summary>
type CrudTestFixture() =
    let db = FractalDb.InMemory()
    let users = db.Collection<User>("users", userSchema)
    
    member _.Db = db
    member _.Users = users
    
    interface IDisposable with
        member _.Dispose() = db.Close()

/// <summary>
/// CRUD integration test suite.
/// </summary>
type CrudTests(fixture: CrudTestFixture) =
    let users = fixture.Users
    
    interface IClassFixture<CrudTestFixture>
    
    [<Fact>]
    member _.``insertOne creates document with auto-generated ID`` () : Task = task {
        let! result = users |> Collection.insertOne {
            Name = "Alice"
            Email = "alice-unique@test.com"
            Age = 30
            Active = true
            Tags = ["developer"]
        }
        
        match result with
        | Ok doc ->
            doc.Id |> should not' (be EmptyString)
            doc.Data.Name |> should equal "Alice"
            doc.Data.Email |> should equal "alice-unique@test.com"
            doc.Data.Age |> should equal 30
            doc.CreatedAt |> should be (greaterThan 0L)
        | Error err ->
            failwith $"Expected Ok, got Error: {err.Message}"
    }
    
    [<Fact>]
    member _.``insertOne fails with UniqueConstraint for duplicate email`` () : Task = task {
        // Use a separate collection to ensure fresh schema
        let testSchema : SchemaDef<User> = {
            Fields = [
                { Name = "email"; Path = None; SqlType = SqliteType.Text; Indexed = true; Unique = true; Nullable = false }
            ]
            Indexes = []
            Timestamps = true
            Validate = None
        }
        let testUsers = fixture.Db.Collection<User>("users_unique_test", testSchema)
        
        let uniqueEmail = $"duplicate-{Guid.NewGuid().ToString()}@test.com"
        
        let! _ = testUsers |> Collection.insertOne {
            Name = "User1"
            Email = uniqueEmail
            Age = 25
            Active = true
            Tags = []
        }
        
        let! result = testUsers |> Collection.insertOne {
            Name = "User2"
            Email = uniqueEmail
            Age = 30
            Active = true
            Tags = []
        }
        
        match result with
        | Error (FractalError.UniqueConstraint (field, _)) ->
            field.ToLower() |> should equal "email"
        | Error err ->
            failwith $"Expected UniqueConstraint, got different error: {err.Message}"
        | Ok _ ->
            failwith "Expected Error, got Ok"
    }
    
    [<Fact>]
    member _.``findById returns Some for existing document`` () : Task = task {
        let! insertResult = users |> Collection.insertOne {
            Name = "Bob"
            Email = $"bob-{Guid.NewGuid().ToString()}@test.com"
            Age = 25
            Active = true
            Tags = []
        }
        
        match insertResult with
        | Error err ->
            failwith $"Insert failed: {err.Message}"
        | Ok doc ->
            let! found = users |> Collection.findById doc.Id
            
            match found with
            | Some d ->
                d.Data.Name |> should equal "Bob"
                d.Id |> should equal doc.Id
            | None ->
                failwith "Expected Some, got None"
    }
    
    [<Fact>]
    member _.``findById returns None for non-existent document`` () : Task = task {
        let! found = users |> Collection.findById "non-existent-id"
        found |> should equal None
    }
    
    [<Fact>]
    member _.``find with query filters correctly`` () : Task = task {
        // Use unique emails to avoid conflicts with other tests
        let testPrefix = Guid.NewGuid().ToString().Substring(0, 8)
        
        // Setup: insert 10 users
        for i in 1..10 do
            let! _ = users |> Collection.insertOne {
                Name = $"FindTest{testPrefix}User{i}"
                Email = $"findtest-{testPrefix}-user{i}@test.com"
                Age = 20 + i
                Active = i % 2 = 0
                Tags = []
            }
            ()
        
        // Query active users with name matching our test prefix
        let queryFilter = 
            Query.all' [
                Query.field "active" (Query.eq true)
                Query.field "name" (Query.startsWith $"FindTest{testPrefix}")
            ]
        let! activeUsers = users |> Collection.find queryFilter
        
        activeUsers.Length |> should equal 5
        for doc in activeUsers do
            doc.Data.Active |> should equal true
            doc.Data.Name |> should startWith $"FindTest{testPrefix}"
    }
    
    [<Fact>]
    member _.``updateById modifies document and updates timestamp`` () : Task = task {
        let! insertResult = users |> Collection.insertOne {
            Name = "Charlie"
            Email = $"charlie-{Guid.NewGuid().ToString()}@test.com"
            Age = 35
            Active = true
            Tags = []
        }
        
        match insertResult with
        | Error err ->
            failwith $"Insert failed: {err.Message}"
        | Ok doc ->
            let originalUpdatedAt = doc.UpdatedAt
            
            let! updateResult =
                users |> Collection.updateById doc.Id (fun u -> { u with Age = 36 })
            
            match updateResult with
            | Error err ->
                failwith $"Update failed: {err.Message}"
            | Ok optUpdated ->
                match optUpdated with
                | None ->
                    failwith "Expected Some updated document, got None"
                | Some updated ->
                    updated.Data.Age |> should equal 36
                    updated.UpdatedAt |> should be (greaterThan originalUpdatedAt)
                    updated.Id |> should equal doc.Id
    }
    
    [<Fact>]
    member _.``deleteById removes document`` () : Task = task {
        let! insertResult = users |> Collection.insertOne {
            Name = "ToDelete"
            Email = $"delete-{Guid.NewGuid().ToString()}@test.com"
            Age = 40
            Active = true
            Tags = []
        }
        
        match insertResult with
        | Error err ->
            failwith $"Insert failed: {err.Message}"
        | Ok doc ->
            let! deleted = users |> Collection.deleteById doc.Id
            deleted |> should equal true
            
            let! found = users |> Collection.findById doc.Id
            found |> should equal None
    }
