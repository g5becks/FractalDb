module FractalDb.Tests.PropertyTests

/// <summary>
/// Property-based tests using FsCheck to verify algebraic laws and invariants.
/// Tests ID generation properties and basic serialization round-trips.
/// </summary>

open System
open Xunit
open FsCheck
open FsCheck.Xunit
open FractalDb.Types
open FractalDb.Serialization

// =============================================================================
// Test Data Types
// =============================================================================

type SimpleUser = {
    Name: string
    Age: int
}

// =============================================================================
// ID Generation Properties
// =============================================================================

[<Properties(MaxTest = 100)>]
module IdGenerationProperties =

    /// Property: Generated IDs are unique
    [<Property>]
    let ``Generated IDs are unique`` () =
        let ids = [ for _ in 1..100 -> IdGenerator.generate() ]
        let uniqueIds = ids |> Set.ofList
        uniqueIds.Count = ids.Length

    /// Property: IDs are valid GUIDs
    [<Property>]
    let ``Generated IDs are valid GUIDs`` () =
        let id = IdGenerator.generate()
        let mutable guid = Guid.Empty
        Guid.TryParse(id, &guid)

    /// Property: Later IDs have later timestamps (UUID v7 time-sortable)
    [<Property>]
    let ``Later generated IDs are lexicographically greater`` () =
        let id1 = IdGenerator.generate()
        System.Threading.Thread.Sleep(2) // Small delay to ensure different timestamp
        let id2 = IdGenerator.generate()
        
        // UUID v7 is time-sortable, so later ID should be lexicographically greater
        String.Compare(id2, id1, StringComparison.Ordinal) > 0

    /// Property: ID validation works correctly for valid IDs
    [<Property>]
    let ``Valid GUIDs pass validation`` () =
        let id = IdGenerator.generate()
        IdGenerator.isValid id

    /// Property: Generated IDs have correct format
    [<Property>]
    let ``Generated IDs have UUID format`` () =
        let id = IdGenerator.generate()
        // UUID format: 8-4-4-4-12 characters
        id.Length = 36 && 
        id.[8] = '-' && 
        id.[13] = '-' && 
        id.[18] = '-' && 
        id.[23] = '-'

    /// Property: Invalid strings fail validation
    [<Property>]
    let ``Empty string fails ID validation`` () =
        not (IdGenerator.isValid "")

    /// Property: Random non-GUID strings fail validation
    [<Property>]
    let ``Non-GUID strings fail ID validation`` (str: string) =
        if String.IsNullOrEmpty(str) || str.Length <> 36 then
            not (IdGenerator.isValid str)
        else
            // If it's 36 chars, might be valid GUID by chance
            true

// =============================================================================
// Serialization Properties
// =============================================================================

[<Properties(MaxTest = 50)>]
module SerializationProperties =

    /// Property: Integer round-trips correctly
    [<Property>]
    let ``Integer serialization round-trips`` (value: int) =
        let json = serialize value
        let deserialized = deserialize<int> json
        deserialized = value

    /// Property: String round-trips correctly
    [<Property>]
    let ``String serialization round-trips`` (value: NonNull<string>) =
        let str = value.Get
        let json = serialize str
        let deserialized = deserialize<string> json
        deserialized = str

    /// Property: Boolean round-trips correctly
    [<Property>]
    let ``Boolean serialization round-trips`` (value: bool) =
        let json = serialize value
        let deserialized = deserialize<bool> json
        deserialized = value

    /// Property: Simple record round-trips correctly
    [<Property>]
    let ``Simple record serialization round-trips`` (name: NonNull<string>) (age: int) =
        let user = { Name = name.Get; Age = age }
        let json = serialize user
        let deserialized = deserialize<SimpleUser> json
        deserialized.Name = user.Name && deserialized.Age = user.Age

    /// Property: List round-trips correctly
    [<Property>]
    let ``List serialization round-trips`` (values: int list) =
        let json = serialize values
        let deserialized = deserialize<int list> json
        deserialized = values

    /// Property: Option Some round-trips correctly
    [<Property>]
    let ``Option Some serialization round-trips`` (value: int) =
        let opt = Some value
        let json = serialize opt
        let deserialized = deserialize<int option> json
        deserialized = opt

    /// Property: Option None round-trips correctly
    [<Property>]
    let ``Option None serialization round-trips`` () =
        let opt: int option = None
        let json = serialize opt
        let deserialized = deserialize<int option> json
        deserialized = None

    /// Property: Serialization produces non-empty output
    [<Property>]
    let ``Serialization produces non-empty JSON`` (value: int) =
        let json = serialize value
        not (String.IsNullOrWhiteSpace(json))

    /// Property: Serialized records contain field names
    [<Property>]
    let ``Serialized records contain field names in JSON`` (name: NonNull<string>) (age: int) =
        let user = { Name = name.Get; Age = age }
        let json = serialize user
        // JSON should contain field names (camelCase)
        json.Contains("name") && json.Contains("age")

// =============================================================================
// Document Metadata Properties
// =============================================================================

[<Properties(MaxTest = 50)>]
module DocumentMetadataProperties =

    /// Property: Document metadata has valid structure
    [<Property>]
    let ``DocumentMeta has valid ID format`` () =
        let id = IdGenerator.generate()
        let meta: DocumentMeta = {
            Id = id
            CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            UpdatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        }
        IdGenerator.isValid meta.Id &&
        meta.CreatedAt > 0L &&
        meta.UpdatedAt >= meta.CreatedAt

    /// Property: Document timestamps are non-negative
    [<Property>]
    let ``Document timestamps are always positive`` (createdMs: PositiveInt) (updatedMs: PositiveInt) =
        let created = int64 createdMs.Get
        let updated = int64 updatedMs.Get + created // Ensure updated >= created
        let meta: DocumentMeta = {
            Id = IdGenerator.generate()
            CreatedAt = created
            UpdatedAt = updated
        }
        meta.CreatedAt > 0L && 
        meta.UpdatedAt >= meta.CreatedAt &&
        meta.UpdatedAt > 0L

    /// Property: Document with data preserves type information
    [<Property>]
    let ``Document preserves user data`` (name: NonNull<string>) (age: PositiveInt) =
        let user = { Name = name.Get; Age = age.Get }
        let doc: Document<SimpleUser> = {
            Id = IdGenerator.generate()
            Data = user
            CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            UpdatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        }
        doc.Data.Name = user.Name && doc.Data.Age = user.Age
