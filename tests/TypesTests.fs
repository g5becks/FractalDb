module FractalDb.Tests.TypesTests

open System
open Xunit
open FsUnit.Xunit
open FractalDb.Types

/// <summary>
/// Unit tests for core types: IdGenerator, Timestamp, and Document modules.
/// Tests foundational functionality for ID generation, timestamp handling, and document creation/manipulation.
/// </summary>

type TestUser = {
    Name: string
    Email: string
    Age: int
}

// ═══════════════════════════════════════════════════════════════
// IdGenerator Tests
// ═══════════════════════════════════════════════════════════════

[<Fact>]
let ``IdGenerator.generate returns non-empty string`` () =
    let id = IdGenerator.generate()
    
    id |> should not' (be EmptyString)
    id.Length |> should be (greaterThan 0)

[<Fact>]
let ``IdGenerator.generate returns valid GUID format`` () =
    let id = IdGenerator.generate()
    
    // Should be parseable as a GUID
    let success, _ = Guid.TryParse(id)
    success |> should be True

[<Fact>]
let ``IdGenerator.generate returns different IDs on successive calls`` () =
    let id1 = IdGenerator.generate()
    let id2 = IdGenerator.generate()
    
    id1 |> should not' (equal id2)

[<Fact>]
let ``IdGenerator.generate returns UUID v7 (time-sortable)`` () =
    let id1 = IdGenerator.generate()
    System.Threading.Thread.Sleep(10) // Ensure time difference
    let id2 = IdGenerator.generate()
    
    // UUID v7 IDs should be lexicographically sortable by time
    // id2 should be greater than id1 since it was generated later
    (String.Compare(id2, id1) > 0) |> should be True

[<Fact>]
let ``IdGenerator.isEmptyOrDefault returns true for empty string`` () =
    IdGenerator.isEmptyOrDefault "" |> should be True

[<Fact>]
let ``IdGenerator.isEmptyOrDefault returns true for null`` () =
    IdGenerator.isEmptyOrDefault null |> should be True

[<Fact>]
let ``IdGenerator.isEmptyOrDefault returns true for Guid.Empty`` () =
    let emptyGuid = Guid.Empty.ToString()
    IdGenerator.isEmptyOrDefault emptyGuid |> should be True

[<Fact>]
let ``IdGenerator.isEmptyOrDefault returns false for valid GUID`` () =
    let validId = IdGenerator.generate()
    IdGenerator.isEmptyOrDefault validId |> should be False

[<Fact>]
let ``IdGenerator.isValid returns true for valid GUID with hyphens`` () =
    let validGuid = "01234567-89ab-cdef-0123-456789abcdef"
    IdGenerator.isValid validGuid |> should be True

[<Fact>]
let ``IdGenerator.isValid returns true for valid GUID without hyphens`` () =
    let validGuid = "0123456789abcdef0123456789abcdef"
    IdGenerator.isValid validGuid |> should be True

[<Fact>]
let ``IdGenerator.isValid returns true for GUID with braces`` () =
    let validGuid = "{01234567-89ab-cdef-0123-456789abcdef}"
    IdGenerator.isValid validGuid |> should be True

[<Fact>]
let ``IdGenerator.isValid returns false for invalid format`` () =
    IdGenerator.isValid "not-a-guid" |> should be False

[<Fact>]
let ``IdGenerator.isValid returns false for empty string`` () =
    IdGenerator.isValid "" |> should be False

// ═══════════════════════════════════════════════════════════════
// Timestamp Tests
// ═══════════════════════════════════════════════════════════════

[<Fact>]
let ``Timestamp.now returns positive value`` () =
    let timestamp = Timestamp.now()
    timestamp |> should be (greaterThan 0L)

[<Fact>]
let ``Timestamp.now returns value in milliseconds`` () =
    let timestamp = Timestamp.now()
    
    // Unix timestamp for year 2020 (Jan 1, 2020) is approximately 1577836800000 ms
    // Unix timestamp for year 2100 (Jan 1, 2100) is approximately 4102444800000 ms
    // Current timestamp should be between these reasonable bounds
    timestamp |> should be (greaterThan 1577836800000L)
    timestamp |> should be (lessThan 4102444800000L)

