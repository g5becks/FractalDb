/// <summary>
/// FractalDb database class providing the main API for database operations.
/// </summary>
/// <remarks>
/// The Database module contains the FractalDb class which serves as the main entry
/// point for all database operations. It manages database connections, collections,
/// and transactions.
///
/// Key Features:
/// - File-based or in-memory databases
/// - Collection management with automatic table creation
/// - Transaction support (manual and automatic)
/// - Connection lifecycle management
/// - Thread-safe collection caching
///
/// Usage Pattern:
/// 1. Open database with FractalDb.Open or FractalDb.InMemory
/// 2. Get collections with database.Collection
/// 3. Perform operations on collections
/// 4. Close database with database.Close or use IDisposable
/// </remarks>
module FractalDb.Database

open System
open System.Collections.Concurrent
open System.Data
open System.Threading.Tasks
open Microsoft.Data.Sqlite
open FractalDb.Types
open FractalDb.Errors
open FractalDb.Schema
open FractalDb.Collection
open FractalDb.SqlTranslator
open FractalDb.TableBuilder
open FractalDb.Transaction

/// <summary>
/// Configuration options for the FractalDb database.
/// </summary>
/// <remarks>
/// DbOptions controls database behavior and features:
///
/// IdGenerator:
/// - Function to generate unique document IDs
/// - Default: IdGenerator.generate (UUID v7 / ULID)
/// - Custom: any function returning unique strings
/// - Must be thread-safe if used concurrently
///
/// EnableCache:
/// - Whether to cache translated SQL queries
/// - Default: false (no caching)
/// - true: Cache queries for performance (uses more memory)
/// - false: Translate queries every time (less memory)
///
/// Cache Benefits:
/// - Faster repeated queries with same structure
/// - Reduces CPU overhead of SQL translation
/// - Beneficial for high-throughput scenarios
///
/// Cache Tradeoffs:
/// - Increased memory usage
/// - Memory grows with query diversity
/// - Not beneficial for one-off queries
/// </remarks>
/// <example>
/// <code>
/// // Use defaults
/// let! db = FractalDb.Open("data.db")
///
/// // Custom ID generator
/// let options = {
///     IdGenerator = fun () -&gt; Guid.NewGuid().ToString()
///     EnableCache = false
/// }
/// let! db2 = FractalDb.Open("data2.db", options)
///
/// // Enable caching for performance
/// let cachedOptions = { DbOptions.defaults with EnableCache = true }
/// let! db3 = FractalDb.Open("data3.db", cachedOptions)
/// </code>
/// </example>
type DbOptions =
    {
        /// <summary>
        /// Function to generate unique document IDs.
        /// </summary>
        IdGenerator: unit -> string

        /// <summary>
        /// Whether to cache translated SQL queries for performance.
        /// </summary>
        EnableCache: bool

        /// <summary>
        /// Command behavior for database operations (default: SequentialAccess).
        /// </summary>
        /// <remarks>
        /// Controls how IDataReader accesses columns during query execution.
        /// Donald uses this setting for optimal performance with different access patterns.
        ///
        /// Options:
        /// - SequentialAccess: Columns must be read in order (best performance, lowest memory)
        /// - Default: Columns can be read in any order (more flexibility, slightly higher memory)
        ///
        /// SequentialAccess (recommended default):
        /// - Best performance for typical use cases
        /// - Lowest memory usage
        /// - Forward-only column access
        /// - Columns must be read in SELECT order
        /// - Ideal for generated SQL queries
        ///
        /// Default mode:
        /// - Allows random column access
        /// - Slightly higher memory usage
        /// - Use when column order doesn't match read order
        /// - Needed for some complex queries or custom SQL
        ///
        /// For FractalDb operations, SequentialAccess is optimal because:
        /// - All queries are generated with consistent column order
        /// - Memory efficiency is important for large result sets
        /// - Performance benefit with no flexibility loss
        /// </remarks>
        /// <example>
        /// <code>
        /// // Use default (SequentialAccess - recommended)
        /// let db = FractalDb.Open("data.db")
        ///
        /// // Use Default mode for custom scenarios
        /// let options = {
        ///     DbOptions.defaults with
        ///         CommandBehavior = CommandBehavior.Default
        /// }
        /// let db2 = FractalDb.Open("data.db", options)
        /// </code>
        /// </example>
        CommandBehavior: CommandBehavior
    }

