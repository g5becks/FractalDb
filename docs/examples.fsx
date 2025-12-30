(**
---
title: Examples
category: Guides
categoryindex: 2
index: 6
---

# FractalDb Examples

This page contains F# examples demonstrating FractalDb features.

## Basic Setup
*)

open FractalDb
open FractalDb.Operators
open FractalDb.Options

type User = {
    Name: string
    Email: string
    Age: int
}

type Post = {
    Title: string
    Content: string
    AuthorId: string
    Tags: string list
}

(**
## Creating a Database

Open a file-based or in-memory database:
*)

// File-based database
use db = FractalDb.Open("example.db", DbOptions.defaults)

// Or in-memory for testing
// use db = FractalDb.InMemory()

(**
## Defining Schemas

Create schemas with indexed fields and validation:
*)

let userSchema =
    schema {
        field "name" SqliteType.Text (fun u -> u.Name) [ Indexed ]
        field "email" SqliteType.Text (fun u -> u.Email) [ Indexed; Unique ]
        field "age" SqliteType.Integer (fun u -> int64 u.Age) [ Indexed ]
        
        validate (fun user ->
            if user.Age < 13 then Error "Must be 13+"
            elif not (user.Email.Contains("@")) then Error "Invalid email"
            else Ok user
        )
    }

let postSchema =
    schema {
        field "authorId" SqliteType.Text (fun p -> p.AuthorId) [ Indexed ]
        field "title" SqliteType.Text (fun p -> p.Title) [ Indexed ]
        
        // Composite index for common queries
        index "idx_author_title" ["authorId"; "title"]
        
        timestamps true
    }

(**
## Getting Collections
*)

let users = db.Collection<User>("users", userSchema)
let posts = db.Collection<Post>("posts", postSchema)

(**
## Insert Operations

Insert single and multiple documents:
*)

// Insert one user
let! aliceResult = users.InsertOne({
    Name = "Alice"
    Email = "alice@example.com"
    Age = 30
})

match aliceResult with
| Ok alice ->
    printfn "Inserted user: %s (ID: %s)" alice.Data.Name alice.Id
| Error err ->
    printfn "Insert failed: %s" err.Message

// Insert many users
let! manyResult = users.InsertMany([
    { Name = "Bob"; Email = "bob@example.com"; Age = 25 }
    { Name = "Charlie"; Email = "charlie@example.com"; Age = 35 }
])

match manyResult with
| Ok docs ->
    printfn "Inserted %d users" docs.Length
| Error err ->
    printfn "Batch insert failed: %s" err.Message

(**
## Query Operations

Find documents using various operators:
*)

// Find by equality
let! activeUsers = users.Find(Query.eq "status" "active")

// Find with comparison operators
let! adults = users.Find(Query.gte "age" 18L)

// Find with string operations
let! gmailUsers = users.Find(Query.endsWith "email" "@gmail.com")

// Complex query with multiple conditions
let! results =
    Query.empty
    |> Query.gte_ "age" 18L
    |> Query.endsWith_ "email" "@example.com"
    |> users.Find

(**
## Query Options

Use options for sorting, pagination, and projection:
*)

// Sort and limit
let! topUsers =
    options {
        sortBy "age" Descending
        limit 10
    }
    |> users.Find Query.empty

// Pagination
let! page2 =
    options {
        skip 20
        limit 10
        sortBy "createdAt" Descending
    }
    |> users.Find Query.empty

// Field projection
let! names =
    options {
        project ["_id"; "name"; "email"]
    }
    |> users.Find Query.empty

(**
## Update Operations

Update documents by ID or query:
*)

match aliceResult with
| Ok alice ->
    // Update by ID
    let! updateResult = users.UpdateById(alice.Id, {| age = 31 |})
    
    match updateResult with
    | Ok (Some updated) ->
        printfn "Updated: %s is now %d years old" 
            updated.Data.Name updated.Data.Age
    | Ok None ->
        printfn "User not found"
    | Error err ->
        printfn "Update failed: %s" err.Message
| _ -> ()

// Update many
let! manyUpdated =
    users.UpdateMany(
        Query.eq "status" "pending",
        {| status = "active" |}
    )

(**
## Delete Operations

Delete documents by ID or query:
*)

// Delete by ID
let! deleteResult = users.DeleteById("some-id")

// Delete many
let! deletedCount =
    users.DeleteMany(Query.eq "status" "inactive")

match deletedCount with
| Ok count ->
    printfn "Deleted %d users" count
| Error err ->
    printfn "Delete failed: %s" err.Message

(**
## Transactions

Group multiple operations in ACID transactions:
*)

