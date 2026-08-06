namespace Tessera.Nexus.AI.Domain.Entities;

/// <summary>
/// Stores an audit record for AI-generated query requests,
/// generated prompts, generated SQL, validation, execution,
/// and result metadata.
/// </summary>
public sealed class QueryHistory
{
    public long QueryHistoryId { get; set; }

    public string UserQuestion { get; set; } = string.Empty;

    public string? TemplateName { get; set; }

    public string GeneratedPrompt { get; set; } = string.Empty;

    public string GeneratedSql { get; set; } = string.Empty;

    public bool WasSuccessful { get; set; }

    public bool SqlValidated { get; set; }

    public bool QueryExecuted { get; set; }

    public int? ResultRowCount { get; set; }

    public long? GenerationElapsedMilliseconds { get; set; }

    public long? ExecutionElapsedMilliseconds { get; set; }

    public string? ErrorMessage { get; set; }

    public string? ResultJson { get; set; }

    public string? ModelName { get; set; }

    public string? PromptTemplateName { get; set; }

    public string? MetadataContextSummary { get; set; }

    public string? RequestedBy { get; set; }

    public string? SourcePage { get; set; }

    public DateTime CreatedDateUtc { get; set; } = DateTime.UtcNow;
}
