using Tessera.Nexus.AI.Domain.Entities;

namespace Tessera.Nexus.AI.Application.Contracts;

public interface IQueryFilterRepository
{
    Task<IReadOnlyList<QueryFilter>> GetActiveAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<QueryFilter>> GetAutoApplyFiltersAsync(
        CancellationToken cancellationToken = default);

    Task<QueryFilter?> GetByNameAsync(
        string filterName,
        CancellationToken cancellationToken = default);

    Task<int> CreateAsync(
        QueryFilter filter,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        QueryFilter filter,
        CancellationToken cancellationToken = default);
}