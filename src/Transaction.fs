/// <summary>
/// Module providing transaction management for FractalDb database operations.
/// </summary>
/// <remarks>
/// Transaction wraps ADO.NET IDbTransaction to provide F#-friendly transaction
/// semantics with explicit Commit/Rollback control and automatic cleanup via IDisposable.
///
/// Key features:
/// - Explicit transaction control (manual commit/rollback)
/// - IDisposable for automatic rollback if not committed
/// - Used by FractalDb.Execute and FractalDb.ExecuteTransaction methods
/// - Thread-safe: transactions are bound to a specific connection
///
/// Usage patterns:
/// 1. Manual transaction: use `create`, explicitly Commit or Rollback, dispose
/// 2. Automatic via FractalDb.Execute: commits on success, rolls back on exception
/// 3. Automatic via ExecuteTransaction: commits on Ok, rolls back on Error or exception
/// </remarks>
module FractalDb.Transaction

open System
open System.Data

/// <summary>
/// Represents a database transaction with explicit commit/rollback control.
/// </summary>
/// <remarks>
/// Transaction is a lightweight wrapper around ADO.NET IDbTransaction.
/// It implements IDisposable to ensure proper cleanup of the underlying transaction.
///
/// Lifecycle:
/// - Create: calls IDbConnection.BeginTransaction()
/// - Commit: commits changes, transaction becomes inactive
/// - Rollback: reverts changes, transaction becomes inactive
/// - Dispose: automatically rolls back if not committed
///
/// Thread safety:
/// - Transactions are not thread-safe
/// - Each thread should create its own transaction
/// - Do not share transactions across threads
/// </remarks>
type Transaction(innerTransaction: IDbTransaction) =
    
    /// <summary>
    /// Commits all changes made during this transaction.
    /// </summary>
    /// <remarks>
    /// After calling Commit, the transaction is complete and cannot be used again.
    /// Subsequent calls to Commit or Rollback will throw an exception.
    ///
    /// Commit makes all changes made during the transaction permanent in the database.
    /// If Commit fails (e.g., constraint violation), the transaction is rolled back.
    ///
    /// Best practice: Always call Commit explicitly when operations succeed.
    /// If Commit is not called before disposal, changes are rolled back.
    /// </remarks>
    /// <example>
    /// <code>
    /// use tx = Transaction.create conn
    /// // Perform database operations...
    /// tx.Commit()  // Make changes permanent
    /// </code>
    /// </example>
    member _.Commit() =
        innerTransaction.Commit()
    
    /// <summary>
    /// Rolls back all changes made during this transaction.
    /// </summary>
    /// <remarks>
    /// After calling Rollback, the transaction is complete and cannot be used again.
    /// Subsequent calls to Commit or Rollback will throw an exception.
    ///
    /// Rollback reverts all changes made during the transaction.
    /// The database returns to its state before the transaction began.
    ///
    /// Common use cases:
    /// - Explicit error handling: catch exception, rollback, handle error
    /// - Validation failure: detect invalid state, rollback, return error
    /// - User cancellation: user cancels operation, rollback changes
    /// </remarks>
    /// <example>
    /// <code>
    /// use tx = Transaction.create conn
    /// try
    ///     // Perform operations...
    ///     tx.Commit()
    /// with ex ->
    ///     tx.Rollback()  // Revert changes on error
    ///     reraise()
    /// </code>
    /// </example>
    member _.Rollback() =
        innerTransaction.Rollback()
    
    /// <summary>
    /// Disposes the transaction, automatically rolling back if not committed.
    /// </summary>
    /// <remarks>
    /// IDisposable implementation for use with `use` keyword.
    /// If Dispose is called before Commit, the transaction is rolled back.
    /// This ensures that uncommitted changes are not accidentally persisted.
    ///
    /// Always use `use` keyword to ensure proper disposal:
    /// - `use tx = ...` automatically disposes at end of scope
    /// - Manual `tx.Dispose()` is rarely needed
    ///
    /// Disposal is idempotent - safe to call multiple times.
    /// </remarks>
    interface IDisposable with
        member _.Dispose() =
            innerTransaction.Dispose()

/// <summary>
/// Creates a new transaction on the specified database connection.
/// </summary>
/// <param name="conn">The IDbConnection to create the transaction on.</param>
/// <returns>A new Transaction instance wrapping IDbTransaction.</returns>
/// <remarks>
/// Calls IDbConnection.BeginTransaction() to start a database transaction.
/// The transaction is bound to the connection and blocks other operations
/// on that connection until committed, rolled back, or disposed.
///
/// Important notes:
/// - Connection must be open before calling create
/// - Transaction holds a lock on the database (SQLite: full database lock)
/// - Always dispose transactions promptly to release locks
/// - Multiple transactions on same connection are not supported (SQLite)
///
/// Default isolation level:
/// - SQLite uses SERIALIZABLE isolation by default
/// - Writes block all other transactions
/// - Reads see snapshot from transaction start
/// </remarks>
/// <example>
/// <code>
/// open System.Data
/// open Microsoft.Data.Sqlite
///
/// // Open connection
/// let conn = new SqliteConnection("Data Source=:memory:")
/// conn.Open()
///
/// // Create transaction
/// use tx = Transaction.create conn
///
/// // Perform operations...
/// // conn |> Db.newCommand sql |> Db.exec |> ignore
///
/// // Commit on success
/// tx.Commit()
///
/// // Transaction automatically disposed (rolled back if not committed)
/// </code>
/// </example>
let create (conn: IDbConnection) : Transaction =
    let innerTx = conn.BeginTransaction()
    new Transaction(innerTx)
