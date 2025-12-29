module FractalDb.Tests.ArrayOperatorTests

open System.Threading.Tasks
open Xunit
open FsUnit.Xunit
open FractalDb
open FractalDb.Types
open FractalDb.Schema
open FractalDb.Database
open FractalDb.Collection
open FractalDb.Query
open FractalDb.Operators

/// <summary>
/// Unit tests for array query operators: All, Size, ElemMatch, Index, In, NotIn.
/// Tests array-specific query operations on list fields.
/// </summary>

// Test types with array/list fields
type BlogPost =
    { Title: string
      Author: string
      Tags: list<string>
      Scores: list<int>
      Published: bool }

type Product =
    { Name: string
      Categories: list<string>
      Ratings: list<int>
      Price: float }

// Helper to create test database
let createTestDb () = FractalDb.InMemory()

// ═══════════════════════════════════════════════════════════════
// Array.All Operator Tests
// ═══════════════════════════════════════════════════════════════

[<Fact>]
let ``ArrayOp.All matches documents with all specified values`` () =
    task {
        let db = createTestDb ()

        let schema =
            { Fields = []
              Indexes = []
              Timestamps = false
              Validate = None }

        let collection = db.Collection<BlogPost>("posts", schema)

        // Insert test posts
        let post1 =
            { Title = "F# Tutorial"
              Author = "Alice"
              Tags = [ "fsharp"; "tutorial"; "programming" ]
              Scores = [ 10; 20; 30 ]
              Published = true }

        let post2 =
            { Title = "Database Guide"
              Author = "Bob"
              Tags = [ "database"; "tutorial"; "sql" ]
              Scores = [ 15; 25 ]
              Published = true }

        let post3 =
            { Title = "Advanced F#"
              Author = "Charlie"
              Tags = [ "fsharp"; "advanced"; "programming"; "tutorial" ]
              Scores = [ 40; 50 ]
              Published = false }

        let! _ = collection |> Collection.insertOne post1
        let! _ = collection |> Collection.insertOne post2
        let! _ = collection |> Collection.insertOne post3

        // Query for posts with both "fsharp" AND "programming" tags
        let query =
            Query.Field("tags", FieldOp.Array(box (ArrayOp.All [ "fsharp"; "programming" ])))

        let! results = collection |> Collection.find query

        results |> should haveLength 2
        results |> List.map (fun doc -> doc.Data.Title) |> should contain "F# Tutorial"
        results |> List.map (fun doc -> doc.Data.Title) |> should contain "Advanced F#"
    }

[<Fact>]
let ``ArrayOp.All with single value works correctly`` () =
    task {
        let db = createTestDb ()

        let schema =
            { Fields = []
              Indexes = []
              Timestamps = false
              Validate = None }

        let collection = db.Collection<BlogPost>("posts", schema)

        let post1 =
            { Title = "F# Tutorial"
              Author = "Alice"
              Tags = [ "fsharp"; "tutorial" ]
              Scores = [ 10 ]
              Published = true }

        let post2 =
            { Title = "Python Guide"
              Author = "Bob"
              Tags = [ "python"; "tutorial" ]
              Scores = [ 20 ]
              Published = true }

        let! _ = collection |> Collection.insertOne post1
        let! _ = collection |> Collection.insertOne post2

        // Query for posts with "fsharp" tag
        let query = Query.Field("tags", FieldOp.Array(box (ArrayOp.All [ "fsharp" ])))
        let! results = collection |> Collection.find query

        results |> should haveLength 1
        results.[0].Data.Title |> should equal "F# Tutorial"
    }

[<Fact>]
let ``ArrayOp.All with empty list matches all documents`` () =
    task {
        let db = createTestDb ()

        let schema =
            { Fields = []
              Indexes = []
              Timestamps = false
              Validate = None }

        let collection = db.Collection<BlogPost>("posts", schema)

        let post1 =
            { Title = "Post 1"
              Author = "Alice"
              Tags = [ "tag1" ]
              Scores = []
              Published = true }

        let post2 =
            { Title = "Post 2"
              Author = "Bob"
              Tags = [ "tag2" ]
              Scores = []
              Published = true }

        let! _ = collection |> Collection.insertOne post1
        let! _ = collection |> Collection.insertOne post2

        // Query with empty list should match all documents
        let emptyList: list<string> = []
        let query = Query.Field("tags", FieldOp.Array(box (ArrayOp.All emptyList)))
        let! results = collection |> Collection.find query

        results |> should haveLength 2
    }

// ═══════════════════════════════════════════════════════════════
// Array.Size Operator Tests
// ═══════════════════════════════════════════════════════════════

