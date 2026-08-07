using Tessera.Nexus.AI.Domain.Entities;

namespace Tessera.Nexus.AI.Application.Contracts;

public interface IBusinessKnowledgeSearchService
{
    /// <summary>
    /// Finds the most relevant business knowledge entries
    /// for a user prompt.
    /// </summary>
    Task<IReadOnlyList<BusinessKnowledge>> FindRelevantKnowledgeAsync(
        string userQuestion,
        string? semanticDomain = null,
        string? company = null,
        string? plant = null,
        int maxResults = 10,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Builds prompt-ready business knowledge context
    /// for injection into AI prompts.
    /// </summary>
    Task<string> BuildKnowledgeContextAsync(
        string userQuestion,
        string? semanticDomain = null,
        string? company = null,
        string? plant = null,
        int maxResults = 10,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds business knowledge entries that match
    /// a specific term.
    /// </summary>
    Task<IReadOnlyList<BusinessKnowledge>> FindByTermAsync(
        string term,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns active knowledge entries
    /// for a semantic domain.
    /// </summary>
    Task<IReadOnlyList<BusinessKnowledge>> FindByDomainAsync(
        string semanticDomain,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds object-specific knowledge.
    /// Example:
    /// Customer
    /// ShipHead
    /// JobHead
    /// Part
    /// Supplier
    /// </summary>
    Task<IReadOnlyList<BusinessKnowledge>> FindByObjectAsync(
        string objectType,
        string objectName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds high-priority guidance that should
    /// always be included in prompts.
    /// </summary>
    Task<IReadOnlyList<BusinessKnowledge>> GetGlobalPromptInstructionsAsync(
        CancellationToken cancellationToken = default);
}