using Tessera.Nexus.AI.Domain.Entities;

namespace Tessera.Nexus.AI.Application.Contracts;

public interface IBusinessRuleRepository
{
    Task<BusinessRule?> GetByIdAsync(
        int businessRuleId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BusinessRule>> GetActiveAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BusinessRule>> GetByDomainAsync(
        string semanticDomain,
        CancellationToken cancellationToken = default);

    Task<int> CreateAsync(
        BusinessRule rule,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        BusinessRule rule,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        int businessRuleId,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BusinessRule>> SearchAsync(
    string searchText,
    CancellationToken cancellationToken = default);
}