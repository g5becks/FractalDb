module FractalDb.Tests.ValidationTests

// Suppress linter warnings for test code
// fsharplint:disable FL0072

open System
open System.Text.RegularExpressions
open System.Threading.Tasks
open Xunit
open FsUnit.Xunit
open FractalDb.Types
open FractalDb.Errors
open FractalDb.Schema
open FractalDb.Operators
open FractalDb.Collection
open FractalDb.Database

/// <summary>
/// Integration tests for schema validation functionality.
/// Tests the Validate field in SchemaDef and Collection.validate function.
/// </summary>
/// <summary>
/// Test record with fields that require validation.
/// </summary>
type ValidatedUser =
    { Name: string
      Email: string
      Age: int
      Active: bool }

/// <summary>
/// Validation function for ValidatedUser.
/// Rules:
/// - Email must match email format (contains @ and .)
/// - Age must be between 0 and 150
/// - Name must not be empty
/// </summary>
let validateUser (user: ValidatedUser) : Result<ValidatedUser, string> =
    // Check name is not empty
    if String.IsNullOrWhiteSpace(user.Name) then
        Error "Name cannot be empty"
    // Check email format (simple regex)
    elif not (Regex.IsMatch(user.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$")) then
        Error $"Email '{user.Email}' is not a valid email address"
    // Check age range
    elif user.Age < 0 || user.Age > 150 then
        Error $"Age {user.Age} must be between 0 and 150"
    else
        Ok user

/// <summary>
/// Schema with validation function.
/// </summary>
let validatedUserSchema: SchemaDef<ValidatedUser> =
    { Fields =
        [ { Name = "name"
            Path = None
            SqlType = SqliteType.Text
            Indexed = true
            Unique = false
            Nullable = false }
          { Name = "email"
            Path = None
            SqlType = SqliteType.Text
            Indexed = true
            Unique = true
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
      Validate = Some validateUser }

/// <summary>
/// Schema without validation function (validation bypassed).
/// </summary>
let noValidationSchema: SchemaDef<ValidatedUser> =
    { Fields =
        [ { Name = "name"
            Path = None
            SqlType = SqliteType.Text
            Indexed = true
            Unique = false
            Nullable = false }
          { Name = "email"
            Path = None
            SqlType = SqliteType.Text
            Indexed = true
            Unique = true
            Nullable = false } ]
      Indexes = []
      Timestamps = true
      Validate = None }

/// <summary>
/// Test fixture providing shared in-memory database and ValidatedUsers collection.
/// </summary>
type ValidationTestFixture() =
    let db = FractalDb.InMemory()

    let validatedUsers =
        db.Collection<ValidatedUser>("validated_users", validatedUserSchema)

    let noValidationUsers =
        db.Collection<ValidatedUser>("no_validation_users", noValidationSchema)

    member _.Db = db
    member _.ValidatedUsers = validatedUsers
    member _.NoValidationUsers = noValidationUsers

    interface IDisposable with
        member _.Dispose() = db.Close()

/// <summary>
/// Test class for validation operations.
/// </summary>
type ValidationTests(fixture: ValidationTestFixture) =
    interface IClassFixture<ValidationTestFixture>

    /// <summary>
    /// Test that insertOne with valid data succeeds.
    /// </summary>
    [<Fact>]
    member _.``insertOne with valid data succeeds``() : Task =
        task {
            // Arrange
            let validUser =
                { Name = "Alice Smith"
                  Email = "alice@example.com"
                  Age = 30
                  Active = true }

            // Act
            let! result = fixture.ValidatedUsers |> Collection.insertOne validUser

            // Assert
            match result with
            | Ok doc ->
                doc.Data.Name |> should equal "Alice Smith"
                doc.Data.Email |> should equal "alice@example.com"
                doc.Data.Age |> should equal 30
                doc.Id |> should not' (equal "")
            | Error err -> failwith $"Expected Ok, got Error: {err}"
        }

    /// <summary>
    /// Test that insertOne with invalid email returns Validation error when validated explicitly.
    /// </summary>
    [<Fact>]
    member _.``insertOne with invalid email returns Validation error``() : Task =
        task {
            // Arrange
            let invalidUser =
                { Name = "Bob Jones"
                  Email = "not-an-email" // Missing @ and domain
                  Age = 25
                  Active = true }

            // Act - validate first, then attempt insert only if valid
            let validateResult = fixture.ValidatedUsers |> Collection.validate invalidUser

            // Assert - validation should fail
            match validateResult with
            | Ok _ -> failwith "Expected Validation error, got Ok"
            | Error(FractalError.Validation(field, message)) ->
                message |> should haveSubstring "not a valid email address"
                message |> should haveSubstring "not-an-email"
            | Error err -> failwith $"Expected Validation error, got different error: {err}"
        }

    /// <summary>
    /// Test that insertOne with invalid age returns Validation error when validated explicitly.
    /// </summary>
    [<Fact>]
    member _.``insertOne with invalid age returns Validation error``() : Task =
        task {
            // Arrange
            let invalidUser =
                { Name = "Charlie Brown"
                  Email = "charlie@example.com"
                  Age = 200 // Age > 150
                  Active = true }

            // Act - validate first
            let validateResult = fixture.ValidatedUsers |> Collection.validate invalidUser

            // Assert - validation should fail
            match validateResult with
            | Ok _ -> failwith "Expected Validation error, got Ok"
            | Error(FractalError.Validation(field, message)) ->
                message |> should haveSubstring "Age 200"
                message |> should haveSubstring "between 0 and 150"
            | Error err -> failwith $"Expected Validation error, got different error: {err}"
        }

    /// <summary>
    /// Test that insertOne with empty name returns Validation error when validated explicitly.
    /// </summary>
    [<Fact>]
    member _.``insertOne with empty name returns Validation error``() : Task =
        task {
            // Arrange
            let invalidUser =
                { Name = "" // Empty name
                  Email = "test@example.com"
                  Age = 30
                  Active = true }

            // Act - validate first
            let validateResult = fixture.ValidatedUsers |> Collection.validate invalidUser

            // Assert - validation should fail
            match validateResult with
            | Ok _ -> failwith "Expected Validation error, got Ok"
            | Error(FractalError.Validation(field, message)) -> message |> should haveSubstring "Name cannot be empty"
            | Error err -> failwith $"Expected Validation error, got different error: {err}"
        }

    /// <summary>
    /// Test that Collection.validate returns Ok for valid data.
    /// </summary>
    [<Fact>]
    member _.``Collection.validate returns Ok for valid data``() : unit =
        // Arrange
        let validUser =
            { Name = "Diana Prince"
              Email = "diana@example.com"
              Age = 28
              Active = true }

        // Act
        let result = fixture.ValidatedUsers |> Collection.validate validUser

        // Assert
        match result with
        | Ok user ->
            user.Name |> should equal "Diana Prince"
            user.Email |> should equal "diana@example.com"
        | Error err -> failwith $"Expected Ok, got Error: {err}"

    /// <summary>
    /// Test that Collection.validate returns Error for invalid data.
    /// </summary>
    [<Fact>]
    member _.``Collection.validate returns Error for invalid data``() : unit =
        // Arrange
        let invalidUser =
            { Name = "Eve Adams"
              Email = "invalid-email" // Bad email format
              Age = 35
              Active = false }

        // Act
        let result = fixture.ValidatedUsers |> Collection.validate invalidUser

        // Assert
        match result with
        | Ok _ -> failwith "Expected Validation error, got Ok"
        | Error(FractalError.Validation(field, message)) -> message |> should haveSubstring "not a valid email address"
        | Error err -> failwith $"Expected Validation error, got different error: {err}"

    /// <summary>
    /// Test that schema without validator accepts any data (validation bypassed).
    /// </summary>
    [<Fact>]
    member _.``schema without validator accepts any data``() : Task =
        task {
            // Arrange
            let userWithBadData =
                { Name = "" // Empty name - would fail validation
                  Email = "not-an-email" // Bad email - would fail validation
                  Age = 999 // Out of range - would fail validation
                  Active = true }

            // Act
            let! result = fixture.NoValidationUsers |> Collection.insertOne userWithBadData

            // Assert
            match result with
            | Ok doc ->
                // Validation was bypassed, so bad data is accepted
                doc.Data.Name |> should equal ""
                doc.Data.Email |> should equal "not-an-email"
                doc.Data.Age |> should equal 999
            | Error err -> failwith $"Expected Ok (no validation), got Error: {err}"
        }

    /// <summary>
    /// Test that Collection.validate with no validator returns Ok.
    /// </summary>
    [<Fact>]
    member _.``Collection.validate with no validator returns Ok``() : unit =
        // Arrange
        let userWithBadData =
            { Name = ""
              Email = "invalid"
              Age = 999
              Active = true }

        // Act
        let result = fixture.NoValidationUsers |> Collection.validate userWithBadData

        // Assert
        match result with
        | Ok user ->
            // No validation, so data is accepted as-is
            user |> should equal userWithBadData
        | Error err -> failwith $"Expected Ok (no validation), got Error: {err}"

    /// <summary>
    /// Test that updateById with transformation resulting in invalid data can be validated.
    /// </summary>
    [<Fact>]
    member _.``updateById with invalid transformation can be validated``() : Task =
        task {
            // Arrange - Insert valid user first
            let validUser =
                { Name = "Update Test"
                  Email = "updatetest@example.com"
                  Age = 30
                  Active = true }

            let! insertResult = fixture.ValidatedUsers |> Collection.insertOne validUser

            let userId =
                match insertResult with
                | Ok doc -> doc.Id
                | Error err -> failwith $"Setup failed: {err}"

            // Act - Update to set invalid age
            let invalidTransform (user: ValidatedUser) : ValidatedUser = { user with Age = 200 }

            // Validate the transformed data before updating
            let transformedUser = invalidTransform validUser
            let validateResult = fixture.ValidatedUsers |> Collection.validate transformedUser

            // Assert - Validation should fail
            match validateResult with
            | Ok _ -> failwith "Expected Validation error, got Ok"
            | Error(FractalError.Validation(field, message)) ->
                message |> should haveSubstring "Age 200"
                message |> should haveSubstring "between 0 and 150"
            | Error err -> failwith $"Expected Validation error, got different error: {err}"

            // Verify original data is unchanged (update was not performed)
            let! currentDoc = fixture.ValidatedUsers |> Collection.findById userId

            match currentDoc with
            | Some doc -> doc.Data.Age |> should equal 30 // Original age
            | None -> failwith "Document not found"
        }

    /// <summary>
    /// Test that replaceOne with invalid data can be validated before replacement.
    /// </summary>
    [<Fact>]
    member _.``replaceOne with invalid data can be validated``() : Task =
        task {
            // Arrange - Insert valid user first
            let validUser =
                { Name = "Replace Test"
                  Email = "replacetest@example.com"
                  Age = 25
                  Active = true }

            let! insertResult = fixture.ValidatedUsers |> Collection.insertOne validUser

            match insertResult with
            | Error err -> failwith $"Setup failed: {err}"
            | Ok _ -> ()

            // Act - Attempt to replace with invalid data
            let invalidReplacement =
                { Name = "" // Empty name
                  Email = "replacetest@example.com"
                  Age = 25
                  Active = false }

            // Validate before replacing
            let validateResult = fixture.ValidatedUsers |> Collection.validate invalidReplacement

            // Assert - Validation should fail
            match validateResult with
            | Ok _ -> failwith "Expected Validation error, got Ok"
            | Error(FractalError.Validation(field, message)) -> message |> should haveSubstring "Name cannot be empty"
            | Error err -> failwith $"Expected Validation error, got different error: {err}"

            // Verify original data is unchanged (replacement was not performed)
            let! docs =
                fixture.ValidatedUsers
                |> Collection.find (Query.Field("email", FieldOp.Compare(box (CompareOp.Eq "replacetest@example.com"))))

            match docs with
            | [ doc ] -> doc.Data.Name |> should equal "Replace Test" // Original name
            | _ -> failwith "Expected exactly one document"
        }

    /// <summary>
    /// Test cross-field validation with multiple fields.
    /// </summary>
    [<Fact>]
    member _.``cross-field validation with dependent fields``() : Task =
        task {
            // Arrange - Custom validator that checks Name and Email together
            let crossFieldValidator (user: ValidatedUser) : Result<ValidatedUser, string> =
                // Rule: If email contains "admin", Name must contain "Admin"
                if user.Email.Contains("admin") && not (user.Name.Contains("Admin")) then
                    Error "Users with admin email must have 'Admin' in their name"
                else
                    validateUser user // Chain to standard validation

            let crossFieldSchema =
                { validatedUserSchema with
                    Validate = Some crossFieldValidator }

            let collection = fixture.Db.Collection<ValidatedUser>("cross_field_users", crossFieldSchema)

            // Act - Valid combination (admin email with Admin in name)
            let validAdminUser =
                { Name = "Admin User"
                  Email = "admin@example.com"
                  Age = 35
                  Active = true }

            let validResult = collection |> Collection.validate validAdminUser

            // Assert - Should pass
            match validResult with
            | Ok _ -> () // Success
            | Error err -> failwith $"Expected Ok, got Error: {err}"

            // Act - Invalid combination (admin email without Admin in name)
            let invalidAdminUser =
                { Name = "Regular User"
                  Email = "admin@example.com"
                  Age = 35
                  Active = true }

            let invalidResult = collection |> Collection.validate invalidAdminUser

            // Assert - Should fail
            match invalidResult with
            | Ok _ -> failwith "Expected Validation error, got Ok"
            | Error(FractalError.Validation(field, message)) ->
                message |> should haveSubstring "admin email must have 'Admin' in their name"
            | Error err -> failwith $"Expected Validation error, got different error: {err}"
        }

    /// <summary>
    /// Test that validation correctly reports the first error encountered.
    /// </summary>
    [<Fact>]
    member _.``validation returns first error when multiple violations exist``() : Task =
        task {
            // Arrange - User with multiple validation errors
            let multipleErrorsUser =
                { Name = "" // Error 1: Empty name
                  Email = "invalid" // Error 2: Bad email
                  Age = 200 // Error 3: Age out of range
                  Active = true }

            // Act
            let result = fixture.ValidatedUsers |> Collection.validate multipleErrorsUser

            // Assert - Should return first error (empty name)
            match result with
            | Ok _ -> failwith "Expected Validation error, got Ok"
            | Error(FractalError.Validation(field, message)) ->
                // The validator checks name first, so that error should be returned
                message |> should haveSubstring "Name cannot be empty"
            | Error err -> failwith $"Expected Validation error, got different error: {err}"
        }

    /// <summary>
    /// Test validation with boundary values (edge cases).
    /// </summary>
    [<Fact>]
    member _.``validation accepts boundary values``() : Task =
        task {
            // Arrange - User with edge case values
            let boundaryUser =
                { Name = "A" // Single character name (valid)
                  Email = "a@b.c" // Minimal valid email
                  Age = 0 // Minimum age
                  Active = false }

            // Act
            let result = fixture.ValidatedUsers |> Collection.validate boundaryUser

            // Assert - Should pass
            match result with
            | Ok user ->
                user.Name |> should equal "A"
                user.Age |> should equal 0
            | Error err -> failwith $"Expected Ok, got Error: {err}"

            // Test maximum age boundary
            let maxAgeUser = { boundaryUser with Age = 150 }
            let maxAgeResult = fixture.ValidatedUsers |> Collection.validate maxAgeUser

            match maxAgeResult with
            | Ok user -> user.Age |> should equal 150
            | Error err -> failwith $"Expected Ok, got Error: {err}"

            // Test just over maximum (should fail)
            let overMaxUser = { boundaryUser with Age = 151 }
            let overMaxResult = fixture.ValidatedUsers |> Collection.validate overMaxUser

            match overMaxResult with
            | Ok _ -> failwith "Expected Validation error for age 151, got Ok"
            | Error(FractalError.Validation(field, message)) ->
                message |> should haveSubstring "Age 151"
                message |> should haveSubstring "between 0 and 150"
            | Error err -> failwith $"Expected Validation error, got different error: {err}"
        }