[<Fact>]
let ``ArrayOp.Size matches documents with exact array length`` () =
    task {
        let db = createTestDb ()

        let schema =
            { Fields = []
              Indexes = []
              Timestamps = false
              Validate = None }

        let collection = db.Collection<BlogPost>("posts", schema)

        let post1 =
            { Title = "Post 1"
              Author = "Alice"
              Tags = [ "tag1"; "tag2"; "tag3" ]
              Scores = [ 10; 20 ]
              Published = true }

        let post2 =
            { Title = "Post 2"
              Author = "Bob"
              Tags = [ "tag1"; "tag2" ]
              Scores = [ 30 ]
              Published = true }

        let post3 =
            { Title = "Post 3"
              Author = "Charlie"
              Tags = [ "tag1"; "tag2"; "tag3" ]
              Scores = []
              Published = false }

        let! _ = collection |> Collection.insertOne post1
        let! _ = collection |> Collection.insertOne post2
        let! _ = collection |> Collection.insertOne post3

        // Query for posts with exactly 3 tags
        let query = Query.Field("tags", FieldOp.Array(box (ArrayOp.Size 3)))
        let! results = collection |> Collection.find query

        results |> should haveLength 2
        results |> List.map (fun doc -> doc.Data.Title) |> should contain "Post 1"
        results |> List.map (fun doc -> doc.Data.Title) |> should contain "Post 3"
    }

[<Fact>]
let ``ArrayOp.Size with zero matches empty arrays`` () =
    task {
        let db = createTestDb ()

        let schema =
            { Fields = []
              Indexes = []
              Timestamps = false
              Validate = None }

        let collection = db.Collection<BlogPost>("posts", schema)

        let post1 =
            { Title = "Post 1"
              Author = "Alice"
              Tags = [ "tag1" ]
              Scores = []
              Published = true }

        let post2 =
            { Title = "Post 2"
              Author = "Bob"
              Tags = [ "tag1" ]
              Scores = [ 10; 20 ]
              Published = true }

        let post3 =
            { Title = "Post 3"
              Author = "Charlie"
              Tags = [ "tag1" ]
              Scores = []
              Published = false }

        let! _ = collection |> Collection.insertOne post1
        let! _ = collection |> Collection.insertOne post2
        let! _ = collection |> Collection.insertOne post3

        // Query for posts with empty scores array
        let query = Query.Field("scores", FieldOp.Array(box (ArrayOp.Size 0)))
        let! results = collection |> Collection.find query

        results |> should haveLength 2
        results |> List.map (fun doc -> doc.Data.Title) |> should contain "Post 1"
        results |> List.map (fun doc -> doc.Data.Title) |> should contain "Post 3"
    }

[<Fact>]
let ``ArrayOp.Size with one matches single-element arrays`` () =
    task {
        let db = createTestDb ()

        let schema =
            { Fields = []
              Indexes = []
              Timestamps = false
              Validate = None }

        let collection = db.Collection<BlogPost>("posts", schema)

        let post1 =
            { Title = "Post 1"
              Author = "Alice"
              Tags = []
              Scores = [ 100 ]
              Published = true }

        let post2 =
            { Title = "Post 2"
              Author = "Bob"
              Tags = []
              Scores = [ 50; 75 ]
              Published = true }

        let! _ = collection |> Collection.insertOne post1
        let! _ = collection |> Collection.insertOne post2

        // Query for posts with exactly one score
        let query = Query.Field("scores", FieldOp.Array(box (ArrayOp.Size 1)))
        let! results = collection |> Collection.find query

        results |> should haveLength 1
        results.[0].Data.Title |> should equal "Post 1"
        results.[0].Data.Scores |> should equal [ 100 ]
    }

// ═══════════════════════════════════════════════════════════════
// CompareOp.In with Array Values Tests
// ═══════════════════════════════════════════════════════════════

[<Fact>]
let ``CompareOp.In operator works with array field values`` () =
    task {
        let db = createTestDb ()

        let schema =
            { Fields = []
              Indexes = []
              Timestamps = false
              Validate = None }

        let collection = db.Collection<BlogPost>("posts", schema)

        let post1 =
            { Title = "Tutorial Post"
              Author = "Alice"
              Tags = [ "tutorial"; "beginner" ]
              Scores = [ 10 ]
              Published = true }

        let post2 =
            { Title = "Advanced Post"
              Author = "Bob"
              Tags = [ "advanced"; "expert" ]
              Scores = [ 90 ]
              Published = true }

        let post3 =
            { Title = "Intermediate Post"
              Author = "Charlie"
              Tags = [ "intermediate" ]
              Scores = [ 50 ]
              Published = false }

        let! _ = collection |> Collection.insertOne post1
        let! _ = collection |> Collection.insertOne post2
        let! _ = collection |> Collection.insertOne post3

        // Query for posts by specific authors (using In on a string field)
        let query =
            Query.Field("author", FieldOp.Compare(box (CompareOp.In [ "Alice"; "Charlie" ])))

        let! results = collection |> Collection.find query

        results |> should haveLength 2
        results |> List.map (fun doc -> doc.Data.Author) |> should contain "Alice"
        results |> List.map (fun doc -> doc.Data.Author) |> should contain "Charlie"
    }

