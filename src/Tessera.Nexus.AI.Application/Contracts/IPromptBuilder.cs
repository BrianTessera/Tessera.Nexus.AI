namespace Tessera.Nexus.AI.Application.Contracts;

/// <summary>
/// Responsible for assembling the final prompt that will be
/// submitted to the AI model.
/// </summary>
public interface IPromptBuilder
{
    /// <summary>
    /// Builds the SQL-generation prompt using the default
    /// SQL Generation template.
    /// </summary>
    Task<string> BuildSqlPromptAsync(
        string userQuestion,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Builds a prompt from a specific template.
    /// </summary>
    Task<string> BuildPromptAsync(
        string templateName,
        string userQuestion,
        CancellationToken cancellationToken = default);
}