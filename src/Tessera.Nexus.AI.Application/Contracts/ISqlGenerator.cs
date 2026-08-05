namespace Tessera.Nexus.AI.Application.Contracts;

/// <summary>
/// Generates SQL from an AI prompt.
/// </summary>
public interface ISqlGenerator
{
    /// <summary>
    /// Generates SQL from the supplied prompt.
    /// </summary>
    Task<string> GenerateSqlAsync(
        string prompt,
        CancellationToken cancellationToken = default);
}