namespace Tessera.Nexus.AI.Application.Contracts;

/// <summary>
/// Builds a metadata context block that can be injected into AI prompts
/// to reduce hallucinated table and column names.
/// </summary>
public interface IMetadataContextBuilder
{
    /// <summary>
    /// Builds metadata context for a user question.
    /// </summary>
    Task<string> BuildMetadataContextAsync(
        string userQuestion,
        CancellationToken cancellationToken = default);
}