/// <summary>
/// Module for DbOptions default values and utilities.
/// </summary>
module DbOptions =

    /// <summary>
    /// Default database options with standard settings.
    /// </summary>
    /// <remarks>
    /// Default configuration:
    /// - IdGenerator: IdGenerator.generate (UUID v7 / ULID)
    /// - EnableCache: false (no query caching)
    /// - CommandBehavior: SequentialAccess (best performance)
    ///
    /// These defaults are suitable for most applications:
    /// - UUID v7 IDs are time-sortable and globally unique
    /// - No caching keeps memory usage predictable
    /// - SequentialAccess provides optimal performance for generated queries
    ///
    /// Consider overriding:
    /// - IdGenerator: if you need custom ID format
    /// - EnableCache: if you have high-throughput repeated queries
    /// - CommandBehavior: if you need random column access (rare)
    /// </remarks>
    let defaults =
        { IdGenerator = IdGenerator.generate
          EnableCache = false
          CommandBehavior = CommandBehavior.SequentialAccess }

    /// <summary>
    /// Sets the CommandBehavior for database operations.
    /// </summary>
    /// <param name="behavior">The CommandBehavior to use for database operations.</param>
    /// <param name="opts">The DbOptions to modify.</param>
    /// <returns>A new DbOptions with the specified CommandBehavior.</returns>
    /// <remarks>
    /// Configures how IDataReader accesses columns during query execution.
    ///
    /// Use cases:
    /// - SequentialAccess (default): Best performance for typical operations
    /// - Default: When you need random column access (rare)
    ///
    /// SequentialAccess is recommended for FractalDb because:
    /// - All queries use consistent column order
    /// - Provides best performance with no practical limitations
    /// - Lower memory footprint for large result sets
    /// </remarks>
    /// <example>
    /// <code>
    /// // Configure for SequentialAccess (default, recommended)
    /// let opts1 = DbOptions.defaults
    ///
    /// // Configure for Default mode (random column access)
    /// let opts2 =
    ///     DbOptions.defaults
    ///     |> DbOptions.withCommandBehavior CommandBehavior.Default
    ///
    /// // Use with FractalDb.Open
    /// let db = FractalDb.Open("data.db", opts2)
    /// </code>
    /// </example>
    let withCommandBehavior (behavior: CommandBehavior) (opts: DbOptions) : DbOptions =
        { opts with CommandBehavior = behavior }

