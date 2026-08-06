namespace Tessera.Nexus.AI.Application.Contracts;

/// <summary>
/// Validates generated SQL before execution.
/// </summary>
public interface ISqlValidator
{
    /// <summary>
    /// Validates a SQL statement and returns validation details.
    /// </summary>
    SqlValidationResult Validate(string sql);

    /// <summary>
    /// Determines whether the SQL statement is safe to execute.
    /// </summary>
    bool IsValid(string sql);
}

/// <summary>
/// Result of SQL validation.
/// </summary>
public sealed class SqlValidationResult
{
    public bool IsValid { get; set; }

    public string? ErrorMessage { get; set; }

    public List<string> Warnings { get; set; } = new();

    public List<string> Violations { get; set; } = new();
}