[<Fact>]
let ``Timestamp.now returns increasing values`` () =
    let ts1 = Timestamp.now()
    System.Threading.Thread.Sleep(10) // Small delay
    let ts2 = Timestamp.now()
    
    ts2 |> should be (greaterThan ts1)

[<Fact>]
let ``Timestamp.toDateTimeOffset converts correctly`` () =
    // Unix timestamp for Jan 1, 2024 00:00:00 UTC
    let timestamp = 1704067200000L
    let dto = Timestamp.toDateTimeOffset timestamp
    
    dto.Year |> should equal 2024
    dto.Month |> should equal 1
    dto.Day |> should equal 1
    dto.Hour |> should equal 0
    dto.Minute |> should equal 0
    dto.Second |> should equal 0
    dto.Offset |> should equal TimeSpan.Zero // UTC

[<Fact>]
let ``Timestamp.fromDateTimeOffset converts correctly`` () =
    let dto = DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero)
    let timestamp = Timestamp.fromDateTimeOffset dto
    
    // Jan 1, 2024 00:00:00 UTC = 1704067200000L
    timestamp |> should equal 1704067200000L

[<Fact>]
let ``Timestamp roundtrip conversion preserves value`` () =
    let originalTimestamp = Timestamp.now()
    let dto = Timestamp.toDateTimeOffset originalTimestamp
    let roundtrippedTimestamp = Timestamp.fromDateTimeOffset dto
    
    roundtrippedTimestamp |> should equal originalTimestamp

[<Fact>]
let ``Timestamp.isInRange returns true for timestamp in range`` () =
    let start = 1704067200000L  // Jan 1, 2024
    let end' = 1704153600000L   // Jan 2, 2024
    let timestamp = 1704110400000L // Jan 1, 2024 12:00:00
    
    Timestamp.isInRange start end' timestamp |> should be True

[<Fact>]
let ``Timestamp.isInRange returns true for timestamp at start boundary`` () =
    let start = 1704067200000L
    let end' = 1704153600000L
    
    Timestamp.isInRange start end' start |> should be True

[<Fact>]
let ``Timestamp.isInRange returns true for timestamp at end boundary`` () =
    let start = 1704067200000L
    let end' = 1704153600000L
    
    Timestamp.isInRange start end' end' |> should be True

[<Fact>]
let ``Timestamp.isInRange returns false for timestamp before range`` () =
    let start = 1704067200000L
    let end' = 1704153600000L
    let timestamp = start - 1L
    
    Timestamp.isInRange start end' timestamp |> should be False

[<Fact>]
let ``Timestamp.isInRange returns false for timestamp after range`` () =
    let start = 1704067200000L
    let end' = 1704153600000L
    let timestamp = end' + 1L
    
    Timestamp.isInRange start end' timestamp |> should be False

// ═══════════════════════════════════════════════════════════════
// Document Tests
// ═══════════════════════════════════════════════════════════════

[<Fact>]
let ``Document.create generates non-empty ID`` () =
    let user = { Name = "Alice"; Email = "alice@example.com"; Age = 30 }
    let doc = Document.create user
    
    doc.Id |> should not' (be EmptyString)
    IdGenerator.isValid doc.Id |> should be True

[<Fact>]
let ``Document.create sets CreatedAt timestamp`` () =
    let user = { Name = "Bob"; Email = "bob@example.com"; Age = 25 }
    let beforeCreate = Timestamp.now()
    let doc = Document.create user
    let afterCreate = Timestamp.now()
    
    doc.CreatedAt |> should be (greaterThanOrEqualTo beforeCreate)
    doc.CreatedAt |> should be (lessThanOrEqualTo afterCreate)

[<Fact>]
let ``Document.create sets UpdatedAt equal to CreatedAt`` () =
    let user = { Name = "Charlie"; Email = "charlie@example.com"; Age = 35 }
    let doc = Document.create user
    
    doc.UpdatedAt |> should equal doc.CreatedAt