// ═══════════════════════════════════════════════════════════════
// CompareOp.NotIn with Array Values Tests
// ═══════════════════════════════════════════════════════════════

[<Fact>]
let ``CompareOp.NotIn operator works with array field values`` () =
    task {
        let db = createTestDb ()

        let schema =
            { Fields = []
              Indexes = []
              Timestamps = false
              Validate = None }

        let collection = db.Collection<BlogPost>("posts", schema)

        let post1 =
            { Title = "Draft Post"
              Author = "Alice"
              Tags = [ "draft" ]
              Scores = []
              Published = false }

        let post2 =
            { Title = "Published Post"
              Author = "Bob"
              Tags = [ "published" ]
              Scores = [ 80 ]
              Published = true }

        let post3 =
            { Title = "Archived Post"
              Author = "Charlie"
              Tags = [ "archived" ]
              Scores = []
              Published = false }

        let! _ = collection |> Collection.insertOne post1
        let! _ = collection |> Collection.insertOne post2
        let! _ = collection |> Collection.insertOne post3

        // Query for posts NOT by specific authors
        let query =
            Query.Field("author", FieldOp.Compare(box (CompareOp.NotIn [ "Alice"; "Charlie" ])))

        let! results = collection |> Collection.find query

        results |> should haveLength 1
        results.[0].Data.Author |> should equal "Bob"
        results.[0].Data.Title |> should equal "Published Post"
    }

// ═══════════════════════════════════════════════════════════════
// Complex Array Query Tests with AND/OR
// ═══════════════════════════════════════════════════════════════

[<Fact>]
let ``Complex array query with AND combines array conditions correctly`` () =
    task {
        let db = createTestDb ()

        let schema =
            { Fields =
                [


                ]
              Indexes = []
              Timestamps = false
              Validate = None }

        let collection = db.Collection<BlogPost>("posts", schema)

        let post1 =
            { Title = "Featured Tutorial"
              Author = "Alice"
              Tags = [ "tutorial"; "featured"; "programming" ]
              Scores = [ 10; 20; 30 ]
              Published = true }

        let post2 =
            { Title = "Tutorial Only"
              Author = "Bob"
              Tags = [ "tutorial"; "programming" ]
              Scores = [ 15 ]
              Published = true }

        let post3 =
            { Title = "Featured Post"
              Author = "Charlie"
              Tags = [ "featured"; "news" ]
              Scores = [ 40; 50; 60 ]
              Published = true }

        let! _ = collection |> Collection.insertOne post1
        let! _ = collection |> Collection.insertOne post2
        let! _ = collection |> Collection.insertOne post3

        // Query: has both "tutorial" AND "featured" tags AND has 3 scores
        let query =
            Query.And
                [ Query.Field("tags", FieldOp.Array(box (ArrayOp.All [ "tutorial"; "featured" ])))
                  Query.Field("scores", FieldOp.Array(box (ArrayOp.Size 3))) ]

        let! results = collection |> Collection.find query

        results |> should haveLength 1
        results.[0].Data.Title |> should equal "Featured Tutorial"
        results.[0].Data.Tags |> should contain "tutorial"
        results.[0].Data.Tags |> should contain "featured"
        results.[0].Data.Scores |> should haveLength 3
    }

[<Fact>]
let ``Complex array query with OR allows alternative array conditions`` () =
    task {
        let db = createTestDb ()

        let schema =
            { Fields = []
              Indexes = []
              Timestamps = false
              Validate = None }

        let collection = db.Collection<BlogPost>("posts", schema)

        let post1 =
            { Title = "F# Post"
              Author = "Alice"
              Tags = [ "fsharp"; "programming" ]
              Scores = []
              Published = true }

        let post2 =
            { Title = "Database Post"
              Author = "Bob"
              Tags = [ "database"; "sql" ]
              Scores = []
              Published = true }

        let post3 =
            { Title = "Web Post"
              Author = "Charlie"
              Tags = [ "web"; "javascript" ]
              Scores = []
              Published = false }

        let! _ = collection |> Collection.insertOne post1
        let! _ = collection |> Collection.insertOne post2
        let! _ = collection |> Collection.insertOne post3

        // Query: has "fsharp" tag OR has "database" tag
        let query =
            Query.Or
                [ Query.Field("tags", FieldOp.Array(box (ArrayOp.All [ "fsharp" ])))
                  Query.Field("tags", FieldOp.Array(box (ArrayOp.All [ "database" ]))) ]

        let! results = collection |> Collection.find query

        results |> should haveLength 2
        results |> List.map (fun doc -> doc.Data.Title) |> should contain "F# Post"

        results
        |> List.map (fun doc -> doc.Data.Title)
        |> should contain "Database Post"
    }