let! txResult =
    db.Transact(fun tx -> task {
        // Insert user
        let! user = users.InsertOne({
            Name = "David"
            Email = "david@example.com"
            Age = 28
        }, tx)
        
        // Insert posts by that user
        let! post1 = posts.InsertOne({
            Title = "Hello World"
            Content = "My first post"
            AuthorId = user.Id
            Tags = ["intro"; "hello"]
        }, tx)
        
        let! post2 = posts.InsertOne({
            Title = "FractalDb Tutorial"
            Content = "How to use FractalDb"
            AuthorId = user.Id
            Tags = ["tutorial"; "fsharp"]
        }, tx)
        
        return (user, post1, post2)
    })

match txResult with
| Ok (user, post1, post2) ->
    printfn "Transaction succeeded!"
    printfn "User: %s" user.Id
    printfn "Posts: %s, %s" post1.Id post2.Id
| Error err ->
    printfn "Transaction rolled back: %s" err.Message

(**
## Error Handling

Handle different error types:
*)

let! insertResult = users.InsertOne({
    Name = "Eve"
    Email = "eve@example.com"
    Age = 20
})

match insertResult with
| Ok doc ->
    printfn "Success: %s" doc.Id
| Error (ConstraintViolation msg) ->
    printfn "Unique constraint failed: %s" msg
| Error (ValidationError msg) ->
    printfn "Validation failed: %s" msg
| Error (SerializationError (msg, _)) ->
    printfn "Serialization error: %s" msg
| Error err ->
    printfn "Other error: %s" err.Message

(**
## Count Documents
*)

let! totalUsers = users.Count(Query.empty)
let! adultCount = users.Count(Query.gte "age" 18L)

match (totalUsers, adultCount) with
| (Ok total, Ok adults) ->
    printfn "Total users: %d, Adults: %d" total adults
| _ ->
    printfn "Count failed"

(**
## Find One and Find by ID
*)

// Find one matching document
let! firstAdult = users.FindOne(Query.gte "age" 18L)

match firstAdult with
| Ok (Some doc) ->
    printfn "First adult: %s (%d years old)" doc.Data.Name doc.Data.Age
| Ok None ->
    printfn "No adults found"
| Error err ->
    printfn "Query failed: %s" err.Message

// Find by ID
let! userById = users.FindById("some-id")

(**
## Complete Example: Blog System

Here's a complete example of a simple blog system:
*)

type BlogUser = {
    Username: string
    Email: string
    Bio: string option
}

type BlogPost = {
    Title: string
    Content: string
    AuthorId: string
    Published: bool
    PublishedAt: int64 option
}

let blogUserSchema =
    schema {
        field "username" SqliteType.Text (fun u -> u.Username) [ Indexed; Unique ]
        field "email" SqliteType.Text (fun u -> u.Email) [ Indexed; Unique ]
        timestamps true
        validate (fun u ->
            if u.Username.Length < 3 then Error "Username too short"
            elif not (u.Email.Contains("@")) then Error "Invalid email"
            else Ok u
        )
    }

let blogPostSchema =
    schema {
        field "authorId" SqliteType.Text (fun p -> p.AuthorId) [ Indexed ]
        field "published" SqliteType.Boolean (fun p -> p.Published) [ Indexed ]
        field "publishedAt" SqliteType.Integer (fun p -> 
            p.PublishedAt |> Option.defaultValue 0L
        ) []
        index "idx_author_published" ["authorId"; "publishedAt"]
        timestamps true
    }

let blogUsers = db.Collection<BlogUser>("blog_users", blogUserSchema)
let blogPosts = db.Collection<BlogPost>("blog_posts", blogPostSchema)

// Create user and posts in a transaction
let! blogResult =
    db.Transact(fun tx -> task {
        // Create user
        let! author = blogUsers.InsertOne({
            Username = "techblogger"
            Email = "blog@example.com"
            Bio = Some "I love F# and functional programming"
        }, tx)
        
        // Create published post
        let! post = blogPosts.InsertOne({
            Title = "Getting Started with FractalDb"
            Content = "FractalDb is an embedded document database..."
            AuthorId = author.Id
            Published = true
            PublishedAt = Some (System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())
        }, tx)
        
        return (author, post)
    })

// Query published posts by author
match blogResult with
| Ok (author, _) ->
    let! authorPosts =
        Query.empty
        |> Query.eq_ "authorId" author.Id
        |> Query.eq_ "published" true
        |> blogPosts.Find
    
    match authorPosts with
    | Ok posts ->
        printfn "Author %s has %d published posts" author.Data.Username posts.Length
    | _ -> ()
| _ -> ()

(**
## Cleanup

Close the database when done (or use `use` for automatic disposal):
*)

db.Close()

(**
## Next Steps

- **[Getting Started](getting-started.html)** - Learn the basics
- **[Query Expressions](query-expressions.html)** - Master querying
- **[Transactions](transactions.html)** - Work with transactions
- **[Schemas](schemas.html)** - Define schemas
- **[Indexes](indexes.html)** - Optimize performance
*)
