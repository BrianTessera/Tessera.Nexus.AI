using Tessera.Nexus.AI.Domain.Entities;

namespace Tessera.Nexus.AI.Application.Contracts;

public interface IBusinessKnowledgeRepository
{
    Task<BusinessKnowledge?> GetByIdAsync(
        int businessKnowledgeId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BusinessKnowledge>> GetActiveAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BusinessKnowledge>> SearchAsync(
        string searchText,
        int maxResults = 25,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BusinessKnowledge>> GetRelevantKnowledgeAsync(
        string userQuestion,
        string? semanticDomain = null,
        string? company = null,
        string? plant = null,
        int maxResults = 10,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BusinessKnowledge>> GetBySemanticDomainAsync(
        string semanticDomain,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BusinessKnowledge>> GetExpiringKnowledgeAsync(
        DateOnly cutoffDate,
        CancellationToken cancellationToken = default);

    Task<int> CreateAsync(
        BusinessKnowledge knowledge,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        BusinessKnowledge knowledge,
        CancellationToken cancellationToken = default);

    Task RetireAsync(
        int businessKnowledgeId,
        string modifiedBy,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(
        string knowledgeTitle,
        CancellationToken cancellationToken = default);
}