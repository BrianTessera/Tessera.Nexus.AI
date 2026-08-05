using Tessera.Nexus.AI.Domain.Entities;

namespace Tessera.Nexus.AI.Application.Contracts;

public interface IBusinessKnowledgeRepository
{
    Task<BusinessKnowledge?> GetByIdAsync(
        int businessKnowledgeId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BusinessKnowledge>> SearchAsync(
        string searchText,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BusinessKnowledge>> GetActiveAsync(
        CancellationToken cancellationToken = default);

    Task<int> CreateAsync(
        BusinessKnowledge knowledge,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        BusinessKnowledge knowledge,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        int businessKnowledgeId,
        CancellationToken cancellationToken = default);
}