/// <summary>
/// Main database class for FractalDb operations.
/// </summary>
/// <remarks>
/// FractalDb is the primary interface for database operations:
///
/// Lifecycle:
/// 1. Create: FractalDb.Open (file) or FractalDb.InMemory
/// 2. Use: Get collections and perform operations
/// 3. Close: db.Close() or use IDisposable
///
/// Collections:
/// - Accessed via db.Collection&lt;'T&gt;(name, schema)
/// - Automatically cached (thread-safe)
/// - Tables created on first access
/// - Indexes created automatically
///
/// Transactions:
/// - Manual: db.Transaction() for explicit control
/// - Automatic: db.Execute or db.ExecuteTransaction
/// - Automatic commit/rollback handling
///
/// Thread Safety:
/// - FractalDb instance is thread-safe
/// - Collections are cached and reused safely
/// - SQLite connection handles serialization
/// - Multiple concurrent readers supported
/// - Single writer at a time (SQLite limitation)
///
/// Resource Management:
/// - Implements IDisposable for cleanup
/// - Automatically closes connection on dispose
/// - Use 'use' binding for automatic cleanup
/// - Can call Close() explicitly if needed
///
/// Performance:
/// - Connection pooling not needed (single connection)
/// - Collection cache avoids repeated table setup
/// - Query caching optional (via DbOptions)
/// - SQLite WAL mode recommended for concurrency
/// </remarks>
/// <example>
/// <code>
/// // File-based database with automatic cleanup
/// use db = FractalDb.Open("app.db")
/// let! users = db.Collection&lt;User&gt;("users", userSchema)
/// let! allUsers = users |&gt; Collection.find Query.Empty
///
/// // In-memory database (testing)
/// use testDb = FractalDb.InMemory()
/// let! testCollection = testDb.Collection&lt;TestData&gt;("test", testSchema)
///
/// // Custom options
/// let options = { DbOptions.defaults with EnableCache = true }
/// use db2 = FractalDb.Open("cached.db", options)
///
/// // Manual transaction
/// use tx = db.Transaction()
/// let! result1 = collection1 |&gt; Collection.insertOne data1
/// let! result2 = collection2 |&gt; Collection.insertOne data2
/// tx.Commit()
///
/// // Automatic transaction with rollback on error
/// let! result = db.Execute(fun tx -&gt;
///     task {
///         let! r1 = collection |&gt; Collection.insertOne doc1
///         let! r2 = collection |&gt; Collection.insertOne doc2
///         return (r1, r2)
///     }
/// )
/// </code>
/// </example>
type FractalDb private (connection: SqliteConnection, options: DbOptions, ownsConnection: bool) =

    /// <summary>
    /// Thread-safe cache of collection instances.
    /// </summary>
    /// <remarks>
    /// Collections are cached by name to avoid repeated initialization.
    /// ConcurrentDictionary ensures thread-safe access and insertion.
    /// Values are boxed (obj) to store different generic types.
    /// </remarks>
    let collections = ConcurrentDictionary<string, obj>()

    /// <summary>
    /// Track whether the database has been disposed.
    /// </summary>
    /// <remarks>
    /// Prevents operations on closed database.
    /// Set to true by Close() or Dispose().
    /// </remarks>
    let mutable disposed = false

    /// <summary>
    /// Gets the underlying database connection for advanced scenarios and Donald interop.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Exposes the IDbConnection to enable:
    /// - Direct SQL execution using Donald
    /// - Custom database operations
    /// - Connection configuration (PRAGMAs, etc.)
    /// - Advanced transaction management
    /// - Integration with other ADO.NET libraries
    /// </para>
    /// <para>
    /// <strong>Donald Integration</strong>
    /// </para>
    /// <para>
    /// The Connection property allows seamless integration with Donald for custom SQL operations
    /// while using FractalDb for document management. This is useful when you need:
    /// - Raw SQL queries not supported by FractalDb's query API
    /// - Custom database functions or aggregations
    /// - Schema migrations or database maintenance
    /// - Performance-critical queries with hand-tuned SQL
    /// - Integration with existing Donald-based code
    /// </para>
    /// <para>
    /// <strong>Important Considerations</strong>
    /// </para>
    /// <para>
    /// Direct connection use bypasses FractalDb's safety features:
    /// - Collection caching - Direct SQL won't update cached collections
    /// - Schema management - Manual schema changes may break FractalDb expectations
    /// - Type safety - SQL is string-based, errors caught at runtime
    /// - Validation - FractalDb validators won't run on direct SQL operations
    /// </para>
    /// <para>
    /// <strong>Best Practices</strong>
    /// </para>
    /// <list type="bullet">
    /// <item>Prefer FractalDb's Collection API for document operations</item>
    /// <item>Use Connection only for operations not supported by FractalDb</item>
    /// <item>Be aware that direct SQL may affect FractalDb-managed data</item>
    /// <item>Test thoroughly when mixing FractalDb and Donald operations</item>
    /// <item>Consider using FromConnection if you need extensive Donald usage</item>
    /// </list>
    /// <para>
    /// <strong>Thread Safety</strong>
    /// </para>
    /// <para>
    /// SQLite connections serialize access automatically. Multiple threads can safely
    /// use the Connection property, but operations will be serialized by SQLite.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// open Donald
    ///
    /// // Example 1: Custom query with Donald
    /// use db = FractalDb.Open("app.db")
    /// let conn = db.Connection
    ///
    /// // Execute custom Donald query
    /// let! userCount =
    ///     Db.scalar conn "SELECT COUNT(*) FROM users WHERE active = @active"
    ///         [ "active", SqlType.Boolean true ]
    ///         (fun rd -> rd.ReadInt32 0)
    ///
    /// printfn "Active users: %d" userCount
    ///
    /// // Example 2: Mix FractalDb document ops with Donald SQL
    /// use db = FractalDb.Open("data.db")
    /// let! users = db.Collection&lt;User&gt;("users", userSchema)
    ///
    /// // Insert document via FractalDb (validated, type-safe)
    /// let! newUser = users.InsertOne({ Name = "Alice"; Email = "alice@example.com" })
    ///
    /// // Query with custom SQL via Donald (flexible, powerful)
    /// let! topUsers =
    ///     Db.query db.Connection
    ///         """
    ///         SELECT json_extract(_data, '$.name') as name,
    ///                COUNT(*) as post_count
    ///         FROM posts
    ///         GROUP BY json_extract(_data, '$.userId')
    ///         ORDER BY post_count DESC
    ///         LIMIT 10
    ///         """
    ///         []
    ///         (fun rd -> {| Name = rd.ReadString "name"; PostCount = rd.ReadInt32 "post_count" |})
    ///
    /// // Example 3: Database configuration with Donald
    /// use db = FractalDb.Open("app.db")
    ///
    /// // Enable SQLite optimizations
    /// do! Db.exec db.Connection "PRAGMA journal_mode=WAL" []
    /// do! Db.exec db.Connection "PRAGMA synchronous=NORMAL" []
    /// do! Db.exec db.Connection "PRAGMA cache_size=-64000" []  // 64MB cache
    ///
    /// // Now use FractalDb normally
    /// let! collection = db.Collection&lt;Document&gt;("docs", schema)
    /// let! docs = collection.Find(Query.Empty)
    ///
    /// // Example 4: Custom aggregation with Donald
    /// use db = FractalDb.Open("analytics.db")
    /// let! events = db.Collection&lt;Event&gt;("events", eventSchema)
    ///
    /// // Insert events via FractalDb
    /// do! events.InsertMany(eventList)
    ///
    /// // Analyze with custom SQL
    /// let! stats =
    ///     Db.querySingle db.Connection
    ///         """
    ///         SELECT
    ///             COUNT(*) as total,
    ///             AVG(json_extract(_data, '$.duration')) as avg_duration,
    ///             MAX(json_extract(_data, '$.timestamp')) as last_event
    ///         FROM events
    ///         WHERE json_extract(_data, '$.type') = @eventType
    ///         """
    ///         [ "eventType", SqlType.String "page_view" ]
    ///         (fun rd ->
    ///             {| Total = rd.ReadInt32 "total"
    ///                AvgDuration = rd.ReadDouble "avg_duration"
    ///                LastEvent = rd.ReadString "last_event" |})
    ///
    /// // Example 5: Schema migration with Donald
    /// use db = FractalDb.Open("app.db")
    ///
    /// // Add custom metadata table
    /// do! Db.exec db.Connection
    ///         """
    ///         CREATE TABLE IF NOT EXISTS migrations (
    ///             id INTEGER PRIMARY KEY,
    ///             name TEXT NOT NULL,
    ///             applied_at TEXT NOT NULL
    ///         )
    ///         """
    ///         []
    ///
    /// // Record migration
    /// do! Db.exec db.Connection
    ///         "INSERT INTO migrations (name, applied_at) VALUES (@name, @timestamp)"
    ///         [ "name", SqlType.String "add_users_table"
    ///           "timestamp", SqlType.String (DateTime.UtcNow.ToString("O")) ]
    ///
    /// // Continue with FractalDb operations
    /// let! users = db.Collection&lt;User&gt;("users", userSchema)
    /// </code>
    /// </example>
    member _.Connection: IDbConnection = connection :> IDbConnection

    /// <summary>
    /// Gets the database options.
    /// </summary>
    /// <remarks>
    /// Returns the DbOptions used to create this database instance.
    /// Useful for inspecting configuration or creating derived instances.
    /// </remarks>
    member _.Options = options

    /// <summary>
    /// Gets whether this FractalDb instance owns and manages the database connection.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Indicates whether FractalDb will dispose the connection when Close() or Dispose() is called.
    /// </para>
    /// <para>
    /// - true: FractalDb created the connection (via Open or InMemory) and will dispose it
    /// - false: Connection was provided externally (via FromConnection) and won't be disposed
    /// </para>
    /// <para>
    /// When using FromConnection with an external connection, the caller retains ownership
    /// and is responsible for disposing the connection after the FractalDb instance is closed.
    /// This allows multiple FractalDb instances or custom Donald operations to share a connection.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // FractalDb owns connection - will dispose on Close
    /// use db1 = FractalDb.Open("data.db")
    /// printfn "Owns connection: %b" db1.OwnsConnection  // true
    ///
    /// // FractalDb owns connection - will dispose on Close
    /// use db2 = FractalDb.InMemory()
    /// printfn "Owns connection: %b" db2.OwnsConnection  // true
    ///
    /// // External connection - caller owns it
    /// use conn = new SqliteConnection("Data Source=data.db")
    /// conn.Open()
    /// use db3 = FractalDb.FromConnection(conn)
    /// printfn "Owns connection: %b" db3.OwnsConnection  // false
    /// // db3.Close() won't dispose conn - caller must do it
    /// conn.Close()
    /// </code>
    /// </example>
    member _.OwnsConnection = ownsConnection

    /// <summary>
    /// Gets whether the database has been disposed.
    /// </summary>
    /// <remarks>
    /// Returns true if Close() or Dispose() has been called.
    /// Operations on disposed database will throw ObjectDisposedException.
    /// </remarks>
    member _.IsDisposed = disposed

    // ═══════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Opens a SQLite database from a file path.
    /// </summary>
    /// <param name="path">Path to the database file.</param>
    /// <param name="options">Optional database configuration options.</param>
    /// <returns>
    /// FractalDb instance connected to the database file.
    /// </returns>
    /// <remarks>
    /// Open creates or opens a SQLite database file:
    ///
    /// Path behavior:
    /// - Absolute path: opens file at exact location
    /// - Relative path: relative to current working directory
    /// - Non-existent file: SQLite creates new database file
    /// - Existing file: opens and uses existing database
    /// - ":memory:": creates in-memory database (use InMemory instead)
    ///
    /// Connection lifecycle:
    /// - Connection opened immediately
    /// - Connection stays open until Close() or Dispose()
    /// - Single connection per FractalDb instance
    /// - Connection shared by all collections
    ///
    /// File permissions:
    /// - Requires read/write access to database file
    /// - Requires read/write access to directory (for journal)
    /// - Creates .db-journal or .db-wal files in same directory
    /// - SQLite handles file locking automatically
    ///
    /// Thread safety:
    /// - SQLite connection serializes access internally
    /// - Multiple threads can share FractalDb instance
    /// - Concurrent reads supported
    /// - Writes are serialized (SQLite limitation)
    ///
    /// Performance considerations:
    /// - WAL mode recommended for concurrency (set via pragma)
    /// - File-based I/O introduces latency vs in-memory
    /// - OS page cache helps with repeated reads
    /// - Consider SSD for better performance
    ///
    /// Error handling:
    /// - Throws if path is invalid or inaccessible
    /// - Throws if file is corrupted
    /// - Throws if another process has exclusive lock
    /// - Check file permissions before calling
    /// </remarks>
    /// <example>
    /// <code>
    /// // Open database with defaults
    /// use db = FractalDb.Open("data.db")
    /// let! users = db.Collection&lt;User&gt;("users", userSchema)
    ///
    /// // Open with custom options
    /// let options = {
    ///     IdGenerator = fun () -&gt; Guid.NewGuid().ToString()
    ///     EnableCache = true
    /// }
    /// use db2 = FractalDb.Open("cached.db", options)
    ///
    /// // Absolute path
    /// use db3 = FractalDb.Open("/var/data/app.db")
    ///
    /// // Relative path (relative to current directory)
    /// use db4 = FractalDb.Open("./data/local.db")
    ///
    /// // New database (will be created)
    /// use db5 = FractalDb.Open("new_database.db")
    /// // File created on first write operation
    ///
    /// // Configure SQLite pragmas
    /// use db6 = FractalDb.Open("wal.db")
    /// db6.Connection.Execute("PRAGMA journal_mode=WAL") |&gt; ignore
    /// db6.Connection.Execute("PRAGMA synchronous=NORMAL") |&gt; ignore
    /// </code>
    /// </example>
    static member Open(path: string, ?options: DbOptions) : FractalDb =
        let opts = defaultArg options DbOptions.defaults
        let connectionString = $"Data Source={path}"
        let conn = new SqliteConnection(connectionString)
        conn.Open()
        new FractalDb(conn, opts, true)

    /// <summary>
    /// Creates an in-memory SQLite database.
    /// </summary>
    /// <param name="options">Optional database configuration options.</param>
    /// <returns>
    /// FractalDb instance with in-memory database.
    /// </returns>
    /// <remarks>
    /// InMemory creates a temporary database in RAM:
    ///
    /// Memory database characteristics:
    /// - Entire database stored in RAM (no disk I/O)
    /// - Very fast operations (no file system overhead)
    /// - Data lost when connection closes
    /// - No persistence across application restarts
    /// - No .db file created
    ///
    /// Lifetime:
    /// - Database exists until Close() or Dispose()
    /// - Data deleted when connection closes
    /// - Cannot be shared across connections
    /// - Each InMemory() call creates isolated database
    ///
    /// Use cases:
    /// - Unit testing (fast, isolated, cleanup automatic)
    /// - Temporary data processing
    /// - Caching with structured queries
    /// - Development and prototyping
    /// - CI/CD pipelines (no file cleanup needed)
    ///
    /// Performance:
    /// - Much faster than file-based (no I/O)
    /// - No WAL mode needed (in-memory is fast)
    /// - Limited by available RAM
    /// - Large datasets may cause out-of-memory
    ///
    /// Thread safety:
    /// - Same as file-based database
    /// - Single connection serializes access
    /// - Multiple threads can share instance
    ///
    /// Limitations:
    /// - Data not persisted
    /// - Memory limited by system RAM
    /// - Cannot be accessed by other processes
    /// - No backup/restore (data is temporary)
    /// </remarks>
    /// <example>
    /// <code>
    /// // Testing with in-memory database
    /// [&lt;Test&gt;]
    /// let testUserOperations() = task {
    ///     use db = FractalDb.InMemory()
    ///     let! users = db.Collection&lt;User&gt;("users", userSchema)
    ///
    ///     let! inserted = users |&gt; Collection.insertOne testUser
    ///     let! found = users |&gt; Collection.findById inserted.Id
    ///
    ///     Assert.IsTrue(found.IsSome)
    /// }
    /// // Database automatically cleaned up at end of 'use' scope
    ///
    /// // Temporary data processing
    /// use tempDb = FractalDb.InMemory()
    /// let! staging = tempDb.Collection&lt;Record&gt;("staging", schema)
    ///
    /// // Load and process data in memory
    /// let! batch = staging |&gt; Collection.insertMany records
    /// let! processed = staging |&gt; Collection.find (Query.Field("status", FieldOp.Compare (box (CompareOp.Eq "valid"))))
    ///
    /// // Custom options for in-memory database
    /// let options = { DbOptions.defaults with EnableCache = true }
    /// use cachedDb = FractalDb.InMemory(options)
    ///
    /// // Multiple isolated databases (each has own data)
    /// use db1 = FractalDb.InMemory()
    /// use db2 = FractalDb.InMemory()
    /// // db1 and db2 are completely independent
    /// </code>
    /// </example>
    static member InMemory(?options: DbOptions) : FractalDb =
        FractalDb.Open(":memory:", ?options = options)

    /// <summary>
    /// Creates a FractalDb instance from an existing database connection.
    /// </summary>
    /// <param name="connection">The ADO.NET database connection to use.</param>
    /// <param name="options">Optional database configuration options.</param>
    /// <returns>
    /// FractalDb instance that uses the provided connection without owning it.
    /// </returns>
    /// <remarks>
    /// <para>
    /// FromConnection allows FractalDb to work with externally-managed connections.
    /// This is useful for:
    /// - Sharing a connection across multiple FractalDb instances
    /// - Using FractalDb alongside custom Donald operations
    /// - Advanced connection lifecycle management
    /// - Integration with existing database infrastructure
    /// - Testing scenarios with mock connections
    /// </para>
    /// <para>
    /// <strong>Important: Connection Ownership</strong>
    /// </para>
    /// <para>
    /// FractalDb created via FromConnection does NOT own the connection:
    /// - Close() and Dispose() will NOT close or dispose the connection
    /// - Caller retains full responsibility for connection lifecycle
    /// - Connection must remain open while FractalDb is in use
    /// - Caller must close/dispose connection after FractalDb is disposed
    /// </para>
    /// <para>
    /// <strong>Connection Requirements</strong>
    /// </para>
    /// <para>
    /// The provided connection must:
    /// - Be a valid IDbConnection implementation (typically SqliteConnection)
    /// - Already be opened (call connection.Open() before passing)
    /// - Support SQLite database operations
    /// - Remain open for the lifetime of FractalDb usage
    /// </para>
    /// <para>
    /// <strong>Thread Safety</strong>
    /// </para>
    /// <para>
    /// Same threading considerations as Open/InMemory:
    /// - SQLite connection serializes access automatically
    /// - Multiple threads can share FractalDb instance safely
    /// - Single writer at a time (SQLite limitation)
    /// - Multiple concurrent readers supported
    /// </para>
    /// <para>
    /// <strong>Donald Compatibility</strong>
    /// </para>
    /// <para>
    /// This method follows Donald's philosophy of accepting IDbConnection,
    /// making FractalDb compatible with any ADO.NET provider and allowing
    /// seamless integration with Donald's database operations.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// open System.Data
    /// open Microsoft.Data.Sqlite
    /// open Donald
    ///
    /// // Example 1: Share connection across multiple databases
    /// use conn = new SqliteConnection("Data Source=app.db")
    /// conn.Open()
    ///
    /// use db1 = FractalDb.FromConnection(conn)  // First FractalDb instance
    /// use db2 = FractalDb.FromConnection(conn)  // Second instance, same connection
    ///
    /// let! users = db1.Collection&lt;User&gt;("users", userSchema)
    /// let! logs = db2.Collection&lt;Log&gt;("logs", logSchema)
    ///
    /// // Both databases share the same connection
    /// // Caller must close connection after db1 and db2 are disposed
    /// conn.Close()
    ///
    /// // Example 2: Mix FractalDb with Donald operations
    /// use conn = new SqliteConnection("Data Source=data.db")
    /// conn.Open()
    ///
    /// // Use Donald for custom SQL
    /// do! Db.exec conn "PRAGMA journal_mode=WAL"
    /// do! Db.exec conn "CREATE TABLE IF NOT EXISTS metadata (key TEXT, value TEXT)"
    ///
    /// // Use FractalDb for document operations
    /// use db = FractalDb.FromConnection(conn, DbOptions.defaults)
    /// let! collection = db.Collection&lt;Document&gt;("docs", schema)
    /// do! collection.InsertOne({ Id = "1"; Data = "..." })
    ///
    /// // Custom Donald query on same connection
    /// let! metadata =
    ///     Db.query conn "SELECT * FROM metadata" (fun rd ->
    ///         { Key = rd.ReadString "key"
    ///           Value = rd.ReadString "value" })
    ///
    /// conn.Close()  // Caller closes the connection
    ///
    /// // Example 3: Testing with owned connection cleanup
    /// [&lt;Test&gt;]
    /// let ``test database operations`` () =
    ///     use conn = new SqliteConnection(":memory:")
    ///     conn.Open()
    ///
    ///     use db = FractalDb.FromConnection(conn)
    ///     let! users = db.Collection&lt;User&gt;("users", userSchema)
    ///
    ///     // Test operations...
    ///     let! result = users.InsertOne(testUser)
    ///     Assert.IsOk(result)
    ///
    ///     // Automatic cleanup via 'use' bindings
    ///     // db.Dispose() doesn't close conn
    ///     // conn.Dispose() closes and disposes connection
    /// </code>
    /// </example>
    static member FromConnection(connection: IDbConnection, ?options: DbOptions) : FractalDb =
        let opts = defaultArg options DbOptions.defaults
        // Cast to SqliteConnection - required for FractalDb's SQLite-specific operations
        match connection with
        | :? SqliteConnection as sqliteConn -> new FractalDb(sqliteConn, opts, false)
        | _ -> failwith "FractalDb.FromConnection requires a SqliteConnection"

    // ═══════════════════════════════════════════════════════════════
    // COLLECTION ACCESS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets or creates a collection with the specified schema.
    /// </summary>
    /// <param name="name">Name of the collection (used as table name).</param>
    /// <param name="schema">Schema definition with fields, indexes, and validation.</param>
    /// <returns>
    /// Collection instance for performing CRUD operations.
    /// </returns>
    /// <remarks>
    /// Collection method provides access to typed collections:
    ///
    /// Collection caching:
    /// - Collections are cached by name (thread-safe)
    /// - First access creates collection and ensures table exists
    /// - Subsequent accesses return cached instance
    /// - Cache persists for lifetime of FractalDb instance
    /// - Different generic types with same name share cache entry
    ///
    /// Table creation:
    /// - Automatically creates table if it doesn't exist
    /// - Creates all indexes defined in schema
    /// - Idempotent: safe to call multiple times
    /// - Only creates missing tables/indexes
    /// - No-op if table already exists
    ///
    /// Schema management:
    /// - Schema is fixed at first collection access
    /// - Changing schema requires dropping and recreating table
    /// - No automatic schema migration
    /// - Adding indexes requires manual ALTER TABLE
    ///
    /// Type safety:
    /// - Generic type 'T must match documents in collection
    /// - Runtime type mismatches cause deserialization errors
    /// - Schema validation (if defined) enforces structure
    /// - Consider versioning documents for schema evolution
    ///
    /// Name constraints:
    /// - Collection names must be valid SQLite table names
    /// - Alphanumeric and underscore characters recommended
    /// - Avoid SQLite reserved words
    /// - Case-sensitive (unless PRAGMA case_sensitive_like)
    ///
    /// Performance:
    /// - First access: table creation + index creation (slow)
    /// - Cached access: instant (no database operations)
    /// - Table creation uses transaction (all-or-nothing)
    /// - Index creation can be slow for large tables
    ///
    /// Thread safety:
    /// - Thread-safe collection caching (ConcurrentDictionary)
    /// - Multiple threads can request same collection safely
    /// - First thread creates, others wait and get cached instance
    /// - Collection operations are thread-safe (SQLite serialization)
    ///
    /// Error handling:
    /// - Throws if database is disposed
    /// - Throws if table creation fails
    /// - Throws if index creation fails
    /// - Throws if name is invalid SQLite identifier
    /// </remarks>
    /// <example>
    /// <code>
    /// // Define user type
    /// type User = {
    ///     Name: string
    ///     Email: string
    ///     Age: int
    /// }
    ///
    /// // Define schema
    /// let userSchema = {
    ///     Fields = [
    ///         { Name = "email"; Type = Text; Unique = true; NotNull = true }
    ///         { Name = "age"; Type = Integer; Unique = false; NotNull = false }
    ///     ]
    ///     Indexes = [
    ///         { Fields = ["name"]; Unique = false }
    ///     ]
    ///     Validate = Some (fun user -&gt;
    ///         if user.Age &lt; 18 then Error "Must be 18+"
    ///         else Ok user
    ///     )
    /// }
    ///
    /// // Get collection (creates table on first access)
    /// use db = FractalDb.Open("app.db")
    /// let users = db.Collection&lt;User&gt;("users", userSchema)
    ///
    /// // Use collection for operations
    /// let! doc = users |&gt; Collection.insertOne { Name = "Alice"; Email = "alice@example.com"; Age = 30 }
    /// let! found = users |&gt; Collection.findById doc.Id
    ///
    /// // Multiple collections
    /// let orders = db.Collection&lt;Order&gt;("orders", orderSchema)
    /// let products = db.Collection&lt;Product&gt;("products", productSchema)
    ///
    /// // Cached access (instant, no table creation)
    /// let users2 = db.Collection&lt;User&gt;("users", userSchema)
    /// // users2 is same instance as users
    ///
    /// // Schema evolution pattern
    /// type UserV2 = {
    ///     Name: string
    ///     Email: string
    ///     Age: int
    ///     CreatedAt: DateTime  // New field
    /// }
    ///
    /// // Old version still works
    /// let usersV1 = db.Collection&lt;User&gt;("users", userSchema)
    ///
    /// // New version with migration
    /// let usersV2 = db.Collection&lt;UserV2&gt;("users_v2", userSchemaV2)
    /// // Migrate data from users to users_v2, then drop old table
    /// </code>
    /// </example>
    member this.Collection<'T>(name: string, schema: SchemaDef<'T>) : Collection<'T> =
        if disposed then
            raise (ObjectDisposedException("FractalDb", "Database has been disposed"))

        // Use GetOrAdd for thread-safe caching
        let result =
            collections.GetOrAdd(
                name,
                fun _ ->
                    // Create collection instance
                    let coll =
                        { Name = name
                          Schema = schema
                          Connection = connection :> IDbConnection
                          IdGenerator = options.IdGenerator
                          Translator = SqlTranslator<'T>(schema, options.EnableCache)
                          EnableCache = options.EnableCache }

                    // Ensure table and indexes exist
                    TableBuilder.ensureTable (connection :> IDbConnection) name schema

                    // Box for storage in untyped dictionary
                    box coll
            )

        // Unbox and return typed collection
        result :?> Collection<'T>

    /// <summary>
    /// Creates a new manual transaction on this database connection.
    /// </summary>
    /// <returns>A new Transaction instance for manual transaction control.</returns>
    /// <remarks>
    /// Creates a transaction that must be explicitly committed or rolled back.
    /// Use this for manual transaction control when you need fine-grained control
    /// over when to commit or rollback.
    ///
    /// For automatic transaction management, use Execute or ExecuteTransaction instead.
    ///
    /// Important notes:
    /// - Transaction holds a lock on the database (SQLite: full database lock)
    /// - Always use `use` keyword to ensure proper disposal
    /// - Call Commit() to make changes permanent
    /// - Call Rollback() to revert changes
    /// - If neither is called before disposal, changes are automatically rolled back
    ///
    /// Thread safety:
    /// - Transactions are not thread-safe
    /// - Do not share transactions across threads
    /// </remarks>
    /// <example>
    /// <code>
    /// // Manual transaction control
    /// use tx = db.Transaction()
    /// try
    ///     // Perform multiple operations...
    ///     users |&gt; Collection.insertOne newUser |&gt; Async.RunSynchronously |&gt; ignore
    ///     posts |&gt; Collection.insertOne newPost |&gt; Async.RunSynchronously |&gt; ignore
    ///
    ///     // Commit if all succeed
    ///     tx.Commit()
    /// with ex -&gt;
    ///     // Rollback on error
    ///     tx.Rollback()
    ///     reraise()
    /// </code>
    /// </example>
    member this.Transaction() : Transaction = Transaction.create connection

    /// <summary>
    /// Executes a function within a transaction, automatically committing on success
    /// or rolling back on exception.
    /// </summary>
    /// <param name="fn">Function to execute within the transaction context.</param>
    /// <returns>Task containing the result of the function.</returns>
    /// <remarks>
    /// Provides automatic transaction management for functions that return Task&lt;'T&gt;.
    /// This is the recommended method for most transaction scenarios.
    ///
    /// Transaction behavior:
    /// - Creates a new transaction
    /// - Executes the provided function with the transaction
    /// - Commits if function completes successfully
    /// - Rolls back if function throws an exception
    /// - Re-raises the exception after rollback
    ///
    /// Best practices:
    /// - Use for functions that throw exceptions on error
    /// - Do not catch exceptions inside fn - let them propagate
    /// - For Result-based error handling, use ExecuteTransaction instead
    ///
    /// Performance:
    /// - Transaction overhead: minimal (single BeginTransaction call)
    /// - Lock duration: entire function execution
    /// - Keep transactions short to avoid blocking other operations
    /// </remarks>
    /// <example>
    /// <code>
    /// // Execute multiple operations in a transaction
    /// let! userId = db.Execute(fun tx -&gt;
    ///     task {
    ///         // Insert user
    ///         let! userResult = users |&gt; Collection.insertOne newUser
    ///         let userId = userResult.Id
    ///
    ///         // Insert related profile
    ///         let profile = { UserId = userId; Bio = "..." }
    ///         let! _ = profiles |&gt; Collection.insertOne profile
    ///
    ///         return userId
    ///     })
    /// // If any operation fails, entire transaction is rolled back
    /// </code>
    /// </example>
    member this.Execute<'T>(fn: Transaction -> Task<'T>) : Task<'T> =
        task {
            use tx = this.Transaction()

            try
                let! result = fn tx
                tx.Commit()
                return result
            with ex ->
                tx.Rollback()
                return raise ex
        }

    /// <summary>
    /// Executes a Result-returning function within a transaction, committing on Ok
    /// or rolling back on Error.
    /// </summary>
    /// <param name="fn">Function returning FractalResult&lt;'T&gt; within transaction.</param>
    /// <returns>Task containing FractalResult&lt;'T&gt; from the function.</returns>
    /// <remarks>
    /// Provides automatic transaction management for Result-based error handling.
    /// This is the recommended method when using FractalResult for error handling.
    ///
    /// Transaction behavior:
    /// - Creates a new transaction
    /// - Executes the provided function with the transaction
    /// - Commits if function returns Ok
    /// - Rolls back if function returns Error
    /// - Rolls back and converts to FractalError.Transaction if exception is thrown
    ///
    /// Error handling:
    /// - Ok results: transaction committed, result returned
    /// - Error results: transaction rolled back, error returned
    /// - Exceptions: transaction rolled back, exception wrapped in FractalError.Transaction
    ///
    /// Best practices:
    /// - Use for functions that return FractalResult&lt;'T&gt;
    /// - Use Result.bind or computation expressions to chain operations
    /// - Avoid throwing exceptions inside fn - use Error instead
    ///
    /// Performance:
    /// - Transaction overhead: minimal (single BeginTransaction call)
    /// - Lock duration: entire function execution
    /// - Keep transactions short to avoid blocking other operations
    /// </remarks>
    /// <example>
    /// <code>
    /// // Execute operations with Result-based error handling
    /// let! result = db.ExecuteTransaction(fun tx -&gt;
    ///     task {
    ///         // Insert user
    ///         let! userResult = users |&gt; Collection.insertOne newUser
    ///         match userResult with
    ///         | Error err -&gt; return Error err  // Rollback on error
    ///         | Ok user -&gt;
    ///             // Insert profile
    ///             let profile = { UserId = user.Id; Bio = "..." }
    ///             let! profileResult = profiles |&gt; Collection.insertOne profile
    ///             return profileResult  // Commit if Ok, rollback if Error
    ///     })
    ///
    /// match result with
    /// | Ok _ -&gt; printfn "Transaction committed"
    /// | Error err -&gt; printfn "Transaction rolled back: %s" err.Message
    /// </code>
    /// </example>
    member this.ExecuteTransaction<'T>(fn: Transaction -> Task<FractalResult<'T>>) : Task<FractalResult<'T>> =
        task {
            use tx = this.Transaction()

            try
                let! result = fn tx

                match result with
                | Ok _ -> tx.Commit()
                | Error _ -> tx.Rollback()

                return result
            with ex ->
                tx.Rollback()
                return Error(FractalError.Transaction ex.Message)
        }

    /// <summary>
    /// Closes the database connection and releases all resources.
    /// </summary>
    /// <remarks>
    /// Closes and disposes the underlying SqliteConnection if not already disposed.
    /// This method is idempotent - safe to call multiple times.
    ///
    /// Important notes:
    /// - After calling Close, the database instance cannot be used
    /// - Attempting to use collections or create transactions after Close will fail
    /// - Always call Close when done with the database to release resources
    /// - Use `use` keyword for automatic disposal instead of manual Close calls
    ///
    /// Resource cleanup:
    /// - Closes SQLite connection
    /// - Disposes SqliteConnection object
    /// - Releases file locks (for file-based databases)
    /// - Marks instance as disposed
    ///
    /// Thread safety:
    /// - Close is thread-safe and can be called from any thread
    /// - Multiple concurrent Close calls are safe (idempotent)
    ///
    /// Best practices:
    /// - Prefer `use db = FractalDb.Open(...)` for automatic disposal
    /// - Only call Close manually when managing lifetime explicitly
    /// - Close before application shutdown to ensure clean shutdown
    /// </remarks>
    /// <example>
    /// <code>
    /// // Manual close
    /// let db = FractalDb.Open("data.db")
    /// try
    ///     // Use database...
    ///     let users = db.Collection&lt;"users", userSchema&gt;
    ///     // ... operations ...
    /// finally
    ///     db.Close()  // Explicit cleanup
    ///
    /// // Preferred: automatic disposal
    /// use db = FractalDb.Open("data.db")
    /// let users = db.Collection&lt;"users", userSchema&gt;
    /// // ... operations ...
    /// // Automatically closed at end of scope
    /// </code>
    /// </example>
    member this.Close() =
        if not disposed then
            if ownsConnection then
                connection.Close()
                connection.Dispose()

            disposed <- true

    /// <summary>
    /// Disposes the database instance, closing the connection and releasing resources.
    /// </summary>
    /// <remarks>
    /// IDisposable implementation that delegates to Close().
    /// Enables automatic disposal with `use` keyword.
    ///
    /// This method is called automatically when:
    /// - Exiting a `use` binding scope
    /// - Exiting a `using` statement
    /// - Garbage collector finalizes the object (not recommended - always dispose explicitly)
    ///
    /// Disposal behavior:
    /// - Same as calling Close() method
    /// - Idempotent - safe to call multiple times
    /// - Thread-safe
    ///
    /// Best practice:
    /// - Always use `use` keyword instead of calling Dispose() directly
    /// - Let the language handle disposal automatically
    /// </remarks>
    /// <example>
    /// <code>
    /// // Automatic disposal with use keyword
    /// use db = FractalDb.Open("data.db")
    /// let users = db.Collection&lt;"users", userSchema&gt;
    /// // ... perform operations ...
    /// // Dispose() called automatically at end of scope
    /// </code>
    /// </example>
    interface IDisposable with
        member this.Dispose() = this.Close()