[<Fact>]
let ``Complex array query combining Size and All operators`` () =
    task {
        let db = createTestDb ()

        let schema =
            { Fields = []
              Indexes = []
              Timestamps = false
              Validate = None }

        let collection = db.Collection<BlogPost>("posts", schema)

        let post1 =
            { Title = "Minimal Post"
              Author = "Alice"
              Tags = [ "tutorial"; "fsharp" ]
              Scores = []
              Published = true }

        let post2 =
            { Title = "Extended Post"
              Author = "Bob"
              Tags = [ "tutorial"; "fsharp"; "advanced" ]
              Scores = []
              Published = true }

        let post3 =
            { Title = "Another Minimal"
              Author = "Charlie"
              Tags = [ "database"; "beginner" ]
              Scores = []
              Published = false }

        let! _ = collection |> Collection.insertOne post1
        let! _ = collection |> Collection.insertOne post2
        let! _ = collection |> Collection.insertOne post3

        // Query: has exactly 2 tags AND includes "tutorial" and "fsharp"
        let query =
            Query.And
                [ Query.Field("tags", FieldOp.Array(box (ArrayOp.Size 2)))
                  Query.Field("tags", FieldOp.Array(box (ArrayOp.All [ "tutorial"; "fsharp" ]))) ]

        let! results = collection |> Collection.find query

        results |> should haveLength 1
        results.[0].Data.Title |> should equal "Minimal Post"
        results.[0].Data.Tags |> should haveLength 2
        results.[0].Data.Tags |> should contain "tutorial"
        results.[0].Data.Tags |> should contain "fsharp"
    }

// ═══════════════════════════════════════════════════════════════
// Integer Array Tests
// ═══════════════════════════════════════════════════════════════

[<Fact>]
let ``ArrayOp.All works with integer arrays`` () =
    task {
        let db = createTestDb ()

        let schema =
            { Fields = []
              Indexes = []
              Timestamps = false
              Validate = None }

        let collection = db.Collection<Product>("products", schema)

        let product1 =
            { Name = "Widget A"
              Categories = [ "tools" ]
              Ratings = [ 5; 4; 5; 4 ]
              Price = 29.99 }

        let product2 =
            { Name = "Widget B"
              Categories = [ "tools" ]
              Ratings = [ 3; 4; 5 ]
              Price = 19.99 }

        let product3 =
            { Name = "Widget C"
              Categories = [ "gadgets" ]
              Ratings = [ 5; 5; 4 ]
              Price = 39.99 }

        let! _ = collection |> Collection.insertOne product1
        let! _ = collection |> Collection.insertOne product2
        let! _ = collection |> Collection.insertOne product3

        // Query for products with ratings containing both 5 and 4
        let query = Query.Field("ratings", FieldOp.Array(box (ArrayOp.All [ 5; 4 ])))
        let! results = collection |> Collection.find query

        results |> should haveLength 2
        results |> List.map (fun doc -> doc.Data.Name) |> should contain "Widget A"
        results |> List.map (fun doc -> doc.Data.Name) |> should contain "Widget C"
    }

[<Fact>]
let ``ArrayOp.Size works with integer arrays`` () =
    task {
        let db = createTestDb ()

        let schema =
            { Fields = []
              Indexes = []
              Timestamps = false
              Validate = None }

        let collection = db.Collection<Product>("products", schema)

        let product1 =
            { Name = "Product 1"
              Categories = []
              Ratings = [ 5; 4; 3 ]
              Price = 29.99 }

        let product2 =
            { Name = "Product 2"
              Categories = []
              Ratings = [ 4; 5 ]
              Price = 19.99 }

        let! _ = collection |> Collection.insertOne product1
        let! _ = collection |> Collection.insertOne product2

        // Query for products with exactly 3 ratings
        let query = Query.Field("ratings", FieldOp.Array(box (ArrayOp.Size 3)))
        let! results = collection |> Collection.find query

        results |> should haveLength 1
        results.[0].Data.Name |> should equal "Product 1"
        results.[0].Data.Ratings |> should equal [ 5; 4; 3 ]
    }
