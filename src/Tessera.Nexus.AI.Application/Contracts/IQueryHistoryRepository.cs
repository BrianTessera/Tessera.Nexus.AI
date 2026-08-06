using Tessera.Nexus.AI.Domain.Entities;

namespace Tessera.Nexus.AI.Application.Contracts;

/// <summary>
/// Provides persistence and retrieval of AI query history.
/// </summary>
public interface IQueryHistoryRepository
{
    /// <summary>
    /// Creates a new query history record.
    /// </summary>
    Task<long> CreateAsync(
        QueryHistory queryHistory,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a query history record by identifier.
    /// </summary>
    Task<QueryHistory?> GetByIdAsync(
        long queryHistoryId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the most recent query history entries.
    /// </summary>
    Task<IReadOnlyList<QueryHistory>> GetRecentAsync(
        int take = 100,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns recent successful query executions.
    /// </summary>
    Task<IReadOnlyList<QueryHistory>> GetSuccessfulAsync(
        int take = 100,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns recent failed query executions.
    /// </summary>
    Task<IReadOnlyList<QueryHistory>> GetFailedAsync(
        int take = 100,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns recent executions for a specific user.
    /// </summary>
    Task<IReadOnlyList<QueryHistory>> GetByRequestedByAsync(
        string requestedBy,
        int take = 100,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns recent executions matching a question search term.
    /// </summary>
    Task<IReadOnlyList<QueryHistory>> SearchByQuestionAsync(
        string searchText,
        int take = 100,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a query history record.
    /// </summary>
    Task UpdateAsync(
        QueryHistory queryHistory,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a query history record.
    /// </summary>
    Task DeleteAsync(
        long queryHistoryId,
        CancellationToken cancellationToken = default);
}