[<Fact>]
let ``Document.create wraps user data correctly`` () =
    let user = { Name = "Diana"; Email = "diana@example.com"; Age = 28 }
    let doc = Document.create user
    
    doc.Data.Name |> should equal "Diana"
    doc.Data.Email |> should equal "diana@example.com"
    doc.Data.Age |> should equal 28

[<Fact>]
let ``Document.createWithId uses provided ID`` () =
    let customId = "01234567-89ab-7def-8123-456789abcdef"
    let user = { Name = "Eve"; Email = "eve@example.com"; Age = 32 }
    let doc = Document.createWithId customId user
    
    doc.Id |> should equal customId

[<Fact>]
let ``Document.createWithId sets timestamps`` () =
    let customId = IdGenerator.generate()
    let user = { Name = "Frank"; Email = "frank@example.com"; Age = 40 }
    let doc = Document.createWithId customId user
    
    doc.CreatedAt |> should be (greaterThan 0L)
    doc.UpdatedAt |> should be (greaterThan 0L)
    doc.UpdatedAt |> should equal doc.CreatedAt

[<Fact>]
let ``Document.update preserves Id`` () =
    let user = { Name = "Grace"; Email = "grace@example.com"; Age = 27 }
    let originalDoc = Document.create user
    let updatedDoc = Document.update (fun u -> { u with Age = 28 }) originalDoc
    
    updatedDoc.Id |> should equal originalDoc.Id

[<Fact>]
let ``Document.update preserves CreatedAt`` () =
    let user = { Name = "Henry"; Email = "henry@example.com"; Age = 45 }
    let originalDoc = Document.create user
    System.Threading.Thread.Sleep(10) // Ensure time passes
    let updatedDoc = Document.update (fun u -> { u with Age = 46 }) originalDoc
    
    updatedDoc.CreatedAt |> should equal originalDoc.CreatedAt

[<Fact>]
let ``Document.update changes UpdatedAt`` () =
    let user = { Name = "Ivy"; Email = "ivy@example.com"; Age = 29 }
    let originalDoc = Document.create user
    System.Threading.Thread.Sleep(10) // Ensure time passes
    let updatedDoc = Document.update (fun u -> { u with Age = 30 }) originalDoc
    
    updatedDoc.UpdatedAt |> should be (greaterThan originalDoc.UpdatedAt)

[<Fact>]
let ``Document.update applies transformation function`` () =
    let user = { Name = "Jack"; Email = "jack@example.com"; Age = 33 }
    let doc = Document.create user
    let updatedDoc = Document.update (fun u -> { u with Age = u.Age + 1 }) doc
    
    updatedDoc.Data.Age |> should equal 34

[<Fact>]
let ``Document.map transforms data type`` () =
    let user = { Name = "Kate"; Email = "kate@example.com"; Age = 26 }
    let userDoc = Document.create user
    
    // Map to a different type
    let summaryDoc = Document.map (fun u -> $"{u.Name} ({u.Age})") userDoc
    
    summaryDoc.Data |> should equal "Kate (26)"

[<Fact>]
let ``Document.map preserves Id`` () =
    let user = { Name = "Liam"; Email = "liam@example.com"; Age = 31 }
    let originalDoc = Document.create user
    let mappedDoc = Document.map (fun u -> u.Name) originalDoc
    
    mappedDoc.Id |> should equal originalDoc.Id

[<Fact>]
let ``Document.map preserves CreatedAt`` () =
    let user = { Name = "Mia"; Email = "mia@example.com"; Age = 24 }
    let originalDoc = Document.create user
    let mappedDoc = Document.map (fun u -> u.Email) originalDoc
    
    mappedDoc.CreatedAt |> should equal originalDoc.CreatedAt

[<Fact>]
let ``Document.map preserves UpdatedAt`` () =
    let user = { Name = "Noah"; Email = "noah@example.com"; Age = 38 }
    let originalDoc = Document.create user
    let mappedDoc = Document.map (fun u -> u.Age) originalDoc
    
    mappedDoc.UpdatedAt |> should equal originalDoc.UpdatedAt
