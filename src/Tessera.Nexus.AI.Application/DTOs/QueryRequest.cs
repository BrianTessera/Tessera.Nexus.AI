namespace Tessera.Nexus.AI.Application.DTOs;

/// <summary>
/// Represents a user request that will be translated
/// into SQL through the AI pipeline.
/// </summary>
public sealed class QueryRequest
{
    /// <summary>
    /// Natural language question supplied by the user.
    /// </summary>
    public string UserQuestion { get; set; } = string.Empty;

    /// <summary>
    /// Optional prompt template name.
    /// If not supplied, the default SQL Generation
    /// template will be used.
    /// </summary>
    public string? TemplateName { get; set; }

    /// <summary>
    /// Optional company filter.
    /// </summary>
    public string? Company { get; set; }

    /// <summary>
    /// Optional plant filter.
    /// </summary>
    public string? Plant { get; set; }

    /// <summary>
    /// Optional semantic domain.
    /// Example:
    /// Jobs, Inventory, Purchasing, Shipping.
    /// </summary>
    public string? SemanticDomain { get; set; }

    /// <summary>
    /// Whether generated SQL should be executed.
    /// During early development this can remain false.
    /// </summary>
    public bool ExecuteQuery { get; set; }

    /// <summary>
    /// Request creation time.
    /// </summary>
    public DateTime RequestUtc { get; set; } = DateTime.UtcNow;
}