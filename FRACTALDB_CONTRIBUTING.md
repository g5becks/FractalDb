# Contributing to FractalDb

Thank you for your interest in contributing to FractalDb! This guide will help you get started with development.

## Table of Contents

- [Getting Started](#getting-started)
- [Project Structure](#project-structure)
- [Development Workflow](#development-workflow)
- [Code Style](#code-style)
- [Testing](#testing)
- [Documentation](#documentation)
- [Submitting Changes](#submitting-changes)

---

## Getting Started

### Prerequisites

- **.NET 9.0 SDK** or higher
- **Git** for version control
- **Task** (optional, for convenient commands via `Taskfile.yml`)

### Clone and Build

```bash
# Clone the repository
git clone https://github.com/g5becks/StrataDB.git
cd StrataDB

# Build the project
dotnet build FractalDb.slnx

# Run tests to verify setup
dotnet test FractalDb.slnx

# Run linter (requires Task)
cd /Users/takinprofit/Dev/StrataDB && task lint
```

### Project Layout

```
/StrataDB/
├── src/                    # F# source code (FractalDb)
├── tests/                  # F# test suite
├── FractalDb.slnx         # Solution file
├── Taskfile.yml           # Task runner configuration
├── FRACTALDB_README.md    # User documentation
├── FSHARP_PORT_DESIGN.md  # Architecture and design
└── backlog/               # Task management
    ├── tasks/             # Completed tasks
    └── docs/              # Documentation standards
```

---

## Project Structure

### Source Files (`src/`)

Files are ordered by dependency (bottom-up in compilation order):

1. **Types.fs** - Core document types, ULID generation, timestamps
2. **Errors.fs** - Error types and Result utilities
3. **Operators.fs** - Query operator discriminated unions
4. **Query.fs** - Query construction helpers
5. **Schema.fs** - Schema definitions and field types
6. **Options.fs** - Query options (sorting, pagination, projection)
7. **Serialization.fs** - JSON serialization utilities
8. **SqlTranslator.fs** - Query to SQL translation
9. **TableBuilder.fs** - SQLite schema management
10. **Transaction.fs** - Transaction support
11. **Collection.fs** - Collection operations (CRUD, queries)
12. **Database.fs** - Database management
13. **Builders.fs** - Computation expressions
14. **Library.fs** - Public API exports

### Test Files (`tests/`)

1. **Assertions.fs** - Custom test assertions
2. **QueryTests.fs** - Query construction
3. **SerializationTests.fs** - JSON serialization
4. **SqlTranslatorTests.fs** - SQL translation
5. **UniqueConstraintDebugTest.fs** - Constraint testing
6. **CrudTests.fs** - Basic CRUD operations
7. **QueryExecutionTests.fs** - Complex queries
8. **TransactionTests.fs** - Transaction behavior
9. **BatchTests.fs** - Batch operations
10. **AtomicTests.fs** - Atomic find-and-modify
11. **ValidationTests.fs** - Schema validation
12. **Tests.fs** - Placeholder
13. **Program.fs** - Test runner

---

## Development Workflow

### Using Task Commands (Recommended)

```bash
# Format code (Fantomas)
task fmt

# Run linter (FSharpLint)
task lint

# Build in debug mode
task build:debug

# Build in release mode
task build

# Run all tests
task test

# Run all checks (fmt + lint + build)
task check
```

### Using dotnet CLI Directly

```bash
# Build
dotnet build FractalDb.slnx

# Build in release mode
dotnet build FractalDb.slnx --configuration Release

# Run tests
dotnet test FractalDb.slnx

# Run specific test file
dotnet test --filter "FullyQualifiedName~CrudTests"

# Run with detailed output
dotnet test FractalDb.slnx --verbosity normal
```

### Development Cycle

1. **Create a branch** for your feature/fix
   ```bash
   git checkout -b feature/my-feature
   ```

2. **Make changes** to source or test files

3. **Run checks** frequently during development
   ```bash
   task check  # Format, lint, and build
   ```

4. **Add tests** for new functionality
   - Unit tests for logic and utilities
   - Integration tests for end-to-end workflows

5. **Update documentation** if changing public APIs
   - XML doc comments in source
   - Update FRACTALDB_README.md for user-facing changes

6. **Verify everything passes**
   ```bash
   task check  # All checks pass
   task test   # All tests pass
   ```

---

## Code Style

### Formatting

FractalDb uses **Fantomas** for automatic code formatting. Configuration is in `fsharplint.json`.

```bash
# Format all F# files
task fmt

# Format checks without modifying files
task fmt -- --check
```

**Key conventions:**
- 4-space indentation
- 100-character line length (flexible for readability)
- Spaces around operators
- Consistent bracket placement

### Linting

FractalDb uses **FSharpLint** to enforce style and quality rules.

```bash
# Run linter
task lint
```

**Acceptable warnings:**
- File size warnings for `Collection.fs`, `Builders.fs`, `Library.fs` (complex modules)
- Test code pattern warnings (line length, `failwith` reuse in assertions)

**Not acceptable:**
- Type errors
- Unused bindings
- Pattern match incompleteness
- Actual code smells

### Naming Conventions

| Element | Convention | Example |
|---------|-----------|---------|
| Modules | PascalCase | `Query`, `Schema`, `Collection` |
| Functions | camelCase | `insertOne`, `findById` |
| Types | PascalCase | `Document<'T>`, `FractalError` |
| Type parameters | `'T`, `'U` | `Result<'T, 'E>` |
| Constants | PascalCase | `DefaultLimit`, `MaxBatchSize` |
| Private functions | camelCase | `executeQuery`, `buildSql` |

### F# Idioms

- **Prefer immutability**: Use `let` bindings, avoid `mutable`
- **Use pipe operator**: `data |> transform |> process`
- **Pattern matching**: Exhaustive matches on discriminated unions
- **Result type**: Return `Result<'T, FractalError>` for operations that can fail
- **Task expressions**: Use `task { }` for async operations
- **Computation expressions**: Use builders for DSL-style APIs

### XML Documentation

All public APIs must have XML documentation:

```fsharp
/// <summary>
/// Inserts a single document into the collection.
/// </summary>
/// <param name="document">The document body to insert (without _id, timestamps)</param>
/// <param name="tx">Optional transaction to execute within</param>
/// <returns>A task that resolves to Ok with the inserted document or Error</returns>
/// <example>
/// <code>
/// let! result = users.InsertOne({ Name = "Alice"; Age = 30 })
/// match result with
/// | Ok doc -> printfn "Inserted: %s" doc._id
/// | Error err -> printfn "Error: %s" err.Message
/// </code>
/// </example>
let InsertOne (document: 'T) (tx: Transaction option) : Task<FractalResult<Document<'T>>> =
    // implementation
```

See `backlog/docs/doc-2 - FractalDb-XML-Documentation-Standards.md` for complete standards.

---

## Testing

### Test Organization

- **Unit tests**: Test individual functions/modules in isolation
- **Integration tests**: Test end-to-end workflows with real SQLite database

### Writing Tests

Use the custom assertions in `Assertions.fs`:

```fsharp
open FractalDb.Tests.Assertions

[<Fact>]
let ``InsertOne creates document with generated ID`` () = task {
    use db = FractalDb.InMemory()
    let users = db.Collection<User>("users", testSchema)
    
    let newUser = { Name = "Alice"; Email = "alice@example.com"; Age = 30 }
    let! result = users.InsertOne(newUser, None)
    
    result |> shouldBeOk
    let doc = result |> Result.get
    doc._id |> shouldNotBeEmpty
    doc.body.Name |> should equal "Alice"
}
```

### Running Tests

```bash
# All tests
task test

# Specific test file
dotnet test --filter "FullyQualifiedName~CrudTests"

# Specific test
dotnet test --filter "InsertOne creates document with generated ID"

# With detailed output
dotnet test FractalDb.slnx --verbosity normal
```

### Test Guidelines

1. **Use descriptive test names** with backticks for readability
2. **Arrange-Act-Assert** structure
3. **Clean up resources** with `use` binding for databases
4. **Test both success and failure cases**
5. **Use in-memory databases** for fast, isolated tests
6. **Add integration tests** for new features
7. **Verify error cases** return appropriate `FractalError` variants

---

## Documentation

### Source Documentation

- **XML doc comments** on all public types, functions, modules
- **Include examples** in `<example>` tags
- **Document parameters** with `<param>`
- **Document return values** with `<returns>`
- **Explain design decisions** in comments when non-obvious

### User Documentation

Update `FRACTALDB_README.md` when:
- Adding new public APIs
- Changing existing behavior
- Adding new features
- Changing configuration options

### Changelog

Add entries to `CHANGELOG.md` under "Unreleased" section:

```markdown
## [Unreleased]

### Added
- New feature description

### Changed
- Changed behavior description

### Fixed
- Bug fix description
```

---

## Submitting Changes

### Before Submitting

1. **All checks pass**: `task check`
2. **All tests pass**: `task test`
3. **Documentation updated**: XML docs, README, CHANGELOG
4. **Commit messages clear**: Use conventional commit format

### Commit Message Format

```
<type>(<scope>): <subject>

<body>

<footer>
```

**Types:**
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation only
- `style`: Code style (formatting, etc.)
- `refactor`: Code refactoring
- `test`: Adding/updating tests
- `chore`: Build, tools, dependencies

**Examples:**
```
feat(query): add support for array size operator

Implements the $size operator for querying array field lengths.
Includes unit tests and integration tests.

Closes #123
```

```
fix(collection): handle null values in field projection

Previously, null values in projected fields caused serialization errors.
Now properly handles nulls by omitting fields from result.

Fixes #456
```

### Pull Request Process

1. **Create descriptive PR title** following commit message format
2. **Describe changes** in PR body:
   - What changed
   - Why it changed
   - How to test it
3. **Reference issues** if applicable
4. **Ensure CI passes** (build, test, lint)
5. **Respond to review feedback**
6. **Squash commits** if requested

### Code Review Checklist

Reviewers will check:
- [ ] Code follows F# idioms and project style
- [ ] All public APIs have XML documentation
- [ ] Tests cover new functionality
- [ ] No regressions in existing tests
- [ ] Error handling is appropriate
- [ ] Performance considerations addressed
- [ ] Documentation is updated

---

## Architecture Guidelines

### Design Principles

Refer to `FSHARP_PORT_DESIGN.md` for complete architecture. Key principles:

1. **Type Safety**: Leverage F# type system for compile-time safety
2. **Functional First**: Immutable data, pure functions, composition
3. **Result-Based Errors**: No exceptions for expected errors
4. **Pipeline Friendly**: Design for `|>` operator usage
5. **Explicit Over Implicit**: Validation, transactions, etc. are explicit

### Adding New Features

1. **Read design doc**: `FSHARP_PORT_DESIGN.md`
2. **Check roadmap**: Does it align with project goals?
3. **Propose design**: Open an issue to discuss approach
4. **Implement with tests**: Write tests first (TDD)
5. **Document thoroughly**: XML docs, examples, README

### Performance Considerations

- **Batch operations**: Use transactions for multiple operations
- **Indexes**: Ensure commonly queried fields are indexed
- **Projections**: Use field projection to reduce payload
- **Parameterized queries**: Always use parameters (never string concatenation)
- **Connection pooling**: Reuse database connections

---

## Questions?

- **Issues**: [GitHub Issues](https://github.com/g5becks/StrataDB/issues)
- **Discussions**: [GitHub Discussions](https://github.com/g5becks/StrataDB/discussions)
- **Design Questions**: Read `FSHARP_PORT_DESIGN.md` first

---

Thank you for contributing to FractalDb! 🎉
