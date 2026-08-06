namespace Tessera.Nexus.AI.Application.Contracts;

/// <summary>
/// Executes validated, read-only SQL queries against Epicor.
/// </summary>
public interface IQueryExecutionService
{
    /// <summary>
    /// Executes a validated read-only SQL statement and returns tabular results.
    /// </summary>
    Task<QueryExecutionResult> ExecuteAsync(
        string sql,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents the result of executing a SQL query.
/// </summary>
public sealed class QueryExecutionResult
{
    /// <summary>
    /// Indicates whether query execution succeeded.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// SQL statement that was executed.
    /// </summary>
    public string Sql { get; set; } = string.Empty;

    /// <summary>
    /// Error message when execution fails.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Number of rows returned.
    /// </summary>
    public int ResultRowCount { get; set; }

    /// <summary>
    /// Query execution duration in milliseconds.
    /// </summary>
    public long ElapsedMilliseconds { get; set; }

    /// <summary>
    /// Column names returned by the query.
    /// </summary>
    public IReadOnlyList<string> Columns { get; set; } =
        Array.Empty<string>();

    /// <summary>
    /// Rows returned by the query.
    /// Each row is represented as a dictionary of column name to value.
    /// </summary>
    public IReadOnlyList<IReadOnlyDictionary<string, object?>> Rows { get; set; } =
        Array.Empty<IReadOnlyDictionary<string, object?>>();
}